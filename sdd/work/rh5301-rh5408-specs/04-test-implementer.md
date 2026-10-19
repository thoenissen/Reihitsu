Role: test implementer
Commit: 1c09380
Verdict: tests written

Lane: specs and tests for existing behavior; no production code touched. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Inputs: `00-lead.md`, `02-skeptic.md`, `03-spec-author.md`, and the specs `analyzer/RH5301.md`–`RH5308.md`, `RH5401.md`–`RH5408.md`, `shared/initializer-layout.md`, `statement-blocks.md`, `statement-braces.md` at `53d69b1`.

Every new test passes on the unchanged code. Every active requirement of the 19 specs is now covered by at least one tagged test (`RequirementCoverageTests` passes). All changes are in `src/Reihitsu.Analyzer.Test/Formatting/RH53*AnalyzerTests.cs`, `RH540*AnalyzerTests.cs`, and `RH5302LogicalExpressionsShouldBeFormattedCorrectlyRepeatedFixTests.cs`; every class keeps `BatchCodeFixTestsBase`. `scripts/format.sh` over the 17 files changed nothing.

## Findings against the specs

The spec text is unchanged. Three examples do not compile as written, so the tests diverge from them as noted. The spec author should repair the examples. No requirement statement turned out wrong against the code.

1. **RH5304-002 example — CS1912.** The example initializes `L` four times in one object initializer (`error CS1912: Duplicate initialization of member 'L'`). The test `VerifyWrappedCollectionInitializerThatIsNotABlockIsReported` declares `A`, `B`, `E`, and `L` and assigns each once. The names have the same length, so the columns are unchanged and the four reported spans are as in the spec.
2. **RH5408-001 and RH5408-005 examples — CS0273.** `int B { get;⏎ private set; } = 5;` is a private property with a `private set`: `The accessibility modifier of the 'C.B.set' accessor must be more restrictive than the property`. The tests `VerifySimpleAutoPropertyOnSeveralLinesIsReported` and `VerifyFixPutsDeclarationOnOneLine` use `public int B`. The span then starts at `public`, and the fix result is `public int B { get; private set; } = 5;`.
3. **RH5302-005 / RH5402-003 examples** compile and match exactly. Only an RH5402-003 note: the fixed state keeps one diagnostic (the comment-withheld brace), and the verifier needs `test.FixedState.MarkupHandling = MarkupMode.Allow` to accept markup there.

One observation that does not contradict a requirement: a list pattern whose `[` follows a comment that trails the token in front of it (`v is // note⏎ [1,⏎ 2]`) is fixed with `[` on its own line at the column of the statement (`var`), inner patterns four deeper, `]` under `[`. RH5306-004 does not state the column of `[`, so `VerifyFixLaysOutListPatternBehindTrailingCommentOfPrecedingToken` pins the observed column.

## Tests

