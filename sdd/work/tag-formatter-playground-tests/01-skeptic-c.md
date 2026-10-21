Role: skeptic
Commit: 293b104
Verdict: proceed

Spec group C: `formatter/structural-transforms.md`, `formatter/blank-lines.md`, `formatter/documentation-comments.md`, `formatter/region-formatting.md`, `formatter/using-directives.md`. No spec diff exists; the subject is the existing specs against the code at 293b104 (production code is unchanged up to 8f78a04, the later commits touch only tests and handovers). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

The lane premise holds: every spec example of group C reproduces, so tagging can proceed. Seven inputs contradict a requirement or look wrong. They are listed under *Open questions to file*. Further instances of decided follow-ups are listed separately. None of them blocks tagging: a test that proves a requirement must avoid the disputed input, and a test that pins disputed behavior must not carry the ID it contradicts (see *Existing tests that pin disputed or decided behavior*).

How this was established:

- Every `csharp` block of the five specs was extracted to its own fixture. Each fixture was formatted twice with the CLI (`Reihitsu.Cli`, Release build of an exported copy of 293b104 in the scratchpad), under LF and again under CRLF. Every input formats to the block that follows it, every "is left unchanged" block stays unchanged, and pass 2 changes nothing. That is 23 structural, 25 blank-line, 17 documentation, 8 region and 15 using examples. The three documentation examples written as prose (a tab separator, whitespace-only `///` lines, the two-line summary) were built byte-exact and hold too.
- Language-version prose was checked with `--lang-version`. Under C# 6, FMT-STRUCT-009's `Value` is unchanged, and FMT-STRUCT-014's `Value` and indexer are converted while `Fail` keeps its accessor list. Under C# 11 only the two records of FMT-STRUCT-017 are converted.
- Candidate inputs were taken from the dispatch code: the rewriter list in `StructuralTransformPhase.CreateRewriters`, the statement switch in `BlankLineStatementSpacingRewriter.NeedsBlankLineForStatementKind`, the token kinds in `BlankLineTokenCleanupRewriter.VisitToken`, the region pairing in `RegionNamingRewriter`, and the safety guard in `UsingDirectiveOrderingSafety`. Each was run through the full pipeline twice. Analyzer agreement was checked with `scripts/apply-fix.sh` for RH5002, RH5008, RH5020, RH5025, RH5026, RH5027 and RH5030. RH8026 has no fix provider, so its verdict below is read from the RH8026/RH8027 spec text and was not run.

## Mechanism and candidates

_N/A._ This is not a bug run. Dispatch facts the test implementer can rely on:

- `StructuralTransformPhase` order: control-flow braces, methods, constructors, operators, then (when enabled) accessors from C# 7 and get-only members from C# 6, then conversions, finalizers, local functions, empty types, enum commas, initializer commas, field split. The accessor transforms are disabled for every node-level entry point (`ReihitsuFormatter.CodeFixDisabledStructuralTransforms`).
- `ExpressionBodiedTransformUtilities.UsesExpressionStatement` decides the statement form from syntax alone. The return type must be `void`, or the member must be `async` with an identifier, qualified, or alias-qualified name whose last part is `Task` or `ValueTask` and not generic. `System.Threading.Tasks.Task` and `global::…Task` are recognized; a `using` alias or a custom task-like type is not (Q-S1).
- Blank-line statement rules run only on `BlockSyntax` and `SwitchSectionSyntax` lists, so top-level statements are never spaced. `BlankLineTokenCleanupRewriter` collapses only the run of line breaks at the start of a token's leading trivia, before `{`, `}`, `else`, `catch`, `finally`, and any `while` keyword.
- FMT-BLANK-011's "first content" exemption fires after every `{` token (namespace, accessor list, initializer, interpolation hole) and after a switch-label colon (`BlankLineEditor.IsFirstInBlock`). Only the first comment in a token's leading trivia is checked.
- `DocumentationCommentFormattingPhase` normalizes every `SingleLineDocumentationCommentTrivia` in the tree, whether or not it documents anything.

## Requirements against the code

Test paths are relative to `src/Reihitsu.Formatter.Test/` unless prefixed with `Core.Test/` (`src/Reihitsu.Core.Test/`). Tests were found by name and spot-checked, not verified one by one. "Holds" means the spec example and the probes above agree with the code.

### formatter/structural-transforms.md

