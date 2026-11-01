# Lead — attribute-list-structural

| | |
|---|---|
| Issue | thoenissen/Reihitsu#1492 — FMT-STRUCT: attribute lists are split and merged by the structural transforms |
| `agent-filed` | yes — goals, "Done when", and the prescribed solution are proposals |
| Parent | thoenissen/Reihitsu#1482 (package C6); blocker #1488 is closed (PR #1513 merged) |
| Slug | `attribute-list-structural` |
| Branch | `claude/brave-curie-dwg8sl` (session-designated branch, used instead of `claude/issue-1492-...`) |
| Draft PR | thoenissen/Reihitsu#1551 |
| Lane | behavior change |

## Lane and reason

The issue requests moving an existing formatter decision (FMT-BREAK-027's attribute-list split and merge) from the line-break phase into the structural-transform phase, with a change doc that re-homes the requirement and updates the code-owner lists and the `AGENTS.md` phase-ownership rows. Shipped source under `src/` plus a change doc under `sdd/changes/`: behavior-change lane. The issue states that formatter output stays unchanged.

Roles: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager before publishing if follow-up candidates exist. None skipped.

Note: the `AGENTS.md` ownership table edit is a tooling path; the behavior-change lane admits it (tooling → behavior change).

## Maintainer clarifications

None. The issue text asks the run to first check every statement about today's code against current `main` and drop statements an earlier package already made true or moot.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Claim | Branch name | Use the session-designated branch `claude/brave-curie-dwg8sl` instead of `claude/issue-1492-<slug>` | The session environment only permits pushing to its designated branch | Branch names are internal | claim commit |

## Follow-up candidates

None yet.
