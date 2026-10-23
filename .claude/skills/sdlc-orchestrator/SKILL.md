---
name: sdlc-orchestrator
description: Drives the development workflow for one GitHub issue - checks which stage the issue is in (plan, implementation, pull request, merge) and performs the next stage, stopping for the user's approval. Use when the user runs /sdlc-orchestrator <issue-number> or asks to continue work on an issue.
argument-hint: <issue-number>
---

# SDLC orchestrator

Drive issue #$ARGUMENTS through the team workflow. Follow the conventions in `CLAUDE.md` (English on GitHub and in the repo, branch and PR naming).

The issue is created beforehand (by an analyst or with a plain prompt) and contains `Requirements`; `Implementation plan` is "To be defined" until a plan is approved. This skill covers the developer's part.

## 1. Report the current stage

Gather the state (read-only):

- `gh issue view $ARGUMENTS` – state, requirements, whether the implementation plan is filled in
- `gh issue develop $ARGUMENTS --list` – linked branch
- `gh pr list --state all --head <linked-branch>` – pull request and its state (if there is a linked branch)
- `git status`, current branch, uncommitted changes

Start the answer with a short status, e.g.:

```
Issue #9 "Admin panel: search products by name"
✓ Requirements   ✗ Implementation plan   ✗ Branch   ✗ PR
Next step: prepare the implementation plan.
```

If the issue does not exist or is closed without a merged PR, say so and stop.

## 2. Perform the next stage

Do only the first stage that is not done yet, then stop.

| Stage | Condition | Action | Stop and wait for |
|---|---|---|---|
| Plan | implementation plan is "To be defined" | Analyse the relevant code and present a plan in the chat: files to change and why, risks, how to verify. Do not change code. Offer a second opinion from the `devils-advocate` agent (pass it the requirements and the plan). | Discussion. Only after the user approves, write the agreed plan into the `Implementation plan` section of the issue (keep `Requirements` unchanged). |
| Implementation | plan exists, no task branch | Switch to `main`, pull, create the branch with `gh issue develop $ARGUMENTS --name feature/<number>-<slug> --base main --checkout`. Implement the plan from the issue, build, run the tests. Do not commit. | Manual check: tell the user how to run and verify the change, and that they should review the diff. |
| Pull request | branch has changes, no PR | Summarise the changes. After the user approves: commit, push, create the PR (title `#<number> <issue title>`; description with summary, how to test and `Closes #<number>`). | Code review. Suggest `/pull-request-review <pr-number>`. |
| Review | PR is open | Report review status and the number of unresolved comments. | Suggest `/pull-request-fix <pr-number>` for comments, or a manual merge on GitHub when the review is done. |
| Clean-up | PR is merged | After the user approves: switch to `main`, pull, delete the task branch locally and on GitHub (if it still exists). | Done. |

## Rules

- One stage per invocation; never jump ahead (e.g. from plan straight to commit).
- Ask before anything that leaves the local machine: editing the issue, commit, push, PR, deleting branches.
- If the state is unexpected (e.g. uncommitted changes on `main`, branch behind `main`), report it and ask how to proceed instead of guessing.
