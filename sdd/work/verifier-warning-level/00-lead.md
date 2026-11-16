# Lead — verifier-warning-level

- Issue: thoenissen/Reihitsu#867 — code-fix verifier compiles source and fixed states at warning level 4, hiding warning-wave diagnostics. Labels: analyzer, maintenance, **agent-filed** (goals are proposals).
- Slug: `verifier-warning-level`
- Branch: `claude/elegant-volta-dng3cn` (session-designated)
- PR: thoenissen/Reihitsu#1798 (draft claim)
- Lane: **tests** — the change is to the analyzer test harness (`src/Reihitsu.Analyzer.Test/Base/`) and possibly fixtures; test-project paths only, no shipped source, no visible surface. A real code-fix defect the raised level exposes is filed separately (issue's own acceptance criterion), not fixed here.
- Roles run: skeptic (premise: the gap exists and is reachable; level choice), test implementer (harness change, the CS8848 proof test, fixture adjustments), cleaner, spec checker, validator.
- Roles skipped: spec author (no spec covers the test harness; added only if the skeptic shows one needs a requirement), implementer (no production code in the tests lane), auditor (not in the tests lane).
- Maintainer clarifications: none.

## Maintainer answers

(none)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | Issue goal "raise the warning level of source and fixed states" — the verifiers compare only compiler errors (`CompilerDiagnostics` default), so a raised level alone changes nothing (01-skeptic.md) | Drop the goal as written; instead add one global check on every code-fix verifier path: a code-fix test fails when its fixed state holds more warning-wave diagnostics (level 9999) than its source state | Meets the issue's purpose (a fix that introduces a warning-wave diagnostic fails its own tests); measured to change no existing test outcome | Test harness only | (pending) |
| 2 | Skeptic | Check the source state for warning-wave diagnostics too? | No: 69 fixtures would need edits and it can never catch a fix defect | Agent-filed goal, proposal only | Test harness only | (pending) |
| 3 | Skeptic | Warning level | 9999 (all waves the compiler knows) rather than 9 | Level 9 misses wave 10 (CS9265) | Test harness only | (pending) |
| 4 | Skeptic | Where the check lives | Must also cover verifiers constructed directly (`RH4102EventNameCasingAnalyzerTests`, `CodeFixInteractionTests`), not only the base classes | Global coverage | Test harness only | (pending) |

## Follow-up candidates

(none)

## Worklist / next step

- Skeptic done: proceed narrower (01-skeptic.md).
- Next: test implementer (`02-test-implementer.md`): harness check + CS8848 proof test (red before, green after; FC-033) with a test-local analyzer/fix.
