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
| 2 | Spec author | Accessor attribute lists: move their split/merge too (B: predict line-break layout; C: decide on written layout) or keep them in the line-break phase (A)? | A — accessor list shape stays in the line-break phase (FMT-BREAK-032); the issue's first "Done when" item is narrowed to declarations, parameters and type parameters | The accessor decision depends on layout the line-break phase produces; B would add a prediction of a later phase (an architecture smell), C changes output and is not idempotent | Formatter output is unchanged under A | 91e64ce |
| 3 | Spec author | When to update code-owner rows and `AGENTS.md` phase-ownership rows | After the implementer, by a fresh spec-author agent, once the new transform types exist (`SpecConsistencyTests` rejects unknown type names) | Ordering constraint of the consistency test | Internal documentation | — |

## Follow-up candidates

1. Lambda attribute lists are split onto their own lines (`var f = [A, B] (int x) => x;`) — outside the run's issue; carries an output question. Source: `01-spec-author.md`.
2. One accessor attribute expands a one-line auto-property whose object initializer spans several lines (`X P { [A] get; set; } = new X { A = 1 };`), apparent conflict with FMT-BREAK-024 — outside the run's issue; carries an output question. Source: `01-spec-author.md`.
