Role: test implementer
Commit: 5668948
Verdict: tests written

Repair round under lead decision 13. My prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntaxAnalyzerTests.FixAllLeavesTheBlockBodyInsideANonVerbatimInterpolationBeforeCSharp11` | RH3004-010 | `Verify(source, fixedSource, configure)` | Passes |
| `RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests.FixAllLeavesTheLocalFunctionInsideANonVerbatimInterpolationBeforeCSharp11` | RH3208-004 | `Verify(source, fixedSource, configure)` | Passes |

Change: in both tests the `onConfigure` lambda now also sets `test.FixedState.MarkupHandling = MarkupMode.Allow` and `test.BatchFixedState.MarkupHandling = MarkupMode.Allow`. This matches the existing pattern for a withheld diagnostic that stays in the fixed source (`RH3004UseLambdaSyntaxAnalyzerTests.cs` around line 990, `RH8204DoNotUsePlaceholderElementsAnalyzerTests.cs:454-455`). The RH3208 test file gained `using Microsoft.CodeAnalysis.Testing;`. Inputs, markup, expected fixed sources, the C# 10 language-version transform, and requirement tags are unchanged.

Fail-for-the-right-reason check: with the two code-fix providers temporarily reverted to `9afcc6b^`, both tests fail with `Context: Iterative code fix application`, a fixed-text mismatch where the hole is rewritten, not a diagnostic-count mismatch. With the fix in place both pass. The production files were restored and are not in this commit.

## Scenarios (bug fix)
Not re-run; the reproduction scenarios are in `03-test-implementer.md`.

## Not covered, and why
No new tests. This round only repairs the harness setting of two existing tests.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~BeforeCSharp11|FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzerTests"
```
Result on the head: 205 passed, 0 failed.

## Follow-up candidates
_None._
