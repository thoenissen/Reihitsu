Role: skeptic
Commit: a4a4569
Verdict: proceed

Spec group A: `formatter/pipeline.md`, `formatter/indentation.md`, `formatter/raw-string-alignment.md`, `formatter/cleanup.md`, `formatter/line-endings.md`, `playground/playground.md`. No spec diff exists; the subject is the existing specs against the code at a4a4569 (production code unchanged since 8e0f35d). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

## Premise

Lane premise holds. Every spec example of group A runs as written, except the node-level ones the CLI cannot reach, and those were checked through the API.

- Every `csharp` block of the five formatter specs and of `playground.md` was extracted to its own fixture (`␍` turned into `\r`) and formatted twice with `scripts/format.sh <dir> --force`. Every input block formats exactly to the block after it, apart from the final line break FMT-CLEAN-005 removes. Every expected block stays unchanged. The second pass changed no file (115 of 119 formatted, 0 changed on pass 2; the 2 syntax-error fixtures and the 1 generated fixture were skipped by the CLI). FMT-PIPE-008 C# 5 was checked with `--lang-version 5` and matches.
- The prose-only examples (tabs, trailing whitespace, U+0085/U+2028/U+2029) were built byte-exact and hold: FMT-INDENT-001, FMT-CLEAN-001 to -004, FMT-EOL-001.
- Node-level, BOM, and Playground behavior has no script entry point. It was checked with a throwaway file-based app in the scratchpad that references `src/Reihitsu.Playground` (which brings in `Reihitsu.Formatter`). It calls `FormatSyntaxTree`, `FormatDocumentAsync`, `FormatNode`, `FormatNodeInDocument*Async`, and `PlaygroundFormatter.Format`. These hold: the FMT-PIPE-010 example, FMT-PIPE-009 (tree and document give the same text), FMT-PIPE-011, FMT-PIPE-012, FMT-CLEAN-008, PLAY-003 (output equals the spec block), PLAY-005 (default options → `int P => 1;`), PLAY-006, and PLAY-007 (`1.1.0+a4a4569…`).

Where a requirement is wrong about current behavior, or the case looks wrong, it is listed under *Open questions to file*. None of them blocks tagging: the tests for a requirement simply avoid the disputed input.

## Mechanism and candidates

_N/A._ Not a bug. Dispatch facts the test implementer can rely on:

