Role: test implementer
Commit: see the commit that adds this file ("Set two Fix All iterations for the nested tab-literal convergence test [skip ci]")
Verdict: tests written

Issue: thoenissen/Reihitsu#1432 (`agent-filed`), behavior change; follow-up round for decision 6 of `00-lead.md`. The prompt carried no intended fix or hoped-for verdict beyond the lead's recorded decision.

## Tests
| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests.VerifyEveryFixApplicationConvergesWhenRawStringNestedInHoleIsIndentedWithTabs` (changed: `NumberOfFixAllIterations = 2`, through the `Verify(source, fixedSource, onConfigure, expected)` overload; input, final output, and diagnostics unchanged) | RH5202-008; FC-007, FC-008 | `Verify` (incremental and Fix All) | green |

Why two: RH5202-008 withholds the outer literal's fix while the nested literal has a tab in front of its closing quotes, and its second example states the two-step sequence (nested fix, then the outer fix on the result). The Fix All pass therefore fixes the nested literal first and the outer literal in a second pass. The count is exact (a positive value), so the test still fails if Fix All took one pass (the outer fix not withheld) or three. Before this change it failed with `Expected '1' iterations but found '2' iterations` (`05-implementer.md`).

## Other tests with the same iteration premise
Checked every test of the run that applies a fix to a tab-indented raw string nested in a hole:
- `…VerifyFixIsWithheldWhenReportedRawStringNestedInHoleIsIndentedWithTabsAndOfferedAfterTheNestedFix` applies the nested fix by hand (`TryApplyCodeFixAtAsync`) and then calls `Verify` on the intermediate, which holds only the outer diagnostic: one Fix All pass is correct.
- The `VerifyFixIsWithheld…` tests use `VerifyNoCodeFixIsOffered`: no Fix All iteration involved.
- `…VerifyFixIsOfferedAndKeepsTheTabOfRawStringNestedInHoleWithoutATabInFrontOfItsClosingQuotes`: the nested literal is not the trigger, the outer fix is offered at once.
- `RH5202…TabIndentationTests.GetFixAllScenario` and `RH5202…HoleTests.GetFixAllScenario`: their literals are not nested in each other with a tab trigger, so one pass stays correct.
No other test has the premise.

## Scenarios (bug fix)
_N/A._

## Not covered, and why
Nothing new; the test matrix of `04-test-implementer.md` stands.

## Commands
```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyTabIndentationTests|FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyHoleTests|FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests|FullyQualifiedName~RH5601UseTabsCorrectlyFormatterTests|FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests.VerifyFixWritesTheStrippedTabs|FullyQualifiedName~RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests"
```
Result on this head: Passed 140/140.

## Follow-up candidates
_None._
