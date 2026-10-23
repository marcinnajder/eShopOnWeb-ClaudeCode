---
name: pull-request-fix
description: Handles code review comments on a GitHub pull request - reads the comments with the code they refer to, judges whether each is valid, proposes fixes, and after approval applies them, then commits and replies to the comments only when the user asks. Use when the user runs /pull-request-fix <pr-number> or asks to address review comments.
argument-hint: <pr-number>
---

# Address pull request review comments

Handle the review comments on pull request #$ARGUMENTS as the author of the change. Follow the conventions in `CLAUDE.md`.

## 1. Collect comments

- `gh pr view $ARGUMENTS` and make sure the PR branch is checked out and up to date (`gh pr checkout $ARGUMENTS`).
- Inline comments: `gh api repos/{owner}/{repo}/pulls/$ARGUMENTS/comments` (skip replies and threads that are already answered).
- Review summaries: `gh api repos/{owner}/{repo}/pulls/$ARGUMENTS/reviews`; general comments: `gh pr view $ARGUMENTS --comments`.

## 2. Analyse and propose (no changes yet)

For each comment, read the code it refers to and decide:
- **Valid** – proposed fix (files, what changes),
- **Partly valid** – what to change and what not, and why,
- **Not valid** – why, with a reference to the code or requirements.

Present this as a numbered list and stop. Do not change any files yet.

## 3. Apply the agreed fixes – without committing

After the user decides which comments to address:
- apply the fixes, build and run the tests,
- do not commit, push or reply on GitHub,
- tell the user to review the changes (e.g. `git diff`) and stop.

## 4. Commit and reply – only when asked

When the user asks:
- commit the fixes to the PR branch (message referencing the review) and push,
- reply in each comment thread: what was changed (with the commit SHA) or why the comment was not addressed:
  `gh api -X POST repos/{owner}/{repo}/pulls/$ARGUMENTS/comments/<comment-id>/replies -f body="..."`
- Replies are written in English.
