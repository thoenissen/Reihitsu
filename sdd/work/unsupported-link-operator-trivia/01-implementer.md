Role: implementer
Commit: 36d94cf0
Verdict: tests pass

CI repair round 2: RH2008 self-hosting failure on `PrecheckPhase.FindUnsupportedSyntax` (66 lines of code, limit 60). Refactor only; no behavior changes.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | The order-dependent first three candidates (FMT-UNSUP-004/009/010, ordered by `isDeclaratorComma`) move unchanged from `FindUnsupportedSyntax` into the new private static `CreateDeclarationCandidates`; the caller appends the remaining candidates in the same order as before. The order comment stays at the call site, and the new method's XML summary states the precedence it encodes | FMT-UNSUP-001 (unchanged candidate order) |

## Policy owners
`rg -n "CreateDeclarationCandidates|isDeclaratorComma" src`: the precheck's candidate order still has one owner, `PrecheckPhase` (`FindUnsupportedSyntax` → `CreateDeclarationCandidates` → `CreateFirstUnsupportedSyntax`). No other copy.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'`: 5 files changed, 154 insertions(+), 60 deletions(-). Round delta: 1 file, +26 −11. One new private method; no new type.

## Deleted special cases
_N/A._ (no shape declared in this round)

## Tests run
- Before the change: `scripts/test.sh --configuration Debug --project selfhosting --filter "FullyQualifiedName~AnalyzerReportsNoViolationsOnOwnSourceCode"` — `Failed AnalyzerReportsNoViolationsOnOwnSourceCode ("RH2008MethodsShouldNotExceedTheLineLimitAnalyzer")`, `PrecheckPhase.cs:84 [RH2008] This member has 66 lines of code, which exceeds the limit of 60.` (MSTest does not filter a single data row, so the whole method was run.)
- After: same command — `Passed! - Failed: 0, Passed: 349, Skipped: 0, Total: 349` (every analyzer row, RH2008 included).
- `scripts/test.sh --configuration Debug --project formatter` — `Passed! - Failed: 0, Passed: 6379, Total: 6379`.
- `scripts/test.sh --configuration Debug --project architecture` — `Passed! - Failed: 0, Passed: 198, Total: 198`.
- `scripts/format.sh` on the changed file: 0 of 1 files changed.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
