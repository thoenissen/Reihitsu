# Beta Code Review — Coverage Checklist

Generated 2026-06-10. Tick a file only after it has been fully reviewed against the criteria in criteria.md.

## Reihitsu.Core (19 files)

### Reihitsu.Core

- [x] Reihitsu.Core\AccessorOrderingUtilities.cs
- [x] Reihitsu.Core\AttributeTargetUtilities.cs
- [x] Reihitsu.Core\CasingUtilities.cs
- [x] Reihitsu.Core\DeclarationModifierUtilities.cs
- [x] Reihitsu.Core\EmptyTypeDeclarationSemicolonAnalysisUtilities.cs
### Reihitsu.Core\Enumerations

- [x] Reihitsu.Core\Enumerations\OrderingAccessibilityGroup.cs
- [x] Reihitsu.Core\Enumerations\OrderingMemberKindGroup.cs
- [x] Reihitsu.Core\Enumerations\TargetAttributeListShapeMode.cs
- [x] Reihitsu.Core\Enumerations\TargetAttributePlacementMode.cs
- [x] Reihitsu.Core\Enumerations\UsingDirectiveOrderingGroup.cs
### Reihitsu.Core

- [x] Reihitsu.Core\FormattingTextAnalysisUtilities.cs
- [x] Reihitsu.Core\ModifierOrderingUtilities.cs
- [x] Reihitsu.Core\OrderingDeclarationUtilities.cs
- [x] Reihitsu.Core\RegionDirectiveUtilities.cs
- [x] Reihitsu.Core\StringInterpolationUtilities.cs
- [x] Reihitsu.Core\SyntaxNodeUtilities.cs
- [x] Reihitsu.Core\SyntaxTreeRegionSearcher.cs
- [x] Reihitsu.Core\SyntaxTriviaUtilities.cs
- [x] Reihitsu.Core\UsingDirectiveOrderingUtilities.cs

## Reihitsu.Cli (24 files)

### Reihitsu.Cli\Abstractions

- [x] Reihitsu.Cli\Abstractions\DefaultConsoleOutput.cs
- [x] Reihitsu.Cli\Abstractions\DefaultDiffGenerator.cs
- [x] Reihitsu.Cli\Abstractions\DefaultFileSystem.cs
- [x] Reihitsu.Cli\Abstractions\DefaultSourceFormatter.cs
- [x] Reihitsu.Cli\Abstractions\IConsoleOutput.cs
- [x] Reihitsu.Cli\Abstractions\IDiffGenerator.cs
- [x] Reihitsu.Cli\Abstractions\IFileSystem.cs
- [x] Reihitsu.Cli\Abstractions\ISourceFormatter.cs
### Reihitsu.Cli\Diff

- [x] Reihitsu.Cli\Diff\DiffHunk.cs
- [x] Reihitsu.Cli\Diff\EditOperation.cs
- [x] Reihitsu.Cli\Diff\EditScriptBuilder.cs
- [x] Reihitsu.Cli\Diff\HunkBuilder.cs
- [x] Reihitsu.Cli\Diff\LcsComputer.cs
- [x] Reihitsu.Cli\Diff\LineSplitter.cs
### Reihitsu.Cli

- [x] Reihitsu.Cli\DiffGenerator.cs
### Reihitsu.Cli\Enumerations

- [x] Reihitsu.Cli\Enumerations\EditKind.cs
### Reihitsu.Cli

- [x] Reihitsu.Cli\ExitCodes.cs
- [x] Reihitsu.Cli\FileProcessResult.cs
- [x] Reihitsu.Cli\FormatCommandDependencies.cs
- [x] Reihitsu.Cli\FormatCommandHandler.cs
- [x] Reihitsu.Cli\ParseResult.cs
- [x] Reihitsu.Cli\Program.cs
### Reihitsu.Cli\Properties

