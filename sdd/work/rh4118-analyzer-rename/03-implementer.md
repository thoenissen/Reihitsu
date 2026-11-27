Role: implementer
Commit: <filled by this commit; see git log>
Verdict: tests pass

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer/Rules/Naming/RH4118TupleArgumentNameCasingAnalyzer.cs` (renamed from `RH4118TupleElementCasingAnalyzer.cs`) | Class and constructor renamed to `RH4118TupleArgumentNameCasingAnalyzer`; no other line changed (`DiagnosticId`, syntax kind, predicate, `GetLocations` byte-for-byte unchanged) | RH4118 code owners (spec author, `dfcde82`) |
| `tests/Reihitsu.Analyzer.Test/Naming/RH4118TupleArgumentNameCasingAnalyzerTests.cs` (renamed from `RH4118TupleElementCasingAnalyzerTests.cs`) | Class name, `<see cref>`, base type argument, and the two `DiagnosticId` references renamed; no assertion, input, or expected output changed; test method names kept (lead decision 3) | RH4118-001 (characterization, unchanged) |

Git records both files as renames (`R`), 6 insertions and 6 deletions in total. `scripts/format.sh` over both files changed nothing.

## Policy owners
_N/A._ No policy is touched. `rg -n "TupleElementCasing" src tests` now finds only `RH4116TupleElementCasingAnalyzer` and its test class, which must stay (skeptic, *Surfaces*); no reference to the old RH4118 name remains outside `sdd/work/`.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 1 file changed, 2 insertions(+), 2 deletions(-) (a rename). No new type.

## Deleted special cases
_N/A._

## Tests run
- Before the rename (production and test changes stashed): `scripts/test.sh --configuration Debug --project architecture --filter "FullyQualifiedName~EveryCodeOwnerNamesAnExistingType|FullyQualifiedName~AnalyzerSpecHeaderMatchesItsRule"` -> `Failed!  - Failed: 2, Passed: 0` (both as the spec author's handover says).
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH4118"` -> `Passed!  - Failed: 0, Passed: 6, Skipped: 0, Total: 6`.
- `scripts/test.sh --configuration Debug --project architecture` -> `Passed!  - Failed: 0, Passed: 198, Skipped: 0, Total: 198`.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._

The prompt carried no intended fix or hoped-for verdict beyond the rename the handovers specify.
