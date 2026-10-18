Role: test implementer
Commit: 9855bdd
Verdict: tests written

Lane: specs and tests for existing behavior. No production code changed. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the spec examples and requirements of `sdd/specs/shared/comment-placement.md` and the 16 rule specs at 8df308f, not from the code.

All changes are in the 16 files `src/Reihitsu.Analyzer.Test/Formatting/RH50{33..47,49}Comments*AnalyzerTests.cs`. Two kinds of change:

- **Tags.** 187 of the 194 existing tests now carry `[Requirement]` attributes. The tags were added by a script keyed on test name; no test body or assertion changed.
- **New tests.** 123 tests, appended before `#endregion // Tests` in each file. Each one is a focused `Verify` on the class's existing `AnalyzerTestsBase<TAnalyzer>`.

Coverage: every one of the 34 requirements (RH5033-001 … RH5049-001, SHARED-COMMENT-001 … 010) has at least one covering test. The fewest are SHARED-COMMENT-002 and RH5033-002, with 2 each. `RequirementCoverageTests` (`EveryRequirementReferenceNamesAnActiveRequirement`, `EveryRequirementOfAnEnforcedSpecIsCovered`), `SpecConsistencyTests` and `SpecStructureTests` pass: 39/39.

## Tests

### Existing tests, tagged

Tags follow the test's own input:

- A same-line comment → SHARED-COMMENT-003.
- A comment before the first element, before a delimiter, or before a separator → SHARED-COMMENT-004/005.
- A nested construct → 006.
- A binary operator → 007/008.
- A chain link → 009.
- A statement kind → 010.
- The rule's own position or region → its rule requirement.

A test that only incidentally shows a requirement does not tag it.

These existing tests stay untagged:

| Test | Reason |
|---|---|
| `VerifyNoDiagnosticForCommentAtStartOfFile` (RH5033, RH5036, RH5037, RH5040, RH5041, RH5042, RH5044, RH5045, RH5049) | The previous token is `None`. No requirement states this case on its own. |
| `VerifyNoDiagnosticForCommentInDisabledCode` (RH5033, RH5038) | Inactive `#if` code is general behavior (`documentation/rules/general-notes.md`). No SHARED-DIAG or SHARED-COMMENT requirement states it. |
| RH5038 `VerifyNoDiagnosticForCommentBeforeConditionalElementAccessInArgument`, `VerifyCommentBeforeNullForgivingConditionalLinkInArgumentIsReported` | They pin thoenissen/Reihitsu#1058 Q1, as 02-skeptic and 03-spec-author require. |

Two existing tests are tagged SHARED-COMMENT-009, which the reader should check:

- RH5038 `VerifyCommentBeforePropertyAfterCallInArgumentIsReported` (`a.B()` / `// c` / `.Name`, reported).
- RH5038 `VerifyCommentBeforeSecondPrefixLinkInArgumentIsReported` (`a.P` / `// c` / `.Q.C()`, reported).

Both chains have no conditional access and no `!?.`, so they are inside 009's coverage clause. thoenissen/Reihitsu#1058 cites the first one only as the comparison input, and it stays reported under both readings.

### New tests

