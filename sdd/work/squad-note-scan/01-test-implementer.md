Role: test implementer
Commit: a4358fe
Verdict: reproduced

The prompt carried no intended fix. The issue text prescribes one (skip `sdd/work/` in the scan); the tests below assert only the outcome its "Done when" states — handovers under `sdd/work/` are not reported, permanent files still are — and do not depend on how the scan achieves it.

## Tests

All in `src/Reihitsu.ArchitectureTests/General/SquadNoteReferenceTests.cs`. No spec under `sdd/specs/` covers the architecture tests, so no test carries a requirement tag.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `ScanIgnoresNoteReferenceInRunHandover` (reproduction) | — (tool) | fixture tree + `ScanForNoteReferences` | red: 1 finding, expected none |
| `ScanIgnoresNoteReferencesAnywhereBelowSddWork` (file directly in `sdd/work/`, and a `.cs` file one level below a run folder) | FC-033 nearest sibling shape | same | red: 2 findings, expected none |
| `ScanDetectsNoteReferenceInPermanentFilesBesideRunHandover` (`sdd/README.md` and `AGENTS.md` next to a handover) | FC-003 other side of the condition | same | red: 3 findings, expected exactly 2 (the permanent files) |
| `ScanDetectsNoteReferenceInPathsSharingSddWorkPrefix` (`sdd/workflow.md`, `sdd/work-archive/run.md`) | FC-003 boundary: prefix without the folder | same | green, must stay green |
| `ScanDetectsNoteReferenceInWorkFolderOutsideRepositorySddWork` (`src/Sample/work/`, `documentation/sdd/work/`) | FC-003 boundary: a folder named `work` that is not the root `sdd/work/` | same | green, must stay green |
| `RepositoryFilesReferenceNoSquadNote` (existing, unchanged) | — | real tree | red while this handover exists (see below) |
| `ScanDetectsRepositoryPathAndRelativeLink`, `ScanIgnoresReadmePlaceholdersNotesAndBuildOutput` (existing, unchanged) | — | fixture | green |

## Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Fixture: `sdd/work/sample-run/01-test-implementer.md` names `squad/notes/2026-01-01-sample-run.md` | LF | `ScanForNoteReferences` | reproduced | observed 1 finding for the handover; expected none |
| Real tree: this handover names the run's future note path on the line below | LF | `RepositoryFilesReferenceNoSquadNote` via `scripts/test.sh` | reproduced | observed `sdd/work/squad-note-scan/01-test-implementer.md:<line>: notes/2026-11-17-squad-note-scan.md`; expected pass |

Real-tree probe, kept on purpose so the repository test reproduces the issue on this branch: the run's note will be `squad/notes/2026-11-17-squad-note-scan.md`.

## Not covered, and why

- CRLF: the scan reads lines with `File.ReadAllLines`, and the issue's difference is about which path is scanned, not line content; line endings do not reach the path decision.
- Letter case of the folder (`SDD/Work`): no caller produces it; the repository folder is `sdd/work/` and the lead creates it with that name.
- Formatter, analyzer, code-fix, idempotency, and unsupported-shape classes: the change is a repository guard in a test project; none of those surfaces is touched.
- Validator-side alternative (`squad/roles/validator.md`): process text, not testable by a fixture.

## Commands

    scripts/test.sh --project architecture --filter "FullyQualifiedName~Reihitsu.ArchitectureTests.General.SquadNoteReferenceTests"

Observed on a4358fe with this handover present: Failed 4, Passed 4, Total 8.

## Follow-up candidates

_None._
