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
| 1 | Spec author | #1507: which guard-column spans must resolve, and against what? | Only identifier / dotted-identifier spans of the guard column; a plain name may also resolve as a Roslyn `SyntaxKind` member; resolve against production projects under `src/` only. No allow-list, no `AGENTS.md` edit | The table legitimately holds `SyntaxKind` names, punctuation and a path; an allow-list drifts; editing `AGENTS.md` widens to tooling for no gain | Architecture test only | pending |
| 2 | Spec author | #1428: what counts as a valid exemption key? | Exactly the relative path of a spec `SpecCatalog.EnumerateSpecFiles` yields (rejects directories and the `specs/README.md` index) | `File.Exists` would accept the index | Test-project and `sdd/README.md` only | 0b74f22 |
| 3 | Spec author | Change doc for #1428? | None | No spec, requirement, rule, or visible surface changes | — | — |

## Follow-up candidates

None yet.

## State

- Last step: spec author (01, 0b74f22): #1507 no text; #1428 sdd/README.md written. No output questions, no follow-ups.
- Next: skeptic (02).
