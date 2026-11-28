# Lead — arch-tests-shared-helpers

- Issue: thoenissen/Reihitsu#1994 — `agent-filed` (goals are proposals), label `maintenance`, milestone 1.6.0.
- Slug: `arch-tests-shared-helpers`
- Branch: `claude/optimistic-babbage-ir30vs` (session-designated)
- PR: thoenissen/Reihitsu#1998 (draft claim)
- Lane: `tests` — the issue changes only test code in `tests/Reihitsu.ArchitectureTests` (deduplicating helpers, no behavior change); the issue itself names this lane.
- Roles run: skeptic, test implementer, cleaner, spec checker, validator.
- Roles skipped: spec author (the run adds no spec), implementer and auditor (not in the tests lane).
- Maintainer clarifications: none. The issue comment (scope note, agent-generated footer, so a proposal) says main has 11 identical `FindRepositoryRoot` copies, 7 bin/obj skip sites with differing case comparison and dot-directory rules (the run picks one rule), and 4 fixture-directory types.

## Maintainer answers

(none)

## Decisions taken

| # | Stage | Question | Decision | Why | Why nothing visible is in question | Commit |
|---|---|---|---|---|---|---|
| 1 | skeptic | Scope: the issue undercounts (12 root lookups incl. `FindScript`, 7 bin/obj sites, 4 fixture types + inline roots in `SquadNoteReferenceTests`) | Cover every site the skeptic lists in `ArchitectureTests` | Meets the issue's purpose: one owner per helper | Test code only | |
| 2 | skeptic | One bin/obj rule for all sites, incl. dot directories? | Shared check decides bin/obj only (OrdinalIgnoreCase, directory segments only); each scan keeps its own dot-directory rule | A uniform dot rule changes which files two scans read | Test code only | |
| 3 | skeptic | Tracker scan splits the absolute path, so a root under a `bin`/`obj` directory silently checks nothing | Adjacent fix: shared check works on paths relative to the scanned root, with a test for a root under `obj` | Small, in the touched component, gets a test | Test code only | |
| 4 | skeptic | Seven root lookups in other test projects | Drop, not a follow-up | Cross-assembly sharing is a separate structural change with no defect behind it; the issue is scoped to `ArchitectureTests` | Nothing filed | |
| 6 | test impl. | Optional: convert `SquadNoteReferenceTests` inline roots to the fixture type | Accepted | Same helper, issue's purpose | Test code only | 43c71da |
| 7 | cleaner | New files lack the UTF-8 BOM `.editorconfig` asks for | Leave as is | 26 of 36 project files on main have none; build and formatter accept it | Test code only | 60ddc43 |
| 5 | skeptic | Fixture shape | One neutral fixture type; scan-specific writers (`WriteSpec`, `CreateFixture`) stay with their scans | Skeptic's recommendation | Test code only | |

## Follow-up candidates

(none)

## State

- Last step: test implementer (02, tests written, 43c71da; lane check tests confirmed).
- Cleaner (03): clean, 0 warnings.
- Next: spec checker, then validator.
