Role: test implementer
Commit: e4f5859
Verdict: tests written

Scope: decision 10 of `00-lead.md`, which retargets the tests of `SyntaxTriviaUtilities.ContainsPositionSensitiveDirectives` so the implementer can delete the method. The prompt carried no intended fix, suspected cause, or hoped-for verdict to ignore. No production code was touched. After this commit, `rg "ContainsPositionSensitiveDirectives\b" tests --glob '*.cs'` finds no test that references the old method.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `SyntaxTriviaUtilitiesTests.GetPositionSensitiveDirectivesReturnsPragmaWarningDirective` (was `ContainsPositionSensitiveDirectivesReturnsTrueForPragmaWarningDirective`) | – | `GetPositionSensitiveDirectiveKinds` (new private helper, replaces `ContainsPositionSensitiveDirectives`) | pass |
| `SyntaxTriviaUtilitiesTests.GetPositionSensitiveDirectivesReturnsNullableDirective` (was `...ReturnsTrueForNullableDirective`) | – | same | pass |
| `SyntaxTriviaUtilitiesTests.GetPositionSensitiveDirectivesReturnsLineDirective` (was `...ReturnsTrueForLineDirective`) | – | same | pass |
| `SyntaxTriviaUtilitiesTests.GetPositionSensitiveDirectivesSkipsRegionDirectives` (was `...ReturnsFalseForRegionDirectives`) | – | same | pass |
| `SyntaxTriviaUtilitiesTests.GetPositionSensitiveDirectivesSkipsConditionalDirectives` (was `...ReturnsFalseForConditionalDirectives`) | – | same | pass |
| `SyntaxTriviaUtilitiesTests.GetPositionSensitiveDirectivesSkipsDirectiveOutsideTheSpan` (was `...ReturnsFalseForDirectiveOutsideTheSpan`) | – | same | pass |
| `OrderingMoveSafetyTests.ContainsPositionSensitiveDirectivesThatBlockRelocationReturnsTrueForMissingRoot` (was `SyntaxTriviaUtilitiesTests.ContainsPositionSensitiveDirectivesReturnsTrueForMissingRoot`; new file `tests/Reihitsu.Core.Test/OrderingMoveSafetyTests.cs`) | – | direct call | pass |

The three "reports" tests now assert the exact list of directive kinds (one `PragmaWarningDirectiveTrivia`, `NullableDirectiveTrivia`, or `LineDirectiveTrivia`), which is at least as strong as the old `true`; the three "skips" tests assert an empty list, which is the old `false`. None of the tests carried a requirement tag before, and none does now.

The missing-root test could not move to `GetPositionSensitiveDirectives`: that method has no null guard (it would throw on enumeration). The behavior the test pinned, "an unusable root refuses the move", now lives in `OrderingMoveSafety.ContainsPositionSensitiveDirectivesThatBlockRelocation`, which returns `true` for a `null` root and had no test for it, so the test moved there, in a new test class for `OrderingMoveSafety` (no such class existed; `rg OrderingMoveSafety tests` found nothing).

## Scenarios (bug fix)
_N/A._ A test retarget, not a reproduction.

## Not covered, and why
- A null-root test for `GetPositionSensitiveDirectives`: its only caller guards `null` first; a test would pin an unspecified exception.
- Failure-class walk: none applies; the change moves tests between two equivalent entry points, and the directive classification itself is unchanged (`ContainsPositionSensitiveDirectives` is `GetPositionSensitiveDirectives(...).Any()` plus the null guard).

## Commands
- `scripts/test.sh --configuration Debug --project core --filter "FullyQualifiedName~SyntaxTriviaUtilitiesTests|FullyQualifiedName~OrderingMoveSafetyTests"`: Passed 89 of 89.
- `scripts/test.sh --configuration Debug --project architecture`: Passed 198 of 198.
- `scripts/format.sh` on both test files: the new file was reformatted (BOM/usings), the retargeted file unchanged.

## Follow-up candidates
_None._ The implementer can now delete `SyntaxTriviaUtilities.ContainsPositionSensitiveDirectives` and update the `<see cref>`s to it in `SyntaxTriviaUtilities.cs` (lines 357, 383, 453, 476 on e4f5859).