New tests (each fix test has a CRLF twin named `…WithCarriageReturnLineFeed`, using `NormalizeToCarriageReturnLineFeed` on both sides):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyBracesOutsideColumnOfNewAreReportedOnCreation` | RH5301-001, RH5301-003 | `Verify` (analyzer) | pass |
| `RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyClosingBraceOneColumnRightOfNewIsReported` | RH5301-001, RH5301-003 | `Verify` (analyzer) | pass |
| `RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyMembersOutsideTheirColumnAreReportedOnTheMember` | RH5301-002, RH5301-003 | `Verify` (analyzer) | pass |
| `RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLaysOutObjectCreationAndKeepsTrailingComment` | RH5301-004 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyCommentsInFrontOfTokensAnchorTheirLines` | SHARED-INIT-001 | `Verify` (analyzer) | pass |
| `RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyCommentsOutsideTheirColumnAreReported` | SHARED-INIT-001, RH5301-001, RH5301-002 | `Verify` (analyzer) | pass |
| `RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyDocumentationContinuationOutsideColumnOfFirstLineIsReported` | SHARED-INIT-001, RH5301-002 | `Verify` (analyzer) | pass |
| `RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyOperatorAtEndOfLineIsReportedOnOperator` | RH5302-001, RH5302-003 | `Verify` (analyzer) | pass |
| `RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyOperatorStartingLineOutsideColumnOfLeftOperandIsReported` | RH5302-002, RH5302-003 | `Verify` (analyzer) | pass |
| `RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyOperatorOneColumnRightOfLeftOperandIsReported` | RH5302-002 | `Verify` (analyzer) | pass |
| `RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLaysOutWholeChainInOneApplication` | RH5302-004 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyCommentBehindOperatorAtEndOfLineWithholdsFix` | RH5302-005 | `VerifyNoCodeFixIsOffered` | pass |
| `RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyCommentInFrontOfOperatorDoesNotWithholdFix` | RH5302-004, RH5302-005 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyBracesOutsideColumnOfNewAreReportedOnCreation` | RH5303-001, RH5303-004 | `Verify` (analyzer) | pass |
| `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyElementsOutsideTheirLineOrColumnAreReportedOnTheElement` | RH5303-002, RH5303-004 | `Verify` (analyzer) | pass |
| `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyElementOnLineOfOpeningBraceIsReported` | RH5303-002, RH5303-004 | `Verify` (analyzer) | pass |
| `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyWrappedComplexElementsAreLaidOutAsBlocks` | SHARED-INIT-002, RH5303-003, RH5303-004 | `Verify` (analyzer) | pass |
| `RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLaysOutCollectionInitializer` | RH5303-005 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifySplitAssignmentIsReportedOnTheAssignment` | RH5304-001, RH5304-003 | `Verify` (analyzer) | pass |
| `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifyWrappedCollectionInitializerThatIsNotABlockIsReported` | RH5304-002, RH5304-003, SHARED-INIT-002 | `Verify` (analyzer) | pass |
| `RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixPutsOpeningBraceOnEqualsLineAndLaysOutBlock` | RH5304-004 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyWrappedBracketsOutOfColumnAreReportedOnCollectionExpression` | RH5305-001, RH5305-003 | `Verify` (analyzer) | pass |
| `RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyWrappedClosingBracketOneColumnRightIsReported` | RH5305-001 | `Verify` (analyzer) | pass |
| `RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyWrappedElementsThatShareALineAreReported` | RH5305-002, RH5305-003 | `Verify` (analyzer) | pass |
| `RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLaysOutCollectionExpressionAndKeepsTrailingComment` | RH5305-004 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzerTests.VerifyWrappedBracketsOutOfColumnAreReportedOnListPattern` | RH5306-001, RH5306-003 | `Verify` (analyzer) | pass |
| `RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzerTests.VerifyWrappedInnerPatternsThatShareALineAreReported` | RH5306-001, RH5306-002, RH5306-003 | `Verify` (analyzer) | pass |
| `RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLaysOutListPattern` | RH5306-004 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLaysOutListPatternBehindTrailingCommentOfPrecedingToken` | RH5306-001, RH5306-004 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzerTests.VerifyCommentInsideListPatternKeepsItFromBeingReported` | characterization (open question) | `Verify` (analyzer) | pass |
| `RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzerTests.VerifyMultiLineInnerPatternKeepsListPatternFromBeingReported` | characterization (open question) | `Verify` (analyzer) | pass |
| `RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.VerifyWrappedElementAccessesAreReported` | RH5307-001, RH5307-003 | `Verify` (analyzer) | pass |
| `RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.VerifyWrappedElementAccessTheFormatterCannotJoinIsNotReported` | RH5307-002 | `Verify` (analyzer) | pass |
| `RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.VerifyFixJoinsArgumentListAndKeepsTrailingComment` | RH5307-004 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.VerifyFixJoinsImplicitElementAccessArgumentList` | RH5307-004 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.VerifyFixIsNotOfferedForElementAccessInsideAnotherArgumentList` | characterization (open question) | `VerifyNoCodeFixIsOffered` | pass |
| `RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests.VerifyFixIsNotOfferedWhenReceiverStartsWithElementAccess` | characterization (open question) | `VerifyNoCodeFixIsOffered` | pass |
| `RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyOperatorThatDoesNotStartItsLineIsReported` | RH5308-001, RH5308-003 | `Verify` (analyzer) | pass |
| `RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyNestedConditionalInBranchIsCheckedTheSameWay` | RH5308-001, RH5308-003 | `Verify` (analyzer) | pass |
| `RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyOperatorThatStartsItsLineOutsideItsColumnIsReported` | RH5308-002, RH5308-003 | `Verify` (analyzer) | pass |
| `RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyOnlyFirstMisplacedOperatorIsReported` | RH5308-003 | `Verify` (analyzer) | pass |
| `RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.VerifyFixLaysOutConditionalAsStair` | RH5308-004 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.VerifyArrowOrExpressionBelowNameIsReportedOnProperty` | RH5401-001, RH5401-002 | `Verify` (analyzer) | pass |
| `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.VerifyDiagnosticCoversAttributeListsAndModifiers` | RH5401-002 | `Verify` (analyzer) | pass |
| `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixPutsPropertyOnOneLine` | RH5401-003 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixPutsExplicitInterfaceImplementationOnOneLine` | RH5401-003 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixIsWithheldWhenPropertyStaysOnSeveralLines` | RH5401-004 | `VerifyNoCodeFixIsOffered` | pass |
| `RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests.VerifyOpeningBraceOnLineOfStatementIsReportedOnBrace` | RH5402-001, RH5402-002 | `Verify` (analyzer) | pass |
| `RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests.VerifyOnlyBlocksOfControlFlowStatementsAreChecked` | SHARED-BLOCK-001, RH5402-001 | `Verify` (analyzer) | pass |
| `RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests.VerifyRemainingControlFlowBlocksAreChecked` | SHARED-BLOCK-001, RH5402-001 | `Verify` (analyzer, unsafe allowed) | pass |
| `RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests.VerifyBlocksOutsideControlFlowStatementsAreNotChecked` | SHARED-BLOCK-001 | `Verify` (analyzer, unsafe allowed) | pass |
| `RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests.VerifyFixMovesBraceToOwnLineUnlessCommentSitsInGap` | RH5402-003, SHARED-GAP-001 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed`, `FixedState.MarkupHandling = Allow` | pass |
| `RH5403StatementMustNotBeOnSingleLineAnalyzerTests.VerifyBlockWithStatementsOnOneLineIsReportedOnBrace` | RH5403-001, RH5403-002 | `Verify` (analyzer) | pass |
| `RH5403StatementMustNotBeOnSingleLineAnalyzerTests.VerifyWhileAndForBlocksOnOneLineAreReported` | SHARED-BLOCK-001, RH5403-001 | `Verify` (analyzer) | pass |
| `RH5403StatementMustNotBeOnSingleLineAnalyzerTests.VerifyBlocksOutsideControlFlowStatementsAreNotChecked` | SHARED-BLOCK-001 | `Verify` (analyzer) | pass |
| `RH5403StatementMustNotBeOnSingleLineAnalyzerTests.VerifyFixLaysOutIfStatementWithItsElse` | RH5403-003 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.VerifyTypeBodyOnOneLineIsReportedOnName` | RH5404-001, RH5404-002 | `Verify` (analyzer) | pass |
| `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.VerifyNestedTypesAreReportedOnTheirOwnNames` | RH5404-002 | `Verify` (analyzer) | pass |
| `RH5404ElementMustNotBeOnSingleLineAnalyzerTests.VerifyFixLaysOutTypeDeclarations` | RH5404-003 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5405BracesMustNotBeOmittedAnalyzerTests.VerifyBodiesOfIfElseIfAndElseAreCheckedButNotLoopBodies` | SHARED-BRACE-001, RH5405-001, RH5405-002 | `Verify` (analyzer) | pass |
| `RH5405BracesMustNotBeOmittedAnalyzerTests.VerifyBodiesOfLoopAndResourceStatementsAreNotChecked` | SHARED-BRACE-001 | `Verify` (analyzer, unsafe allowed) | pass |
| `RH5405BracesMustNotBeOmittedAnalyzerTests.VerifyUnbracedBodyOnOneLineIsReported` | RH5405-001, RH5405-002 | `Verify` (analyzer) | pass |
| `RH5405BracesMustNotBeOmittedAnalyzerTests.VerifyFixWrapsBodyOnLineOfIfInBraces` | SHARED-BRACE-002 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests.VerifyUnbracedBodyOnSeveralLinesIsReported` | RH5406-001, RH5406-002, SHARED-BRACE-001 | `Verify` (analyzer) | pass |
| `RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests.VerifyMultiLineLoopBodyIsNotChecked` | SHARED-BRACE-001 | `Verify` (analyzer) | pass |
| `RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests.VerifyFixWrapsMultiLineBodyInBraces` | SHARED-BRACE-002 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5407UseBracesConsistentlyAnalyzerTests.VerifyBranchWithoutBracesNextToBracedBranchIsReported` | RH5407-001, RH5407-002, SHARED-BRACE-001 | `Verify` (analyzer) | pass |
| `RH5407UseBracesConsistentlyAnalyzerTests.VerifyIfStatementWithoutElseIsNotReported` | RH5407-001 | `Verify` (analyzer) | pass |
| `RH5407UseBracesConsistentlyAnalyzerTests.VerifyFixWrapsUnbracedBranchInBraces` | SHARED-BRACE-002 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifySimpleAutoPropertyOnSeveralLinesIsReported` | RH5408-001, RH5408-004 | `Verify` (analyzer) | pass |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyPropertyThatIsNotSimpleOrCannotBeJoinedIsNotReported` | RH5408-002 | `Verify` (analyzer) | pass |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyCommentThatBlocksTheJoinKeepsPropertyFromBeingReported` | RH5408-003 | `Verify` (analyzer) | pass |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyCommentBehindDeclarationDoesNotKeepPropertyFromBeingReported` | RH5408-003 | `Verify` (analyzer) | pass |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyDiagnosticExcludesPropertyAttributeLists` | RH5408-004 | `Verify` (analyzer) | pass |
| `RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests.VerifyFixPutsDeclarationOnOneLine` | RH5408-005 | `Verify` (code fix); LF and `…WithCarriageReturnLineFeed` | pass |