All pass on the unchanged code. "reported ×2" means two diagnostics at markup `#0` and `#1`. Helper for all: `AnalyzerTestsBase<TAnalyzer>.Verify` with the class's analyzer.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5033…Tests.VerifyCommentAfterEnumMemberAttributeIsReported` | RH5033-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5033…Tests.VerifyCommentAfterAccessorAttributeIsReported` | RH5033-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5033…Tests.VerifyCommentAfterTypeParameterAttributeIsReported` | RH5033-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5033…Tests.VerifyCommentAfterLocalFunctionAttributeIsReported` | RH5033-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5033…Tests.VerifyCommentAfterLambdaAttributeIsReported` | RH5033-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5033…Tests.VerifyNoDiagnosticForCommentAfterModuleAttribute` | RH5033-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5033…Tests.VerifyEmptyBlockCommentAfterAttributeIsReportedButBlockDocumentationCommentIsNot` | SHARED-COMMENT-001, RH5033-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5033…Tests.VerifyEmptyBlockCommentAfterAttributeIsReportedButBlockDocumentationCommentIsNotWithoutDocumentationParsing` | SHARED-COMMENT-001, RH5033-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, DocumentationMode.None | pass (reported) |
| `RH5033…Tests.VerifyNoDiagnosticForDocumentationCommentAfterAttributeWithDocumentationParseMode` | SHARED-COMMENT-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, DocumentationMode.Parse | pass (not reported) |
| `RH5033…Tests.VerifyCommentAfterAttributeIsReportedWithCrLfLineEndings` | RH5033-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5034…Tests.VerifyCommentAfterDocumentationAndRegionDirectiveIsReported` | RH5034-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5034…Tests.VerifyNoDiagnosticForCommentAfterFirstTokenOfDeclaration` | RH5034-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5034…Tests.VerifyCommentBetweenDocumentationAndDelegateIsReported` | RH5034-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5034…Tests.VerifyNoDiagnosticForCommentAfterDocumentationBeforeNamespace` | RH5034-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5034…Tests.VerifyNoDiagnosticForCommentAfterDocumentationBeforeAccessor` | RH5034-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5034…Tests.VerifyNoDiagnosticForCommentAfterDocumentationBeforeParameter` | RH5034-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5034…Tests.VerifyNoDiagnosticForCommentAfterDocumentationBeforeLocalFunction` | RH5034-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5034…Tests.VerifyCommentAfterBlockDocumentationWithoutDocumentationParsingIsReported` | RH5034-001, SHARED-COMMENT-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, DocumentationMode.None | pass (reported) |
| `RH5034…Tests.VerifyCommentAfterDocumentationIsReportedWithCrLfLineEndings` | RH5034-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5035…Tests.VerifyCommentInFinalizerParameterListIsReported` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentInOperatorParameterListIsReported` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentInConversionOperatorParameterListIsReported` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentInAnonymousMethodParameterListIsReported` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentInRecordPrimaryConstructorParameterListIsReported` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentInStructPrimaryConstructorParameterListIsReported` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentInExtensionBlockParameterListIsReported` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentBetweenStaticAndLambdaParameterListIsReported` | RH5035-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentBetweenThisAndIndexerParameterListIsReported` | RH5035-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentBetweenParameterAttributeAndParameterIsReported` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentAfterDefaultValueEqualsIsReported` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5035…Tests.VerifyCommentBeforeFirstParameterIsReportedWithCrLfLineEndings` | RH5035-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5036…Tests.VerifySameLineCommentAfterElseIfHeaderIsReported` | RH5036-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5036…Tests.VerifyCommentAfterAwaitForeachHeaderIsReported` | RH5036-001, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5036…Tests.VerifyCommentAfterAwaitUsingHeaderIsReported` | RH5036-001, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5036…Tests.VerifyCommentAfterDeconstructingForeachHeaderIsReported` | RH5036-001, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5036…Tests.VerifyNoDiagnosticForCommentBetweenSwitchHeaderAndBrace` | RH5036-002, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5036…Tests.VerifyNoDiagnosticForCommentBetweenTryAndBody` | RH5036-002, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5036…Tests.VerifyNoDiagnosticForCommentBetweenCatchAndBody` | RH5036-002, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5036…Tests.VerifyNoDiagnosticForCommentBetweenFinallyAndBody` | RH5036-002, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5036…Tests.VerifyNoDiagnosticForCommentBetweenCheckedAndBody` | RH5036-002, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5036…Tests.VerifyNoDiagnosticForCommentBetweenUncheckedAndBody` | SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5036…Tests.VerifyCommentAfterIfHeaderIsReportedWithCrLfLineEndings` | RH5036-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5037…Tests.VerifyCommentBeforeElseIfIsReported` | RH5037-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5037…Tests.VerifyNoDiagnosticForCommentAfterElseKeyword` | RH5037-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5037…Tests.VerifyCommentBeforeSecondCatchIsReported` | RH5037-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5037…Tests.VerifyCommentBeforeElseIsReportedWithCrLfLineEndings` | RH5037-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5038…Tests.VerifyCommentInConstructorInitializerArgumentListIsReported` | RH5038-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyCommentInPrimaryConstructorBaseArgumentListIsReported` | RH5038-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyCommentInConditionalElementAccessIsReported` | RH5038-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyCommentInTargetTypedObjectCreationIsReported` | RH5038-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyCommentInIndexerAssignmentOfObjectInitializerIsReported` | RH5038-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentInArraySize` | RH5038-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentLookingTextInStringArgument` | RH5038-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyTwoCommentsInOneGapAreReportedSeparately` | SHARED-COMMENT-003, RH5038-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported ×2) |
| `RH5038…Tests.VerifyCommentInArgumentListInsideStatementHeaderIsReported` | SHARED-COMMENT-003 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentAfterArgumentList` | SHARED-COMMENT-004 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyCommentBeforeSeparatorAfterAnonymousObjectIsReported` | SHARED-COMMENT-005 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentInsideCollectionExpressionOperand` | SHARED-COMMENT-006 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyCommentBeforeCollectionExpressionOperandIsReported` | SHARED-COMMENT-006 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentBeforeNestedCollectionExpression` | SHARED-COMMENT-006 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyCommentBeforeOuterOfNestedCollectionExpressionOperandIsReported` | SHARED-COMMENT-006 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyCommentInArgumentListInsideLambdaBlockIsReported` | SHARED-COMMENT-006 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentBeforeShiftOperator` | SHARED-COMMENT-007, SHARED-COMMENT-008 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentBeforeTypeTest` | SHARED-COMMENT-007, SHARED-COMMENT-008 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentBeforeAsOperator` | SHARED-COMMENT-007, SHARED-COMMENT-008 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyCommentBeforeNullPatternIsKeywordIsReported` | SHARED-COMMENT-007 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyCommentBeforePatternCombinatorIsReported` | SHARED-COMMENT-007 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyCommentBeforeRangeOperatorIsReported` | SHARED-COMMENT-007 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentInsideDirectiveInArgumentList` | SHARED-COMMENT-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyNoDiagnosticForDocumentationCommentInArgumentList` | SHARED-COMMENT-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5038…Tests.VerifyNoDiagnosticForDocumentationCommentInArgumentListWithoutDocumentationParsing` | SHARED-COMMENT-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, DocumentationMode.None | pass (not reported) |
| `RH5038…Tests.VerifyCommentBeforeFirstArgumentIsReportedWithCrLfLineEndings` | RH5038-001, SHARED-COMMENT-004 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5038…Tests.VerifyNoDiagnosticForCommentBeforeLaterChainCallWithCrLfLineEndings` | SHARED-COMMENT-009 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (not reported) |
| `RH5039…Tests.VerifyNoDiagnosticForCommentInCatchDeclaration` | RH5039-001, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5039…Tests.VerifyNoDiagnosticForCommentInDoWhileCondition` | RH5039-001, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5039…Tests.VerifyCommentInDeconstructingForeachHeaderIsReported` | RH5039-001, SHARED-COMMENT-010 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5039…Tests.VerifyCommentInArgumentListInsideHeaderIsReported` | SHARED-COMMENT-003 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5039…Tests.VerifyCommentInHeaderIsReportedWithCrLfLineEndings` | RH5039-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5040…Tests.VerifyCommentAroundConstructorArrowIsReported` | RH5040-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5040…Tests.VerifyCommentAroundFinalizerArrowIsReported` | RH5040-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5040…Tests.VerifyCommentAroundOperatorArrowIsReported` | RH5040-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5040…Tests.VerifyCommentAroundConversionOperatorArrowIsReported` | RH5040-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5040…Tests.VerifyCommentAroundIndexerArrowIsReported` | RH5040-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5040…Tests.VerifyNoDiagnosticForCommentElsewhereInExpressionBody` | RH5040-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5040…Tests.VerifyCommentBeforePropertyArrowIsReportedWithCrLfLineEndings` | RH5040-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5041…Tests.VerifyCommentInForInitializerDeclaratorListIsReported` | RH5041-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5041…Tests.VerifyCommentInUsingStatementDeclaratorListIsReported` | RH5041-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5041…Tests.VerifyCommentInUsingDeclarationDeclaratorListIsReported` | RH5041-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5041…Tests.VerifyCommentInFixedStatementDeclaratorListIsReported` | RH5041-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, unsafe | pass (reported) |
| `RH5041…Tests.VerifyCommentInEventFieldDeclaratorListIsReported` | RH5041-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5041…Tests.VerifyCommentBeforeLocalTerminatorIsReported` | RH5041-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5041…Tests.VerifyNoDiagnosticForCommentAfterDeclarationTerminator` | RH5041-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5041…Tests.VerifyCommentAfterDeclaratorSeparatorIsReportedWithCrLfLineEndings` | RH5041-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5042…Tests.VerifyCommentAfterEventFieldInitializerEqualsIsReported` | RH5042-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5042…Tests.VerifyCommentAfterEnumMemberEqualsIsReported` | RH5042-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5042…Tests.VerifyCommentAfterObjectInitializerMemberEqualsIsReported` | RH5042-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5042…Tests.VerifyCommentAfterWithExpressionMemberEqualsIsReported` | RH5042-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5042…Tests.VerifyCommentAfterCoalesceAssignmentOperatorIsReported` | RH5042-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5042…Tests.VerifyCommentBeforeDeconstructingAssignmentOperatorIsReported` | RH5042-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5042…Tests.VerifyNoDiagnosticForUsingAliasEquals` | RH5042-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5042…Tests.VerifyCommentAfterInitializingEqualsIsReportedWithCrLfLineEndings` | RH5042-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5043…Tests.VerifyCommentInStructBaseListIsReported` | RH5043-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5043…Tests.VerifyCommentInInterfaceBaseListIsReported` | RH5043-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5043…Tests.VerifyCommentInRecordBaseListIsReported` | RH5043-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5043…Tests.VerifyCommentAfterBaseListColonIsReportedWithCrLfLineEndings` | RH5043-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5044…Tests.VerifyCommentInModuleAttributeListIsReported` | RH5044-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5044…Tests.VerifyNoDiagnosticForCommentBeforeBinaryOperatorInAttributeArgument` | RH5044-001, SHARED-COMMENT-008 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5044…Tests.VerifyCommentBeforeFirstAttributeIsReportedWithCrLfLineEndings` | RH5044-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5045…Tests.VerifyCommentInNestedConditionalExpressionIsReported` | RH5045-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5045…Tests.VerifyCommentAfterQuestionMarkIsReportedWithCrLfLineEndings` | RH5045-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5046…Tests.VerifyCommentInTypeArgumentListIsReportedWithCrLfLineEndings` | RH5046-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5047…Tests.VerifyNoDiagnosticForCommentInEventAccessorList` | RH5047-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5047…Tests.VerifyNoDiagnosticForCommentInsideAccessorListWithExpressionBody` | RH5047-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5047…Tests.VerifyCommentBeforeAccessorListOfInterfacePropertyIsReported` | RH5047-002 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5047…Tests.VerifyCommentBeforeAutoAccessorIsReportedWithCrLfLineEndings` | RH5047-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |
| `RH5049…Tests.VerifyCommentAfterAsOperatorIsReported` | RH5049-001, SHARED-COMMENT-007 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5049…Tests.VerifyCommentAfterTypeTestIsKeywordIsReported` | RH5049-001, SHARED-COMMENT-007 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5049…Tests.VerifyNoDiagnosticForCommentAfterPatternIsKeyword` | RH5049-001, SHARED-COMMENT-007 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5049…Tests.VerifyNoDiagnosticForCommentAfterRangeOperator` | SHARED-COMMENT-007 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (not reported) |
| `RH5049…Tests.VerifyCommentAfterShiftOperatorIsReported` | RH5049-001, SHARED-COMMENT-007 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5049…Tests.VerifyCommentAfterOperatorInFieldInitializerIsReported` | RH5049-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5049…Tests.VerifyCommentAfterOperatorInArgumentIsReported` | RH5049-001 | `AnalyzerTestsBase<TAnalyzer>.Verify` | pass (reported) |
| `RH5049…Tests.VerifyCommentAfterLogicalOperatorIsReportedWithCrLfLineEndings` | RH5049-001 | `AnalyzerTestsBase<TAnalyzer>.Verify`, CRLF | pass (reported) |

How the new tests were checked:

- **Proof that they can fail.** I generated a mutated copy of every new test: each positive test without its expected diagnostics, and each negative test with its one ordinary comment marked as an expected diagnostic. That gave 118 mutants, and all 118 failed (`comm` of the mutated names against the failed names was empty both ways).
- **Five negatives with no single ordinary comment to mark.** These are the doc-comment and parse-mode cases, the directive case, and the string-literal case. I mutated them by hand:
  - In both `VerifyNoDiagnosticForDocumentationCommentInArgumentList*`, I turned `///` into `//`. Both tests failed.
  - In `VerifyNoDiagnosticForCommentInsideDirectiveInArgumentList`, I moved `// note` off the `#if` line. The test failed.
  - `VerifyNoDiagnosticForDocumentationCommentAfterAttributeWithDocumentationParseMode` has the same input as the existing default-mode test. The quadruple-slash and `/**/` tests show the other side.
  - `VerifyNoDiagnosticForCommentLookingTextInStringArgument` holds no comment.
