Role: implementer
Commit: 6acc442
Verdict: tests pass

Scope: decision 10 of `00-lead.md`, after the test retarget of `08-test-implementer.md` (e4f5859). The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Core/SyntaxTriviaUtilities.cs` | `ContainsPositionSensitiveDirectives` deleted (no caller in `src` or `tests`). Its summary — why `#pragma warning`, `#nullable`, `#line` are position sensitive and why region and conditional directives are excluded — moved onto `GetPositionSensitiveDirectives`, whose summary used to point at the deleted method. The three `<see cref>`s to it in the summaries of `ContainsUnbalancedRegionDirectives`, `ContainsDirectives`, and `ContainsConditionalCompilationBoundary` now name `GetPositionSensitiveDirectives` ("that predicate" became "that check", since the target returns directives, not a bool) | decision 10; FC-031 |

No behavior changes: the deleted method was `GetPositionSensitiveDirectives(...).Any()` plus a null guard, and its null-root behavior is covered where it lives now (`OrderingMoveSafetyTests.ContainsPositionSensitiveDirectivesThatBlockRelocationReturnsTrueForMissingRoot`).

## Policy owners
- `rg -n "ContainsPositionSensitiveDirectives\b" src tests`: no match.
- `rg -n "GetPositionSensitiveDirectives" src --glob '*.cs'`: owner `SyntaxTriviaUtilities`, one caller `OrderingMoveSafety.ContainsPositionSensitiveDirectivesThatBlockRelocation`; unchanged.

## Production diff size
`git diff --shortstat origin/main...HEAD -- src`: 6 files changed, 281 insertions(+), 370 deletions(-) — the shortstat counts the file as changed against `main` either way. This round: 1 file, 7 insertions(+), 29 deletions(-). No new type or member.

## Deleted special cases
_N/A._ No shape is declared unsupported (decision 2).

## Tests run
- `scripts/format.sh src/Reihitsu.Core/SyntaxTriviaUtilities.cs`: Formatted 0 of 1 file(s).
- `scripts/build.sh --configuration Debug`: 0 errors.
- `scripts/test.sh --configuration Debug --project core`: Passed 815 of 815.
- `scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH7102|FullyQualifiedName~RH7103|FullyQualifiedName~RH7107|FullyQualifiedName~RH7108|FullyQualifiedName~RH7109|FullyQualifiedName~RH7309"`: Passed 617 of 617.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198 of 198.
- `scripts/lane-check.sh behavior-change`: confirmed.

## Disputed tests or scope
_None._

A note for the cleaner, not a dispute: a full rebuild reports `MSTEST0037` (use `Assert.IsEmpty`) at `tests/Reihitsu.Core.Test/SyntaxTriviaUtilitiesTests.cs(524,9)`, in the retargeted tests of e4f5859. It is a test-code warning, so it was left alone.

## Architecture findings
_None._

## Follow-up candidates
_None._
