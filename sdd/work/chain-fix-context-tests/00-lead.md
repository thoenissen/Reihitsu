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

## Follow-up candidates

None yet.

## State

- Last step: claim, lane, prepare done.
- Next: skeptic.
