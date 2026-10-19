Role: test implementer
Commit: bf95e57
Verdict: tests written

The lead's prompt carried no intended fix, suspected cause, or hoped-for verdict. No production code was touched. Every test passes against the unchanged code; `RequirementCoverageTests` and the other architecture tests pass (72/72).

## What was done

- **Re-tagged retired IDs.** `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests`: RH3202-002 → SHARED-BODY-001, RH3202-003 → SHARED-BODY-002, RH3202-004 → SHARED-BODY-003 (all six tests listed in `02-spec-author.md`; none of their inputs has anything outside `=> … ;` to relay out). `RH5030BlankLineAfterClosingBraceAnalyzerTests`: RH5030-003 → SHARED-BRACE-001 on all eleven tests. No tag of a retired ID remains (`rg 'RH3202-00[234]|RH5030-003' src` is empty).
- **Tagged existing tests** in the 21 rule test classes, RH3202, and RH5030 with the requirements they verify, following the mappings of `01`, `02`, `04`, `06`, `08`. Tests that verify a requirement only incidentally, and tests whose expected output is behavior filed as an open question, stay untagged (see *Untagged on purpose*).
- **Added 108 tests**: one per spec example that no existing test covered exactly (copied from the spec text, blank lines included; checked with a script that compares every new literal with the spec blocks), and one untagged characterization test per open-question input that had none.
- The two-pass Fix All settings of RH3001, RH3002, RH3004, RH3102 are unchanged. No new test needed a Fix All iteration setting.

## Tests

All new tests are in `src/Reihitsu.Analyzer.Test/`. Helper column: `Verify` = markup verifier (analyzer only, or analyzer + fix + Fix All when a fixed source is given); `NoFix` = `VerifyNoCodeFixIsOffered`; `Apply` = `ApplyCodeFixAsync` with an exact-text assertion (used where the fixed text does not compile or the line endings are asserted).

