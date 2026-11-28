# Lead — fmt-using-004-scope

- Issue: thoenissen/Reihitsu#1972 — `agent-filed` (goals are proposals)
- Branch: `claude/exciting-pasteur-x7li0l` (session-designated); draft PR thoenissen/Reihitsu#1991
- Lane: docs — the issue asks only for spec text to be reworded, with no output and no test change; the issue's own comment narrows it to a docs-lane run.
- Roles run: spec author (writes the text), spec checker. Validator only if a compiled file changes. Skipped: skeptic, test implementer, implementer, cleaner, auditor (not in the docs lane).

## Maintainer clarifications

- Issue comment (by the maintainer's account): the scope is narrowed — SHARED-USING-009, in `sdd/specs/shared/using-directive-ordering.md`, already states on `main` that the using phase leaves blank lines around a directive as written and that FMT-BLANK-013 writes those around a `#region`. Only the requirement sentence of FMT-USING-004 in `sdd/specs/formatter/using-directives.md` still needs scoping to the using phase. The run comes before #1971, which edits the same file.

## Maintainer answers

(none)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

(none)

## State

- Last step: claim, lane claimed.
- Next: spawn the spec author.
