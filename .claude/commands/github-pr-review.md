---
name: github-pr-review
description: >
  Use this skill whenever a user wants to review a GitHub Pull Request. Triggers
  on any mention of "review PR", "review pull request", "check PR #", "PR review",
  or any request that includes a pull request ID or URL and implies a code review.
  The skill fetches the PR diff and linked issue, runs a general review for any
  kind of finding (bugs, logic errors, security issues, code quality, missing
  requirements), and posts only actual findings as inline GitHub comments.
  No findings = no output. Always use this skill when a PR ID is given and a
  review is requested.
compatibility:
  tools:
    - GitHub MCP (mcp__github__ tools for reading PRs, issues, and posting review comments)
---

# GitHub PR Review Skill

## Goal

Perform a thorough, unbiased code review of a GitHub Pull Request. Report **only findings** — no praise, no filler, no forced output. If there is nothing to report, do nothing and stay silent.

All review output (findings) must be posted as GitHub review comments on the PR, **not** written in the chat.

---

## Step 1 — Gather all context

Using the GitHub MCP tools, collect:

1. **PR metadata** — title, description, base branch, author
   - Tool: `mcp__github__get_pull_request`
2. **PR diff** — the full file changes
   - Tool: `mcp__github__get_pull_request_diff` or `mcp__github__get_pull_request_files`
3. **Linked issue** — parse the PR body for `Closes #NNN`, `Fixes #NNN`, or `Resolves #NNN` patterns, then fetch the issue
   - Tool: `mcp__github__get_issue`
4. **Existing PR comments** — to avoid duplicating findings already raised
   - Tool: `mcp__github__list_review_comments` or `mcp__github__get_pull_request_comments`

---

## Step 2 — Analyze

Perform a general review across all changed files. Focus areas (non-exhaustive):

- **Correctness** — logic errors, off-by-one errors, wrong conditions, edge cases not handled
- **Security** — injection risks, exposed secrets, insecure defaults, missing input validation
- **Error handling** — missing error checks, swallowed exceptions, unclear failure modes
- **Code quality** — dead code, overly complex logic, misleading naming, missing null checks
- **Consistency** — deviates from patterns already established in the codebase (visible in diff context)
- **Issue coverage** — does the PR actually fulfill the requirements described in the linked issue? Flag any requirement that is not addressed.

**Only report findings.** A finding is something that is wrong, risky, incomplete, or missing. Do not report things that are done well.

---

## Step 3 — Post findings as GitHub review comments

For each finding:

- Post it as a **review comment on the specific line** where the problem occurs, using:
  - Tool: `mcp__github__create_review_comment`  
  - Fields: `path`, `line` (or `position`), `body`
- The comment body must:
  - Be written in **English**
  - Describe **what the problem is** and, where helpful, **why it matters**
  - Be concise and factual — no softening language, no praise

If a finding is not tied to a specific line (e.g., a missing feature from the issue), post it as a **general PR comment**:
- Tool: `mcp__github__add_pull_request_comment`

---

## Step 4 — Nothing to report?

If no findings are identified after a thorough review, **do nothing**. Do not post a comment saying "LGTM" or similar. Do not write anything in the chat. Silence is the correct output.

---

## Rules

- **No chat output** — results go to GitHub only, never to the conversation
- **No positive feedback** — findings only
- **English only** — all comments must be written in English
- **No duplicates** — check existing comments before posting
- **No forced findings** — a clean PR is a valid outcome