- [x] Reihitsu.Cli\Properties\AssemblyInfo.cs
- [x] Reihitsu.Cli\Properties\GlobalUsings.cs

## Reihitsu.Formatter (101 files)

### Reihitsu.Formatter\Enumerations

- [x] Reihitsu.Formatter\Enumerations\ExpressionBodyStatementForm.cs
### Reihitsu.Formatter

- [x] Reihitsu.Formatter\FormattingContext.cs
### Reihitsu.Formatter\Pipeline\BlankLines

- [x] Reihitsu.Formatter\Pipeline\BlankLines\BlankLineBreakSpacingRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\BlankLines\BlankLineCollapser.cs
- [x] Reihitsu.Formatter\Pipeline\BlankLines\BlankLineEditor.cs
- [x] Reihitsu.Formatter\Pipeline\BlankLines\BlankLinePhase.cs
- [x] Reihitsu.Formatter\Pipeline\BlankLines\BlankLineStatementSpacingRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\BlankLines\BlankLineTokenCleanupRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\BlankLines\BlankLineTriviaBoundaryRewriter.cs
### Reihitsu.Formatter\Pipeline\Cleanup

- [x] Reihitsu.Formatter\Pipeline\Cleanup\CleanupPhase.cs
### Reihitsu.Formatter\Pipeline\DocumentationComments

- [x] Reihitsu.Formatter\Pipeline\DocumentationComments\DocCommentElementNormalizer.cs
- [x] Reihitsu.Formatter\Pipeline\DocumentationComments\DocumentationCommentFormattingPhase.cs
### Reihitsu.Formatter\Pipeline

- [x] Reihitsu.Formatter\Pipeline\FormattingPipeline.cs
### Reihitsu.Formatter\Pipeline\HorizontalSpacing

- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\AttributeListCloseBracketSpacingRule.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\CommaSpacingRule.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\ForLoopSemicolonSpacingRule.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\HorizontalSpacingPhase.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\HorizontalSpacingRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\ISpacingRule.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\KeywordSpacingRule.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\NoSpaceSpacingRule.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\OperatorSpacingRule.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\SingleSpaceSpacingRule.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\SpacingPolicy.cs
- [x] Reihitsu.Formatter\Pipeline\HorizontalSpacing\TrailingWhitespaceWriter.cs
### Reihitsu.Formatter\Pipeline

- [x] Reihitsu.Formatter\Pipeline\IFormattingPhase.cs
### Reihitsu.Formatter\Pipeline\Indentation\Contributors

- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\AnonymousObjectContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\ArgumentAlignmentContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\BaseTypeListContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\BinaryExpressionContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\CollectionExpressionContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\CommentIndentationContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\ConditionalExpressionContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\ConstructorInitializerContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\GenericConstraintContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\ILayoutContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\LambdaAlignmentContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\ListPatternContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\MethodChainAlignmentContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\ObjectInitializerContributor.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\Contributors\SwitchExpressionContributor.cs
### Reihitsu.Formatter\Pipeline\Indentation

- [x] Reihitsu.Formatter\Pipeline\Indentation\IndentationPhase.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\IndentationRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\LayoutComputer.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\LayoutModel.cs
- [x] Reihitsu.Formatter\Pipeline\Indentation\TokenLayout.cs
### Reihitsu.Formatter\Pipeline\LineBreaks

- [x] Reihitsu.Formatter\Pipeline\LineBreaks\AttributeTargetFormattingRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\AttributeTargetFormattingShared.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\BinaryOperatorLineBreakRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\BracePlacer.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\ChainLineBreakRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\ChainWalker.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\DeclarationBraceLineBreakRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\GenericConstraintLineBreakRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\LineBreakAssignmentRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\LineBreakBlockRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\LineBreakContainedBlockRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\LineBreakDetection.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\LineBreakInitializerRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\LineBreakListRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\LineBreakPhase.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\LineBreakTriviaUtilities.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\PropertyLayoutLineBreakRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\TernaryLineBreakRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\TokenGapNormalizer.cs
- [x] Reihitsu.Formatter\Pipeline\LineBreaks\TokenLocator.cs
### Reihitsu.Formatter\Pipeline\LineEndings

