Role: test implementer
Commit: 40714ba20e36dca475e29d2e8c8cc079777d2262
Verdict: tests written

This round covers findings 3 and 4 of `24-spec-checker.md`. No production file, spec, or existing assertion changed.

Every new test passes on the unchanged code. No spec statement turned out to be false.

The prompt contained only the task. The new RH3001 tests are left untagged, as the prompt instructed, because the spec author is rewording RH3001-004 in parallel.

## Tests

All tests are under `src/Reihitsu.Analyzer.Test/Clarity/`.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3001…Tests.NegationAsAWholeObjectCreationArgumentIsReportedWithoutACodeFix` | untagged, thoenissen/Reihitsu#1245 Q2 (`new C(!y)`) | `VerifyNoCodeFixIsOffered` | reported on `!`, no action: pass |
| `RH3001…Tests.NegationAsAWholeElementAccessArgumentIsReportedWithoutACodeFix` | untagged, #1245 Q2 (`this[!y]`) | `VerifyNoCodeFixIsOffered` | reported, no action: pass |
| `RH3001…Tests.NegationAsAWholeAttributeArgumentIsReportedWithoutACodeFix` | untagged, #1245 Q2 (`[A(!true)]`) | `VerifyNoCodeFixIsOffered` | reported, no action: pass |
| `RH3001…Tests.NegationAsAWholeCollectionExpressionElementIsReportedWithoutACodeFix` | untagged, `24-spec-checker.md` finding 1 (`bool[] a = [!x, y];`) | `VerifyNoCodeFixIsOffered` | reported, no action: pass |
| `RH3001…Tests.NegationAsAWholeIsPatternIsReportedWithoutACodeFix` | untagged, finding 1 (`x is !true`) | `VerifyNoCodeFixIsOffered` | reported, no action: pass |
| `RH3001…Tests.NegationAsAWholeSwitchArmPatternIsReportedWithoutACodeFix` | untagged, finding 1 (`x switch { !true => 1, _ => 2 }`) | `VerifyNoCodeFixIsOffered` | reported, no action: pass |
| `RH3002…Tests.ParenthesesAroundTheExpressionBodyOfALocalFunctionAreReported` | RH3002-001, RH3002-007 | `Verify(source, fixed)` | `int L() => {\|RH3002:(a)\|};` → `int L() => a;`: pass |
| `RH3002…Tests.ParenthesesAroundTheLeftSideOfAnAssignmentAreReported` | RH3002-001, RH3002-007 | `Verify(source, fixed)` | `{\|RH3002:(_a)\|} = 3;` → `_a = 3;`: pass |

Each `VerifyNoCodeFixIsOffered` test checks two things:

- the analyzer reports exactly the marked `!`, and the input has no compiler errors;
- the provider registers no action for any reported diagnostic.

With these tests, all five #1245 Q2 inputs and all three of the spec checker's further inputs are pinned.

## Not covered, and why

- **The fixed counterparts the spec checker lists:** `new bool[] { !x }`, `x switch { true => !y, _ => y }`, `case !true:`, `$"{!x}"`, and `[y, !x && y]`. They were not asked for, and they belong to the RH3001-004 rewording that is still in progress. Whether they are tagged is a decision for after the rewording.
- **CRLF variants.** None of these tests pins any layout.
- **Failure classes.** Nothing changed since `22-test-implementer.md`, and no production code or rewrite changed.

## Commands

```shell
scripts/format.sh src/Reihitsu.Analyzer.Test/Clarity/RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs src/Reihitsu.Analyzer.Test/Clarity/RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests.cs
# Formatted 0 of 2 file(s).

scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperator|FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3002StatementMust"
# Passed: 264, Failed: 0

scripts/test.sh --project analyzer --filter "Name~ReportedWithoutACodeFix&FullyQualifiedName~RH3001NotOperator|Name=ParenthesesAroundTheExpressionBodyOfALocalFunctionAreReported|Name=ParenthesesAroundTheLeftSideOfAnAssignmentAreReported"
# Passed: 11, Failed: 0 (the 8 new tests plus 3 existing RH3001 no-fix tests)

scripts/test.sh --project architecture
# Passed: 72, Failed: 0
```
