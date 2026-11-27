# Lead — RH4001 top-level statements

- Issue: thoenissen/Reihitsu#1086 — "RH4001: do not report a file with top-level statements". Labels: enhancement, analyzer, `agent-filed` (goals are proposals; the issue records a maintainer decision from question round 3 of #1011, Q3, option (a), dated 2026-10-18).
- Slug: `rh4001-top-level-statements`
- Branch: `claude/optimistic-hypatia-w436gv` (designated by the session)
- Draft PR: thoenissen/Reihitsu#1973
- Lane: `behavior-change` — the issue requests changed analyzer behavior (RH4001-001 changes: a file containing top-level statements is no longer reported), which needs a change doc and production code.
- Roles run: spec author → skeptic → test implementer → implementer → cleaner → spec checker → auditor → validator; issue manager only if follow-up candidates exist. Roles skipped: none.

## Maintainer clarifications

- Per the issue text: the maintainer decided (round 3, #1011 Q3, option a) that RH4001 does not report a file that contains top-level statements, because such a file is the program's entry point, not the home of the types declared next to it. Today `Program.cs` with top-level statements and `class Helper` is reported and the fix renames the file to `Helper.cs`.

## Maintainer answers

- Visible output change of the change doc (a file with top-level statements is no longer reported by RH4001, no fix offered): already decided by the maintainer in question round 3 on thoenissen/Reihitsu#1011 (Q3, decision comment 2026-10-18), tracked as #1086. Not re-asked.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | spec author | Ask the change doc's visible output change again? | No; treat the #1011 Q3 decision as the maintainer answer | The decision comment on #1011 names exactly this behavior | The maintainer decided it | 7411541 |
| 3 | skeptic | Accept the change doc? | Yes, no amendment needed; spec author sets `Status: Accepted` in a status-only follow-up before the spec checker | Skeptic: proceed, no conflict | Decided by maintainer (#1011 Q3) | d366fb0 |
| 4 | skeptic | Any vs. first top-level member a statement | Any, as the spec says | Same result on every compiling file; skeptic recommendation | Identical output | 7411541 |
| 2 | spec author | Change the rule page? | No | The exception is narrow; spec author's choice; spec checker will verify | Rule page unchanged | 7411541 |

## Follow-up candidates

(none yet)

## State

- Last step finished: spec checker (07, 710f346, consistent, no findings); main up to date; Release build green.
- Next step: auditor on head 710f346 against main 0feb518; then validator, publish.
