# Spec index

One row per spec file under `sdd/specs/`. Add the row in the PR that adds the spec, and keep the code owners in step with the spec's header. The rules for specs are in [`sdd/README.md`](../README.md).

| Spec | ID prefix | Code owners |
|---|---|---|
| [formatter/pipeline.md](formatter/pipeline.md) | FMT-PIPE | `ReihitsuFormatter`, `FormattingPipeline`, `IFormattingPhase`, `FormattingContext`, `ReihitsuFormatterHelpers`, `LanguageVersionResolver`, `LineEndingUtilities` |
| [formatter/structural-transforms.md](formatter/structural-transforms.md) | FMT-STRUCT | `StructuralTransformPhase`, `ConfigurableStructuralTransforms`, `ControlFlowBraceTransform`, `ExpressionBodiedMethodTransform`, `ExpressionBodiedConstructorTransform`, `ExpressionBodiedOperatorTransform`, `ExpressionBodiedConversionTransform`, `ExpressionBodiedFinalizerTransform`, `ExpressionBodiedLocalFunctionTransform`, `ExpressionBodiedTransformUtilities`, `ExpressionBodyToBlockConverter`, `AccessorExpressionBodyTransform`, `GetOnlyMemberExpressionBodyTransform`, `EmptyTypeDeclarationSemicolonTransform`, `EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform`, `TrailingCommaRemovalUtilities`, `FieldDeclarationSplitTransform` |
| [formatter/region-formatting.md](formatter/region-formatting.md) | FMT-REGION | `RegionFormattingPhase`, `RegionNamingRewriter`, `RegionDirectiveUtilities` |
| [formatter/documentation-comments.md](formatter/documentation-comments.md) | FMT-DOC | `DocumentationCommentFormattingPhase`, `DocCommentElementNormalizer`, `ReihitsuFormatterHelpers`, `DocumentationCommentUtilities` |
| [formatter/using-directives.md](formatter/using-directives.md) | FMT-USING | `UsingDirectiveOrderingPhase`, `UsingDirectiveOrderingRewriter`, `UsingGrouping`, `UsingLeadingTriviaBuilder`, `UsingTrailingTriviaBuilder` |
| [formatter/blank-lines.md](formatter/blank-lines.md) | FMT-BLANK | `BlankLinePhase`, `BlankLineTokenCleanupRewriter`, `BlankLineTriviaBoundaryRewriter`, `BlankLineRegionDirectiveRewriter`, `BlankLineStatementSpacingRewriter`, `BlankLineBreakSpacingRewriter`, `BlankLineCollapser`, `BlankLineEditor` |
| [formatter/raw-string-alignment.md](formatter/raw-string-alignment.md) | FMT-RAW | `RawStringAlignmentPhase`, `RawStringLiteralUtilities` |
| [shared/blank-line-spacing.md](shared/blank-line-spacing.md) | SHARED-BLANK | `BlankLineSpacingPolicy`, `TokenGapAnalysis`, `RegionDirectiveBlankLineUtilities`, `FormattingTextAnalysisUtilities` |
| [shared/using-directive-ordering.md](shared/using-directive-ordering.md) | SHARED-USING | `UsingDirectiveOrderingUtilities`, `UsingDirectiveOrderingSafety`, `UsingDirectiveOrderingGroup` |
