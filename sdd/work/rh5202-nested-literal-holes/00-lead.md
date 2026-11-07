# Lead — rh5202-nested-literal-holes

- Issue: thoenissen/Reihitsu#1458 — "RH5202: the fix leaves nested verbatim strings and comments in a hole unchanged" — labels `bug`, `analyzer`, `agent-filed` (goals are proposals).
- Slug: `rh5202-nested-literal-holes`
- Branch: `claude/tender-shannon-bcg2tr` (session-designated branch, used instead of `claude/issue-1458-...`)
- PR: thoenissen/Reihitsu#1610 (draft claim)
- Lane: bug fix — the issue reports an observed-versus-expected difference in the RH5202 code fix against SHARED-FIX-003 / FMT-RAW-009.
- Roles: skeptic (premise + reachability) → test implementer (reproduction) → implementer → cleaner → spec checker → auditor → validator. Spec author only for a clarification.
- Maintainer clarification: the maintainer suspects the issue may already be fixed on `main` (later raw-string work landed, e.g. #1469, #1457, #1565).

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/tender-shannon-bcg2tr` | The session harness requires it | Branch name is internal | claim commit |

## Follow-up candidates

None yet.

## Worklist / next step

- Next: skeptic.