- **Restored state.** The committed files are byte-identical to the version that passed (332/332) before mutation.

## Scenarios (bug fix)

_N/A — specs and tests lane._

## Not covered, and why

- **Out of the tagged tests on purpose.** These are left out through the spec's coverage clauses, as 02-skeptic and 03-spec-author require. No new test asserts any of them:
  - the thoenissen/Reihitsu#1058 Q1 inputs and the Q1 additions (postfix `++`/`--` before `.`, `->`);
  - the Q2 inputs (an anonymous object's `new`, the switched-on expression, a pattern's type);
  - the Q3 input (`is Color.Red`).
- **SHARED-COMMENT-003's "reported by each rule".** The test base runs one analyzer per test. The RH5038/RH5039 double report is therefore covered by the same input in two classes: RH5038 `VerifyCommentInArgumentListInsideStatementHeaderIsReported` and RH5039 `VerifyCommentInArgumentListInsideHeaderIsReported`. The RH5035/RH5033 and RH5035/RH5042 overlaps are covered on the RH5035 side only. RH5033's existing parameter-attribute test and RH5042's parameter-default test cover the other side.
- **SHARED-COMMENT-006 to 009 for RH5035, RH5043, RH5044, RH5046, and RH5047.** These rules reach the region logic through one call site, `CommentRegionAnalyzerBase.IsMisplaced`, which is `sealed override`. The operator and chain step is `CommentPositionUtilities.IsValidInsideRegion(nextToken)`, and the owning-construct step is `CommentPositionUtilities.IsOwnedByNestedConstruct(node, nextToken)`. A rule supplies only `TryGetRegion` and, for RH5035 and RH5047, `IsMisplacedBeforeRegion`. Proof:
  - `rg -n "override" src/Reihitsu.Analyzer/Rules/Layout/RH50{35,38,39,43,44,46,47}*.cs` lists only those two overrides.
  - `rg -l -g "*.cs" "IsOwnedByNestedConstruct|IsLaterChainLink" src/Reihitsu.Analyzer` hits only `CommentRegionAnalyzerBase`, `CommentPositionUtilities`, and `FluentChainAnalysisHelper`.

  So the full nested-construct, operator, and chain matrix is tested once, on RH5038. RH5035 (operator), RH5039 (operator, pattern, lambda block, switch expression), and RH5044 (operator) carry sample tests. There is no per-rule matrix.