- **FMT-STRUCT-001** holds. Tests: `Unit/StructuralTransforms/ControlFlowBraceTransformTests.UnbracedIfBodyWithoutCommentsIsWrappedInBlock`; `Regression/Structural/ControlFlowBraceElsePlacementTests` (`ElseIfChainWithAllBranchesUnbracedPlacesEveryElseOnItsOwnLine`, `ThenBranchAlreadyBracedElseBranchUnbracedBracesTheElseBranch`, `ThenBranchUnbracedElseBranchAlreadyBracedPlacesElseOnItsOwnLine`, `UnbracedIfWithNoElseIsBracedWithoutDisturbingTheEnclosingBrace`); `Regression/Structural/ElseOnOwnLineAfterUnbracedIfBlockTests.LiteralExampleBothBranchesUnbraced`.
- **FMT-STRUCT-002** holds for one comment on each side. Two inputs are open, see Q-S2. Tests: `Unit/StructuralTransforms/ControlFlowBraceTransformTests` (`LeadingCommentAboveIfBodyIsPreserved`, `TrailingCommentOnIfBodyIsPreserved`, `LeadingCommentAboveElseBodyIsPreserved`); `Regression/Structural/ControlFlowBraceCommentTests` (`LeadingCommentAboveUnbracedIfBodyIsPreserved`, `TrailingCommentOnUnbracedIfBodyIsPreserved`).
- **FMT-STRUCT-003** holds. Tests: `Regression/Structural/DirectivePreservationTests.ControlFlowBraceInsertionKeepsDirectivesAroundBody`. `ControlFlowBraceInsertionKeepsDirectiveBeforeElse` covers a directive outside the body's trivia, which is braced.
- **FMT-STRUCT-004** holds, apart from aliases and custom task-like types (Q-S1). Tests: `Regression/Structural/ExpressionBodiedMethodTests` (`VoidMethodConvertsToExpressionStatement`, `ReturningMethodConvertsToReturnStatement`, `AsyncTaskMethodConvertsToExpressionStatement`, `AsyncValueTaskMethodConvertsToExpressionStatement`, `AsyncGenericValueTaskMethodConvertsToReturnStatement`, `ThrowingMethodConvertsToThrowStatement`); `Unit/StructuralTransforms/ExpressionBodiedTransformTests` (`ConvertsNonAsyncTaskExpressionBodiedMethodToReturnStatement`, `ConvertsAsyncGenericTaskExpressionBodiedMethodToReturnStatement`, `ConvertsAsyncVoidExpressionBodiedMethodToExpressionStatement`, `ConvertsExpressionBodiedMethodThrowingException`); `Regression/Structural/ExpressionBodiedLocalFunctionTests.*`; `Unit/StructuralTransforms/ExpressionBodiedLocalFunctionTransformTests.*`; `Regression/FullPipeline/ExpressionBodiedMethodFullPipelineTests.ConvertsExpressionBodiedMethodsToBlockBodies`. For "under every language version" no dedicated test was found; `Regression/Structural/EmptyTypeDeclarationSemicolonLanguageVersionAfterEarlierTransformTests.EmptyClassStaysBracedBelowCSharp12AfterExpressionBodiedMethodTransform` converts a method under C# 11 incidentally.
- **FMT-STRUCT-005** holds. Tests: `Regression/Structural/ExpressionBodiedConstructorTests` (`ExpressionBodiedConstructorConvertsToBlockBody`, `ConstructorWithThisOrBaseConvertsCorrectly`); `Regression/Structural/ExpressionBodiedOperatorTests` (`ReturningOperatorConvertsToReturnStatement`, `VoidCompoundAssignmentOperatorConvertsToExpressionStatement`, `VoidIncrementOperatorConvertsToExpressionStatement`, `VoidOperatorWithThrowExpressionConvertsToThrowStatement`); `Unit/StructuralTransforms/ExpressionBodiedTransformTests` (`ConvertsExpressionBodiedFinalizerToBlockBody`, `ConvertsExpressionBodiedConversionToBlockBody`, `ConvertsExpressionBodiedOperatorThrowingException`, `ConvertsExpressionBodiedConversionThrowingException`); `Regression/FullPipeline/ExpressionBodiedConstructorFullPipelineTests.ConvertsExpressionBodiedConstructorsToBlockBodies`.
- **FMT-STRUCT-006** holds for the spec's two examples. "Carried into the block" and "joins the return keyword" are open, see Q-S3. Tests: `Regression/Structural/ExpressionBodyNextLineExpressionTests` (`ReturningMethodWithExpressionOnNextLineConvertsInOnePass`, `ReturningMethodWithTabIndentedExpressionOnNextLineConvertsInOnePass`, `OperatorWithExpressionOnNextLineConvertsInOnePass`, `ConversionOperatorWithExpressionOnNextLineConvertsInOnePass`, `LocalFunctionWithExpressionOnNextLineConvertsInOnePass`, `RefReturningMethodWithExpressionOnNextLineConvertsInOnePass`, `AwaitedExpressionOnNextLineConvertsInOnePass`, `MultiLineCallOnNextLineConvertsInOnePass`); `Regression/Structural/ExpressionBodiedMethodTests.PreservesCommentBeforeSemicolon`; `Unit/StructuralTransforms/ExpressionBodiedTransformTests` (`PreservesCommentLeadingSemicolonIn{Method,Constructor,Operator,Conversion,Finalizer}`, `PreservesDirectiveLeadingSemicolonInMethod`); `Regression/BlankLines/ExpressionBodyConversionCommentPlacementTests.*` (void members). See also the pinned list below.
- **FMT-STRUCT-007** holds. Tests: `Regression/Structural/ExpressionBodyDirectiveGuardTests.*` (directive before the expression for each member kind, `#pragma`, `#nullable`, `#line`, `#region` before the expression, region after the expression, region before the arrow, a directive or balanced conditional after the expression still converted). Core.Test has no `ExpressionBodyRewriteUtilities` tests. For "a conditional directive with its partner outside the span" no test was found by name.
- **FMT-STRUCT-008** holds. Tests: `Unit/StructuralTransforms/ExpressionBodiedTransformTests` (`DoesNotConvertExpressionBodiedProperty`, `KeepsExpressionBodiedIndexer`, `KeepsExpressionBodiedIndexerThrowingException`); `Regression/Structural/ExpressionBodiedIndexerTests` (`ExpressionBodiedIndexerRemainsUnchanged`, `ExpressionBodiedPropertyRemainsUnchanged`, `AccessorLevelExpressionBodiesRemainUnchanged`); `Regression/FullPipeline/ExpressionBodiedIndexerFullPipelineTests.KeepsExpressionBodiedIndexer`. For lambdas no test was found by name.
- **FMT-STRUCT-009** holds. Tests: `Regression/Structural/ExpressionBodiedAccessorTests` (`PropertyGetAndSetAccessorsConvertToExpressionBodies`, `IndexerGetAndSetAccessorsConvertToExpressionBodies`, `InitAccessorConvertsToExpressionBody`, `GetterThrowStatementConvertsToThrowExpression`, `SetterAndInitThrowStatementsConvertToThrowExpressions`, `AccessorBlocksStayBelowCSharp7`, `AccessorBlocksConvertFromCSharp7`, `AccessorBlocksStayBelowCSharp7AfterEarlierTransformChangedTree`); `Unit/ReihitsuFormatterLanguageVersionTests.FormatSyntaxTreeKeepsAccessorBlockBelowCSharp7AfterEarlierTransform`.
- **FMT-STRUCT-010** holds. A statement with attributes (`get { [Obsolete] return _v; }`) was probed and keeps its block. Tests: `Regression/Structural/ExpressionBodiedAccessorTests` (`TwoStatementsKeepBlock`, `EmptySetterKeepsBlock`, `GetterExpressionStatementKeepsBlock`, `GetterRethrowKeepsBlock`, `GetterYieldReturnKeepsBlock`, `SetterReturnKeepsBlock`, `SetterDeclarationKeepsBlock`, `SetterIfStatementKeepsBlock`, `EventAccessorsKeepBlocks`). For the attributed statement no test was found.
- **FMT-STRUCT-011** holds. Tests: `Regression/Structural/ThrowOperandPrecedenceExpressionBodyTests.*`.
- **FMT-STRUCT-012** holds. Tests: `Regression/Structural/ExpressionBodiedAccessorTests` (`TrailingCommentAfterStatementMovesToNewSemicolon`, `TrailingCommentAfterClosingBraceMovesToNewSemicolon`, `CommentsAfterStatementAndClosingBraceKeepBlock`, `OwnLineCommentBeforeStatementKeepsBlock`, `CommentBeforeClosingBraceKeepsBlock`, `CommentAfterOpeningBraceKeepsBlock`, `CommentBeforeOpeningBraceKeepsBlock`, `CommentAfterAccessorKeywordKeepsBlock`, `CommentAfterReturnKeywordKeepsBlock`, `CommentBeforeReturnedExpressionKeepsBlock`, `CommentBeforeStatementSemicolonKeepsBlock`, `LineCommentBeforeSemicolonKeepsBlock`, `PragmaBeforeStatementKeepsBlock`, `PragmaBeforeOpeningBraceKeepsBlock`, `ConditionalBeforeStatementSemicolonKeepsBlock`, `DocumentationCommentInsideBodyKeepsBlock`, `BlockCommentBeforeSemicolonTravelsWithExpression`, `TrailingLineCommentMovesToNewSemicolon*`); `Regression/Structural/ThrowOperandPrecedenceExpressionBodyTests` (`LineCommentBeforeSemicolonKeepsBlockBody`, `CommentAfterClosingBraceFollowsSemicolon`).
- **FMT-STRUCT-013** holds. Tests: `Regression/Structural/ExpressionBodiedAccessorTests` (`ConditionalStartingInsideExpressionAndEndingOutsideBodyKeepsBlock`, `RegionInsideBodyKeepsBlock`, `RegionInsideExpressionKeepsBlock`, `BalancedConditionalInsideExpressionConverts`, `BalancedConditionalInsideExpressionConvertsWhenSymbolIsDefined`).
- **FMT-STRUCT-014** holds. Tests: `Regression/Structural/GetOnlyMemberExpressionBodyTests` (`BlockGetterPropertyBecomesExpressionBodiedProperty`, `SingleLineExpressionGetterPropertyBecomesExpressionBodiedProperty`, `ThrowingGetterPropertyBecomesThrowExpressionBody`, `BlockGetterIndexerBecomesExpressionBodiedIndexer`, `ExpressionGetterIndexerBecomesExpressionBodiedIndexer`, `BlockGetterIsKeptBelowCSharp6`, `ReturningBlockGetterIsConvertedInCSharp6`, `ThrowingBlockGetterIsKeptInCSharp6`, `ThrowingBlockGetterIsConvertedInCSharp7`, `BlockGetterIsKeptBelowCSharp6AfterEarlierTransform`); `Unit/ReihitsuFormatterLanguageVersionTests` (`FormatSyntaxTreeKeepsGetOnlyAccessorListBelowCSharp6`, `FormatSyntaxTreeConvertsGetOnlyPropertyInCSharp6`); `Regression/FullPipeline/GetOnlyMemberFullPipelineTests.GetOnlyMembersBecomeExpressionBodiedMembers`.
- **FMT-STRUCT-015** holds. Tests: `Regression/Structural/GetOnlyMemberExpressionBodyTests` (`AutoGetterIsUnchanged`, `AttributedGetterKeepsAccessorList`, `GetterModifierKeepsAccessorList`, `PropertyWithInitializerKeepsAccessorList`, `BlockGetterPropertyWithInitializerKeepsAccessorList`, `MultiStatementGetterKeepsAccessorList`, `LineCommentAfterSignatureKeepsAccessorList`, `BlockCommentAfterIndexerSignatureKeepsAccessorList`, `CommentBeforeGetterKeepsAccessorList`, `CommentAfterGetKeywordKeepsAccessorList`, `CommentBeforeGetterArrowKeepsAccessorList`, `CommentAfterGetterArrowKeepsAccessorList`, `CommentBeforeGetterExpressionKeepsAccessorList`, `CommentBeforeGetterSemicolonKeepsAccessorList`, `CommentBeforeClosingBraceKeepsAccessorList`, `Directive*KeepsAccessorList`, `PragmaBeforeGetterKeepsAccessorList`, `RegionInsideAccessorListKeepsAccessorList`, `UnbalancedConditionalAcrossClosingBraceKeepsAccessorList*`, `CommentTrailingGetterSemicolonFollowsMemberSemicolon`, `CommentTrailingClosingBraceFollowsMemberSemicolon`, `CommentsTrailingSemicolonAndClosingBraceKeepAccessorList`).
- **FMT-STRUCT-016** holds. Tests: `Unit/ReihitsuFormatterTests` (`FormatNodeKeepsSingleStatementAccessorBlock`, `FormatNodeInDocumentAsyncKeepsSingleStatementAccessorBlock`, `FormatNodeInDocumentAsyncOnRootKeepsSingleStatementAccessorBlock`, `FormatNodeInDocumentWithContextAsyncKeepsSingleStatementAccessorBlock`, `FormatNodeKeepsGetOnlyAccessorList`, `FormatNodeInDocumentAsyncKeepsGetOnlyAccessorList`, `FormatNodeInDocumentAsyncOnRootKeepsGetOnlyAccessorList`, `FormatNodeInDocumentWithContextAsyncKeepsGetOnlyAccessorList`, `FormatSyntaxTreeConvertsSingleStatementAccessorToExpressionBody`, `FormatDocumentAsyncConvertsSingleStatementAccessorToExpressionBody`, and `FormatNodeInDocumentWithContextAsyncTracksTargetAcrossStructuralTransforms` for the other transforms at node level).
- **FMT-STRUCT-017** holds. Tests: `Regression/Structural/EmptyTypeDeclarationSemicolonConversionTests.*` (5 kinds); `Regression/Structural/EmptyTypeDeclarationSemicolonLanguageVersionAfterEarlierTransformTests.*`; `Unit/ReihitsuFormatterLanguageVersionTests` (`FormatDocumentAsyncKeepsEmptyClassBracedInCSharp11Project`, `FormatNodeKeepsEmptyClassBracedForCSharp11Tree`, …). For "an empty enum keeps its braces" no test was found.
- **FMT-STRUCT-018** holds. Tests: `Regression/Structural/EmptyTypeDeclarationSemicolonCommentTests.*` (comment before the open brace and behind the header for 5 kinds, directive between header and brace); `Core.Test/EmptyTypeDeclarationSemicolonAnalysisUtilitiesTests` (`CanConvertSafelyReturnsFalseWhenBodyContainsCommentTrivia`, `CanConvertSafelyReturnsFalseWhenCommentPrecedesOpenBrace`, `CanConvertSafelyReturnsFalseWhenCommentTrailsHeaderLine`). For a comment or directive inside the body at formatter level no test was found.
- **FMT-STRUCT-019** holds. Tests: `Integration/StructuralTransformPhaseIntegrationTests` (`RemovesTrailingCommaFromFinalEnumMember`, `RemovesTrailingCommaFromFinalEnumMemberWhilePreservingComment`, `PreservesTrailingCommaBeforeConditionalBlockInFinalEnumMember`, `PreservesTrailingCommaInEnumWhenActiveIfTrueBranchFollowsFinalMember`).
- **FMT-STRUCT-020** holds. Tests: `Integration/StructuralTransformPhaseIntegrationTests` (array and collection initializer tests, including `PreservesTrailingCommaBeforeConditionalBlockInArrayInitializer`, `PreservesInnerTrailingCommaInNestedArrayInitializerWhileRemovingOuter`, `RemovesTrailingCommaWhenRegionDirectiveFollowsFinalElement`, `RemovesTrailingCommaWhenPragmaDirectiveFollowsFinalElement`). For object initializers, anonymous objects, and collection expressions keeping their comma no test was found by name.
- **FMT-STRUCT-021** holds. Tests: `Regression/Structural/FieldDeclarationSplitTests` (`CombinedFieldsAreSplit`, `CombinedStaticInterfaceFieldsAreSplit`, `DocumentedCombined{Struct,Record,RecordStruct,Interface}FieldsAreSplitAtMemberIndentation`, `DocumentedCombinedFieldsWithAttributeAreSplitAtMemberIndentation`). For event fields and locals not being split no test was found. See Q-S5 for the RH8026/RH8027 conflict.
- **FMT-STRUCT-022** holds for the spec example. See Q-S4 for a block comment between a comma and the next declarator. Tests: `Regression/Structural/FieldDeclarationSplitTests` (`TrailingCommentAfterSeparatorIsPreserved`, `StandaloneCommentBeforeDeclaratorIsPreserved`, `DocumentedCombinedFieldsAreSplitAtMemberIndentation`, `CommentsBeforeEveryTerminatorStayOnTheirOwnGeneratedField`, `DeclaratorAndSeparatorCommentsKeepTheirSourceOrder`, `DocumentationCommentAfterSeparatorDocumentsFollowingField`).
- **FMT-STRUCT-023** holds. Tests: `Regression/Structural/DirectivePreservationTests.FieldDeclarationSplitKeepsDirectivesInDeclaration`; `Regression/Structural/FieldDeclarationSplitTests` (`DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact`, `DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact`).

