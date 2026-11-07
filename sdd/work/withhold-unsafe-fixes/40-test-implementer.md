Role: test implementer
Commit: 810dad80
Verdict: #1279: tests written (RH3004-008, RH3004-009 narrowed); #1281: tests written (RH3106-003 gap by outcome); #1257, #1282, #1417, #1422: nothing to do

Class-ending narrowing after audit 6, lead rows 46–49 (`00-lead.md`), auditor findings 20–22 (`38-auditor.md`, fixtures in the session scratchpad's `audit6/`), spec 8f19631c (`39-spec-author.md`). Expected values come from the spec examples of RH3004-006/-008/-009 and RH3106-003 and from the auditor's inputs, never from the code. No production code and no spec touched. My prompt listed the work to do and carried no intended fix, suspected cause, or hoped-for verdict. No output questions raised (standing instruction, row 46).

"Fails" means the test fails on 810dad80 because a code fix is offered: every one of the 45 failures is "No code fix may be offered for the diagnostic", after the diagnostic check of `VerifyNoCodeFixIsOffered` passed (reported exactly as marked, compiles in the configured build). Class names drop the `AnalyzerTests` suffix.

## Flipped tests (fix expected before, withheld now)

Each keeps its input and now asserts the diagnostic with no code fix (`VerifyNoCodeFixIsOffered`, with `DEBUG` where the old test defined it; the four CRLF tests gained the `{|RH3004:delegate|}` markup). Renamed so the name states the new behavior; all fail now.

| Old name | New name | Tag(s) |
|---|---|---|
| `FixKeepsConditionalDirectivesInColumnZeroAndShiftsTheStatements` (old RH3004-006 example 1, `DEBUG`) | `FixWithheldForConditionalDirectivesInColumnZeroInsideTheBlockWithDebug` | RH3004-009 |
| `FixKeepsConditionalDirectivesInColumnZeroWhenTheFirstBranchIsDisabled` | `FixWithheldForConditionalDirectivesInColumnZeroInsideTheBlock` | RH3004-009 |
| `FixKeepsConditionalDirectivesInColumnZeroWithCarriageReturnLineFeed` | `FixWithheldForConditionalDirectivesInColumnZeroInsideTheBlockWithCarriageReturnLineFeed` | RH3004-009 |
| `FixMovesRegionDirectivesAndIndentedDirectivesWithTheStatements` (old example 2) | `FixWithheldForRegionDirectivesAndIndentedDirectivesInsideTheBlockWithDebug` | RH3004-009 |
| `FixMovesRegionDirectivesAndIndentedDirectivesWithTheStatementsWhenTheStatementIsDisabled` | `FixWithheldForRegionDirectivesAndIndentedDirectivesInsideTheBlock` | RH3004-009 |
| `FixMeasuresTheShiftOnADisabledStatementInFrontOfTheFirstActiveStatement` (old example 3) | `FixWithheldForADisabledStatementInFrontOfTheFirstActiveStatement` | RH3004-009 |
| `FixKeepsANullableDirectiveInColumnZero` | `FixWithheldForANullableDirectiveInsideTheBlock` | RH3004-009 |
| `FixKeepsTheContinuationLinesOfAnInterpolatedVerbatimStringInDisabledTextInTheirColumns` | `FixWithheldForAnInterpolatedVerbatimStringInDisabledText` | RH3004-009 |
| `FixKeepsALineInsideAStringOfDisabledTextThatClosesBeforeTheEndingDirectiveInItsColumns` (old example 8) | `FixWithheldForAStringOfDisabledTextThatClosesBeforeTheEndingDirective` | RH3004-009 |
| `FixKeepsALineInsideAStringOfAnActiveBranchInItsColumnsWithDebugAndCarriageReturnLineFeed` | `FixWithheldForAMultiLineStringOfAnActiveBranchWithDebugAndCarriageReturnLineFeed` | RH3004-008, RH3004-009 |
| `FixKeepsAColumnZeroDirectiveLineInColumnZeroWhenTheShiftMovesTheLinesLeft` | `FixWithheldForAColumnZeroConditionalChainInABlockWhoseLinesWouldMoveLeft` | RH3004-009 |
| `FixMovesADirectiveLineInAnotherColumnIntoTheStatementColumnWhenTheShiftWouldCrossColumnZero` | `FixWithheldForRegionDirectivesInABlockWhoseShiftWouldCrossColumnZero` | RH3004-009 |
| `FixMeasuresTheShiftOnAnActiveStatementInFrontOfAStatementInAnotherColumn` | `FixWithheldForAConditionalStatementInFrontOfAStatementInAnotherColumnWithDebug` | RH3004-009 |
| `FixMeasuresTheShiftOnADisabledStatementInFrontOfAStatementInAnotherColumn` | `FixWithheldForADisabledStatementInFrontOfAStatementInAnotherColumn` | RH3004-009 |
| `FixKeepsTheContinuationLineOfAMultiLineVerbatimStringInItsColumns` (was RH3004-006/-007) | `FixWithheldForABlockHoldingAMultiLineVerbatimString` | RH3004-008 |
| `FixKeepsAContinuationLineOfAnInterpolatedVerbatimStringThatStartsWithAHoleInItsColumns` | `FixWithheldForAnInterpolatedVerbatimStringWhoseContinuationLineStartsWithAHole` | RH3004-008 |
| `…StartsWithAHoleInItsColumnsWithCarriageReturnLineFeed` | `FixWithheldForAnInterpolatedVerbatimStringWhoseContinuationLineStartsWithAHoleWithCarriageReturnLineFeed` | RH3004-008 |
| `FixKeepsTheClosingQuoteLineOfAnInterpolatedVerbatimStringInItsColumns` | `FixWithheldForAnInterpolatedVerbatimStringWhoseClosingQuoteStartsALine` | RH3004-008 |
| `FixKeepsAContinuationLineOfAnInterpolatedVerbatimStringAfterASharedOpeningBraceLineInItsColumns` | `FixWithheldForAnInterpolatedVerbatimStringBehindASharedOpeningBraceLine` | RH3004-008 |
| `FixKeepsTheClosingQuoteLineOfAVerbatimStringInItsColumns` | `FixWithheldForAVerbatimStringWhoseClosingQuoteStartsALine` | RH3004-008 |
| `FixKeepsContinuationLinesOfAnInterpolatedVerbatimStringThatStartWithTextInTheirColumns` | `FixWithheldForAnInterpolatedVerbatimStringWhoseContinuationLinesStartWithText` | RH3004-008 |
| `FixKeepsEveryLineThatStartsInsideAStringBegunOnAnEarlierLineInItsColumns` (old example 5) | `FixWithheldForInterpolatedVerbatimStringsWithHolesAndClosingQuotesThatStartLines` | RH3004-008 |
| `FixKeepsALineInsideAHoleThatSpansLinesInItsColumns` | `FixWithheldForAHoleThatSpansLines` | RH3004-008 |
| `FixMovesTheLineOnWhichAVerbatimStringStartsAndKeepsItsContinuationLine` (old example 6) | `FixWithheldForAVerbatimStringThatStartsALine` | RH3004-008 |
| `FixKeepsALineInsideAStringAfterASharedOpeningBraceLineAndDoesNotCountItForTheLeftmostLine` (old example 7) | `FixWithheldForAStringThatSpansLinesBehindASharedOpeningBraceLine` | RH3004-008 |
| `FixKeepsAStringWhoseLineHoldsHashBehindOtherTextInItsColumns` (old RH3004-008 offered example) | `FixWithheldForAStringWhoseLineHoldsHashBehindOtherText` | RH3004-008 |
| `FixKeepsAStringWhoseLineHoldsHashBehindOtherTextInItsColumnsWithCarriageReturnLineFeed` | `FixWithheldForAStringWhoseLineHoldsHashBehindOtherTextWithCarriageReturnLineFeed` | RH3004-008 |
| `FixKeepsBothLinesOfABlockCommentThatStartsInColumnZeroInTheirColumns` | `FixWithheldForADelimitedCommentThatStartsInColumnZeroAndSpansLines` | RH3004-008 |
| `FixMovesBothLinesOfABlockCommentThatStartsInAnotherColumnByTheShift` | `FixWithheldForADelimitedCommentThatStartsInAnotherColumnAndSpansLines` | RH3004-008 |
| `FixKeepsOrMovesEveryLineOfABlockCommentWithTheLineOnWhichTheCommentStarts` (old example 4) | `FixWithheldForTwoDelimitedCommentsThatSpanLines` | RH3004-008 |

All eight examples the spec author lists as removed from RH3004-006, and the old RH3004-008 offered example, are among them. Found with a scan of every RH3004 test input for a directive line, a delimited comment or a string literal that spans lines, then read by hand; the expression-body tests with multi-line strings (`FixRewritesASingleReturnOfAMultiLineRawStringLiteralAsAnExpressionBody`, `FixRewritesASingleReturnOfAnInterpolatedVerbatimStringAsAnExpressionBodyAndKeepsItsValue`, `FixRewritesASingleReturnOfAStringThatHoldsALineStartingWithHashAsAnExpressionBody`) and `FixRewritesAnAnonymousMethodInsideAChainThatLiesWhollyOutsideIt` stay offered with their exact outputs and pass.

## Retagged tests (assertion unchanged)

| Test | Before | After |
|---|---|---|
| `FixWithheldForAStringThatDisabledTextLeavesOpen` (no symbol: disabled text) | RH3004-008 | RH3004-009 |
| `FixWithheldForAStringThatDisabledTextLeavesOpenWithTheFirstBranchActive` (`A`: directives and an active string over lines) | RH3004-008 | RH3004-008, RH3004-009 |
| the four `FixWithheldForABlockHoldingAMultiLine…RawStringLiteral…` tests | RH3004-007 | RH3004-007, RH3004-008 (RH3004-007 now points to -008 for the block) |
| `FixMovesAMultiLineDocumentationCommentWithTheStatements` (`///` run, offered) | RH3004-006 | RH3004-006, RH3004-008 ("neither do comments written with `///`") |

Unchanged tags, checked: `FixWithheldForAStringOfAnActiveBranchOpenedBeforeTheAnonymousMethodThatHoldsADirectiveLine` and the three "line starting with hash" tests stay RH3004-008 (no directive inside the anonymous method in the tested build); `FixWithheldForAChainOpenedBeforeTheAnonymousMethodWhoseElseLiesInAStringOfTheBlock` stays RH3004-008 + -009; the other RH3004-009 tests stay.

## New tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH3004UseLambdaSyntax.FixWithheldForAMultiLineCommentThatHidesTheElseOfAChainAroundTheAnonymousMethod` (spec example, finding 20(a), `#define A`) | RH3004-008 | VerifyNoCodeFixIsOffered | fails: fix offered |
| `….FixWithheldForAMultiLineDocumentationCommentThatHidesTheElseOfAChainAroundTheAnonymousMethod` (20(b), `/** */`) | RH3004-008 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForAMultiLineCommentThatHidesTheElseOfAChainInsideTheBlock` (20(c), `#define A`) | RH3004-008, RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForAStringWhoseDirectiveLineStartsWithAFormFeed` (spec example, 20(d); `\f` inserted by `Replace`) | RH3004-008 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForStringsAndCommentsThatSpanLinesWithoutAnyDirective` (spec example, three diagnostics) | RH3004-008 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForADelimitedCommentThatSpansLinesWithCarriageReturnLineFeed` | RH3004-008 / FC-033 | VerifyNoCodeFixIsOffered (CRLF) | fails |
| `….FixRewritesABlockWhoseStringsAndCommentsEachStandOnOneLine` (spec example, exact output) | RH3004-006, RH3004-008 | Verify | passes |
| `….FixRewritesABlockWhoseStringsAndCommentsEachStandOnOneLineWithCarriageReturnLineFeed` | RH3004-008 / FC-033 | ApplyCodeFixAsync (CRLF) | passes |
| `….FixWithheldForAChainFromTheParameterListIntoTheBlock` (spec example, 21(a), `#define A`) | RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForAChainFromTheParameterListIntoTheBlockWithoutTheSymbol` (21(a) without `A`) | RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForAChainFromTheParameterListPastTheAnonymousMethod` (21(b)) | RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForAChainFromTheParameterListPastTheAnonymousMethodOnTheExpressionBodyPath` (21(c)) | RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForAChainInsideTheParameterListOnTheExpressionBodyPath` (spec example) | RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForARegionBeforeTheAnonymousMethodThatEndsInsideItsBlock` (21(d)) | RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForAChainAndARegionThatCrossTheDelegateKeyword` (spec example, two diagnostics, `#define A`) | RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForARegionInsideTheBlockThatEndsAfterTheAnonymousMethod` (audit holding case `p4_region_after`, now withheld) | RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixWithheldForRegionAndPragmaDirectivesInsideTheBlock` (spec example) | RH3004-009 | VerifyNoCodeFixIsOffered | fails |
| `….FixRewritesAnAnonymousMethodInsideARegionThatLiesWhollyOutsideIt` (other side: `#region`/`#endregion` wholly outside; output laid out as the spec's chain-outside example) | RH3004-009 | Verify | passes |
| `RH3106UnnecessaryDelegateParenthesesShouldBeRemoved.AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsInvokedThroughBeginInvokeIsReportedWithoutACodeFix` | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementInitializesADelegateTypedLocalIsReportedWithoutACodeFix` (`Action x = …[0]`) | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementInitializesAFuncTypedLocalIsReportedWithoutACodeFix` | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsAssignedToADelegateTypedLocalIsReportedWithoutACodeFix` | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsPassedAsADelegateArgumentIsReportedWithoutACodeFix` (`Run(Action)` only) | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayThatInitializesADelegateArrayLocalIsReportedWithoutACodeFix` | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayThatInitializesADelegateCollectionLocalIsReportedWithoutACodeFix` | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodsInImplicitlyTypedArraysOfTheDeliberateGapAreReportedWithoutACodeFix` (spec example verbatim, ten diagnostics) | RH3106-003 | VerifyNoCodeFixIsOffered | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementIsInvokedThroughDynamicInvokeIsNotReported` | RH3106-003 | Verify (analyzer) | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementInitializesAnObjectLocalIsNotReported` | RH3106-003 | Verify (analyzer) | passes |
| `….AnonymousMethodInAnImplicitlyTypedArrayWhoseElementInitializesADelegateLocalIsNotReported` | RH3106-003 | Verify (analyzer) | passes |
| `….AnonymousMethodsInImplicitlyTypedArraysOfTheNotReportedShapesAreNotReported` (spec example verbatim) | RH3106-003 | Verify (analyzer) | passes |

The RH3106 tests characterize today's behavior, which rows 48–49 keep; the implementer keeps them green. The existing RH3106 gap tests are left as they are.

## Not covered, and why

- RH3004 Fix All for the withheld inputs: a withheld single fix is withheld in Fix All (same provider registration); the existing Fix All scenario still passes.
- RH3004-009 `#define`/`#undef`/`#line`/`#error`/`#warning` inside the block: none can stand there as a tested input without changing the compilation (`#define` after the first token is CS1032; `#error` fails the build). `#region`, `#endregion`, `#pragma`, `#nullable`, and conditional directives are tested, and the requirement decides on "any directive", so one implementation sees all trivia kinds alike.
- RH3106 CRLF and Fix All: no code change in rows 48–49 and no fix to compare.
- Failure classes: FC-003 (both sides: delimited comment over lines vs. on one line, `///` run offered; directive inside vs. wholly outside, for a conditional chain and a region; block path vs. expression-body path), FC-016 (sibling shapes: `/** */`, chain inside the block, without the symbol), FC-022 (string values across builds), FC-033 (CRLF) apply and are covered. No formatter phase, rename, token-gap, or Fix All provider change: FC-008, FC-010–FC-014, FC-018/036, FC-029 cannot reach it.

## Commands

```shell
scripts/test.sh --configuration Debug --project analyzer --filter "FullyQualifiedName~RH3004UseLambdaSyntaxAnalyzerTests|FullyQualifiedName~RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests"
scripts/test.sh --configuration Debug --project architecture
```

Result on 810dad80 plus these tests: analyzer 45 failed / 165 passed — the 30 flipped tests and the 15 new RH3004 withheld tests, each on "a code fix is offered". Architecture 115 passed.