- **Failure classes.**
  - FC-003 (both sides): covered for each boundary 02-skeptic lists. Examples: before/after the arrow, `=`, separators and operators; element start vs. interior gap; first vs. later call; a collection expression's own bracket vs. a nested one; `is` of a type test vs. of a pattern.
  - FC-019/020 (comment or directive in the gap): covered. Tests include a directive-line comment, an active `#if` branch, `#if` disabled text, and `#region` between a doc comment and a comment.
  - FC-021: covered. Tests include a same-line `/* */` mid-line, and comments before the opening and the closing delimiter.
  - FC-022: covered by the string-literal test and the interpolation hole.
  - FC-025: all expression-bodied member kinds for RH5040, with the local function as the negative.
  - FC-026: `else if` for RH5036 and RH5037, and a second `catch`.
  - FC-027: nested region in a lambda block, nested collection expression, nested conditional.
  - FC-033: `DocumentationMode.None` and `Parse`. A CRLF variant per rule.
- **Not applicable.**
  - FC-004, 007–011, 018, 028, 030: no code fix and no formatter change. The spec headers say `Code fix: None`.
  - FC-012/013/014: the formatter is not involved (02-skeptic, *Surfaces*). Formatter parity (`FormatterTestsBase.VerifyFormatter`) was not added, because a comment is user-authored and these rules have no formatter phase that owns them.
  - FC-006, 023, 024, 029, 036: no rewrite and no rename.
  - FC-015/016: no guard changes.
  - FC-031/035/037: no production code. Expected values come from the spec.
- **Follow-up candidates, out of scope.**
  - `scripts/format.sh` prints S1125 warnings in `src/Reihitsu.Cli/FormatCommandHandler.cs`. That file is untouched by this run.
  - RH5048's future spec should list SHARED-COMMENT-001 under `Uses` (02-skeptic).

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~CommentsMustNotBePlaced"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-COMMENT-006"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests|FullyQualifiedName~SpecConsistencyTests|FullyQualifiedName~SpecStructureTests"
```

Results on this tree:

- analyzer filter: 332/332 passed;
- architecture filter: 39/39 passed;
- `scripts/format.sh` over the 16 files: "Formatted 0 of 16 file(s)".