| Test | Requirement / open question | Helper | Expected now |
|---|---|---|---|
| RH3001 `NotOperatorInConditionAndBeforeInvocationIsReportedOnTheOperator` | RH3001-001, -003 | Verify | pass |
| RH3001 `NullableAndUserDefinedNotOperandsAreNotReported` | RH3001-002 | Verify | pass |
| RH3001 `FixParenthesizesTheComparisonOnlyWhereItWouldBindDifferently` | RH3001-004 | Verify | pass |
| RH3001 `CommentsAfterTheOperatorMoveInFrontOfTheOperand` | RH3001-005 | Verify | pass |
| RH3001 `DirectiveAfterTheOperatorIsReportedWithoutACodeFix` | RH3001-006 | NoFix | pass |
| RH3001 `DirectivesInsideTheParenthesizedOperandDoNotWithholdTheFix` | RH3001-006 | Verify | pass |
| RH3002 `ParenthesesAroundAWholeExpressionAreReportedInEveryListedPosition` | RH3002-001, -007 | Verify | pass |
| RH3002 `ConditionalOperandOfThrowExpressionInExpressionBodyIsNotReported` | RH3002-001 | Verify | pass |
| RH3002 `ParenthesesAroundPrimaryReceiversAndAwaitOperandAreReportedAndFixed` | RH3002-002 | Verify | pass |
| RH3002 `ParenthesesAroundNonPrimaryReceiversAreNotReported` | RH3002-002 | Verify | pass |
| RH3002 `ParenthesesAroundLambdaCastAndSwitchInitializersAreNotReported` | RH3002-003 | Verify | pass |
| RH3002 `PairsAroundDelegateThatWouldLeaveACastAreNotReported` | RH3002-004 | Verify | pass |
| RH3002 `ArgumentsThatWouldJoinIntoAGenericNameKeepTheSecondPair` | RH3002-004 | Verify | pass |
| RH3002 `OnlyTheAssignmentTargetThatTheOperatorWouldJoinKeepsItsParentheses` | RH3002-005 | Verify | pass |
| RH3002 `NestedPairsKeepOnePairAroundCastLambdaAndBinaryReceiver` | RH3002-006 | Verify | pass |
| RH3002 `FixSeparatesTheKeywordAndKeepsTheCommentsAroundTheParentheses` | RH3002-008 | Verify | pass |
| RH3002 `CommentAfterTheOpeningParenthesisIsReportedWithoutACodeFix` | RH3002-009 | NoFix | pass |
| RH3002 `RedundantParenthesesOutsideTheListedPositionsAreNotReported` | RH3002 Q1 (untagged) | Verify | pass |
| RH3003 `RegularAndVerbatimEmptyLiteralsAreReportedOnTheLiteral` | RH3003-001, -003 | Verify | pass |
| RH3003 `EmptyLiteralsWhereAConstantIsRequiredAreNotReported` | RH3003-002 | Verify | pass |
| RH3003 `FixKeepsTheCommentsAroundTheLiteral` | RH3003-004 | Verify | pass |
| RH3003 `EmptyLiteralInCaseLabelWhenClauseIsNotReported` | RH3003 Q1 (untagged) | Verify | pass |
| RH3004 `AnonymousMethodsWithParameterListAreReportedOnTheDelegateKeyword` | RH3004-001, -002 | Verify | pass |
| RH3004 `FixWritesAnExpressionBodyOnlyForASingleReturnStatement` | RH3004-003 | Verify | pass |
| RH3004 `CommentBetweenParameterListAndBodyIsReportedWithoutACodeFix` | RH3004-004 | NoFix | pass |
| RH3004 `FixOfStaticAnonymousMethodDropsTheStaticModifier` | RH3004 Q1 (untagged) | Verify | pass |
| RH3004 `FixOfCastOperandWritesTheLambdaWithoutParentheses` | RH3004 Q3 (untagged) | Apply | pass |
| RH3004 `FixOfLeftBinaryOperandWritesTheLambdaWithoutParentheses` | RH3004 Q3 (untagged) | Apply | pass |
| RH3004 `FixOfRightBinaryOperandWithBlockBodyWritesTheLambdaWithoutParentheses` | RH3004 Q3 (untagged) | Apply | pass |
| RH3004 `FixOfMiddleBinaryOperandWritesTheLambdaWithoutParentheses` | RH3004 Q3 (untagged) | Apply | pass |
| RH3005 `ConstantLikeLeftOperandsAreReportedAndSwappedWithMirroredOperator` | RH3005-001, -003, -004 | Verify | pass |
| RH3005 `ComparisonsWithoutAConstantOnlyOnTheLeftAreNotReported` | RH3005-002 | Verify | pass |
| RH3005 `UserDefinedAndDynamicComparisonsAreReportedWithoutACodeFix` | RH3005-005 | NoFix | pass |
| RH3005 `BuiltInComparisonWhoseMirrorBindsToAUserDefinedOperatorIsReportedWithoutACodeFix` | RH3005-005 | NoFix | pass |
| RH3005 `CommentBeforeTheOperatorIsReportedWithoutACodeFix` | RH3005-006 | NoFix | pass |
| RH3005 `CommentInFrontOfTheComparisonStaysInFront` | RH3005-004, -006 | Verify | pass |
| RH3005 `FixOfComparisonWithComparisonOperandWritesTheSwapWithoutParentheses` | RH3005 Q1 (untagged) | Apply | pass |
| RH3006 `MixedOperandsAreReportedOverTheWholeOperand` | RH3006-001, -002 | Verify | pass |
| RH3006 `FixKeepsTheCommentInFrontOfTheOperandOutsideTheParentheses` | RH3006-003 | Verify | pass |
| RH3006 `FixOfOperandOnTheNextLineInCarriageReturnLineFeedDocument` | RH3006 Q1 (untagged), CRLF | Apply | pass |
| RH3007 `EveryQueryExpressionIsReportedOnceOnItsFirstFrom` | RH3007-001, -002 | Verify | pass |
| RH3101 `BaseAccessesOfInheritedMembersAreReportedOnTheBaseKeyword` | RH3101-001, -003 (report only) | Verify | pass |
| RH3101 `BaseAccessesTheTypeNeedsAreNotReported` | RH3101-002 | Verify | pass |
| RH3101 `FixRemovesTheQualifierAndWritesThisForAnElementAccess` | RH3101-004 | Verify | pass |
| RH3101 `FixOfVirtualMethodInUnsealedTypeRemovesTheQualifier` | RH3101 Q1 (untagged) | Verify | pass |
| RH3101 `FixRemovesCommentsAroundTheDotWithTheQualifier` | RH3101 Q2 (untagged) | Verify | pass |
| RH3102 `EmptyStatementsInBlockLoopBodyAndLabelAreReportedOnTheSemicolon` | RH3102-001, -002 | Verify | pass |
| RH3102 `FixRemovesTheLinesOfEmptyStatementsInBlockAndSwitchSection` | RH3102-003 | Verify | pass |
| RH3102 `EmbeddedLabeledAndCommentedEmptyStatementsAreReportedWithoutACodeFix` | RH3102-004 | NoFix | pass |
| RH3102 `FixOfEmptyStatementBehindAStatementJoinsTheNextLine` | RH3102 Q1 (untagged) | Apply | pass |
| RH3103 `NullableGenericInEveryListedTypePositionIsReportedOverTheWholeName` | RH3103-001, -003 | Verify | pass |
| RH3103 `NullableGenericAsNameofArgumentInExpressionBodyIsNotReported` | RH3103-002 | Verify | pass |
| RH3103 `FixWritesTheShorthandAndKeepsTheCommentsAroundTheTypeArgument` | RH3103-004 | Verify | pass |
| RH3103 `CommentInsideTheQualifiedFieldTypeIsReportedWithoutACodeFix` | RH3103-005 | NoFix | pass |
| RH3103 `OwnLineBlockCommentInsideTheFieldTypeIsReportedWithoutACodeFix` | RH3103-005 | NoFix | pass |
| RH3103 `FixOfUnboundNullableGenericInTypeofWritesAQuestionMarkOnly` | RH3103 Q1 (untagged) | Apply | pass |
| RH3103 `FixOfNullableGenericQualifyingAMemberInNameofWritesTheShorthand` | RH3103 Q1 (untagged) | Apply | pass |
| RH3103 `FixOfNullableGenericReceivingAStaticCallWritesTheShorthand` | RH3103 Q1 (untagged) | Apply | pass |
| RH3103 `FixOfNullableGenericInUsingAliasWritesTheShorthand` | RH3103 Q1 (untagged) | Apply | pass |
| RH3104 `ParameterlessValueTypeCreationsAreReportedOnTheTypeOrOnNew` | RH3104-001, -003 | Verify | pass |
| RH3104 `CreationsThatRunCodeAreNotReported` | RH3104-002 | Verify | pass |
| RH3104 `FixWritesDefaultWithTheShortestTypeName` | RH3104-004 | Verify | pass |
| RH3104 `FixRemovesCommentsInsideTheCreation` | RH3104 Q1 (untagged) | Verify | pass |
| RH3105 `ThisAccessesAreReportedOnTheThisKeywordAlsoInsideALambda` | RH3105-001, -003 | Verify | pass |
| RH3105 `ThisAccessThatSelectsAShadowedFieldIsNotReported` | RH3105-002 | Verify | pass |
| RH3105 `FixRemovesTheThisQualifier` | RH3105-004 | Verify | pass |
| RH3105 `FixRemovesCommentsAroundTheDotWithTheQualifier` | RH3105 Q1 (untagged) | Verify | pass |
| RH3106 `EmptyParameterListsOfAnonymousMethodsAreReportedOverTheParentheses` | RH3106-001, SHARED-EMPTY-001 | Verify | pass |
| RH3106 `FixRemovesTheEmptyParameterList` | SHARED-EMPTY-002 | Verify | pass |
| RH3106 `FixOfAnonymousMethodPassedToOverloadsRemovesTheParameterList` | RH3106 Q1 (untagged) | Apply | pass |
| RH3106 `FixOfAnonymousMethodInImplicitlyTypedVariableRemovesTheParameterList` | RH3106 Q1 (untagged) | Apply | pass |
| RH3106 `FixRemovesACommentInsideTheParameterList` | SHARED-EMPTY Q1 (untagged) | Apply | pass |
| RH3106 `FixKeepsTheWhitespaceOnBothSidesOfTheParameterList` | SHARED-EMPTY Q2 (untagged) | Apply | pass |
| RH3107 `EmptyArgumentListsOfAttributesAreReportedOverTheParentheses` | RH3107-001, SHARED-EMPTY-001 | Verify | pass |
| RH3107 `FixRemovesTheEmptyArgumentListAndKeepsACommentInFrontOfIt` | SHARED-EMPTY-002 | Verify | pass |
| RH3107 `FixRemovesACommentInsideTheArgumentList` | SHARED-EMPTY Q1 (untagged) | Apply | pass |
| RH3107 `FixKeepsTheWhitespaceOnBothSidesOfTheArgumentList` | SHARED-EMPTY Q2 (untagged) | Apply | pass |
| RH3201 `EmptyCommentsAreReportedOverTheWholeComment` | RH3201-001, -004 | Verify | pass |
| RH3201 `EmptyLineInsideBlocksOfSlashCommentsOnMembersIsNotReported` | RH3201-002 | Verify | pass |
| RH3201 `EmptyLineNextToADocumentationCommentIsReported` | RH3201-002 | Verify | pass |
| RH3201 `FixRemovesTheCommentLineOrTheWhitespaceInFrontOfTheComment` | RH3201-005 | Verify | pass |
| RH3201 `EmptyCommentsNextToBlockCommentLinesAreReportedAndRemoved` | RH3201 Q1 (untagged) | Verify | pass |
| RH3201 `FixOfCommentInFrontOfCodeRemovesTheIndentation` | RH3201 Q2 (untagged) | Apply | pass |
| RH3201 `FixOfBlockCommentSpanningSeveralLinesLeavesAnEmptyLine` | RH3201 Q3 (untagged) | Apply | pass |
| RH3201 `FixOfCommentBetweenKeywordAndIdentifierJoinsTheTokens` | RH3201 Q4 (untagged) | Apply | pass |
| RH3201 `FixOfCommentDirectlyInFrontOfAnIdentifierJoinsTheTokens` | RH3201 Q4 (untagged) | Apply | pass |
| RH3201 `EmptyLineAboveTripleSlashCommentInMethodBodyIsReportedAndRemoved` | RH3201 Q5 (untagged) | Verify | pass |
| RH3201 `TrailingEmptyCommentBetweenCommentLinesIsNotReported` | RH3201 Q6 (untagged) | Verify | pass |
| RH3202 `VerifyFixCollapsesTwoBlankLinesAboveTheMethod` | RH3202 Q1 (untagged) | Verify | pass |
| RH3203 `StaticAndInitializedExpressionBodiedConstructorsAreReported` | RH3203-001 (report only) | Verify | pass |
| RH3203 `VerifyExpressionBodyWithConditionalBeforeExpressionIsNotReported` | SHARED-BODY-001 | Verify | pass |
| RH3203 `VerifyExpressionBodyWithRegionAroundSemicolonIsNotReported` | SHARED-BODY-001 | Verify | pass |
| RH3203 `VerifyExpressionBodyWithBalancedGroupBeforeSemicolonIsFixed` | SHARED-BODY-001, -003 (see finding 1) | Verify | pass |
| RH3203 `VerifyStaticAndThrowingConstructorsAreConverted` | SHARED-BODY-002, -003 | Verify | pass |
| RH3203 `VerifyFixLaysOutTheWholeConstructor` | SHARED-BODY Q1 (untagged) | Verify | pass |
| RH3203 `VerifyFixMovesTheConstructorInitializerOntoItsOwnLine` | SHARED-BODY Q1 (untagged) | Verify | pass |
| RH3203 `VerifyFixCollapsesTwoBlankLinesAboveTheConstructor` | RH3202 Q1, RH3203 instance (untagged) | Verify | pass |
| RH3204 `InterpolatedStringsWithoutInterpolationAreReportedOnlyWhenConvertedToString` | RH3204-001, -002 | Verify, no markup | pass |
| RH3204 `FixWritesTheLiteralWithTheSameValue` | RH3204-003 | Verify, no markup | pass |
| RH3205 `EveryExpressionBodiedOperatorKindIsReported` | RH3205-001 | Verify | pass |
| RH3205 `DiagnosticCoversTheArrowAndTheExpressionOnTheNextLine` | SHARED-BODY-002 | Verify | pass |
| RH3206 `EveryExpressionBodiedConversionOperatorKindIsReported` | RH3206-001 | Verify | pass |
| RH3207 `ExpressionBodiedFinalizerIsReportedAndBlockBodiedFinalizerIsNot` | RH3207-001 | Verify | pass |
| RH3207 `FixKeepsACommentBetweenTheExpressionAndTheSemicolon` | SHARED-BODY-003 | Verify | pass |
| RH3208 `LocalFunctionsInLambdaAndAsyncAreReportedButBlockBodiedAndExternAreNot` | RH3208-001 | Verify | pass |
| RH3208 `FixSeparatesTheNewBlockFromTheNextStatementExceptATerminalBreak` | RH3208-002 | Verify | pass |
| RH3208 `FixLaysOutTheBlockFromTheLineStartOrTheFunctionColumn` | RH3208-003 | Verify | pass |
| RH3208 `FixInsertsTheBlankLineAboveACommentAndBehindADirective` | SHARED-BRACE-001 | Verify | pass |

