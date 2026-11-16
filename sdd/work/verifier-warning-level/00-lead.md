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

## Follow-up candidates

(none)

## Worklist / next step

- Next: skeptic (`01-skeptic.md`).
