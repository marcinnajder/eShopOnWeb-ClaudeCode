---
name: pull-request-review
description: Code review of a GitHub pull request - delegates the analysis of the PR's changes to the independent reviewer agent, shows the findings (critical issues and suggestions) in the chat first and posts only the ones the user selects as inline PR comments. Use when the user runs /pull-request-review <pr-number> or asks for a review of a pull request.
argument-hint: <pr-number>
---

# Pull request code review

Organise a review of pull request #$ARGUMENTS. The analysis itself is done by the `reviewer` agent (fresh context, read-only), so it is not influenced by this conversation; this skill handles the interaction with the user. Follow the conventions in `CLAUDE.md`.

## 1. Collect context

- `gh pr view $ARGUMENTS` – title, description, linked issue (`Closes #...`)
- `gh issue view <issue-number>` – requirements and acceptance criteria
- `gh pr diff $ARGUMENTS` – the changes under review
- Make sure the reviewer reads the PR version of the code: if the working tree is clean, check out the PR branch (`gh pr checkout $ARGUMENTS`); otherwise tell the user and ask how to proceed.

## 2. Delegate the analysis to the `reviewer` agent

Start the `reviewer` agent and pass it everything it needs, since it does not see this conversation:
- PR number, title and description,
- the requirements and acceptance criteria of the linked issue,
- the full diff.

Do not add your own opinion about the changes to the task – the review should be independent.

## 3. Show, then publish on request

- Present the reviewer's numbered findings (Critical / Suggestions) in the chat and stop. Do not publish anything yet.
- If a finding looks wrong to you, you may add a short note, but keep the reviewer's list intact.
- When the user picks findings (e.g. "post 1 and 3"), post them as one review with inline comments on the relevant lines:
  `gh api -X POST repos/{owner}/{repo}/pulls/$ARGUMENTS/reviews` with `commit_id` (PR head), `event: COMMENT` and a `comments` array (`path`, `line`, `side: RIGHT`, `body`).
- Review comments are written in English.