Every filed open question now has a pinning test: the table above, plus RH3004 Q2, which `AnonymousMethodWithBlockBodyIsConverted` and `DocumentationCommentedAnonymousMethodKeepsItsBody` already pin (`(string message) => {` with the line break before `{` dropped).

## Findings against the specs

1. **SHARED-BODY-001, third example (balanced group): the diagnostic markup does not hold.** The spec marks `{|RH3203:=> _value = value` … `#endif|}`, which ends the diagnostic after `#endif`. The analyzer reports `(5,25)-(5,42)`, `=> _value = value`. With `DEBUG` undefined, `+ 1` is disabled text, so the expression ends at `value`. With `DEBUG` defined, it would end at `1`; it never includes `#endif`. SHARED-BODY-002's text ("from the `=>` token to the end of the expression") holds, and so does the example's fixed output. `VerifyExpressionBodyWithBalancedGroupBeforeSemicolonIsFixed` uses the actual span, which matches SHARED-BODY-002, and the spec's fixed text unchanged.
2. **RH3103-003, alias-qualified names: ambiguous.** For `typeof(S::Nullable<int>)` (with `using S = System;`), the existing `AliasQualifiedNullableGenericInTypeofIsReportedAndFixed` shows the diagnostic only on `Nullable<int>`, while the fix replaces all of `S::Nullable<int>` with `int?`. RH3103-003 says "the whole qualified name when the generic name is the last part of a qualified name". Whether `S::Nullable<int>` counts as such a name is open: Roslyn's `AliasQualifiedNameSyntax` is not a `QualifiedNameSyntax`. `global::System.Nullable<long>` is covered in full, as the spec's example shows. I tagged that test RH3103-001/-004 only, not -003.
3. **RH3204 examples contain `$$`, which the Roslyn markup parser reads as a position marker.** This is not a spec defect. The two RH3204 example tests therefore run with `MarkupMode.None` and explicit `WithSpan` diagnostics. A later test author copying a `$$` example with markup will see "missing diagnostics" that are an artifact of the test harness.