### formatter/blank-lines.md

- **FMT-BLANK-001** holds inside blocks and switch sections. See Q-B4 for where the line goes when a directive and a comment precede the statement. Tests: `Regression/BlankLines/BlankLineBeforeStatementTests` (one per statement kind, `IfStatementFirstInBlockNoBlankLine`, `ReturnStatementFirstInBlockNoBlankLine`, `AlreadyHasBlankLineNoDoubleInsert`, `ElseIfNoBlankLine`); `Unit/BlankLines/BlankLinePhaseTests.InsertsBlankLineBefore*Statement`, `DoesNotInsertBlankLineForFirstStatementInBlock`, `PreservesExistingBlankLineBeforeStatement`; `Regression/FullPipeline/BlankLineBeforeStatementFullPipelineTests.InsertsBlankLinesBeforeStatements` (also `unchecked`).
- **FMT-BLANK-002** holds. Compound and deconstruction assignments count as assignments; `x++` does not. Tests: `Unit/ReihitsuFormatterNodeBlankLineTests` (`InsertsBlankLineAboveDeclarationAfterExpression`, `DoesNotInsertBlankLineAboveDeclarationAfterDeclaration`, `InsertsBlankLineAboveInvocationAfterDeclaration`, `DoesNotInsertBlankLineAboveAssignmentAfterDeclaration`, `InsertsBlankLineAboveUnchangedStatementAfterDeclaration`). These are node-level; no document-level test of the assignment exception was found by name.
- **FMT-BLANK-003** holds. Tests: `Regression/BlankLines/BlankLineBeforeStatementTests` (`YieldReturnInsertsBlankLine`, `YieldReturnAfterYieldReturnNoBlankLine`); `Unit/ReihitsuFormatterNodeBlankLineTests` (`InsertsBlankLineAboveYieldAfterExpression`, `DoesNotInsertBlankLineAboveYieldAfterYield`). For `yield break` no test was found by name.
- **FMT-BLANK-004** holds. See Q-B3 for a directive before the next section. Tests: `Regression/BlankLines/BlankLineAfterStatementTests` (`BreakInBlockInsertsBlankLineAfter`, `BreakLastInBlockNoBlankLine`, `BreakInSwitchSectionInsertsBlankLineBeforeNextLabel`, `BreakInSwitchSectionLastSectionNoBlankLine`, `NonBreakStatementNoBlankLine`); `Regression/BlankLines/BlankLineBeforeStatementTests.BreakStatementInsertsBlankLine`; `Regression/BlankLines/BracedSwitchSectionBlankLineTests.*`; `Unit/ReihitsuFormatterNodeBlankLineTests` (`InsertsBlankLineAboveSwitchSectionAfterBreakSection`, `DoesNotInsertBlankLineAboveSwitchSectionAfterReturnSection`, `InsertsBlankLineAboveStatementAfterBreak`, `DoesNotInsertBlankLineAboveTerminalSwitchSectionBreakAfterExpression`); `Regression/FullPipeline/BlankLineAfterStatementFullPipelineTests.InsertsBlankLinesAfterBreakStatements`.
- **FMT-BLANK-005** holds except for a block statement after `}` (Q-B2). Tests: `Unit/ReihitsuFormatterNodeBlankLineTests` (`InsertsBlankLineBelowDirectiveAboveStatementAfterClosingBrace`, `InsertsBlankLineBelowDisabledTextAboveStatementAfterClosingBrace`, `InsertsBlankLineAboveUnchangedStatementAfterClosingBrace`, `InsertsBlankLineAboveRewrittenStatementAfterClosingBrace`). For the same-line pair at document level no test was found by name.
- **FMT-BLANK-006** holds for statements. Tests: `Regression/BlankLines/BlankLineBeforeStatementTests` (`CommentPrecededByDirectiveNoBlankLineInserted`, `ReturnStatementPrecededByDirectiveNoBlankLineInsertedInsideRegion`); `Regression/BlankLines/BlankLineAfterStatementTests.BreakFollowedByDirectiveNoBlankLineInsertedInsideRegion`; `Unit/ReihitsuFormatterNodeBlankLineTests.DoesNotInsertBlankLineBelowDirectiveAboveRewrittenStatementAfterDeclaration`.
- **FMT-BLANK-007** holds. Tests: `Unit/BlankLines/BlankLinePhaseTests.RemovesBlankLineAfterOpenBrace`; `Regression/FullPipeline/BlankLineStructureFullPipelineTests.RemovesBlankLineAfterOpenBrace`; `Integration/BlankLinePhaseIntegrationTests.RemovesBlankLineAfterOpenBrace`; `Regression/BlankLines/BlankLineBeforeIfDirectiveAfterStatementTests` (`BlankLineDirectlyAboveClosingBraceWithNoDirectiveIsStillRemoved`, `BlankLineAfterOpenBraceBeforeDirectiveBlockIsStillRemoved`, `BlankLineAtFileStartBeforeDirectiveIsStillRemoved`); `Regression/BlankLines/BlankLineBeforeTrailingScopeCommentTests.BlankLineDirectlyBeforeClosingBraceIsRemovedWhenTrailingCommentPrecedesIt`.
- **FMT-BLANK-008** does not hold when a comment stands between the closing brace and the blank line (Q-B1). It is also open for block statements (Q-B2). Tests for the plain case: `Unit/ReihitsuFormatterNodeBlankLineTests.RemovesBlankLineBeforeOpeningBraceOfRewrittenBlock`. For a blank line directly before `else`, `catch`, `finally` or do-`while` without a directive, no document-level test was found by name.
- **FMT-BLANK-009** holds. Tests: `Regression/BlankLines/BlankLineBeforeIfDirectiveAfterStatementTests` (`TrailingDirectiveBefore{OpenBrace,Else,Catch,Finally,DoWhileFooter,TypeDeclarationClosingBrace,EnumClosingBrace,PropertyAccessorListClosingBrace,InitializerClosingBrace}KeepsItsBlankLine`, `BlankLineOnlyAfterTrailingDirectiveBefore{Else,Catch,Finally,DoWhileFooter}IsRemoved`, `BlankLineBeforeAndAfterTrailingDirectiveKeepsOnlyTheFirst`, `ExcessBlankLinesBeforeTrailingDirectiveCollapseToOne`).
- **FMT-BLANK-010** holds except for a region directive on the next line (see #1348 under further instances). Tests: `Unit/BlankLines/BlankLinePhaseTests` (`RemovesBlankLineAfterSingleLineDocumentationComment`, `RemovesBlankLineAfterMultiLineDocumentationComment`, `PreservesCommentAfterDocumentationBlankLine`, `PreservesDirectiveAfterDocumentationBlankLine`); `Regression/FullPipeline/BlankLineStructureFullPipelineTests.RemovesDocumentationBlankLineWithTrailingWhitespaceInOnePass`.
- **FMT-BLANK-011** holds as written. Its scope is narrowed by decided #1148; see further instances. Tests: `Regression/BlankLines/BlankLineBeforeTrailingScopeCommentTests` (`BlankLineIsInsertedBeforeTrailingCommentThatHasNone`, `NoBlankLineIsInsertedBeforeCommentFirstInBlock`, `CommentAfterPragmaDirectiveStaysAttachedToFollowingStatement`, `CommentAfterDirectiveBlockStaysAttachedToFollowingStatement`, `CommentAfterDisabledTextStaysAttachedToFollowingStatement`, `DocumentationCommentBeforeClosingBraceIsUnaffected`); `Unit/BlankLines/BlankLinePhaseTests.CollapsesExtraBlankLinesBeforeCommentedStatement`; `Unit/ReihitsuFormatterTests.FormatSyntaxTreeCollapsesExtraBlankLinesBeforeCommentedStatement`; `Regression/BlankLines/CommentBelowMultiLineTrailingBlockCommentTests.InsertsBlankLineAbove*`; `Unit/ReihitsuFormatterNodeBlankLineTests.DoesNotInsertBlankLineAboveCommentOfRewrittenStatementAfterSwitchLabel`.
- **FMT-BLANK-012** holds. Tests: `Regression/BlankLines/DelimitedDocumentationBlankLineConvergenceTests.*`; `Regression/BlankLines/DelimitedDocumentationBeforeBlockStatementTests.*`; `Regression/BlankLines/CommentBelowMultiLineTrailingBlockCommentTests` (`KeepsStrandedSingleLineDocumentationBehindTwoLineBlockComment`, `KeepsStrandedDelimitedDocumentationBehindTwoLineBlockComment`, `SeparatesOwnLineStrandedDocumentationBelowTwoLineBlockComment`); `Regression/FullPipeline/TrailingDocumentationCommentPreservationTests.MovesDelimitedDocumentationAfterFieldSemicolonAboveFollowingMemberInOnePass`.
- **FMT-BLANK-013** holds. Tests: `Regression/BlankLines/BlankLineRegionTests.*`; `Unit/BlankLines/BlankLineRegionDirectivePhaseTests` (`InsertsBlankLineBeforeRegionAfterSwitchLabel`, `InsertsBlankLineBeforeEndRegionAfterSwitchLabel`); `Unit/BlankLines/BlankLineRegionDirectiveRewriterTests.ExistingBlankLineBeforeRegionIsNotDuplicatedWhenBlankLineAfterIsAlsoInserted`; `Unit/BlankLines/BlankLinePhaseTests.InsertsBlankLineBeforeEndRegionNotAfterNextRegion`; `Regression/Regions/DisabledRegionLayoutTests.RegionInsideDisabledCodeIsRenamedWithoutBlankLinePadding` (SHARED-BLANK-007).
- **FMT-BLANK-014** holds. Tests: `Unit/BlankLines/BlankLineCollapserTests.*` (including after a region, `#endif`, and a documentation comment); `Integration/BlankLinePhaseIntegrationTests.CollapsesExcessiveBlankLines`.
- **FMT-BLANK-015** holds. Tests: no dedicated one was found by name. `Regression/Regions/DisabledRegionLayoutTests.EmptyRegionInsideDisabledCodeKeepsItsLineCount` covers disabled text incidentally; nothing was found for a verbatim string or block comment.

### formatter/documentation-comments.md

- **FMT-DOC-001** holds. Its open clause (statements, expressions) is decided by #962. Tests: `Unit/ReihitsuFormatterTests.FormatSyntaxTreeMovesTrailingDocumentationAboveFollowingMember`; `Regression/FullPipeline/TrailingDocumentationCommentPreservationTests` (`MovesTrailingSummaryAboveFollowingMember`, `MovesTrailingPlainDocumentationTextAboveFollowingMember`, `MovesTrailingMultilineElementAboveFollowingMember`, `MovesSeveralTrailingDocumentationCommentsInOnePass`); `Unit/ReihitsuFormatterNodeBlankLineTests.MovesSingleLineDocumentationBehindOpeningBraceOfType` (behind `{`, node level).
- **FMT-DOC-002** holds. Tests: `Regression/FullPipeline/TrailingDocumentationCommentPreservationTests` (`MovesFirstTokenDocumentationBelowPrecedingBannerComment`, `MovesDocumentationBelowLeadingBlockCommentSibling`, `MovesDocumentationBelowLeadingMultilineBlockCommentSibling`).
- **FMT-DOC-003** holds for position, but the comment's text is destroyed when its element gets rebuilt (Q-D1). Tests: `Regression/FullPipeline/TrailingDocumentationCommentPreservationTests` (`PreservesTrailingDocumentationWithoutFollowingMember`, `PreservesSingleLineDocumentationAfterFieldSemicolon`); `Regression/Spacing/StrandedDocumentationTokenSpacingTests.KeepsTerminatorIndentationBelowStrayDocumentationCommentBehindReturnExpression` (before `;`). All use plain text, no element.
- **FMT-DOC-004** holds. Tests: `Regression/FullPipeline/TrailingDocumentationCommentPreservationTests` (`MovesTrailingSummaryAboveFollowingMember`, `MovesTrailingSummaryWithNestedXmlAboveFollowingMember`).
- **FMT-DOC-005** holds. Tests: `Unit/DocumentationComments/DocumentationCommentFormattingPhaseTests.NormalizesLinePrefixSpacing`; `Regression/FullPipeline/DocumentationCommentNestedNormalizationTests` (`IndentedDocumentationExteriorGainsItsSpace`, `ColumnZeroDocumentationExteriorGainsItsSpace`); `Core.Test/DocumentationCommentUtilitiesTests` (`NormalizeDocumentationLinePrefixesInsertsMissingSeparator`, `NormalizeDocumentationLinePrefixesReplacesTabSeparator`, `NormalizeDocumentationLinePrefixesKeepsContentIndentation`, `NormalizeExteriorSuffixAddsMissingSeparator`, `NormalizeExteriorSuffixPreservesContentIndentation`).
- **FMT-DOC-006** holds. Tests: `Core.Test/DocumentationCommentUtilitiesTests` (`NormalizeDocumentationLinePrefixesReducesWhitespaceOnlySuffix`, `NormalizeDocumentationLinePrefixesKeepsEmptySuffix`, `NormalizeExteriorSuffixRemovesWhitespaceOnlyExcess`). No formatter-level test was found. Decided #959 will change this behavior.
- **FMT-DOC-007** holds. Tests: `Unit/DocumentationComments/DocumentationCommentFormattingPhaseTests.LeavesLaterSlashesOnTheLineAsContent`; `Core.Test/DocumentationCommentUtilitiesTests.NormalizeDocumentationLinePrefixesTreatsLaterSlashesAsContent`.
- **FMT-DOC-008** holds. The case-insensitive match is current behavior that decided #1355 will change. Tests: `Unit/DocumentationComments/DocumentationCommentFormattingPhaseTests.ExpandsSingleLineSummary`; `Unit/DocumentationComments/DocCommentElementNormalizerTests` (`RequiresNormalizationReturnsTrueForSingleLineSummary`, `BuildReplacementExpandsSingleLineSummary`); `Regression/FullPipeline/DocumentationSummaryFormattingFullPipelineTests.*`; `Regression/FullPipeline/DocumentationElementTextLineAlignmentFullPipelineTests.KeepsSummaryElementExpanded`. For a non-lower-case `<Summary>` no test was found.
- **FMT-DOC-009** holds. Its inline-element clause is decided by #961. Tests: `Regression/FullPipeline/DocumentationElementTextLineAlignmentFullPipelineTests` (`MovesMultilineContentBelowOpeningTag`, `CollapsesSingleContentLineElement`, `MovesLeadingInlineXmlContentBelowOpeningTag`, `ExpandsElementWithoutReclassifyingInvalidSeparatorAsContent`); `Unit/DocumentationComments/DocCommentElementNormalizerTests` (`RequiresNormalizationReturnsTrueForMisalignedElement`, `BuildReplacementCollapsesNonSummaryElement`).
- **FMT-DOC-010** holds. Its end-tag side is decided by #961. Tests: `Unit/DocumentationComments/DocCommentElementNormalizerTests` (`RequiresNormalizationReturnsFalseForSummarySpanningThreeLines`, `RequiresNormalizationReturnsFalseForWellAlignedElement`); `Unit/DocumentationComments/DocumentationCommentFormattingPhaseTests.LeavesWellFormedCommentUnchanged`.
- **FMT-DOC-011** holds for a comment on its own line (see Q-D1 for one behind code). Tests: `Regression/FullPipeline/DocumentationCommentNestedNormalizationTests` (`NestedCodeElementIsNormalizedInTheSameRun`, `TwiceNestedElementIsNormalizedInTheSameRun`, `UnnestedElementIsNormalized`); `Regression/FullPipeline/DocumentationElementTextLineAlignmentFullPipelineTests` (`PreservesBlankSeparatorLineInsideCodeElement`, `PreservesBlankSeparatorLineInsideRemarksElement`); `Unit/DocumentationComments/DocCommentElementNormalizerTests.BuildReplacementDoesNotDuplicateLeadingSentenceText`.
- **FMT-DOC-012** holds. Tests: for the documentation phase leaving `/** <summary>…</summary> */` text alone, none was found. `Unit/BlankLines/BlankLinePhaseTests.PreservesCommentAfterDocumentationBlankLine` contains one but runs only the blank-line phase.

### formatter/region-formatting.md

- **FMT-REGION-001** holds. Tests: `Regression/Regions/RegionFormattingTests` (`LowercaseRegionNameCapitalizesFirstLetter`, `AlreadyUppercaseNoChange`, `LowercaseTrailingOpeningAndBareCloseAreCanonicalized`); `Unit/RegionFormatting/RegionNamingRewriterTests.CapitalizesRegionDescription`; `Unit/RegionFormatting/RegionFormattingPhaseTests` (`CapitalizesRegionDescription`, `PreservesRegionWithCorrectCasing`). For a non-ASCII first letter or a description that starts with a digit or `_`, no test was found.
- **FMT-REGION-002** holds. Tests: `Regression/Regions/RegionFormattingTests` (`EndRegionCommentSynchronizedWithRegionName`, `EndRegionCommentAlreadyCorrectNoChange`, `MismatchedEndRegionCommentCorrected`, `MismatchedCloseUsesNormalizedOpeningDescription`); `Unit/RegionFormatting/RegionNamingRewriterTests.SyncsEndregionComment`; `Unit/RegionFormatting/RegionFormattingPhaseTests` (`SyncsEndregionWithRegion`, `HandlesEndregionWithMismatchedDescription`); `Core.Test/RegionDirectiveUtilitiesTests.CreateCanonicalEndRegionTriviaPreservesExteriorTriviaAndLineEndings`.
- **FMT-REGION-003** holds. Tests: `Regression/Regions/RegionFormattingTests.NestedRegionsBothCorrected`; `Unit/RegionFormatting/RegionFormattingPhaseTests.HandlesNestedRegions`; `Core.Test/RegionDirectiveUtilitiesTests.GetRegionPairsReturnsMatchedPairsInClosingTraversalOrder`.
- **FMT-REGION-004** holds. Tests: `Regression/Regions/RegionFormattingTests.RegionWithoutNameIsSkipped`; `Unit/RegionFormatting/RegionNamingRewriterTests.LeavesRegionWithoutDescriptionUnchanged`; `Unit/RegionFormatting/RegionFormattingPhaseTests.HandlesRegionWithoutDescription`. All three use a bare `#endregion`; for "including any text after that `#endregion`" no test was found.
- **FMT-REGION-005** holds. Tests: `Regression/Regions/StraddlingRegionPairTests.*`; `Regression/FullPipeline/RegionFormattingFullPipelineTests` (`RegionInsideAccessorBodyIsPreservedAndFormatted`, `RegionAroundStatementIsPreservedAndFormatted`, `RegionInsidePropertyAccessorListIsPreserved`, …); `Regression/Regions/DisabledRegionLayoutTests.RegionInsideDisabledMethodBodyIsNotRemoved`.
- **FMT-REGION-006** holds. Tests: `Regression/Regions/DisabledRegionLayoutTests` (`RegionInsideDisabledCodeIsRenamedWithoutBlankLinePadding`, `CanonicalRegionInsideDisabledCodeIsUnchanged`); `Regression/FullPipeline/RegionFormattingFullPipelineTests.InactiveRegionInsideAccessorListIsStillNamed`.

### formatter/using-directives.md

- **FMT-USING-001** holds. Tests: `Unit/UsingDirectives/UsingGroupingTests.*`; `Unit/UsingDirectives/UsingDirectiveOrderingRewriterTests` (`RegularUsingsWithoutTriviaAreReordered`, `AliasDirectivesWithoutTriviaAreReordered`, `UsingStaticDirectivesWithoutTriviaAreReordered`, `CrossGroupUsingsWithoutTriviaAreReordered`, `NamespaceUsingsAreReordered`, `FileScopedNamespaceUsingsAreReordered`); `Regression/UsingDirectives/UsingDirectiveOrderingTests` (`AliasesAreOrderedByTargetRootNamespace`, `GlobalSectionPrecedesLocalSectionAndKeepsOrdinalRootsContiguous`).
- **FMT-USING-002** holds. Tests: for the blank line between groups, `Unit/UsingDirectives/UsingDirectiveOrderingRewriterTests` (`CrossGroupUsingsWithoutTriviaAreReordered`, `CommentPrefixedDifferentGroupUsingReceivesSeparator`, `CommentSeparatedSameGroupUsingsRemainTogether`). For removing a same-group blank line and collapsing several blank lines between groups, none was found by name.
- **FMT-USING-003** holds. Tests: `Unit/UsingDirectives/UsingDirectiveOrderingRewriterTests` (`DirectivesSharingOneLineWithNoLineBreakAnywhereInTheBlockAreSplit`, `OrderedSameGroupDirectivesSeparatedBySpaceAreSplit`, `OrderedSameGroupDirectivesSeparatedByTabAreSplit`, `CrossGroupDirectivesSeparatedBySpaceAreSplitWithBlankLine`); `Regression/UsingDirectives/UsingDirectiveOrderingTests.CrossGroupPairSharingOneLineIsSeparatedInOnePass`.
- **FMT-USING-004** holds. Tests: `Unit/UsingDirectives/UsingDirectiveOrderingRewriterTests` (`ConditionalDirectiveSkipsReordering`, `NullableDirectiveSkipsReordering`, `PragmaDirectiveSkipsReordering`, `DirectiveBearingBlockSplitsSameLineDirectivesWithoutReordering`); `Regression/UsingDirectives/UsingDirectiveOrderingTests` (`BlockWith{Nullable,Conditional,PragmaWarning}DirectiveIsNotReordered`, `SameLineDirectivesInDirectiveBearingBlockAreSplitWithoutReordering`).
- **FMT-USING-005** holds. Tests: `Unit/UsingDirectives/UsingDirectiveOrderingRewriterTests` (`SameGroupDirectiveAfterMultiLineBlockCommentIsNotSplit`, `SameGroupDirectiveBehindMultiLineDelimitedDocumentationCommentIsNotSplit`, `SameGroupDirectiveAfterSingleLineDocumentationCommentIsNotSplit`, `SameGroupDirectiveBehindSingleLineDelimitedDocumentationCommentIsSplit`, `ReorderKeepsSourceNeighborBehindDelimitedDocumentationCommentOnItsLine`, `SameGroupDirectiveBehind{LineSeparator,NextLineCharacter}IsNotSplit`, `SameGroupDirectiveOnLaterLineBehindDelimitedDocumentationCommentIsNotSplit`); `Regression/UsingDirectives/UsingDirectiveOrderingTests.SameGroupDirectiveBehindMultiLineBlockCommentIsLeftUnchanged`.
- **FMT-USING-006** holds. Tests: `Regression/UsingDirectives/UsingDirectiveOrderingTests` (`LineCommentMovesWithUsingDirective`, `InlineTrailingCommentMovesWithUsingDirective`, `AliasDirectiveCommentMovesWithUsingDirective`, `StaticUsingCommentMovesWithUsingDirective`); `Unit/UsingDirectives/UsingDirectiveOrderingRewriterTests` (`TrailingCommentOnLastDirectiveIsNotAbsorbedIntoNextDirectiveAfterReorder`, `BlockCommentBetweenDirectivesOnOneLineStaysWithThePrecedingDirective`).
- **FMT-USING-007** holds. Tests: `Unit/UsingDirectives/UsingDirectiveOrderingRewriterTests` (`RegularUsingsWithoutTriviaButWithTerminatingNewlineKeepTheNewline`, `TerminatingNewlineLessDirectiveThatStaysLastAfterReorderIsUnaffected`, `CommentOnDirectiveMovedToLastPositionIsTerminatedBeforeTheBlockTerminatorIsAppended`, `CommentOnDirectiveMovedToLastPositionDoesNotDuplicate*`, `LastDirectiveSharingItsLineWithTheClosingBraceKeepsTheSpaceBeforeIt`); `Regression/UsingDirectives/UsingDirectiveOrderingTests.FileScopedNamespaceUsingOnlyBlockKeepsSeparatorsWhenBlockEndsTheFile`.
- **FMT-USING-008** holds. Tests: `Unit/UsingDirectives/UsingDirectiveOrderingRewriterTests` (`CommentPrefixedDifferentGroupUsingReceivesSeparator`, `LeadingCommentAfterTerminatingNewlineLessPredecessorStartsItsOwnLine`, `NamespaceUsingsWithCommentsKeepAttachedTrivia`); `Unit/UsingDirectives/UsingLeadingTriviaBuilderTests.CreateLeadingTriviaForNewGroupPreservesAttachedComment`; `Regression/UsingDirectives/UsingDirectiveOrderingTests.NamespaceUsingsWithCommentsKeepAttachedTrivia`.
- **FMT-USING-009** holds. Tests: `Regression/UsingDirectives/UsingDirectiveOrderingTests` (`BannerStaysAtTopWhenOriginalFirstDirectiveIsDemoted`, `MultiLineBannerStaysAtTopWhenOriginalFirstDirectiveIsDemotedAcrossGroups`, `BannerStaysAtTopWithGlobalUsingReordering`, `BannerStaysAtTopWithAliasReordering`, `IndentedBannerIsNotDuplicatedWhenOriginalFirstDirectiveIsDemoted`); `Unit/UsingDirectives/UsingLeadingTriviaBuilderTests.SplitOriginalFirstHeaderTrivia*` (4 tests).

## Open questions to file

The per-spec issues of group C are all closed: `structural-transforms.md` #913, `region-formatting.md` #914, `documentation-comments.md` #915, `using-directives.md` #916, `blank-lines.md` #917. The lead decides between reopening them and opening a new issue with the same title. Outputs are full-pipeline outputs, idempotent unless stated otherwise.

### `SDD open questions: sdd/specs/formatter/structural-transforms.md`

**Q-S1 — An `async` method whose non-generic `Task` return type is written through an alias gets `return await …;` and no longer compiles.**

Input:

```csharp
using T = System.Threading.Tasks.Task;

class C
{
    async T RunAsync() => await T.Delay(1);
}
```

Output:

```csharp
using T = System.Threading.Tasks.Task;

class C
{
    async T RunAsync()
    {
        return await T.Delay(1);
    }
}
```

The same happens for `using VT = System.Threading.Tasks.ValueTask;` with `async VT RunValueAsync() => …` (method and local function), and for a custom non-generic task-like type (`async MyTask RunAsync() => await Task.Delay(1);`).

Why it looks wrong: `return <expression>;` in an `async` method that returns a non-generic task is CS1997, so the formatted file does not compile (FMT-PIPE-003). FMT-STRUCT-004 says the expression-statement form applies when the function "returns the non-generic `Task` or `ValueTask`". The code compares the name's text (`ExpressionBodiedTransformUtilities.IsNonGenericTaskLikeReturnType`), so an alias or a custom task-like type falls through to `return`. `System.Threading.Tasks.Task` and `global::System.Threading.Tasks.Task` are recognized.

Touches: FMT-STRUCT-004, FMT-PIPE-003.

**Q-S2 — Bracing an `if`/`else` body drops a blank line between two leading comments and the space between two trailing comments.**

Inputs:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
            // one

            // two
            Foo();
    }
}
```

```csharp
class C
{
    void M(bool a)
    {
        if (a)
            Foo(); /* a */ // b
    }
}
```

Outputs, inside the new block: `// one` directly followed by `// two`, with the blank line gone; and `Foo(); /* a */// b`, with no space between the comments.

