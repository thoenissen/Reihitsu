Role: implementer
Commit: see the commit that adds this file (subject "Implementer: resolve S2259 in TryGetListIndentation [skip ci]")
Verdict: tests pass

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Core/BlankLineCodeFixUtilities.cs` | `TryGetListIndentation`: `label.GetFirstToken()` becomes `label?.GetFirstToken()` in the `isLabelOfSwitch` branch, resolving SonarCloud `csharpsquid:S2259` ("'label' is null on at least one execution path") at line 229. No behavior change: `isLabelOfSwitch` is true only when `label?.Parent?.Parent == switchStatement` with `switchStatement != null`, so `label` is non-null whenever the right-hand operand is evaluated; the conditional access only makes that visible to the analyzer. No suppression. | SHARED-STMT-004 (unchanged) |

## Policy owners
_N/A_ — no policy touched; the change is a null-safe access with identical behavior. The method's XML summary and comments still describe the code (FC-031).

## Production diff size
`git diff --shortstat origin/main...HEAD -- src ':(exclude,glob)src/*Test*/**'` before this commit: 25 files changed, 597 insertions(+), 293 deletions(-). Delta of this round: 1 file changed, 1 insertion(+), 1 deletion(-). No new type.

## Deleted special cases
_N/A._

## Tests run
- `scripts/build.sh --configuration Debug` — 0 Warning(s), 0 Error(s)
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3208|FullyQualifiedName~RH5029|FullyQualifiedName~RH5002|FullyQualifiedName~RH5030|FullyQualifiedName~PrecededByABlankLine"` — Passed! Failed: 0, Passed: 452, Skipped: 0, Total: 452

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None._
