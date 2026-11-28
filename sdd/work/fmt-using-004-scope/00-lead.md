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
| 1 | Spec author | Leave SHARED-USING-009 and `code-fixes.md` unchanged, against the issue's file list | Yes | The issue's comment narrows the scope, and SHARED-USING-009 (in `shared/using-directive-ordering.md`) already scopes the statement | Spec text only | e5ed931 |
| 2 | Spec author | Keep FMT-USING-004's title | Yes | The title does not claim the blank-line rule for the whole formatter | Spec text only | e5ed931 |
| 3 | Lead | No change doc | Yes | Records existing behavior; no output changes | Spec text only | e5ed931 |

## Follow-up candidates

(none)

## State

- Last step: spec author done (01-spec-author.md, e5ed931), pushed. No output questions.
- Next: spec checker.