Why it looks wrong: `ControlFlowBraceTransform` rebuilds the body's trivia from its comments only. It writes one line break after each leading comment, and a single space only before the first trailing comment. The comments survive, but layout the author wrote between them is lost, and no later phase restores it. A hand-written `/* a */ // b` keeps its space everywhere else.

Touches: FMT-STRUCT-002.

**Q-S3 — Comments around a converted expression body do not land "in the block".**

Inputs:

```csharp
class C
{
    int Other()
        // before arrow
        => 1;
}
```

```csharp
class C
{
    int Get() =>
        // c
        42;
}
```

Outputs:

```csharp
class C
{
    int Other()

    // before arrow
    {
        return 1;
    }
}
```

```csharp
class C
{
    int Get()
    {
        return

        // c
        42;
    }
}
```

Why it looks wrong: FMT-STRUCT-006 says a comment in the trivia of the removed arrow "MUST be carried into the block" and that an expression on the line after the arrow "MUST join the created `return` keyword". In the first output the comment sits between the signature and `{`, outside the block. In the second, the expression does not join `return`; `return` stands alone with the comment between it and its operand. For a `void` member the same comment becomes the block's first line (`Regression/BlankLines/ExpressionBodyConversionCommentPlacementTests.PlacesCommentOnOwnLineInConvertedMethodBody`), so the two statement forms disagree. The blank lines in both outputs are the decided #1148 scope, and the operand's column is #953.