No requirement text turned out false otherwise. Every other spec example passes against the unchanged code as written.

## Untagged on purpose

- Behavior filed as an open question, or as a known defect:
  - `AnonymousMethodWithBlockBodyIsConverted` and `DocumentationCommentedAnonymousMethodKeepsItsBody` (RH3004 Q2).
  - `VerifyCommentsAroundExpressionBodySurviveFix` in RH3205, RH3206, RH3207, and RH3208: the expected `{// why` is thoenissen/Reihitsu#963.
  - `EmptyMultiLineCommentWithNewlinesIsReportedAndFixed`: tagged RH3201-001/-004 only, not -005. Its comment spans two lines, which is outside RH3201-005's first bullet.
- Incidental or non-goal tests:
  - RH3001 `NotOperatorTriviaIsPreserved`;
  - RH3005 `DocumentedMemberWithYodaConditionIsFixedAndKeepsItsComment`;
  - RH3103 `NullableShorthandIsNotReported`;
  - RH3104 `ReferenceTypeConstructorIsNotReported`;
  - the Fix All scenarios. They are covered by `FixAllCorrectsEveryDiagnostic`, and the second-pass settings stay as they are (thoenissen/Reihitsu#1143).
- RH3101: no fix test of a virtual member in an unsealed type carries RH3101-004. The only such test is the Q1 characterization.

## Not covered, and why

- **FC-033 (line endings):** RH3006 Q1 is pinned under CRLF. The LF arm is plain fix behavior, already covered by `FixKeepsTheCommentInFrontOfTheOperandOutsideTheParentheses`. No other requirement in scope states line-ending behavior beyond SHARED-BRACE-001, which `VerifyFixInsertsCarriageReturnLineFeedBlankLineBeforeNextStatement` (RH5030) already covers.
- **FC-003 (both sides of a guard):** each fix-withholding requirement has a withheld side and an offered side:
  - RH3001-006: before the operand vs. inside its parentheses;
  - RH3002-009: inside vs. before the parentheses;
  - RH3004-004: `CommentBetween…`/`DirectiveBetween…` tests vs. `CommentInsideSingleReturnBlockIsPreserved`;
  - RH3005-006: inside vs. in front;
  - RH3102-004: embedded, labeled, or commented vs. block statements;
  - RH3103-005: own-line comment vs. same-line comment.
- **FC-030 (fix scope):** the member-wide relayout is pinned only by untagged characterization tests, as the open question requires.
- **FC-025 (member kinds):** every kind named in RH3205-001, RH3206-001, RH3207-001, RH3208-001, and RH3203-001 appears in a tagged test. The compound-assignment, increment, static virtual interface, `checked`, static constructor, lambda, and top-level local-function cases are existing tests.
- **Formatter parity tests** (`Formatter/Formatting/RH320*FormatterTests`): unchanged and untagged. The specs state no formatter-parity requirement of their own.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Clarity.RH30|FullyQualifiedName~Clarity.RH31|FullyQualifiedName~Clarity.RH3201|FullyQualifiedName~Design.RH310"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~Formatting.RH320|FullyQualifiedName~Formatting.RH5030"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-BODY-001"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```
