Role: implementer
Commit: 1ae0050
Verdict: tests pass

Repair round 1, auditor finding #1, lead decision 14, amended RH3004-010 (09), tests from 10. My prompt carried no intended fix, suspected cause, or hoped-for verdict beyond the lead's recorded decision.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Analyzer.CodeFixes/Rules/Clarity/RH3004UseLambdaSyntaxCodeFixProvider.cs` | `WithholdsFixForLines`: the `StringInterpolationUtilities.IsInsideSingleLineInterpolation(anonymousMethodExpression)` check moves to the top of the method, ahead of the `keepsBlockBody` branch, so it withholds the fix on the expression-body path as well as the block path. The check is the same position-only predicate as before; no guard predicts which expressions the formatter spreads over lines (FC-044). The XML summary is rewritten to state the check on both paths and its cause (detached target, formatter layout) (FC-031). Fix All registers through the same per-diagnostic guard, so it leaves the hole diagnostic too (tested). | RH3004-010 |

## Policy owners
`rg -n "IsInsideSingleLineInterpolation" src`: one owner, `Reihitsu.Core.StringInterpolationUtilities`; RH3004 calls the existing `(SyntaxNode)` overload, unchanged. No new copy.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 13 files changed, 79 insertions(+), 29 deletions(-). Round delta: 1 file, 10 insertions(+), 9 deletions(-) (the check moved, the summary rewritten). No new type or public member.

## Deleted special cases
_N/A._

## Tests run
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests"`: Passed 57, Failed 0 (the four failing tests of 10 now pass).
- `scripts/test.sh --configuration Debug --project analyzer`: Passed 12259, Failed 0.
- `scripts/format.sh` on the changed file: 0 of 1 formatted.

## Disputed tests or scope
_None._

## Architecture findings
_None._

## Follow-up candidates
_None_ beyond the lead's existing `apply-fix.sh` candidate.