Touches: FMT-STRUCT-006.

**Q-S4 — A block comment between a comma and the next declarator on the same line trails the previous field.**

Input:

```csharp
class C
{
    private int _c, /* before d */ _d /* after d */;
}
```

Output:

```csharp
class C
{
    private int _c; /* before d */
    private int _d /* after d */;
}
```

Why it looks wrong: the comment stands directly in front of `_d`, but it now trails `_c`. FMT-STRUCT-022 states that "a comment that trailed a separating comma MUST trail the field that comma now ends", which matches the code: Roslyn files the comment as the comma's trailing trivia. Its `Why:`, though, says "each comment keeps describing the declarator it was written next to". There are two readings, with different outputs:

- (A) keep the current output;
- (B) treat a same-line comment directly before a declarator as that declarator's, giving `/* before d */` on its own line in front of `private int _d …;`.

Touches: FMT-STRUCT-022.

**Q-S5 — Splitting a documented combined field leaves the later fields undocumented, which RH8026/RH8027 then report.**

Input: the FMT-STRUCT-022 example, or

```csharp
/// <summary>
/// Type
/// </summary>
public class C
{
    /// <summary>
    /// The coordinates
    /// </summary>
    public int X, Y;
}
```

Output: `/// …` above `public int X;`, then `public int Y;` without documentation.