- [x] Reihitsu.Formatter\Pipeline\LineEndings\LineEndingNormalizationPhase.cs
### Reihitsu.Formatter\Pipeline\RawStringAlignment

- [x] Reihitsu.Formatter\Pipeline\RawStringAlignment\RawStringAlignmentPhase.cs
### Reihitsu.Formatter\Pipeline\RegionFormatting

- [x] Reihitsu.Formatter\Pipeline\RegionFormatting\NestedRegionRemovalStep.cs
- [x] Reihitsu.Formatter\Pipeline\RegionFormatting\RegionFormattingPhase.cs
- [x] Reihitsu.Formatter\Pipeline\RegionFormatting\RegionNamingRewriter.cs
### Reihitsu.Formatter\Pipeline\StructuralTransforms

- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ControlFlowBraceTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\EmptyTypeDeclarationSemicolonTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\EnumTrailingCommaRemovalTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ExpressionBodiedConstructorTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ExpressionBodiedConversionTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ExpressionBodiedFinalizerTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ExpressionBodiedIndexerTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ExpressionBodiedLocalFunctionTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ExpressionBodiedMethodTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ExpressionBodiedOperatorTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ExpressionBodiedTransformUtilities.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\ExpressionBodyToBlockConverter.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\FieldDeclarationSplitTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\InitializerTrailingCommaRemovalTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\InterpolationMarkerRemovalTransform.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\StructuralTransformPhase.cs
- [x] Reihitsu.Formatter\Pipeline\StructuralTransforms\TrailingCommaRemovalUtilities.cs
### Reihitsu.Formatter\Pipeline\SwitchCaseBraces

- [x] Reihitsu.Formatter\Pipeline\SwitchCaseBraces\SwitchCaseBracePhase.cs
- [x] Reihitsu.Formatter\Pipeline\SwitchCaseBraces\SwitchCaseBraceRewriter.cs
### Reihitsu.Formatter\Pipeline

- [x] Reihitsu.Formatter\Pipeline\TokenGapAnalysis.cs
- [x] Reihitsu.Formatter\Pipeline\TokenGapUtilities.cs
### Reihitsu.Formatter\Pipeline\UsingDirectives

- [x] Reihitsu.Formatter\Pipeline\UsingDirectives\UsingDirectiveOrderingPhase.cs
- [x] Reihitsu.Formatter\Pipeline\UsingDirectives\UsingDirectiveOrderingRewriter.cs
- [x] Reihitsu.Formatter\Pipeline\UsingDirectives\UsingDirectiveOrderingSafety.cs
- [x] Reihitsu.Formatter\Pipeline\UsingDirectives\UsingGrouping.cs
- [x] Reihitsu.Formatter\Pipeline\UsingDirectives\UsingLeadingTriviaBuilder.cs
### Reihitsu.Formatter\Properties

- [x] Reihitsu.Formatter\Properties\AssemblyInfo.cs
- [x] Reihitsu.Formatter\Properties\GlobalUsings.cs
### Reihitsu.Formatter

- [x] Reihitsu.Formatter\ReihitsuFormatter.cs
- [x] Reihitsu.Formatter\ReihitsuFormatterHelpers.cs

## Reihitsu.Analyzer (333 files)

### Reihitsu.Analyzer

- [x] Reihitsu.Analyzer\AnalyzerResources.cs
### Reihitsu.Analyzer\Base

