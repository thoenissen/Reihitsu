Role: test implementer
Commit: e0ea3c5
Verdict: tests written

Repair round 1, auditor finding 1 only (`10-auditor.md`, decision 24 in `00-lead.md`). My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond naming the finding.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LaneCheckCommandTests` constant `WordingChangesWithChangeDoc` (fixture; used by `ExecuteConfirmsAWordingClaimForAWordingChangeWithItsChangeDoc`-style tests at lines 212, 229, 284, 329, 396) | FC-032 | `Run` | pass |

Change: the fixture line `A\tsquad/notes/2026-11-21-rh5201-member-access-wording.md` is now `M\tsquad/notes/README.md`, the path `SquadNoteReferenceTests` allows and `LaneClassifierTests.ClassifyPathAssignsTheCategoryOfEachKindOfPath` already pins as `Text`. The constant's summary now says "a squad notes edit" instead of "run note". No assertion names the line; it is classified `Text` either way, so every expected value is unchanged.

## Scenarios (bug fix)
Not applicable.

## Not covered, and why
No new test: the finding is a fixture that tripped an existing architecture guard, and `SquadNoteReferenceTests.RepositoryFilesReferenceNoSquadNote` is the test that pins it.

## Commands
- `scripts/test.sh --project tooling` — 204 passed, 0 failed
- `scripts/test.sh --project architecture` — 198 passed, 0 failed (includes `RepositoryFilesReferenceNoSquadNote`)
- `scripts/format.sh src/Reihitsu.Tooling.Test/Unit/LaneCheckCommandTests.cs` — no change

## Follow-up candidates
_None._