Why it looks wrong: RH8026-001 and RH8027-001 say "a declaration of several fields is one declaration, documented by the documentation comments before it". So the input is clean, and the formatter's output has an undocumented `Y` that they report (read from the specs; RH8026 has no fix provider for `apply-fix.sh`, so this was not run). FMT-STRUCT-022's `Why:` ("a documentation comment documents the declaration it precedes, which is the first field") also differs from what the compiler does: it attaches the comment to every declarator. Three options with different outputs:

- (A) keep the current output;
- (B) repeat the documentation comment on every generated field;
- (C) refuse the split for a documented declaration, as #963 decided for a comment between the type and the first declarator.

Touches: FMT-STRUCT-021, FMT-STRUCT-022, RH8026-001, RH8027-001.

### `SDD open questions: sdd/specs/formatter/blank-lines.md`

**Q-B1 — A blank line between a comment and `else`, `catch`, `finally`, or do-`while` is kept, and RH5026/RH5027 report the output.**

Input:

```csharp
class C
{
    void M(bool a)
    {
        if (a)
        {
        }
        // c

        else
        {
        }
    }
}
```

Output (lines inside `M`):

```csharp
        if (a)
        {
        }

        // c

        else
        {
        }
```

The same happens for `catch`, `finally`, and the `while` of a `do` statement. A blank line between a comment and `{` or `}` is removed.

