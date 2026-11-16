# Lead — arch-doc-checks

- Branch: `claude/quirky-bardeen-50g338` (session-designated); draft PR thoenissen/Reihitsu#1796
- Slug: `arch-doc-checks`
- Lane: **tests**

## Issues

| Issue | agent-filed | Lane | Reason |
|---|---|---|---|
| thoenissen/Reihitsu#1507 | yes (goals are proposals) | tests | Adds an architecture test in `src/Reihitsu.ArchitectureTests/` that checks the guard names of the formatter-phase-ownership table in `AGENTS.md` against declarations under `src/`; no shipped source changes. |
| thoenissen/Reihitsu#1428 | yes (goals are proposals) | tests | Tightens `RequirementCoverageTests` (in `src/Reihitsu.ArchitectureTests/Sdd/`) for exemption keys and `Uses`, plus `sdd/README.md` text; no shipped source changes. |

Shared mechanism: both are repository-convention checks in `Reihitsu.ArchitectureTests` that hold documentation/spec metadata against the repository.

Roles run (tests lane): spec author (the `sdd/README.md` text of #1428), skeptic, test implementer, cleaner, spec checker, validator.
Roles skipped: implementer and auditor (not in the tests lane; no production code changes).

Possible widening: if #1507's new test exposes stale names in `AGENTS.md` and the fix edits `AGENTS.md`, the diff needs the tooling lane (tests → tooling).

## Maintainer answers

None yet.

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|

## Follow-up candidates

None yet.

## State

- Last step: claim, draft PR, prepare.
- Next: spec author (01) for both issues, then skeptic.