- [x] Reihitsu.Analyzer\Base\AttributeTargetRuleAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\CasingAnalyzerBase{T}.cs
- [x] Reihitsu.Analyzer\Base\DiagnosticAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\DocumentationModeAnalysisContextExtensions.cs
- [x] Reihitsu.Analyzer\Base\EmptyParenthesesAnalyzerBase{TAnalyzer,TNode}.cs
- [x] Reihitsu.Analyzer\Base\EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\FluentChainAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\NonOverrideMembersShouldNotBePlacedInOverrideRegionsAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\OverrideMembersShouldBeGroupedByBaseTypeRegionsAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\OverrideMemberUtilities.cs
- [x] Reihitsu.Analyzer\Base\RegionDescriptionAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\StatementShouldBeFollowedByABlankLineAnalyzerBase{TStatement,TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\StatementShouldBePrecededByABlankLineAnalyzerBase{TStatement,TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\StructEqualityPerformanceAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\TargetAttributeListShapeAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\TargetAttributePlacementAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Base\TypesOrganizedWithRegionsAnalyzerBase{TAnalyzer}.cs
### Reihitsu.Analyzer\Core

- [x] Reihitsu.Analyzer\Core\DirectDocumentationSyntaxChecker.cs
- [x] Reihitsu.Analyzer\Core\DocumentationAnalysisUtilities.cs
- [x] Reihitsu.Analyzer\Core\FluentChainAnalysisHelper.cs
- [x] Reihitsu.Analyzer\Core\NamespaceCasingHelper.cs
- [x] Reihitsu.Analyzer\Core\NestedTypeAnalyzerHelper.cs
- [x] Reihitsu.Analyzer\Core\SplitElementDocumentationAnalyzerBase{TAnalyzer}.cs
- [x] Reihitsu.Analyzer\Core\XmlDocumentationExpander.cs
### Reihitsu.Analyzer\Data

- [x] Reihitsu.Analyzer\Data\Configuration.cs
- [x] Reihitsu.Analyzer\Data\ConfigurationCategoryCopyright.cs
- [x] Reihitsu.Analyzer\Data\ConfigurationCategoryNaming.cs
- [x] Reihitsu.Analyzer\Data\ConfigurationLoadResult.cs
- [x] Reihitsu.Analyzer\Data\ConfigurationManager.cs
- [x] Reihitsu.Analyzer\Data\ConfigurationValidationError.cs
- [x] Reihitsu.Analyzer\Data\CopyrightHeaderTemplateResolver.cs
### Reihitsu.Analyzer\Enumerations

- [x] Reihitsu.Analyzer\Enumerations\DiagnosticCategory.cs
- [x] Reihitsu.Analyzer\Enumerations\DocumentationAccessibilityGroup.cs
### Reihitsu.Analyzer\Extensions

- [x] Reihitsu.Analyzer\Extensions\PropertySymbolExtensions.cs
- [x] Reihitsu.Analyzer\Extensions\SyntaxTokenExtensions.cs
### Reihitsu.Analyzer\Properties

- [x] Reihitsu.Analyzer\Properties\AssemblyInfo.cs
- [x] Reihitsu.Analyzer\Properties\GlobalUsings.cs
### Reihitsu.Analyzer\Rules\Analyzer

- [x] Reihitsu.Analyzer\Rules\Analyzer\RH0001ConfigurationFileMustBeValidAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Analyzer\RH0002DocumentationModeMustNotBeNoneAnalyzer.cs
### Reihitsu.Analyzer\Rules\Clarity

- [x] Reihitsu.Analyzer\Rules\Clarity\RH3001NotOperatorShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3003UseStringEmptyForEmptyStringsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3004UseLambdaSyntaxAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3005UseReadableConditionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3007DoNotUseQuerySyntaxAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3102CodeMustNotContainEmptyStatementsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3103UseShorthandForNullableTypesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3104DoNotUseDefaultValueTypeConstructorAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3105DoNotPrefixLocalMembersWithThisAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3201CommentsMustContainTextAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Clarity\RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.cs
### Reihitsu.Analyzer\Rules\Design

- [x] Reihitsu.Analyzer\Rules\Design\RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2002AsyncVoidShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2003NotImplementedExceptionShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2004AccessModifierMustBeDeclaredAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2005FieldsMustBePrivateAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2006DebugAssertMustProvideMessageTextAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2007DebugFailMustProvideMessageTextAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2101NestedClassesShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2102ClassesShouldNotUseParameterizedPrimaryConstructorsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2103StructsShouldNotUseParameterizedPrimaryConstructorsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2104NestedStructsShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2105NestedInterfacesShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2106NestedRecordsShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2107NestedEnumsShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2108NestedDelegatesShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Design\RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.cs
### Reihitsu.Analyzer\Rules\Documentation

- [x] Reihitsu.Analyzer\Rules\Documentation\RH8001NonPrivateClassesMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8002PrivateClassesMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8003NonPrivateStructsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8004PrivateStructsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8006PrivateInterfacesMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8007NonPrivateRecordsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8008PrivateRecordsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8010PrivateRecordStructsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8011NonPrivateEnumsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8012PrivateEnumsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8013NonPrivateEnumMembersMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8014PrivateEnumMembersMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8015NonPrivateDelegatesMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8016PrivateDelegatesMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8018PrivateConstructorsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8019DestructorsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8020NonPrivateMethodsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8021PrivateMethodsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8023PrivatePropertiesMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8024NonPrivateIndexersMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8025PrivateIndexersMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8026NonPrivateFieldsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8027PrivateFieldsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8028NonPrivateEventsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8029PrivateEventsMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8030ElementDocumentationMustHaveSummaryTextAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8031ExtensionDeclarationsMustHaveSummaryTextAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8032ExtensionDeclarationParametersMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8033ExtensionDeclarationTypeParametersMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8101ElementParametersMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8104ElementParameterDocumentationMustHaveTextAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8105ElementReturnValueMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8106ElementReturnValueDocumentationMustHaveTextAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8107VoidReturnValueMustNotBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8108GenericTypeParametersMustBeDocumentedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8201InheritdocShouldBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8202ValueTagMustNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8203InheritdocMustBeUsedWithInheritingClassAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8204DoNotUsePlaceholderElementsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8301DocumentationLinesMustBeginWithSingleSpaceAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Documentation\RH8501CodeAnalysisSuppressionMustHaveJustificationAnalyzer.cs
### Reihitsu.Analyzer\Rules\Layout

- [x] Reihitsu.Analyzer\Rules\Layout\RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5007ForStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5020SingleLineCommentsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5030BlankLineAfterClosingBraceAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5101FirstArgumentShouldBeOnSameLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5103CodeMustNotContainMultipleStatementsOnOneLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5104CommentsMustBeOnTheirOwnLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5106ClosingParenthesisMustBeOnLineOfOpeningParenthesisAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5107CommaMustBeOnSameLineAsPreviousParameterAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5108ParameterListMustFollowDeclarationAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5111AssignmentsMustHaveProperLineBreaksAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5201MethodChainsShouldBeAlignedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5203MultiLineArgumentsShouldBeAlignedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5204IndentationMustUseFourSpacesPerScopeLevelAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5303CollectionInitializerShouldBeFormattedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5403StatementMustNotBeOnSingleLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5404ElementMustNotBeOnSingleLineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5405BracesMustNotBeOmittedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5407UseBracesConsistentlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5502AssemblyAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5503ModuleAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5505ClassAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5507StructAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5509EnumAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5510EnumAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5512ConstructorAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5513MethodAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5515PropertyAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5517FieldAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5519EventAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5520EventAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5525DelegateAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5526DelegateAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5529GenericParameterAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5601UseTabsCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5603FileMustNotEndWithANewlineAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Layout\RH5604CodeMustNotContainMixedLineEndingsAnalyzer.cs
### Reihitsu.Analyzer\Rules\Naming

- [x] Reihitsu.Analyzer\Rules\Naming\RH4001TypeNameShouldMatchFileNameAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4001TypeNameShouldMatchFileNameHelper.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4002ClassNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4003StructNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4004EnumNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4005InterfaceNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4006DelegateNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4007FileScopedNamespaceCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4008NamespaceCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4009NamespaceNotAllowedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4010RecordNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4011RecordStructNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4101EnumMemberCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4102EventNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4103MethodNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4104LocalFunctionNameCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4105MethodParameterCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4106PrivateFieldCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4107ProtectedFieldCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4108InternalFieldCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4109PublicFieldCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4110ConstFieldCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4111PrivatePropertyCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4112ProtectedPropertyCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4113InternalPropertyCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4114PublicPropertyCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4115LocalVariableCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4116TupleElementCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4117DeconstructionVariableCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4118TupleElementCasingAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4119SingleLetterIdentifiersShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Naming\RH4120RecordPrimaryConstructorParameterCasingAnalyzer.cs
### Reihitsu.Analyzer\Rules\Organization

- [x] Reihitsu.Analyzer\Rules\Organization\RH7001FileMayOnlyContainASingleNamespaceAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7002FileMayOnlyContainASingleTopLevelTypeAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7003FileScopedNamespacesShouldBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7004UsingDeclarationsShouldNotBeUsedAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7101DoNotCombineFieldsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7102ConstantsMustAppearBeforeFieldsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7104PartialElementsMustDeclareAccessModifierAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7105DeclarationKeywordsMustFollowOrderAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7106ProtectedMustComeBeforeInternalAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7107PropertyAccessorsMustFollowOrderAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7108EventAccessorsMustFollowOrderAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7301RegionsShouldMatchAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7303DoNotPlaceRegionsWithinElementsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7304RegionDirectivesMustUseConsistentIndentationAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7305ATypesShouldBeOrganizedWithRegionsForMultipleKindsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7305TypesShouldBeOrganizedWithRegionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7307RegionDescriptionsShouldNotBeMemberOrMembersAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7401OverrideMethodsShouldBeGroupedByBaseTypeRegionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7402OverridePropertiesShouldBeGroupedByBaseTypeRegionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7403OverrideEventsShouldBeGroupedByBaseTypeRegionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7404OverrideIndexersShouldBeGroupedByBaseTypeRegionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7406NonOverridePropertiesShouldNotBePlacedInOverrideRegionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7407NonOverrideEventsShouldNotBePlacedInOverrideRegionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7408NonOverrideIndexersShouldNotBePlacedInOverrideRegionsAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Organization\RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzer.cs
### Reihitsu.Analyzer\Rules\Performance

- [x] Reihitsu.Analyzer\Rules\Performance\RH1001TypesUsedAsKeysMustImplementEqualityMembersAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Performance\RH1002TypesUsedForEqualityComparisonMustImplementEqualityMembersAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Performance\RH1003UseStringInterpolationInsteadOfStringConcatenationAnalyzer.cs
### Reihitsu.Analyzer\Rules\Spacing

- [x] Reihitsu.Analyzer\Rules\Spacing\RH6001KeywordsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6002CommasMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6021ColonsMustBeSpacedCorrectlyAnalyzer.cs
- [x] Reihitsu.Analyzer\Rules\Spacing\RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer.cs

## Reihitsu.Analyzer.CodeFixes (227 files)

### Reihitsu.Analyzer.CodeFixes\Base

- [ ] Reihitsu.Analyzer.CodeFixes\Base\CasingCodeFixProviderBase{T}.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Base\EmptyTypeDeclarationShouldUseSemicolonCodeFixProviderBase.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Base\ModifierOrderingCodeFixProviderBase.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Base\StatementBracesCodeFixProviderBase.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Base\StatementShouldBePrecededByABlankLineCodeFixProviderBase.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Base\TargetAttributeListShapeCodeFixProviderBase.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Base\TargetAttributePlacementCodeFixProviderBase.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Base\TypeMemberOrderingCodeFixProviderBase.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Base\UsingDirectiveOrderingCodeFixProviderBase.cs
### Reihitsu.Analyzer.CodeFixes

- [ ] Reihitsu.Analyzer.CodeFixes\CodeFixResources.cs
### Reihitsu.Analyzer.CodeFixes\Core

- [ ] Reihitsu.Analyzer.CodeFixes\Core\DocumentationCommentCodeFixUtilities.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Core\TrailingCommaCodeFixHelper.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Core\UsingDirectiveCodeFixUtilities.cs
### Reihitsu.Analyzer.CodeFixes\Properties

- [ ] Reihitsu.Analyzer.CodeFixes\Properties\AssemblyInfo.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Properties\GlobalUsings.cs
### Reihitsu.Analyzer.CodeFixes\Rules\Clarity

- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3001NotOperatorShouldNotBeUsedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3003UseStringEmptyForEmptyStringsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3004UseLambdaSyntaxCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3005UseReadableConditionsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3006ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3102CodeMustNotContainEmptyStatementsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3103UseShorthandForNullableTypesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3104DoNotUseDefaultValueTypeConstructorCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3105DoNotPrefixLocalMembersWithThisCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3106UnnecessaryDelegateParenthesesShouldBeRemovedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3201CommentsMustContainTextCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3203ExpressionStyleConstructorsShouldNotBeUsedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Clarity\RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarCodeFixProvider.cs
### Reihitsu.Analyzer.CodeFixes\Rules\Design

- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Design\RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Design\RH2004AccessModifierMustBeDeclaredCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Design\RH2005FieldsMustBePrivateCodeFixProvider.cs
### Reihitsu.Analyzer.CodeFixes\Rules\Documentation

- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8107VoidReturnValueMustNotBeDocumentedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8201InheritdocShouldBeUsedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8202ValueTagMustNotBeUsedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8204DoNotUsePlaceholderElementsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8301DocumentationLinesMustBeginWithSingleSpaceCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8303ElementDocumentationHeaderMustBePrecededByBlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8304XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8306XmlDocumentationElementTextMustNotEndWithPeriodCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8308NoContentShouldAppearAfterClosingXmlTagsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Documentation\RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderCodeFixProvider.cs
### Reihitsu.Analyzer.CodeFixes\Rules\Layout

- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5001TryStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5003WhileStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5004DoStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5005UsingStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5006ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5007ForStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5008ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5009GotoStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5010BreakStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5013ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5014SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5019YieldStatementsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5020SingleLineCommentsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5021LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5022OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5023CodeMustNotContainMultipleBlankLinesInARowCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5024ClosingBraceMustNotBePrecededByBlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5025OpeningBraceMustNotBePrecededByBlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5027WhileDoFooterMustNotBePrecededByBlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5030BlankLineAfterClosingBraceCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5101FirstArgumentShouldBeOnSameLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5102ArgumentsShouldBeOnSingleOrSeparateLinesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5103CodeMustNotContainMultipleStatementsOnOneLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5104CommentsMustBeOnTheirOwnLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5105OpeningParenthesisMustBeOnDeclarationLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5106ClosingParenthesisMustBeOnLineOfOpeningParenthesisCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5107CommaMustBeOnSameLineAsPreviousParameterCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5108ParameterListMustFollowDeclarationCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5109ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5111AssignmentsMustHaveProperLineBreaksCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5201MethodChainsShouldBeAlignedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5202RawStringLiteralsShouldBeFormattedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5203MultiLineArgumentsShouldBeAlignedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5204IndentationMustUseFourSpacesPerScopeLevelCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5205StatementLambdaOpeningBraceShouldBeAlignedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5206SwitchExpressionBracesShouldBeAnchoredCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5301ObjectInitializerShouldBeFormattedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5303CollectionInitializerShouldBeFormattedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5304NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5305CollectionExpressionsShouldBeFormattedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5306ListPatternsShouldBeFormattedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5402BracesForMultiLineStatementsMustNotShareLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5403StatementMustNotBeOnSingleLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5404ElementMustNotBeOnSingleLineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5405BracesMustNotBeOmittedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5407UseBracesConsistentlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5408SimpleAutoPropertiesShouldBeSingleLinedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5409FinalEnumMemberMustNotHaveTrailingCommaCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5412EmptyClassesShouldUseSemicolonDeclarationsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5413EmptyStructsShouldUseSemicolonDeclarationsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5414EmptyInterfacesShouldUseSemicolonDeclarationsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5415EmptyRecordsShouldUseSemicolonDeclarationsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5501AssemblyAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5502AssemblyAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5503ModuleAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5504ModuleAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5505ClassAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5506ClassAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5507StructAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5508StructAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5509EnumAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5510EnumAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5511ConstructorAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5512ConstructorAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5513MethodAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5514MethodAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5515PropertyAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5516PropertyAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5517FieldAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5518FieldAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5519EventAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5520EventAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5521InterfaceAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5522InterfaceAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5523ParameterAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5524ParameterAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5525DelegateAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5526DelegateAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5527ReturnValueAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5528ReturnValueAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5529GenericParameterAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5530AccessorAttributesMustFollowPlacementRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5531AccessorAttributeListsMustFollowShapeRulesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5601UseTabsCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5602CodeMustNotContainTrailingWhitespaceCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5603FileMustNotEndWithANewlineCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Layout\RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider.cs
### Reihitsu.Analyzer.CodeFixes\Rules\Naming

- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4001TypeNameShouldMatchFileNameCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4002ClassNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4003StructNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4004EnumNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4005InterfaceNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4006DelegateNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4010RecordNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4011RecordStructNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4101EnumMemberCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4102EventNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4103MethodNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4104LocalFunctionNameCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4105MethodParameterCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4106PrivateFieldCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4107ProtectedFieldCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4108InternalFieldCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4109PublicFieldCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4110ConstFieldCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4111PrivatePropertyCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4112ProtectedPropertyCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4113InternalPropertyCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4114PublicPropertyCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4115LocalVariableCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4116TupleElementCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4117DeconstructionVariableCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4118TupleElementCasingCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Naming\RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider.cs
### Reihitsu.Analyzer.CodeFixes\Rules\Organization

- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7004UsingDeclarationsShouldNotBeUsedCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7101DoNotCombineFieldsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7104PartialElementsMustDeclareAccessModifierCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7105DeclarationKeywordsMustFollowOrderCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7106ProtectedMustComeBeforeInternalCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7107PropertyAccessorsMustFollowOrderCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7108EventAccessorsMustFollowOrderCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7207UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7301RegionsShouldMatchCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7302RegionsShouldStartWithAUpperCaseLetterCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7303DoNotPlaceRegionsWithinElementsCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7304RegionDirectivesMustUseConsistentIndentationCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7306RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Organization\RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksCodeFixProvider.cs
### Reihitsu.Analyzer.CodeFixes\Rules\Spacing

- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6001KeywordsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6002CommasMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6003SemicolonsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6004PreprocessorKeywordsMustNotBePrecededBySpaceCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6005OperatorKeywordMustBeFollowedBySpaceCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6007OpeningSquareBracketsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6008ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6009OpeningBracesMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6010ClosingBracesMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6011OpeningGenericBracketsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6012ClosingGenericBracketsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6015NullableTypeSymbolsMustNotBePrecededBySpaceCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6016MemberAccessSymbolsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6018NegativeSignsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6019PositiveSignsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6021ColonsMustBeSpacedCorrectlyCodeFixProvider.cs
- [ ] Reihitsu.Analyzer.CodeFixes\Rules\Spacing\RH6022NoSpaceAfterNewForImplicitlyTypedArraysCodeFixProvider.cs

Total: 704 files