- Phase list: `FormattingPipeline.CreatePhases` (12 phases, in the FMT-PIPE-001 order).
- Block scopes: `SyntaxIndentationUtilities.GetIndentingScopeRange` covers `NamespaceDeclarationSyntax`, `BaseTypeDeclarationSyntax` (so enums and C# 14 extension blocks count), `BlockSyntax`, `SwitchStatementSyntax`, `AccessorListSyntax`, and a non-empty `SwitchSectionSyntax` (the statement range). Embedded statements come from `GetEmbeddedStatement`: if, else (not `else if`), while, do, for, `CommonForEachStatementSyntax`, using, lock, fixed. This is exactly the FMT-INDENT-004 list.
- Alignment contributors: `LayoutComputer.CreateContributors` (17, later ones win), then `CommentIndentationContributor` last. The FMT-INDENT-012 list kinds are the `ArgumentAlignmentContributor` switch cases. `BaseTypeListContributor` does not call `ListElementAligner.AlignContinuations` (see Q-I3).
- Directive column: `IndentationRewriter.IsNonRegionDirectiveTrivia` (active non-region directives). Region directives get `LayoutComputer.SetDirectiveIndentation`, active only.
- Raw strings: `RawStringAlignmentPhase` handles `MultiLineRawStringLiteralToken`, `Utf8MultiLineRawStringLiteralToken`, and `InterpolatedMultiLineRawStringStartToken`. It splits lines on `'\n'` only (see Q-R1).
- Cleanup: `CleanupPhase` works on `WhitespaceTrivia` and `EndOfLineTrivia` of tokens, plus every `DirectiveTriviaSyntax` interior, active or inactive.
- EOL: `LineEndingNormalizationPhase` rewrites `XmlTextLiteralNewLineToken` and `EndOfLineTrivia` with `descendIntoTrivia: true`. Detection is in `Reihitsu.Core.LineEndingUtilities.DetectEndOfLine`: a terminator of length ≥ 2 counts as `\r\n`, everything else as `\n`.

## Requirements against the code

"Holds" means the requirement states today's behavior and its example reproduces. Tests are starting points found by name and spot-reading, not a verified final set. `AssertRuleResult` (`Helpers/FormatterTestsBase`) runs the whole `FormattingPipeline` under LF and CRLF, two passes, but with the line ending passed in, no entry-point guards, and no line-ending detection. Paths are relative to `src/Reihitsu.Formatter.Test/` unless stated otherwise.

### formatter/pipeline.md

| ID | Against the code | Existing tests (file: method) |
|---|---|---|
| FMT-PIPE-001 | Holds | `Unit/Pipeline/FormattingPipelineTests.cs`: `ExecuteObserverReportsAllPhasesInOrderWithoutChangingOutput` (asserts the 12 names in order), `ExecutePhaseOrderStructuralBeforeIndentation` |
| FMT-PIPE-002 | Holds for the example. Regions in disabled code are renamed by the explicit allowance FMT-REGION-006 | No test runs the spec example through an entry point. Partial: `Unit/Cleanup/CleanupPhaseTests.cs`: `PreservesCommentTrivia`, `PreservesTabsInsideComments`; `Regression/FullPipeline/RawStringAlignmentFullPipelineTests.cs`: `ContentSemanticValueIsPreserved`; `Regression/Regions/DisabledRegionLayoutTests.cs`: `RegionInsideDisabledMethodBodyIsNotRemoved` |
| FMT-PIPE-003 | Holds except the tracked violation thoenissen/Reihitsu#948 (a braced switch section scopes a local another section uses). Tests must avoid that input | No test compiles before and after. Partial guard tests in other groups' areas (expression-body directive guards, switch `goto` guards) |
| FMT-PIPE-004 | Holds except the tracked violations thoenissen/Reihitsu#949 and thoenissen/Reihitsu#951 | `Idempotency/IdempotencyTests.cs` (all 19); `Idempotency/SelfHostingTests.cs`: `FormatterIsIdempotentOnAllSourceFiles`, `FormatterProducesNoChangesOnSourceFiles` |
| FMT-PIPE-005 | Holds. "Any error-level diagnostic" includes an active `#error` directive (CS1029) and an unmatched `#region`: such files come back unchanged | `Unit/ReihitsuFormatterTests.cs`: `FormatSyntaxTreeWithSyntaxErrorsReturnsOriginalTree`, `FormatDocumentAsyncWithSyntaxErrorsReturnsOriginalDocument`, `FormatSyntaxTreeWithUnmatchedRegionReturnsOriginalTree`, `FormatSyntaxTreeWithUnmatchedEndregionReturnsOriginalTree`; `Unit/ReihitsuFormatterHelpersTests.cs`: `HasSyntaxErrorsReturnsTrueWhenTreeContainsErrors`, `HasSyntaxErrorsReturnsFalseWhenTreeIsValid` |
| FMT-PIPE-006 | Holds. A `///` header comment counts. A marker in disabled text does not count (it is not comment trivia). A marker after `using System;` does not count | `Unit/ReihitsuFormatterTests.cs`: `FormatSyntaxTreeWithSingleLineAutoGeneratedMarkerReturnsOriginalTree`, `FormatSyntaxTreeWithMultiLineAutoGeneratedMarkerReturnsOriginalTree`, `FormatDocumentAsyncWithAutoGeneratedMarkerReturnsOriginalDocument`; `Unit/ReihitsuFormatterHelpersTests.cs`: `IsAutoGeneratedSource*` (7). The second example (marker inside the type is still formatted) has only helper-level tests (`IsAutoGeneratedSourceReturnsFalseWhenMarkerIsOnlyInBodyComment`, `…InBodyBlockComment`), none through an entry point |
| FMT-PIPE-007 | Holds for files. U+0085/U+2028/U+2029 count as `\n` (further instance of #1081, see below). Wrong for `FormatNode` on an attached node (Q-P1) | `Unit/ReihitsuFormatterTests.cs`: `FormatSyntaxTreeDetectsCarriageReturnLineFeed`, `FormatSyntaxTreeDetectsLineFeed`, `FormatSyntaxTreeNormalizesMixedLineEndingsToPredominantStyle`; `Unit/ReihitsuFormatterHelpersTests.cs`: `DetectEndOfLine*` (5, including the `Environment.NewLine` fallback). Tie → `\r\n`: none found by name |
| FMT-PIPE-008 | Holds (Roslyn 5.9.0: `Default` maps to C# 14) | `Unit/ReihitsuFormatterLanguageVersionTests.cs`: `FormatSyntaxTreeKeepsGetOnlyAccessorListBelowCSharp6`, `FormatSyntaxTreeConvertsGetOnlyPropertyInCSharp6` (the spec example), `FormatDocumentAsyncKeepsEmptyClassBracedInCSharp11Project`, `FormatNodeInDocument*InCSharp*Project`, `FormatNodeFormatsDetachedNodeForTheNewestSupportedVersion`; `Unit/LanguageVersionResolverTests.cs` (all 9) |
| FMT-PIPE-009 | Holds (`H1`: same text) | Equal output of `FormatDocumentAsync` and `FormatSyntaxTree`: none found. `src/Reihitsu.ArchitectureTests/Formatter/PublicApiSurfaceTests.cs`: `ReihitsuFormatterShouldBePublic`, `OnlyReihitsuFormatterShouldBePublicInFormatterAssembly` (surface only) |
| FMT-PIPE-010 | The example holds. Leading trivia is kept for a target that does not start its line or is a root child; trailing trivia is kept, trailing whitespace and comment included. Mid-line it is incomplete: a target that starts its line keeps its original column (Q-P2). `…WithContextAsync` keeps the target's leading trivia in every case | `Unit/ReihitsuFormatterTests.cs`: `FormatNodeInDocumentAsyncKeepsBlankLineChangesScopedToTargetNode`, `FormatNodeInDocumentAsyncKeepsUsingOrderingChangesScopedToTargetNode`, `FormatNodeInDocumentAsyncFormatsWholeDocumentWhenTargetIsRoot`, `FormatNodeInDocumentWithContextAsyncTracksTargetAcrossStructuralTransforms`; `Unit/ReihitsuFormatterNodeBlankLineTests.cs`: `Moves*DocumentationBehind*WithoutTrailingWhitespace`, `RemovesTabBehindPreviousMemberWhenSeparatingDelimitedDocumentation` (the whitespace exception), `KeepsSingleLineDocumentationBehindPreviousTopLevelType`, `KeepsWhitespaceInFrontOfDelimitedDocumentationBehindPreviousTopLevelType` (root child, by name) |
| FMT-PIPE-011 | Holds | `Unit/ReihitsuFormatterTests.cs`: `FormatNodeKeepsSingleStatementAccessorBlock`, `FormatNodeInDocumentAsyncKeepsSingleStatementAccessorBlock`, `FormatNodeInDocumentAsyncOnRootKeepsSingleStatementAccessorBlock`, `FormatNodeInDocumentWithContextAsyncKeepsSingleStatementAccessorBlock`, `FormatNodeKeepsGetOnlyAccessorList`, `FormatNodeInDocumentAsyncKeepsGetOnlyAccessorList`, `FormatNodeInDocumentAsyncOnRootKeepsGetOnlyAccessorList`, `FormatNodeInDocumentWithContextAsyncKeepsGetOnlyAccessorList`, `FormatNodeInDocumentAsyncKeepsExpressionBodiedIndexer`; counterparts `FormatSyntaxTreeConvertsSingleStatementAccessorToExpressionBody`, `FormatDocumentAsyncConvertsSingleStatementAccessorToExpressionBody`, `FormatDocumentAsyncConvertsGetOnlyPropertyToExpressionBodiedProperty` |
| FMT-PIPE-012 | Holds (`H3`: formats in a generated file and in a file with errors; `indentLevel: 2` gives 8 columns) | `Unit/ReihitsuFormatterTests.cs`: `FormatNodeIndentsStatementInsideSwitchSectionToSwitchSectionLevel` (level from position), `FormatNodeWithPreviousTokenOutsideTargetPreservesTrailingDocumentation` (`indentLevel: 1`). Guards skipped: none found |

Inputs the code decides that pipeline.md does not mention:

- `FormatNodeInDocumentAsync` and `FormatNodeInDocumentWithContextAsync` also return the document unchanged on syntax errors and on a generated header. FMT-PIPE-005/006 name only the document- and tree-level entry points, and FMT-PIPE-012 names only `FormatNode`. Tests exist: `FormatNodeInDocumentAsyncWithSyntaxErrorsReturnsOriginalDocument`, `FormatNodeInDocumentAsyncWithAutoGeneratedMarkerReturnsOriginalDocument`.
- `FormatNodeInDocumentWithContextAsync` returns the document unchanged when the target is not inside the context node or not in the document's tree (`H9`).

### formatter/indentation.md

| ID | Against the code | Existing tests |
|---|---|---|
| FMT-INDENT-001 | Holds | `Regression/Indentation/IndentationTests.cs`: `TabsReplacedWithSpaces`; `Regression/Spacing/TabGapSpacingTests.cs`: `TabIndentationIsStillExpandedToSpaces`; `Idempotency/IdempotencyTests.cs`: `TabNormalizationIsIdempotent` |
| FMT-INDENT-002 | Holds. A label at column 0 moves to the statement column (probe). Enum and extension-block braces also add a level (`BaseTypeDeclarationSyntax`) | `Regression/Indentation/IndentationTests.cs`: `BlockScopedNamespaceMembersOneLevel`, `FileScopedNamespaceNoExtraIndent`, `ClassMembersCorrectIndentation`, `MethodBodyCorrectIndentation`, `NestedBlocksCumulativeIndentation`, `PropertyAccessorCorrectIndentation`, `EnumMembersCorrectIndentation`, `WrongIndentationCorrected`; `Integration/IndentationPhaseIntegrationTests.cs`: `IndentsClassMembers`, `IndentsMethodBody`, `IndentsNamespaceMembers`; `Unit/Indentation/LayoutComputerTests.cs`: `ComputeHandles*`. Top-level statements: none found (`TopLevelCodeNoIndentation` holds only an already formatted class). Labeled statement moved to the statement column: none found (`Regression/LineBreaks/ContainedBlockBraceTests.cs`: `LabeledBlockBraceMovesWhenContentsAreReflowed` already has the label there) |
| FMT-INDENT-003 | Holds, braced section body included (probe: `{` at the statement column) | `IndentationTests.cs`: `SwitchStatementCorrectLevels`, `SwitchSectionStatementsIndented`, `SwitchCaseBlockBracesRemainUnchanged` (already formatted); `IndentationPhaseIntegrationTests.cs`: `IndentsSwitchStatement` |
| FMT-INDENT-004 | Holds | `IndentationTests.cs`: `Unbraced{While,For,ForEach,Using,Lock,Fixed}BodyStaysIndentedOneLevelDeeper`, `UnbracedWhileBodyWrongIndentationIsCorrected`, `UnbracedFixedBodyWrongIndentationIsCorrected`, `UnbracedNestedWhileBodyIndentsEachLevel`; `LayoutComputerTests.cs`: `ComputeHandlesUnbracedWhileBody`, `ComputeHandlesUnbracedFixedBody`, `ComputeHandlesUnbracedElseIfChainWithoutAccumulatingIndentation`; `IdempotencyTests.cs`: `UnbracedEmbeddedStatementIsIdempotent`. `do`, `if`, `else`: none found (FMT-STRUCT braces if/else). `src/Reihitsu.Core.Test/SyntaxIndentationUtilitiesTests.cs` covers `GetChildIndentLevel*` (embedded, else-if) |
| FMT-INDENT-005 | Holds | Trailing comment moved with its line: none found by name |
| FMT-INDENT-006 | Holds as worded. A comment before `]` takes the bracket's column (Q-I5) | `IndentationTests.cs`: `CommentsPreservedWithCorrectIndentation`, `CommentOnlyInScopePreservesIndentation` (already formatted); `Unit/Indentation/Contributors/CommentIndentationContributorTests.cs`: `AlignsCommentToFollowingTokenLayout`, `AlignsMultipleCommentsBeforeToken`, `AlignsCommentWhenOnlyElementInScope`, `DoesNotAlignInlineComment`, `DoesNotAlignDocumentationCommentBehindPrecedingToken`; `Regression/Indentation/InlineDocumentationCommentIndentationTests.cs`: `KeepsOpeningBraceColumnWithDelimitedDocumentationBehindIt` (second example) |
| FMT-INDENT-007 | Holds | `IndentationTests.cs`: `MultiLineCommentBeforeStatementIsReIndented` |
| FMT-INDENT-008 | Holds | `IndentationTests.cs`: `DocumentationCommentBeforeMemberIsReIndented`, `DocumentationCommentWithXmlSummaryBeforeMemberIsReIndented`, `DocumentationCommentBeforeFieldIsReIndented`, `DocumentationCommentContinuationLineAtColumnZeroIsRealigned`, `DocumentationCommentContinuationLineWithTabIsRealigned`, `DocumentationCommentBeforeMemberAcrossDirectiveIsReIndented` |
| FMT-INDENT-009 | Holds except thoenissen/Reihitsu#952 (a directive right after disabled text keeps its indentation; reproduced again with `#endif` in an argument list). Inactive directives keep their whitespace | `Regression/Indentation/ArgumentContinuationLineAlignmentTests.cs`: `ArgumentLeadingCommaAfterPragmaDirectiveIsAlignedWithTheArguments` (one indented `#pragma` moved to column 0). Nothing else found that moves an indented `#if`/`#else`/`#endif`/`#nullable`/`#define` |
| FMT-INDENT-010 | Holds for type, block, switch, enum, accessor list, and lambda body. Looks wrong inside initializers, collection expressions, switch expressions, and argument lists (Q-I1). "MUST NOT be touched" conflicts with FMT-REGION-006 and FMT-CLEAN-002 (Q-I6) | `Regression/Regions/RegionFormattingTests.cs`: `MisindentedRegionDirectivesAreAlignedWithContainingCode`. Switch-label column, and a misindented inactive region keeping its column: none found |
| FMT-INDENT-011 | Holds | `Unit/Indentation/IndentationRewriterTests.cs`: `ApplyPreservesStringLiterals` (single-line literal only). Verbatim continuation lines and disabled text: none found |
| FMT-INDENT-012 | Holds except an empty list whose `)` starts a line (Q-I4) | `Unit/Indentation/Contributors/ArgumentAlignmentContributorTests.cs`: `AlignsMultiLineArgumentsToOpenParen`, `AlignsMultiLineParametersToOpenParen`, `AlignsBracketedArgumentList`, `AlignsAttributeArguments`, `AlignsTupleArgumentElements`; `IndentationTests.cs`: `TupleArgumentElementsAlignVertically`; `Regression/Indentation/NestedListInParameterAlignmentTests.cs`: `WrappedTupleTypeElementInParameterIsMovedUnderTheFirstElement`, `WrappedUnmanagedCallingConventionListIsAlignedWithTheFirstConvention`; `ArgumentContinuationLineAlignmentTests.cs`: `*LeadingComma*`; `Regression/Indentation/ParameterContinuationLineAlignmentTests.cs`: `IndexerLeadingCommaStaysAligned`, `LeadingComma*` |
| FMT-INDENT-013 | Holds for 012 and 015 lists. Wrong for base lists (FMT-INDENT-016): `BaseTypeListContributor` never aligns element interiors (Q-I3) | `ArgumentContinuationLineAlignmentTests.cs`: `ArgumentInteriorLineHeldByLineCommentIsAlignedWithTheArgument`, `TupleElementInteriorLineHeldByLineCommentIsAlignedWithTheElement`, `TypeArgumentInteriorLineHeldByLineCommentIsAlignedWithTheTypeArgument`, `QueryClausesOfNamedArgumentAreAlignedWithTheQueryStart`, `HeldLineInsideSwitchExpressionArgumentIsLeftToTheSwitchExpression`, `HeldLineInsideObjectInitializerArgumentIsLeftToTheInitializer`, `MemberAttributeNamedValueHeldByLineCommentIsAlignedUnderTheName`; `ParameterContinuationLineAlignmentTests.cs`: `*StaysAligned` |
| FMT-INDENT-014 | Holds | `ArgumentAlignmentContributorTests.cs`: `AlignsMultiLineDictionaryIndexerKey`, `DoesNotAlignSingleLineDictionaryIndexerKey`; `Regression/Indentation/DictionaryIndexerKeyAlignmentTests.cs` (7); `ArgumentContinuationLineAlignmentTests.cs`: `DictionaryKeyLeadingCommaIsAlignedWithTheKeyBody` |
| FMT-INDENT-015 | Holds | `NestedListInParameterAlignmentTests.cs`: `WrappedAttributeListOfParameterIsAlignedWithTheFirstAttribute`, `WrappedTargetedAttributeListIsAlignedWithTheFirstAttribute`, `WrappedTupleTypeInTypeArgumentIsAlignedWithItsOwnFirstElement`; `ArgumentContinuationLineAlignmentTests.cs`: `TypeArgumentLeadingCommaStaysAlignedWithTheFirstTypeArgument`; `Regression/LineBreaks/WrappedAngleBracketListTests.cs`: `ActiveDirectiveBetweenTypeArgumentsRefusesJoinAndAligns`. Omitted type argument `<,>`: none found |
| FMT-INDENT-016 | Holds | `Unit/Indentation/Contributors/BaseTypeListContributorTests.cs`: `AlignsSubsequentBaseTypesToFirst`, `DoesNotAlignSingleBaseType`, `DoesNotAlignBaseTypesOnSameLine`; `IndentationTests.cs`: `BaseTypeContinuationRemainsAligned` |
| FMT-INDENT-017 | Holds | `Unit/Indentation/Contributors/ConstructorInitializerContributorTests.cs`: `AlignsBaseInitializerColonToConstructorPlusIndent`, `AlignsThisInitializerColonToConstructorPlusIndent`; `Unit/Indentation/Contributors/GenericConstraintContributorTests.cs`: `AlignsWhereKeywordToParentPlusIndent`, `AlignsMethodConstraintToMethodPlusIndent`. Full pipeline: none found |
| FMT-INDENT-018 | Holds | `Unit/Indentation/Contributors/BinaryExpressionContributorTests.cs`: `AlignsOperatorToLeftOperandColumn`, `SkipsNestedBinaryOfSameKind`, `AlignsNullCoalescingOperatorWithOneIndent`, `AlignsStringConcatenationPlusOperator`; `Regression/Indentation/LogicalExpressionAlignmentTests.cs`: `MultiLineAndAlignsOperators`, `MultiLineOrAlignsOperators`, `NestedMixedOperatorsAlignToLeftOperand`, `MultiLineNullCoalescingAlignsOperator`; `IndentationTests.cs`: `NullCoalescingOperatorIndentedOnceFromStatement`, `StringConcatenationWrappedPlusAlignedCorrectly`; `Regression/Indentation/StringConcatenationAlignmentTests.cs` (2) |
| FMT-INDENT-019 | Holds | `BinaryExpressionContributorTests.cs`: `AlignsIsKeywordToExpressionColumn`, `AlignsOrKeywordsInIsPatternExpression`; `LogicalExpressionAlignmentTests.cs`: `OrPatternInIsStatementAlignsToIsColumn`, `IsOrPatternInReturnStatementAlignsAllAlternatives`, `IsAndPatternInReturnStatementAlignsToIsColumn`, `PatternOrInsideStatementLambdaAlignsToPatternAnchor`; `IndentationTests.cs`: `MultilineOrPatternExpressionAlignsToIsKeyword`. Combinator outside an `is` expression: none found by name |
| FMT-INDENT-020 | Holds | `Unit/Indentation/Contributors/ConditionalExpressionContributorTests.cs`: `AlignsQuestionAndColonToConditionPlusIndent`, `AlignsNestedConditionalRelativeToParentOperator`; `LogicalExpressionAlignmentTests.cs`: `MultiLineTernaryAlignsQuestionAndColonTokens`, `NestedTernaryInObjectInitializerIndentsRelativeToParentOperator`; `IndentationTests.cs`: `NestedTernaryContinuationRemainsAligned`; `Regression/FullPipeline/ConditionalExpressionFullPipelineTests.cs` |
| FMT-INDENT-021 | Holds | `Unit/Indentation/Contributors/CaseWhenClauseContributorTests.cs`: `AlignsWrappedWhenKeywordToCasePlusIndent`. Full pipeline: none found |
| FMT-INDENT-022 | Holds | `Unit/Indentation/Contributors/ObjectInitializerContributorTests.cs`: `AlignsBracesToNewKeyword`, `IndentsMembersOneLevelFromNewKeyword`, `AlignsArrayCreationInitializer`, `AlignsWithExpressionInitializerBraces`, `IndentsWithExpressionMembersOneLevelFromWithKeyword`, `AlignsStackAllocInitializerBraces`, `AlignsImplicitStackAllocInitializerBraces`; `IndentationTests.cs`: `InitializerExpressionIndentsCorrectly`, `WithExpressionInitializerAlignsToWithKeyword`, `StackAllocInitializerAlignsToStackAllocKeyword`, `ImplicitStackAllocInitializerAlignsToStackAllocKeyword`; `Regression/Indentation/ObjectInitializerAlignmentTests.cs`: `MultiLineInitializerAlignsToNewKeyword`, `TargetTypedCollectionInitializerIsReformatted`, `NestedInitializerAlignsCorrectly` |
| FMT-INDENT-023 | Holds for the three named shapes. A nested array initializer (the rows of `int[,] a = { { … } }`) has no owner (Q-I2) | `ObjectInitializerContributorTests.cs`: `IndentsEqualsValueClauseInitializerMembersOneLevel`, `AlignsCollectionInitializerInsideObjectInitializer`; `IndentationTests.cs`: `EqualsValueClauseArrayInitializerIndentsMembersCorrectly`; `ObjectInitializerAlignmentTests.cs`: `CollectionInitializerInsideObjectInitializerIndentsCorrectly`; `Regression/Indentation/ComplexElementInitializerAlignmentTests.cs` (6) |
| FMT-INDENT-024 | Holds | `Unit/Indentation/Contributors/AnonymousObjectContributorTests.cs`: `AlignsBracesToNewKeyword`, `IndentsMembersOneLevelFromNewKeyword`; `IndentationTests.cs`: `AnonymousObjectIndentsCorrectly` |
| FMT-INDENT-025 | Holds. Leading commas are not aligned (non-goal fallback) | `Unit/Indentation/Contributors/CollectionExpressionContributorTests.cs`: `IndentsElementsOneLevelFromOpenBracket`, `AlignsCloseBracketToOpenBracket`; `Regression/Indentation/CollectionExpressionAlignmentTests.cs` (several); `Unit/Indentation/Contributors/ListPatternContributorTests.cs`: `IndentsPatternsOneLevelFromOpenBracket`, `AlignsToReIndentedBracketColumn`. List pattern through the full pipeline: none found |
| FMT-INDENT-026 | Holds | `Regression/Indentation/RecursivePatternAlignmentTests.cs`: `MultiLineIsPatternAlignsBracesToIsKeyword`, `NestedRecursivePatternIndentsFromContainingSubpattern`, `MultiLineNotPatternAlignsBracesToIsKeyword`, `OrPatternRecursivePatternsAlignToIsKeyword`, `ParenthesizedRecursivePatternBreaksParenthesisOntoOwnLine`; `Unit/Indentation/Contributors/RecursivePatternContributorTests.cs` (3). No unit test file for `ParenthesizedPatternContributor` |
| FMT-INDENT-027 | Holds | `Unit/Indentation/Contributors/SwitchExpressionContributorTests.cs`: `AlignsBracesToGoverningExpression`, `IndentsArmsOneLevelFromGoverningExpression`; `IndentationTests.cs`: `SwitchExpressionArmsRemainAlignedWithSwitchKeyword` (misnamed, thoenissen/Reihitsu#950), `SwitchExpressionWithoutLambdaBreaksAndAligns` |
| FMT-INDENT-028 | Holds as worded. With an explicit return type the body aligns under `(`, not under the lambda's start (Q-I7) | `Unit/Indentation/Contributors/LambdaAlignmentContributorTests.cs`: `ShiftsParenthesizedLambdaBlockToAnchor`, `ShiftsSimpleLambdaBlockToParameterAnchor`; `IndentationTests.cs`: `LambdaBlockBodyInArgumentListIndentsCorrectly`, `NestedAsyncLambdaInObjectInitializerIndentsCorrectly` (async anchor, by name) |
| FMT-INDENT-029 | Holds | `Regression/Indentation/MethodChainAlignmentTests.cs` (many); `Regression/Indentation/MethodChainDirectiveAlignmentTests.cs`: `DirectiveAboveFirstWrappedCallAlignsChainUnderRoot` (second example); `Unit/Indentation/Contributors/MethodChainAlignmentContributorTests.cs` |
| FMT-INDENT-030 | Holds | `MethodChainAlignmentTests.cs`: `ArgumentListUnderChainContinuationLineFollowsTheResolvedChainColumn`, `StatementLambdaUnderChainContinuationLineFollowsTheResolvedChainColumn`, `TernaryUnderChainContinuationLineFollowsTheResolvedChainColumn`; `ParameterContinuationLineAlignmentTests.cs`: `LambdaBlockInDefaultValueFollowsMovedParameter`, `RawStringDefaultValueFollowsMovedParameter`; `ListPatternContributorTests.cs`: `AlignsToReIndentedBracketColumn` |

Inputs the code decides that indentation.md does not mention (no question unless named):

- An anonymous method (`delegate { … }`) gets block indentation: `{` at the statement column, body one level deeper.
- The leading commas of collection expressions, switch-expression arms, base lists, and initializers have no owner and fall back to the block column (covered by the continuation non-goal and thoenissen/Reihitsu#953).
- Lines inside a multi-line interpolation hole of a verbatim interpolated string take the statement column. This is a further instance of #953 (see below).

### formatter/raw-string-alignment.md

| ID | Against the code | Existing tests |
|---|---|---|
| FMT-RAW-001 | Holds for `\n`/`\r\n` literals. Wrong when a literal's line breaks are lone `\r` (Q-R1) | `Unit/RawStringAlignment/RawStringAlignmentPhaseTests.cs`: `MisalignedNonInterpolatedRawStringIsCorrected`, `FourQuoteRawStringIsAlignedCorrectly`, `MisalignedInterpolatedRawStringIsCorrected`, `InterpolatedRawStringWithMultipleHolesIsAligned`; `Regression/FullPipeline/RawStringAlignmentFullPipelineTests.cs`: `MisalignedRawStringInMethodBodyIsCorrected`, `MisalignedInterpolatedRawStringIsCorrected` |
| FMT-RAW-002 | Holds. Whitespace-only content lines are shifted like content (value-preserving; only empty lines are exempt) | `RawStringAlignmentPhaseTests.cs`: `MultipleContentLinesAreAdjustedUniformly`, `LeftwardShiftIsHandledCorrectly`; `RawStringAlignmentFullPipelineTests.cs`: `RelativeIndentationWithinRawStringIsPreserved`, `EmptyLinesInRawStringArePreserved`, `ContentSemanticValueIsPreserved` |
| FMT-RAW-003 | Holds | `RawStringAlignmentPhaseTests.cs`: `DoubleDollarInterpolatedRawStringIsAlignedCorrectly` |
| FMT-RAW-004 | Holds | `RawStringAlignmentPhaseTests.cs`: `MisalignedUtf8RawStringIsCorrected`, `AlreadyAlignedUtf8RawStringIsNotModified` |
| FMT-RAW-005 | Holds | `RawStringAlignmentFullPipelineTests.cs`: `RawStringInNestedBlockIsAlignedCorrectly`, `RawStringAsMethodArgumentIsAlignedCorrectly`, `BadlyIndentedCodeAndRawStringAreBothCorrected`, `RawStringAsArgumentWithMisalignedContentIsCorrectedWithoutDamage`; `ParameterContinuationLineAlignmentTests.cs`: `RawStringDefaultValueFollowsMovedParameter`; `Regression/LineBreaks/ChainLinkLayoutTests.cs`: `ChainOnMisindentedRawStringRootAlignsInOnePass`. A literal joined onto its `=` line, through the full pipeline: none found (`Unit/LineBreaks/LineBreakRewriterTests.cs`: `CollapsesRawMultilineStringAssignmentToEqualsLine` covers only the join) |
| FMT-RAW-006 | Holds | `RawStringAlignmentPhaseTests.cs`: `AlreadyAlignedNonInterpolatedRawStringIsNotModified`, `SingleLineRawStringIsNotModified`, `AlreadyAlignedInterpolatedRawStringIsNotModified`; `RawStringAlignmentFullPipelineTests.cs`: `AlreadyAlignedRawStringIsNotModified`, `AlreadyAlignedInterpolatedRawStringIsNotModified` |

A `\r\n` raw literal in a `\n` file is shifted and keeps its `\r\n` inside (probe). FMT-EOL-004 therefore also holds after alignment. Tracked open bugs in this area: thoenissen/Reihitsu#951 (holes, nested literals, tabs).

### formatter/cleanup.md

| ID | Against the code | Existing tests |
|---|---|---|
| FMT-CLEAN-001 | Holds | `Unit/Cleanup/CleanupPhaseTests.cs`: `RemovesTrailingWhitespaceBeforeEndOfLine`, `RemovesWhitespaceBetweenEndOfLines`, `HandlesMultipleCleanupIssuesInOneFile`; `Regression/Cleanup/TrailingTriviaCleanupTests.cs`: `TrailingWhitespaceRemoved`, `WhitespaceBetweenEOLsRemoved`. The second clause (line break in the next token's leading trivia, `EndOfLineWhitespace.StripBeforeFollowingLineBreak`) at document level: none found |
| FMT-CLEAN-002 | Holds (`int w; → // f` gives 6 spaces) | `CleanupPhaseTests.cs`: `ReplacesTabsInWhitespaceTriviaWithSpaces` (one tab). Column independence: none found |
| FMT-CLEAN-003 | Holds. The tab between `#region` and its name becomes four spaces (it is token whitespace). A tab inside the name and inside a `#warning` message is kept | `CleanupPhaseTests.cs`: `ReplacesTabsInsideDirectiveInteriorWithSpaces`. Description/message kept: none found |
| FMT-CLEAN-004 | Holds | `CleanupPhaseTests.cs`: `PreservesTabsInsideComments`, `PreservesCommentTrivia`. Literals and disabled text: none found |
| FMT-CLEAN-005 | Holds, including a whitespace-only last line with no final break (`}⏎···` → `}`). `}···` with no final break is thoenissen/Reihitsu#960 | `CleanupPhaseTests.cs`: `RemovesTrailingEndOfLineAtEndOfFile`, `RemovesTrailingWhitespaceOnlyBlankLineAtEndOfFile`; `TrailingTriviaCleanupTests.cs`: `FileWithSingleTrailingNewlineStripsIt`, `FileWithMultipleTrailingNewlinesStripsAll`, `FileWithoutTrailingNewlineRemainsUnchanged` |
| FMT-CLEAN-006 | Holds | `Unit/Cleanup/CleanupPhaseTrailingDirectiveAtEndOfFileTests.cs`: `TrailingCommentAtEndOfFileKeepsSeparateLine{CrLf,Lf}`, `TrailingBlockCommentAtEndOfFileKeepsSeparateLine{CrLf,Lf}` |
| FMT-CLEAN-007 | Holds (current behavior; change decided in thoenissen/Reihitsu#1082) | `CleanupPhaseTrailingDirectiveAtEndOfFileTests.cs`: `TrailingPragmaAtEndOfFileKeepsSeparateLine*`, `NonPragmaDirectiveAtEndOfFileKeepsSeparateLine*`, `DisabledTextAtEndOfFileKeepsSeparateLine*`, `BlankLineBeforeTrailingPragmaAtEndOfFileIsPreserved*`, `DirectiveOnlyFileIsUnchanged`; `Unit/ReihitsuFormatterTrailingDirectiveAtEndOfFileTests.cs`: `FormatSyntaxTreeKeepsTrailingPragmaOnOwnLineAtEndOfFile`; `Regression/Cleanup/TrailingPragmaAtEndOfFileTests.cs` (2) |
| FMT-CLEAN-008 | Holds | `CleanupPhaseTrailingDirectiveAtEndOfFileTests.cs`: `DetachedNodeTrailingEndOfLineIsStillRemoved` (phase level) |
| FMT-CLEAN-009 | Wrong when the BOM is directly followed by a line break (Q-C1) | `TrailingTriviaCleanupTests.cs`: `BomPreserved` (BOM followed by `class`) |

Not mentioned by cleanup.md: `FormatNode` on an attached node also loses the node's trailing line break (`H4b`, `H4c`: `    int x;⏎` → `    int x;`). FMT-CLEAN-008 names only detached nodes.

### formatter/line-endings.md

| ID | Against the code | Existing tests |
|---|---|---|
| FMT-EOL-001 | Holds (`\r\n`, lone `\r`, U+0085, U+2028, U+2029) | `Unit/ReihitsuFormatterTests.cs`: `FormatSyntaxTreeNormalizesMixedLineEndingsToPredominantStyle` (CRLF → LF only). Lone `\r` and Unicode terminators between tokens: none found (`Unit/DocumentationComments/DocumentationCommentFormattingPhaseTests.cs`: `NormalizesEveryDocumentationLineTerminator` is the doc phase, which keeps them) |
| FMT-EOL-002 | Holds | None found |
| FMT-EOL-003 | Holds for `///` and `/** */` (probe: CRLF inside `/** */` in an LF file normalized) | `Unit/ReihitsuFormatterTests.cs`: `FormatSyntaxTreeNormalizesDocumentationNewlinesToPredominantStyle`, `FormatSyntaxTreePreservesDocumentationOnlyLineEndings` (single-line form). `/** */`: none found |
| FMT-EOL-004 | Holds, also for a literal the raw phase shifted | None found |

### playground/playground.md

| ID | Against the code | Existing tests (`src/Reihitsu.Playground.Test/PlaygroundFormatterTests.cs`) |
|---|---|---|
| PLAY-001 | Holds (`Pages/Home.razor`; the sentence spans two source lines, so the rendered text matches; no HTTP use) | None found |
| PLAY-002 | Holds (`textarea` with `spellcheck="false"`, the output also `readonly`) | None found |
| PLAY-003 | Holds (`Home.ExampleSource`, `OnInitialized` → `FormatInput`; output equals the spec block) | None found (`ExampleSource` and `FormatInput` are private) |
| PLAY-004 | Holds (`@bind:event="oninput"` updates only `_input`) | None found |
| PLAY-005 | Holds (`CSharpSyntaxTree.ParseText(source)`, then `FormatSyntaxTree`) | `FormatReformatsMisindentedInput`, `FormatIsIdempotentForAlreadyFormattedInput` (partial: neither compares with `FormatSyntaxTree` nor pins the language version) |
| PLAY-006 | Holds. An active `#error` directive also gets the "syntax errors" message (CS1029 is error-level) | `FormatLeavesSyntacticallyInvalidInputUnchanged` (output and `Formatted`; the message text is set in `Home.FormatInput`: none found) |
| PLAY-007 | Holds (`MainLayout.razor`, `PlaygroundVersion.FormatterVersion`) | None found |

Fact for the test implementer: `Reihitsu.Playground.Test` references only MSTest (no component-test library). `Home`'s members are private, and the texts live in `.razor` markup. PLAY-001/002/003/004/007 and the PLAY-006 message have no test seam today.

## Open questions to file

Each is self-contained and goes to the named issue. Notation: `⏎` line break, `␍` `\r`, `→` tab, `·` space. Ranked by how wrong the output looks.

### `SDD open questions: sdd/specs/formatter/indentation.md`

**Q-I1: Region directives inside an initializer, a collection expression, a switch expression, or an argument list take the statement column.** Touches FMT-INDENT-010.

Input:

```csharp
class P
{
    public int X { get; set; }
}

class C
{
    void M()
    {
        var p = new P
                {
                    #region X
                    X = 1,
                    #endregion
                };
    }
}
```

Real output (idempotent):

```csharp
        var p = new P
                {
        #region X

                    X = 1,

        #endregion // X
                };
```

The same happens to `#region` between collection-expression elements (`int[] a = [` … at column 8 while elements are at 22), between switch-expression arms (column 8, arms at 19), and between arguments (`N(1,⏎#region R⏎2);` → `#region R` at 8, `2` at 10). Region directives inside a lambda body, an enum, an accessor list, and a switch section are placed correctly.

Why it looks wrong: FMT-INDENT-010's `Why:` says a region takes the column of the structure it groups, so it does not look as if it belonged to an outer scope. `SetDirectiveIndentation` uses block indentation only, and initializers, collection expressions, switch expressions, and argument lists are not block scopes, so the region gets the enclosing statement's column. The requirement's wording ("the block indentation of the position it is in") literally gives that column, but its examples name only type, block, and switch-section positions.

**Q-I2: The rows of a nested array initializer have no alignment owner.** Touches FMT-INDENT-023.

Input:

```csharp
class C
{
    void M()
    {
        int[,] a = {
                       {
                       1, 2
                       },
                       {
                       3, 4
                       }
                   };
    }
}
```

Real output (idempotent):

```csharp
        int[,] a = {
                       {
        1, 2
        },
                       {
        3, 4
        }
                   };
```

Why it looks wrong: the inner `{` is aligned as a member of the outer initializer, but its content and its `}` fall to the statement column. `ObjectInitializerContributor` handles an initializer whose parent is an assignment, an equals-value clause, or a creation keyword, plus the complex element initializer. An `ArrayInitializerExpression` nested in another initializer matches none of them. FMT-INDENT-023 names the declaration's array initializer and the complex element initializer. The nested row is neither, and the continuation non-goal does not describe it. (The planned #953 rule would put the row at the statement column + 4, which is still far from the braces at 23.)

**Q-I3: FMT-INDENT-013 claims base-list elements, but their interior lines are not aligned.** Touches FMT-INDENT-013, FMT-INDENT-016.

Input:

```csharp
class C : System.IDisposable,
          System. // c
    ICloneable
{
    public void Dispose()
    {
    }

    public object Clone()
    {
        return null;
    }
}
```

Real output: `ICloneable` moves to column 0. The other lines are unchanged.

Why it looks wrong: FMT-INDENT-013 says a line inside an element of a list from FMT-INDENT-016 aligns with the element's first token (column 10 here). `BaseTypeListContributor` never calls `ListElementAligner.AlignContinuations`, unlike the FMT-INDENT-012 and FMT-INDENT-015 contributors. The same input in an argument list (`N(1, System. // c⏎String.Empty)`) aligns `String` under `System`. This is a requirement wrong about current behavior: either FMT-INDENT-013 drops FMT-INDENT-016 from its list, or the code is wrong.

**Q-I4: The closing delimiter of an empty wrapped list is not aligned.** Touches FMT-INDENT-012.

Input: `M( // c⏎              );` inside a method body. Real output: `)` at column 8 (the statement column), not at column 10 directly after `(`.

Why it looks wrong: FMT-INDENT-012 requires the closing delimiter that starts a line to sit directly after the opening delimiter whenever the list spans several lines. `ArgumentAlignmentContributor.AlignToOpenToken` returns early when the list has no elements. Edge case: it only appears when a comment holds the break.

**Q-I5: A comment before a closing `]` takes the bracket's column, while one before a closing `}` takes the content column.** Touches FMT-INDENT-006 (FMT-INDENT-025 for context).

Input:

```csharp
        int[] a = [
                      1,
                      2
            // last
                  ];
```

Real output: `// last` at column 18 (the `]` column), with the elements at 22. The same comment before the `}` of an object initializer or a switch expression lands on the member/arm column (20 and 19).

Why it looks wrong: FMT-INDENT-006 adds one level only for a closing brace. Its `Why:` ("A comment at the end of a scope describes the scope's contents, not the brace") applies just as much to a collection expression or list pattern, which FMT-INDENT-025 lays out like an initializer block. The current behavior is what the requirement literally says. Two readings: (a) intended, keep as is; (b) a comment before `]` of a multi-line collection expression or list pattern is indented one level deeper than the bracket, like before `}`.

**Q-I6: "A region directive in disabled text MUST NOT be touched" conflicts with FMT-REGION-006 and FMT-CLEAN-002.** Touches FMT-INDENT-010, FMT-CLEAN-002, FMT-CLEAN-003, FMT-REGION-006.

Input:

```
class C
{
#if NEVER
→#region Inner
→disabled
→#endregion
#endif
}
```

Real output:

```
class C
{
#if NEVER
····#region Inner
→disabled
····#endregion // Inner
#endif
}
```

The indentation phase leaves the inactive directives' whitespace alone, as FMT-INDENT-010 says. Cleanup still turns the tab in front of them into four spaces, because the tab is whitespace trivia (FMT-CLEAN-002). The region phase adds `// Inner` (FMT-REGION-006). The same tab replacement happens between the tokens of an inactive `#if→INNER` (FMT-CLEAN-003).

Why it looks wrong: the indentation code says it leaves a directive in a skipped branch "where the author wrote it", because the code around it is untouched disabled text. Two other phases rewrite that same directive. Read formatter-wide, FMT-INDENT-010's last sentence is false today. Readings: (a) the sentence means "not re-indented", and FMT-INDENT-010 should say so; (b) the whitespace in front of and inside an inactive directive is part of the disabled code and Cleanup must keep its tabs.

**Q-I7: A lambda with an explicit return type aligns its body under `(`, not under the lambda's start.** Touches FMT-INDENT-028.

Input:

```csharp
        Func<int, int> a = int (x) =>
        {
        return x;
        };
```

Real output: `{` at column 31 (under `(`), not at column 27 (under `int`).

Why it looks wrong: the `Why:` says the body aligns with the lambda's own start, but `LambdaAlignmentContributor` anchors to `ParameterList.OpenParenToken`. The return type and any attribute list are skipped. The requirement's anchor sentence matches the code; the case is not named. Readings: (a) intended, the anchor is always `(` or the parameter; (b) the first token of the lambda (attribute list, return type, `static`, `async`).

### `SDD open questions: sdd/specs/formatter/cleanup.md`

**Q-C1: The byte order mark is lost when a line break follows it.** Touches FMT-CLEAN-009 (FMT-BLANK for the leading blank line).

Input (`FormatSyntaxTree`, text built as a string): `﻿⏎class C{int x;}`. Real output: `class C⏎{⏎    int x;⏎}`, with no BOM. Also lost for `﻿⏎⏎class…` and `﻿→⏎class…`. Kept for `﻿class…`, `﻿···class…`, `﻿// c⏎…`, `﻿#nullable enable⏎…`, and `﻿`.

Why it looks wrong: FMT-CLEAN-009 requires the BOM at the start of the text to be kept. Roslyn files U+FEFF as whitespace trivia, and removing the blank or whitespace line that follows it removes the BOM too. Low practical reach: the CLI decodes files with their encoding before formatting, so the BOM never reaches the formatter there. It does reach API callers that pass a string.

### `SDD open questions: sdd/specs/formatter/raw-string-alignment.md`

**Q-R1: A raw string whose line breaks are lone `\r` is never aligned.** Touches FMT-RAW-001 (FMT-EOL-001 treats a lone `\r` as a line break).

Input (an `\n` file, the literal's two line breaks are lone `\r`): `        var a = """␍    Hello␍    """;`. Real output: unchanged; the closing marker stays at column 4.

Why it looks wrong: FMT-RAW-001 covers every multi-line raw string literal, and the compiler treats `\r` as a line break. `RawStringAlignmentPhase` looks for `'\n'` only. The non-interpolated path returns early, and the interpolated path finds a single line. The same applies to U+0085/U+2028/U+2029 inside a literal. Edge case.

### `SDD open questions: sdd/specs/formatter/pipeline.md`

**Q-P1 (Decided by principle P24): `FormatNode` detects the line ending from the node, not from the file.** Touches FMT-PIPE-007, SHARED-FIX-008 (planned in thoenissen/Reihitsu#1138).

Input: a `\r\n` file `class C␍⏎{␍⏎    object M() { return new System.Action(() => { M(); }); }␍⏎}␍⏎`, then `FormatNode` on the lambda. Real output: `() =>⏎        {⏎            M();⏎        }`. The lambda has no line break of its own, so `DetectEndOfLine(node)` falls back to `Environment.NewLine` (`\n` here) inside a `\r\n` document.

Why it is wrong: FMT-PIPE-007 says the file's predominant line ending is detected. P24 says a fix writes only the document's line ending. `FormatNodeInDocumentAsync` detects from the root and is not affected. The only shipped caller of `FormatNode` is the RH5401 code fix.

**Q-P2 (Decided by principle P10, thoenissen/Reihitsu#1112 Q1): node-level formatting keeps a misindented target at its original column.** Touches FMT-PIPE-010.

Input (`FormatNodeInDocumentAsync` on the second local declaration):

```csharp
class C
{
    void M()
    {
        var a = 1;


              var b=1;
    }
}
```

Real output: `              var b = 1;` stays at column 14 (spacing fixed, blank lines collapsed to one). Document formatting puts it at column 8. A misindented root child (`      class B{int x;}`) also keeps column 6, with its body laid out relative to it. `AdjustNodeIndentation` shifts the formatted target back by `originalColumn - formattedColumn`.

Why it is wrong: under P10, the code a fix changes is laid out as document formatting lays it out, and the #1112 decision adds that sentence to FMT-PIPE-010. Formatting this result again at document level moves the target. A candidate for a one-line comment on thoenissen/Reihitsu#1144 instead, if the lead prefers.

## Further instances of existing follow-ups (one-line comments, not new questions)

- thoenissen/Reihitsu#1081, item 2 (lone `\r` counts as `\n`). U+0085, U+2028, and U+2029 also count as `\n`: `class C {     int x; }\r\n` (three U+2028, one `\r\n`) comes out all `\n`.
- thoenissen/Reihitsu#953 (one continuation rule). In a verbatim interpolated string `$@"line one⏎      {⏎   x⏎}"`, the hole tokens `x` and `}` take the statement column (8).
- thoenissen/Reihitsu#952, already known: `N(1,⏎#if !NEVER⏎2⏎#else⏎3⏎          #endif⏎);` keeps `#endif` at column 10. Tests for FMT-INDENT-009 must not put a directive right after disabled text.

## Breaking inputs

The tests are for existing behavior, so these are the inputs a tagged or new test must include, or must avoid because the behavior is disputed:

- FC-019/FC-020 for FMT-INDENT-006/009/010 and FMT-CLEAN-003: directives of every kind (`#if/#elif/#else/#endif`, `#pragma`, `#nullable`, `#define`, `#region`) indented in the input, active and inactive. Avoid a directive right after disabled text (#952), and region positions inside initializers, collection expressions, switch expressions, and argument lists (Q-I1).
- FC-017/FC-010: every new test under LF and CRLF, two passes (`AssertRuleResult` does both). Line-ending tests (FMT-PIPE-007, FMT-EOL-*) must go through `FormatSyntaxTree` or `FormatDocumentAsync` (detection) and not `AssertRuleResult`, which passes the line ending in. The same holds for FMT-PIPE-005/006, because `AssertRuleResult` skips the guards.
- FC-022: raw, interpolated raw, UTF-8 raw, verbatim, and interpolated verbatim literals for FMT-EOL-004, FMT-INDENT-011, FMT-CLEAN-004. Avoid multi-line holes, nested literals, and tab indentation (#951), and lone-`\r` literals (Q-R1).
- FC-027: initializer shapes for FMT-INDENT-022/023 (object, collection, array, implicit array, target-typed `new`, `with`, `stackalloc`, nested `Child = {`, complex element). Avoid nested array rows (Q-I2).
- FC-033: the Playground's CRLF behavior cannot be observed through the browser (`textarea` normalizes to LF); test `PlaygroundFormatter.Format` directly.
- FMT-PIPE-003/004: avoid #948 and #949 inputs.
- FMT-CLEAN-007: tests describe today's behavior. #1082 will change it, so a test pinned here is expected to flip there.

## Surfaces

| Surface | Must change / must stay / not involved |
|---|---|
| `Reihitsu.Formatter` production code | Must stay (lane: tests only) |
| `src/Reihitsu.Formatter.Test` | Must change: tags, focused tests where "none found" above |
| `src/Reihitsu.Playground.Test` | Must change: PLAY tags. PLAY-001/002/003/004/007 have no test seam today (see Playground facts) |
| `src/Reihitsu.Core.Test` | Not involved by the lane. `SyntaxIndentationUtilitiesTests` covers FMT-INDENT-002/003/004 mechanics at Core level, if the lead wants cross-project tags |
| Analyzer, code fixes, rule docs | Not involved |
| Specs under `sdd/specs/` | Must stay (no spec edits). The questions above go to the issues |
| `RequirementCoverageTests.EnforcedSpecPaths` | Must change at the end (per `00-lead.md`) |

## Simpler option

_None._

## Questions for the maintainer

_None blocking._ The items under *Open questions to file* go to the per-spec issues, which the lead files.

## Out of scope, for the lead to route

- Group B (`formatter/horizontal-spacing.md`): single-line array initializers get a space before `}` but none after `{`. `var d = new int[] {1,2};` → `new int[] {1, 2 };`, and `int[] e = {1,2};` → `{1, 2 };` (`{ 1, 2 }` input stays). It looks asymmetric. Not checked against FMT-SPACE.
- Stale test comment (FC-031): `Unit/Indentation/Contributors/ObjectInitializerContributorTests.cs`: `AlignsCollectionInitializerInsideObjectInitializer` says it "documents bug: contributor does not handle standalone InitializerExpressionSyntax", but asserts that entries are produced.
- Stale non-goal wording (no edit in this run): these spec texts still call questions undecided that are now decided: cleanup.md (#959, #960), line-endings.md (#959), indentation.md (#951, #953), raw-string-alignment.md (#951), playground.md (#976).
