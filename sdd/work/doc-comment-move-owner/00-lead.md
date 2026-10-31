# Lead — issue thoenissen/Reihitsu#1491

## Run

- Issue: thoenissen/Reihitsu#1491 — "FMT-DOC: the documentation phase moves every documentation comment written behind code". Label `agent-filed`: its goals, "Done when" items, and target behavior are proposals. Parent: thoenissen/Reihitsu#1482 (package C2). Its blockers thoenissen/Reihitsu#1487 (PR #1510) and thoenissen/Reihitsu#1488 (PR #1513) are closed and merged.
- Slug: `doc-comment-move-owner`
- Branch: `claude/hopeful-dirac-8nqwtv` (the session's designated branch; see Decisions taken #1). Draft PR: thoenissen/Reihitsu#1532.
- Lane: **behavior change**. The issue asks to move the ownership of a formatter decision between phases and requires a change doc that widens FMT-DOC-001 and narrows FMT-BLANK-012.
- Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator. None skipped.
- The issue was written against an older `main` (`735d0dba`); every statement about today's code is checked against current `main` first. The issue states that a statement an earlier package made true or moot is noted and dropped, and that a difference that changes the target goes to the maintainer.
- Issue comments: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session's designated branch `claude/hopeful-dirac-8nqwtv` instead of `claude/issue-1491-<slug>` | The session environment forbids pushing to any other branch | Branch names are internal | ebba7a2 |

## Follow-up candidates

None yet.