Existing tests tagged (no assertion changed):

| Test class | Tagged tests → requirements |
|---|---|
| RH5301 | `VerifyDiagnosticsCase1`/`3`/`6` → 001, 003, 004; `Case2` → 002, 003, 004; `Case4`/`5` → 001–004; `VerifyNoDiagnosticForCommentPrefixedOpeningBrace`/`ClosingBrace` → SHARED-INIT-001, 001; `…Assignment` → SHARED-INIT-001, 002; `VerifyShiftedCommentBeforeAssignmentIsDetectedAndFixed` → SHARED-INIT-001, 002–004 |
| RH5302 | `VerifyMisalignedLogicalOperatorsAreDetectedAndFixed` → 001–004; the three `…Converges` tests → 001/002, 003, 004; `VerifyOperatorFollowedByLineCommentIsNotFixed`, `…DirectiveIsNotFixed`, `VerifyOwnLineCommentAboveOperandLeavesOnlyItsOwnGapUnfixed` → 005; `VerifyCommentBeforeOperatorIsStillFixed` → 004, 005; `RepeatedFixTests.VerifySingleFixApplicationConvergesTheWholeChain` → 004 |
| RH5303 | 17 tests → 001–005, SHARED-INIT-001, SHARED-INIT-002 (brace, element, complex-element, comment-anchor and doc-continuation cases) |
| RH5304 | 16 tests → 001–004, SHARED-INIT-001, SHARED-INIT-002 |
| RH5305 | 4 tests → 001–004 |
| RH5306 | 4 tests → 001–004 |
| RH5307 | 8 tests → 001–004 (`…WithDocumentationComment` → 002: a `///` between the brackets is a comment) |
| RH5308 | 7 tests → 001–004 |
| RH5401 | 4 tests → 001–004 |
| RH5402 | `VerifyNoDiagnosticsWhenCodeIsClean` → 001; `VerifyIssueIsDetectedAndFixed` → 001–003; `VerifyOrdinaryAndVariableForeachBodiesAreDetected` → SHARED-BLOCK-001, 001, 002; `VerifyCatchBlockIsExcluded` → SHARED-BLOCK-001 |
| RH5403 | `VerifyIssueIsDetectedAndFixed` → 001–003; statement-parent, empty-body/catch, lambda and local-function tests → SHARED-BLOCK-001 (+ 001/002) |
| RH5404 | 17 tests → 001–003 (semicolon-body and per-kind tests → 001/002) |
| RH5405–RH5407 | detection and fix tests → 001, 002, SHARED-BRACE-001, SHARED-BRACE-002; `VerifyBlankLineAboveFixedStatementIsKept` → SHARED-BRACE-002 |
| RH5408 | 46 tests → 001–005 |

