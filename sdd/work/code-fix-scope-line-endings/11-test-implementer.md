Role: test implementer
Commit: 1b5d499
Verdict: tests written

Repair round 2, auditor finding #1 (09-auditor.md), decisions 14 and 18, requirement RH3001-009 (10-spec-author.md). The prompt carried no intended fix, suspected cause, or hoped-for verdict; this handover states none.

Every expected value comes from the RH3001-009 examples and its normative sentence (withheld: "no code fix offered"; fixed: the outputs the requirement names, or the unchanged text around `a == false` where SHARED-FIX-003 and SHARED-FIX-006 keep the comment and the line break). None was taken from a run of the code. All tests were run on 1b5d499 with the code unchanged. No production code was touched.

File: `src/Reihitsu.Analyzer.Test/Clarity/RH3001NotOperatorShouldNotBeUsedAnalyzerTests.cs` (20 new tests, appended to the `Tests` region).

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NegationsTouchingAWordAreReportedWithoutACodeFix` (spec example 1) | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationsSeparatedFromAWordBySpaceOrCommentAreFixed` (spec example 2) | RH3001-009; FC-003 | `Verify` | passes |
| `NegationDirectlyBehindReturnIsReportedWithoutACodeFix` | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationDirectlyBehindYieldReturnIsReportedWithoutACodeFix` | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationDirectlyBehindCaseIsReportedWithoutACodeFix` (case label without `when`, so not RH3001-008) | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationDirectlyBehindGotoCaseIsReportedWithoutACodeFix` | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationDirectlyBehindWhenOfASwitchExpressionArmIsReportedWithoutACodeFix` | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationBehindWhenOfASwitchExpressionArmAndASpaceIsFixed` | RH3001-009; FC-003 | `Verify` | passes |
| `NegationDirectlyBehindWhereIsReportedWithoutACodeFix` | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationDirectlyBehindSelectIsReportedWithoutACodeFix` | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationDirectlyBehindOrderbyIsReportedWithoutACodeFix` | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationsDirectlyBehindGroupAndByAreReportedWithoutACodeFix` | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationsDirectlyBehindOnAndEqualsAreReportedWithoutACodeFix` (join) | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationWhoseOperandTouchesSelectBehindItIsReportedWithoutACodeFix` (trailing side, `where !(w)select w`) | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationWhoseOperandTouchesIsBehindItIsReportedWithoutACodeFix` (trailing side outside a query, `return !(a)is true;`) | RH3001-009 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `NegationWhoseOperandIsSeparatedFromSelectByACommentIsFixed` (`where (w) == false/* c */select w`) | RH3001-009; FC-003, FC-016 | `Verify` | passes |
| `NegationBehindReturnAndASpaceIsFixed` | RH3001-009; FC-003 | `Verify` | passes |
| `NegationSeparatedFromReturnByACommentIsFixed` (`return/* c */a == false`) | RH3001-009; FC-003, FC-016 | `Verify` | passes |
| `NegationOnTheLineBelowReturnIsFixed` (LF and CRLF) | RH3001-009; FC-003 | `VerifyCodeFixWithEndOfLine` | passes (both rows) |
| `NegationDirectlyBehindAnOpeningParenthesisIsFixed` (`if(!a)`, `M(!a)`: punctuation, no whitespace) | RH3001-009; FC-003 | `Verify` | passes |
| `FixAllLeavesANegationTouchingAWordReported` | RH3001-009; FC-008 | `Verify` (markup allowed in fixed state) | fails: iterative fix writes `return(a == false)` |

Every red test fails on the requirement's difference: the withheld tests with "Expected collection to be empty" (a code action is offered), the Fix All test with `if (b) return(a == false);` against the expected `if (b) return!a;` (the auditor's observed output). `RequirementCoverageTests.EveryRequirementOfAnEnforcedSpecIsCovered` is green again (51/51 Sdd tests).

## Not covered, and why

- An ordinary identifier (not a contextual keyword) directly in front of `!`: `a!b` parses `!` as the null-forgiving postfix, so a prefix negation behind an identifier token is not reachable; only contextual keywords (`where`, `select`, `when`, `on`, `equals`, `by`, `orderby`, `group`) occur there, and they are covered.
- `case!true when x:` (touching `case` and a constant pattern): RH3001-008 already withholds it; the two requirements agree, so no separate test.
- FC-006, FC-010 to FC-014, FC-022 to FC-024: the guard only withholds the fix; no formatter phase or layout is involved. Line endings are tested once on the fixed side with a line break (`NegationOnTheLineBelowReturnIsFixed`); the withheld side writes nothing.
- FC-038 to FC-043: no shape is declared unsupported (decision 14).
- Formatter counterpart: the formatter writes `return !a` but leaves the query and `case`/`when` shapes as written (10-spec-author.md); RH3001-009 is about the code fix only, so no formatter test.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Reihitsu.Analyzer.Test.Clarity.RH3001NotOperatorShouldNotBeUsedAnalyzerTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```
