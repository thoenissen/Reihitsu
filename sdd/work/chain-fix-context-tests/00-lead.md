# Lead — chain-fix-context-tests

- Issues: thoenissen/Reihitsu#889 (`agent-filed`), thoenissen/Reihitsu#1476 (`agent-filed`) — goals are proposals.
- Slug: `chain-fix-context-tests`
- Branch: `claude/festive-rubin-74bcud` (session-designated); draft PR #1857.
- Lane: **tests**.
  - #889: asks for tests pinning existing behavior of the RH5112/RH5201 code fixes' context selection (`FluentChainCodeFixHelper.FormatChainAsync`); no behavior change requested.
  - #1476: asks for tests pinning RH5201 analyzer and fix behavior on a chain behind a comment that leads its line; the issue names lane tests; a disagreement found is to be filed as a follow-up, not fixed.
  - Shared mechanism: the RH5201/RH5112 chain code-fix path through `FluentChainCodeFixHelper` and its tests.
- Roles run: skeptic (spec against code, premise and reachability), test implementer, cleaner, spec checker, validator.
- Roles skipped: spec author (specs `sdd/specs/analyzer/RH5201.md` and `RH5112.md` exist; run only if a spec gap/clarification surfaces), implementer and auditor (not in the tests lane).
- Maintainer clarifications: none.

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | Skeptic | #889: the premise (context choice sets a column) is out of date since #1760; how to pin the `IsFirstOnLine` boundary? | Option 1A of `01-skeptic.md`: one refusal-scope test (unsupported shape in the line-starting statement before a mid-line chain statement → no fix offered, SHARED-FIX-010) plus mid-line layout tests for `if`, `case`, lambda block with the misspacing in front checked as SHARED-FIX-006; #889's goal "a layout test fails without the clause" is replaced by the refusal test | Only observable effect of the clause today is the refusal scope | Tests pin current `main` output; nothing changes | — |
| 2 | Skeptic | Follow-up: narrow the chain fix's refusal scope to the nearest statement | Dropped, not filed | Serves only a rare input class (a declared unsupported shape in the same line-starting statement) — `lead.md`, *Follow-ups* | Nothing changes | — |
| 3 | Skeptic | Stale doc comment of `GetContextNode`/`FormatChainAsync` ("positional context", FC-031) | Adjacent fix: the cleaner rewrites the comment to the current reason (refusal scope / context for the formatter), proven text-only | Few lines, file the run's tests target, keeps tests lane when the text-only proof passes | Comment only | — |
| 4 | Skeptic | #1476: issue example shows wrong columns | Tests use the formatter's real output quoted in `01-skeptic.md` | Issue is `agent-filed`; HEAD output matches `phase-prediction-removal.md` | Tests pin current output | — |

## Follow-up candidates

None yet.

## State

- Last step: skeptic (`01-skeptic.md`, 312bc82): #889 proceed narrower, #1476 proceed; no output questions.
- Test implementer: `02-test-implementer.md` (62009c4), tests written, lane check confirmed tests. Corrects one #1476 formatter output quoted by the skeptic; tests use measured output.
- Next: cleaner (incl. decision 3), then spec checker, validator.
