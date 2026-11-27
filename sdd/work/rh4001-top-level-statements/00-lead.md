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

(none yet)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none yet)

## State

- Last step finished: claim, draft PR, prepare.
- Next step: spawn spec author.