Deliberately left untagged as characterization: the open-question shapes and their neighbors. These are the RH5304 one-line nested collection (`VerifyNoDiagnosticForSingleLineNestedCollectionInitializer`, `…PropertyInitializer…`; RH5304 Q1) and the RH5306 doc-comment tests (`VerifyDiagnosticForListPatternWithDocumentationComment`, `VerifyCodeFixForListPatternWithDocumentationCommentPreservesTheComment`; RH5306 Q2). They also include the RH5405 blank-line insertion tests (`VerifyBlankLineIsInsertedAbove…`, `VerifyBlankLinesAreInsertedAboveAdjacentFixedStatements`; statement-braces Q3) and the RH5404 extension-block tests (RH5404 Q1). Finally: the RH5408 doc-comment expansion tests (`VerifyDocumentedProperty…`; RH5408 Q1), the RH5302 own-line-comment and `#if` tests (P12/#1148 class), the RH5303 directive tests, the CRLF-only `ApplyCodeFixAsync` tests, and the "not offered a code fix" tests for undiagnosed input. The new characterization tests are listed above with "characterization": RH5306 Q1 and Q3 exemptions, and the RH5307 Q3 withheld fix.

## Not covered, and why

- Open-question shapes with a no-op fix (S1 two comments, S4 comment-led `]`, RH5302 Q1, RH5308 Q1/Q2, the #1150 directive body): not pinned. The verifier cannot express "fix offered and changes nothing", and they must not be tagged.
- P9/P10 instances (fix rewrites code outside the diagnostic, `else if` brace column): not pinned; planned in #1138 and filed as open questions.
- FC-013 (formatter keeps not-reported examples clean): the existing `Formatter/Formatting/RH53*/RH54*FormatterTests.cs` (`VerifyFormatter`) and `SelfHostingTests` cover parity; not duplicated, not tagged.
- Fix All: the shared `FixAllCorrectsEveryDiagnostic` per class carries SHARED-FIX-001/-002; no rule-specific Fix All requirement exists.
- Analyzer-only examples were run on LF only. Column checks read character columns per line and do not depend on the line-ending sequence. Every fix example runs on LF and CRLF.
- A nested complex element inside a complex element (SHARED-INIT-002 last sentence) has no compilable shape in a plain collection initializer that I could construct. It is covered only through the RH5304 complex-element tests, whose complex elements sit one level inside the assignment's collection.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH530|FullyQualifiedName~RH540"
scripts/test.sh --project analyzer --filter "TestCategory=SHARED-INIT-001"
scripts/test.sh --project architecture --filter "FullyQualifiedName~RequirementCoverageTests"
scripts/test.sh --project analyzer
scripts/test.sh --project architecture
```

## Results

Run on the source of `1c09380` (the full runs started after the last test edit; `git diff 1c09380 -- src` is empty):

- `scripts/test.sh --project analyzer`: Passed 5192, Failed 0, Skipped 0.
- `scripts/test.sh --project architecture`: Passed 72, Failed 0, Skipped 0, including `RequirementCoverageTests` (`EveryRequirementOfAnEnforcedSpecIsCovered`, `EveryRequirementReferenceNamesAnActiveRequirement`).