Why it looks wrong: FMT-BLANK-008 says the formatter "MUST remove the blank lines directly before … the `else`, `catch`, `finally`, and do-statement `while` keywords". `BlankLineTokenCleanupRewriter` collapses only the run of line breaks at the start of the token's leading trivia, so a run after a comment survives. `apply-fix.sh` reports the formatter output: RH5026 for `else`/`catch`/`finally` and RH5027 for `while`, both `fixed`. Formatter output reported by an analyzer is FC-013. The blank line inserted before `// c` is the decided #1148 scope.

Touches: FMT-BLANK-008, RH5026, RH5027.

**Q-B2 — A block statement directly after a statement that ends with `}` gets no blank line, and RH5030 reports it; RH5025 reports the blank line RH5030's fix inserts.**

Input:

```csharp
class C
{
    int M(int value)
    {
        if (value > 1)
        {
            value--;
        }

        {
            value++;
        }

        return value;
    }
}
```

Output: the blank line between `}` and `{` is removed. Without the blank line the input is left unchanged.

Why it looks wrong: FMT-BLANK-005 applies SHARED-BLANK-003, which requires a blank line after a statement ending in `}` and has no exception for a block statement. FMT-BLANK-008 removes every blank line before an opening brace, and the formatter follows FMT-BLANK-008. Its `Why:` ("these tokens continue the statement above them") does not fit a block statement. `apply-fix.sh`: RH5030 reports the formatter's output (`fixed`, it inserts the line), and RH5025 reports the input (`fixed`, it removes the line), so the two fixes undo each other. The node-level test `Unit/ReihitsuFormatterNodeBlankLineTests.DoesNotInsertBlankLineAboveBlockStatementAfterClosingBrace` pins the current output. After an ordinary statement (`a++;`, blank line, `{ … }`), the formatter and RH5025 agree on removing the blank line; whether that is intended belongs in the same decision.

Touches: FMT-BLANK-005, FMT-BLANK-008, SHARED-BLANK-003, RH5025-001, RH5030.

**Q-B3 — A directive in front of the next switch section gets a blank line above it, while a directive in front of a statement suppresses it.**

Input:

```csharp
class C
{
    void M(int a)
    {
        switch (a)
        {
            case 1:
                break;
#pragma warning disable CS0168
            case 2:
                break;
        }
        while (a > 0)
        {
            break;
#pragma warning disable CS0168
            a++;
        }
    }
}
```

Output: a blank line between `break;` and `#pragma` in the switch, and none between `break;` and `#pragma` in the loop. The same happens with `#if DEBUG … #endif` in front of a section.

Why it looks wrong: FMT-BLANK-006's `Why:` ("a blank line inserted in front of the directive would change which code the author grouped") applies to both gaps, but the section rule of FMT-BLANK-004 inserts at the start of the gap without the exemption. RH5011-002 does not report a `break` whose first trivia after the semicolon is a directive. The output is not reported either, so this is a question of intent; P11 allows the formatter to do more than the analyzer.

Touches: FMT-BLANK-004, FMT-BLANK-006, RH5011-002.

**Q-B4 — When a directive and then a comment precede a statement, the statement's blank line goes above the directive.**

Input:

```csharp
class C
{
    void M(int a)
    {
        a++;
#if DEBUG
        a--;
#endif
        // why
        if (a > 0)
        {
        }
    }
}
```

Output: a blank line between `a++;` and `#if DEBUG`, none between `#endif` and `// why`. Same with `#pragma` + comment + `return`.

Why it looks wrong: FMT-BLANK-006 exempts only a statement whose first token is directly preceded by a directive. With a comment in between, FMT-BLANK-001 applies and the line is inserted at the start of the gap, above the whole conditional group, which is the placement FMT-BLANK-006's `Why:` warns about. FMT-BLANK-005 places its line after the directives instead. The RH5002 and RH5008 fixes insert at the same place, so analyzer and formatter agree, although SHARED-STMT-004 says the line goes above the first comment.

Touches: FMT-BLANK-001, FMT-BLANK-006 (shared side: SHARED-STMT-004).

### `SDD open questions: sdd/specs/formatter/documentation-comments.md`

**Q-D1 — A documentation comment written behind code that is not relocated loses its text when its element is rebuilt.** Decided by principle P13 (with P6, #912): a `///` that documents nothing is an ordinary comment. Independent of that, deleting comment text violates FMT-PIPE-002. Highest priority of this list.

Inputs and outputs (one pass, `DocumentationCommentFormattingPhase` only, stable afterwards):

- `int A; /// <summary>end of type</summary>` as the last member, before `}` → `int A; /// <summary>`
- `var x = 1; /// <summary>stray</summary>` before `}` → `var x = 1; /// <summary>`
- `int A /// <summary>stray</summary>` with `;` on the next line → `int A /// <summary>`
- `M(1 /// <remarks>a` / `/// b</remarks>` / `);` → `M(1 /// <remarks>` / `);` (the second comment line disappears)

Why it looks wrong: FMT-DOC-003 keeps such a comment in place, and FMT-DOC-011 says rebuilding "MUST keep the content's text". `DocumentationCommentUtilities.GetContinuationPrefix` takes the whole line up to `///` as the continuation prefix, code included. The rebuilt text therefore no longer parses as one documentation comment, and only its first line is kept. Plain-text stranded comments (no element) are unaffected, which is what the existing tests use.

Touches: FMT-DOC-003, FMT-DOC-008, FMT-DOC-009, FMT-DOC-011, FMT-PIPE-002.

### Region formatting and using directives

No new questions. Every probed input behaves as specified or is already a decided follow-up: #964, #966, #1083, #1239, #1354, #1355, #965.

## Further instances of existing follow-ups (one-line comments, not new questions)

- **#963** (structural comments):
  - `int Get() => /* c */ 42;` gives `{/* c */`, the block-comment twin of Q1.
  - `private /* c */ int _a, _b;` and `[Obsolete /* why */] private int _c, _d;` copy the comment onto every split field. These are the sibling slots of Q3 (between modifiers, inside a repeated attribute list).
- **#1148** (FMT-BLANK-011 only between statements and members). Shapes that get a blank line before a line-leading comment inside a construct:
  - a comment between `}` and `else`/`catch`/`finally`/do-`while` (Q-B1 input);
  - a comment between `return` and its operand after an expression-body conversion (Q-S3);
  - a comment before `=>` that becomes leading trivia of `{` (Q-S3);
  - a comment or documentation comment between an attribute list and its declaration.
- **#1077** (P6/P13, a `///` that documents nothing is an ordinary comment). FMT-DOC-005 to FMT-DOC-011 rewrite such comments:
  - `///<summary>Local.</summary>` before a statement in a method body is expanded to three lines;
  - the same after an attribute list (`[Obsolete]` / `///<summary>…`) is expanded and gets a blank line;
  - `var x = 1; ///stray` becomes `/// stray`.
  - The data-loss shape is Q-D1.
- **#962** (relocate only before a documentable node): `using System.Text; /// about text?` / `using System;` relocates the comment onto its own line above `using System;`, a using directive, which documents nothing.
- **#1348** (RH5031 owns the line above a region directive): the formatter keeps `/// …`, blank line, `#region R` unchanged (FMT-BLANK-013 re-inserts what FMT-BLANK-010 removes). FMT-BLANK-010's text needs the same exception as RH8302.
- **#1076** (P7, blank-line fixes must not undo each other): the RH5025 and RH5030 fixes undo each other on a block statement after `}` (Q-B2).

## Inputs the code decides that the specs do not mention

Facts for the test implementer, behavior as observed. None of these looks wrong enough to file.

- **Structural**
  - `class C { } // note` becomes `class C; // note`: the comment behind the closing brace follows the semicolon, and FMT-STRUCT-018 does not list that slot.
  - A `with`-expression initializer keeps its trailing comma; neither FMT-STRUCT-020 nor the non-goals list it.
  - An empty-statement body is braced (`if (a) ;` gives `{ ; }`).
  - A primary-constructor class or a class with constraint clauses gets the semicolon form (`class Generic<T> where T : class;`).
  - A comment behind an `if` condition moves above the `if`. That is FMT-BREAK-005, not this phase.
- **Blank lines**
  - Top-level statements are never spaced: no FMT-BLANK-001, -002, or -005. SHARED-STMT-002 states this for the analyzer; FMT-BLANK-001 implies it only through "its block or switch section".
  - `unsafe` blocks, labeled statements, nested blocks, and local functions get no statement-kind line.
  - A statement after `do { } while (x);` gets none either, because the last token is `;`.
  - A section whose `break` is inside its block (`case 1: { …; break; }`) does not "end in a `break`", so the next section gets no blank line. RH5011-001 agrees: a `break` followed by `}` is not reported.
  - The FMT-BLANK-011 first-content exemption applies after any `{`: a namespace, an accessor list, an object initializer. It does not apply after a labeled statement's colon (read from `BlankLineEditor.IsFirstInBlock`, not run).
  - Blank lines after an interpolation hole's `{` are removed (FMT-BLANK-007 treats it as an opening brace).
- **Documentation**
  - An empty `<summary/>` element is not rebuilt.
  - Rebuilt content lines lose trailing whitespace; lines that are not rebuilt keep it (#959).
- **Region**
  - Capitalization works per UTF-16 unit: a lower-case letter outside the BMP is not upper-cased, and a title-case letter such as U+01C5 is changed although it is not lower-case. Read from `RegionNamingRewriter.CapitalizeFirstLetter`, not run.
  - `#region` and `#endregion` that cross `#if` branches make the file a syntax error, so it is skipped (FMT-PIPE-005); `scripts/trace.sh` reports "Skipped (syntax errors)" for both probed shapes.
- **Using**
  - `extern alias` directives are not reordered; the spec does not mention them.

## Existing tests that pin disputed or decided behavior

Do not tag these with the requirement they contradict. Each is a characterization of the open question or decided follow-up named.

- `Regression/Structural/ExpressionBodiedMethodTests.PreservesCommentAfterArrow`, `Unit/StructuralTransforms/ExpressionBodiedTransformTests.PreservesCommentTrailingArrowIn{Method,Constructor,Operator,Conversion,Finalizer}`, `Unit/StructuralTransforms/ExpressionBodiedLocalFunctionTransformTests.PreservesCommentTrailingArrowInLocalFunction`: pin `{// why` (#963 Q1).
- `Regression/Structural/ExpressionBodiedMethodTests.PreservesCommentBeforeArrow`, `Regression/Structural/ExpressionBodiedConstructorTests.PreservesCommentBeforeArrow`: pin Q-S3 and #1148.
- `Regression/Structural/ExpressionBodyNextLineExpressionTests.LineCommentBeforeNextLineExpressionKeepsOwnLine`, `BlockCommentLeadingNextLineExpressionKeepsOwnLine`: pin Q-S3 and #1148.
- `Regression/Structural/FieldDeclarationSplitTests.CommentOnTheTypeLineIsStillDuplicatedOntoEveryField`: pins #963 Q3.
- `Unit/ReihitsuFormatterNodeBlankLineTests.DoesNotInsertBlankLineAboveBlockStatementAfterClosingBrace`: pins Q-B2.
- `Regression/Regions/RegionFormattingTests.TrailingOpeningWhitespaceWithCanonicalCloseIsStable`: pins #964.

## Breaking inputs

For the focused tests the test implementer writes:

- FC-004, FC-005, FC-019, FC-020, FC-021: a comment *and* a directive in every gap a requirement names, including `#if false` blocks and a `/* */` comment that ends mid-line. The guards differ per slot (FMT-STRUCT-003/007/012/013/015/018/019/023, FMT-BLANK-006/009/011).
- FC-003: both sides of each guard, for example a comment versus no comment before the getter's semicolon, and a balanced versus an unbalanced conditional group.
- FC-010, FC-017: assert the exact output and a second pass, under LF and CRLF. All examples of group C pass under both today.
- FC-012: the cleanup rewriter removes blank lines that the trivia-boundary, region, and statement rewriters re-insert within the same pass (FMT-BLANK-001/011/013 after FMT-BLANK-007/008); assert the full-phase result, not one rewriter.
- FC-025: the seven expression-bodied kinds, plus conversion operators, for FMT-STRUCT-004 to FMT-STRUCT-007, including `async` with `Task`, `Task<T>`, `ValueTask`, `ValueTask<T>`, and a non-`async` `Task`.
- FC-028: FMT-STRUCT-016 differs between node-level and document-level entry points by design.
- FC-013: Q-B1, Q-B2, Q-S5. Avoid these inputs in tests tagged with FMT-BLANK-005/008 and FMT-STRUCT-022.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Formatter.Test` | Must change: `[Requirement]` tags, plus focused tests where "none found" above |
| `Reihitsu.Core.Test` | May carry tags for the Core predicates named above (FMT-STRUCT-018, FMT-DOC-005/006/007, FMT-REGION-002/003) |
| Formatter and Core production code | Must stay (lane) |
| Specs under `sdd/specs/` | Must stay (no spec edits in this run) |
| Analyzer and code fixes | Not involved; RH5025/RH5026/RH5027/RH5030/RH8026 appear only as evidence |
| `RequirementCoverageTests.EnforcedSpecPaths` | Must change at the end (lead's plan) |

## Simpler option

_None._

## Questions for the maintainer

None beyond the open questions above. Q-S4 and Q-S5 carry competing readings, with their outputs listed.

## Out of scope, for the lead to route

- `formatter/line-breaks.md` / `formatter/pipeline.md` (groups B and A): a `with`-expression initializer is not idempotent. `R P => _r with { A = 1, B = 2 };` gives `R P => _r with {` with the members wrapped on the first pass, and the second pass moves `{` onto its own line. The same happens inside a block-bodied `return r with { … };`. This is FMT-PIPE-004 and FMT-BREAK-021. No existing issue was found.
