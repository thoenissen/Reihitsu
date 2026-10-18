# Spec index

One row per spec file under `sdd/specs/`. Add the row in the PR that adds the spec, and keep the code owners in step with the spec's header. The rules for specs are in [`sdd/README.md`](../README.md).

| Spec | ID prefix | Code owners |
|---|---|---|
| [formatter/pipeline.md](formatter/pipeline.md) | FMT-PIPE | `ReihitsuFormatter`, `FormattingPipeline`, `IFormattingPhase`, `FormattingContext`, `ReihitsuFormatterHelpers`, `LanguageVersionResolver`, `LineEndingUtilities` |
| [formatter/structural-transforms.md](formatter/structural-transforms.md) | FMT-STRUCT | `StructuralTransformPhase`, `ConfigurableStructuralTransforms`, `ControlFlowBraceTransform`, `ExpressionBodiedMethodTransform`, `ExpressionBodiedConstructorTransform`, `ExpressionBodiedOperatorTransform`, `ExpressionBodiedConversionTransform`, `ExpressionBodiedFinalizerTransform`, `ExpressionBodiedLocalFunctionTransform`, `ExpressionBodiedTransformUtilities`, `ExpressionBodyToBlockConverter`, `AccessorExpressionBodyTransform`, `GetOnlyMemberExpressionBodyTransform`, `EmptyTypeDeclarationSemicolonTransform`, `EnumTrailingCommaRemovalTransform`, `InitializerTrailingCommaRemovalTransform`, `TrailingCommaRemovalUtilities`, `FieldDeclarationSplitTransform` |
| [formatter/region-formatting.md](formatter/region-formatting.md) | FMT-REGION | `RegionFormattingPhase`, `RegionNamingRewriter`, `RegionDirectiveUtilities` |
| [formatter/documentation-comments.md](formatter/documentation-comments.md) | FMT-DOC | `DocumentationCommentFormattingPhase`, `DocCommentElementNormalizer`, `DocumentationCommentUtilities` |
| [formatter/using-directives.md](formatter/using-directives.md) | FMT-USING | `UsingDirectiveOrderingPhase`, `UsingDirectiveOrderingRewriter`, `UsingGrouping`, `UsingLeadingTriviaBuilder`, `UsingTrailingTriviaBuilder` |
| [formatter/blank-lines.md](formatter/blank-lines.md) | FMT-BLANK | `BlankLinePhase`, `BlankLineTokenCleanupRewriter`, `BlankLineTriviaBoundaryRewriter`, `BlankLineRegionDirectiveRewriter`, `BlankLineStatementSpacingRewriter`, `BlankLineBreakSpacingRewriter`, `BlankLineCollapser`, `BlankLineEditor` |
| [formatter/line-breaks.md](formatter/line-breaks.md) | FMT-BREAK | `LineBreakPhase`, `LineBreakBlockRewriter`, `LineBreakInitializerRewriter`, `LineBreakContainedBlockRewriter`, `LineBreakAssignmentRewriter`, `DeclarationSemicolonLineBreakRewriter`, `ListElementInteriorJoinRewriter`, `ListElementInteriorUtilities`, `LineBreakListRewriter`, `PropertyLayoutLineBreakRewriter`, `GenericConstraintLineBreakRewriter`, `DeclarationBraceLineBreakRewriter`, `MemberLineStartRewriter`, `AttributeTargetFormattingRewriter`, `AttributeTargetFormattingShared`, `BinaryOperatorLineBreakRewriter`, `ChainLineBreakRewriter`, `TernaryLineBreakRewriter`, `SwitchCaseWhenLineBreakRewriter`, `BracePlacer`, `TokenGapNormalizer`, `AccessorListLayout`, `LineBreakDetection`, `LineBreakTriviaUtilities` |
| [formatter/switch-case-braces.md](formatter/switch-case-braces.md) | FMT-SWITCH | `SwitchCaseBracePhase`, `SwitchCaseBraceRewriter` |
| [formatter/horizontal-spacing.md](formatter/horizontal-spacing.md) | FMT-SPACE | `HorizontalSpacingPhase`, `HorizontalSpacingRewriter`, `SpacingPolicy`, `ISpacingRule`, `AttributeListCloseBracketSpacingRule`, `NoSpaceSpacingRule`, `CommaSpacingRule`, `SingleSpaceSpacingRule`, `OperatorSpacingRule`, `ForLoopSemicolonSpacingRule`, `KeywordSpacingRule`, `TrailingWhitespaceWriter`, `CommaSpacingUtilities`, `UnaryOperatorSpacingUtilities` |
| [formatter/indentation.md](formatter/indentation.md) | FMT-INDENT | `IndentationPhase`, `LayoutComputer`, `LayoutModel`, `TokenLayout`, `IndentationRewriter`, `ListElementAligner`, `SyntaxIndentationUtilities`, `ArgumentAlignmentContributor`, `AngleBracketListAlignmentContributor`, `AnonymousObjectContributor`, `BaseTypeListContributor`, `BinaryExpressionContributor`, `CaseWhenClauseContributor`, `CollectionExpressionContributor`, `CommentIndentationContributor`, `ConditionalExpressionContributor`, `ConstructorInitializerContributor`, `GenericConstraintContributor`, `LambdaAlignmentContributor`, `ListPatternContributor`, `MethodChainAlignmentContributor`, `ObjectInitializerContributor`, `ParenthesizedPatternContributor`, `PatternAnchor`, `RecursivePatternContributor`, `SwitchExpressionContributor` |
| [formatter/raw-string-alignment.md](formatter/raw-string-alignment.md) | FMT-RAW | `RawStringAlignmentPhase`, `RawStringLiteralUtilities` |
| [formatter/cleanup.md](formatter/cleanup.md) | FMT-CLEAN | `CleanupPhase`, `EndOfLineWhitespace` |
| [formatter/line-endings.md](formatter/line-endings.md) | FMT-EOL | `LineEndingNormalizationPhase` |
| [shared/attribute-targets.md](shared/attribute-targets.md) | SHARED-ATTR | `AttributeTargetRuleAnalyzerBase`, `TargetAttributePlacementAnalyzerBase`, `TargetAttributePlacementCodeFixProviderBase`, `TargetAttributeListShapeAnalyzerBase`, `TargetAttributeListShapeCodeFixProviderBase` |
| [shared/blank-line-spacing.md](shared/blank-line-spacing.md) | SHARED-BLANK | `BlankLineSpacingPolicy`, `TokenGapAnalysis`, `RegionDirectiveBlankLineUtilities`, `FormattingTextAnalysisUtilities` |
| [shared/casing.md](shared/casing.md) | SHARED-CASING | `CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `NamingAccessibilityClassifier`, `NamespaceCasingHelper`, `CasingUtilities` |
| [shared/code-fixes.md](shared/code-fixes.md) | SHARED-FIX | — |
| [shared/comment-placement.md](shared/comment-placement.md) | SHARED-COMMENT | `CommentPositionAnalyzerBase`, `CommentRegionAnalyzerBase`, `CommentPositionUtilities`, `FluentChainAnalysisHelper` |
| [shared/diagnostics.md](shared/diagnostics.md) | SHARED-DIAG | `DiagnosticAnalyzerBase` |
| [shared/element-documentation.md](shared/element-documentation.md) | SHARED-DOC | `SplitElementDocumentationAnalyzerBase`, `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `XmlDocumentationExpander`, `DocumentationModeAnalysisContextExtensions` |
| [shared/fluent-chain-layout.md](shared/fluent-chain-layout.md) | SHARED-CHAIN | `FluentChain`, `FluentChainLink`, `FluentChainAttachedPart`, `FluentChainUtilities` |
| [shared/ordering-move-safety.md](shared/ordering-move-safety.md) | SHARED-ORDER | `OrderingMoveSafety`, `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase` |
| [shared/region-directive-blank-lines.md](shared/region-directive-blank-lines.md) | SHARED-REGION | `RegionDirectiveBlankLineAnalyzerBase`, `RegionDirectiveBlankLineCodeFixProviderBase` |
| [shared/single-letter-names.md](shared/single-letter-names.md) | SHARED-SINGLE | `SingleLetterIdentifierUtilities` |
| [shared/statement-blank-lines.md](shared/statement-blank-lines.md) | SHARED-STMT | `StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement>`, `StatementShouldBePrecededByABlankLineCodeFixProviderBase` |
| [shared/using-directive-ordering.md](shared/using-directive-ordering.md) | SHARED-USING | `UsingDirectiveOrderingUtilities`, `UsingDirectiveOrderingSafety`, `UsingDirectiveOrderingGroup` |
| [shared/whitespace-removal-fixes.md](shared/whitespace-removal-fixes.md) | SHARED-WS | `RemoveWhitespaceRunCodeFixProviderBase`, `WhitespaceSpanRemovalCodeFixProviderBase`, `BlankLineSpanRemovalCodeFixProviderBase` |
| [analyzer/RH0001.md](analyzer/RH0001.md) | RH0001 | `RH0001ConfigurationFileMustBeValidAnalyzer`, `ConfigurationManager`, `ConfigurationLoadResult`, `ConfigurationValidationError`, `CopyrightHeaderTemplateResolver` |
| [analyzer/RH1003.md](analyzer/RH1003.md) | RH1003 | `RH1003UseStringInterpolationInsteadOfStringConcatenationAnalyzer` |
| [analyzer/RH2005.md](analyzer/RH2005.md) | RH2005 | `RH2005FieldsMustBePrivateAnalyzer`, `RH2005FieldsMustBePrivateCodeFixProvider`, `DeclarationModifierUtilities` |
| [analyzer/RH3202.md](analyzer/RH3202.md) | RH3202 | `RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzer`, `RH3202ExpressionStyleMethodsShouldNotBeUsedCodeFixProvider`, `ExpressionBodyRewriteUtilities` |
| [analyzer/RH4001.md](analyzer/RH4001.md) | RH4001 | `RH4001TypeNameShouldMatchFileNameAnalyzer`, `RH4001TypeNameShouldMatchFileNameCodeFixProvider`, `RH4001TypeNameShouldMatchFileNameHelper` |
| [analyzer/RH4002.md](analyzer/RH4002.md) | RH4002 | `RH4002ClassNameCasingAnalyzer`, `RH4002ClassNameCasingCodeFixProvider` |
| [analyzer/RH4003.md](analyzer/RH4003.md) | RH4003 | `RH4003StructNameCasingAnalyzer`, `RH4003StructNameCasingCodeFixProvider` |
| [analyzer/RH4004.md](analyzer/RH4004.md) | RH4004 | `RH4004EnumNameCasingAnalyzer`, `RH4004EnumNameCasingCodeFixProvider` |
| [analyzer/RH4005.md](analyzer/RH4005.md) | RH4005 | `RH4005InterfaceNameCasingAnalyzer`, `RH4005InterfaceNameCasingCodeFixProvider` |
| [analyzer/RH4006.md](analyzer/RH4006.md) | RH4006 | `RH4006DelegateNameCasingAnalyzer`, `RH4006DelegateNameCasingCodeFixProvider` |
| [analyzer/RH4007.md](analyzer/RH4007.md) | RH4007 | `RH4007FileScopedNamespaceCasingAnalyzer` |
| [analyzer/RH4008.md](analyzer/RH4008.md) | RH4008 | `RH4008NamespaceCasingAnalyzer` |
| [analyzer/RH4009.md](analyzer/RH4009.md) | RH4009 | `RH4009NamespaceNotAllowedAnalyzer` |
| [analyzer/RH4010.md](analyzer/RH4010.md) | RH4010 | `RH4010RecordNameCasingAnalyzer`, `RH4010RecordNameCasingCodeFixProvider` |
| [analyzer/RH4011.md](analyzer/RH4011.md) | RH4011 | `RH4011RecordStructNameCasingAnalyzer`, `RH4011RecordStructNameCasingCodeFixProvider` |
| [analyzer/RH4101.md](analyzer/RH4101.md) | RH4101 | `RH4101EnumMemberCasingAnalyzer`, `RH4101EnumMemberCasingCodeFixProvider` |
| [analyzer/RH4102.md](analyzer/RH4102.md) | RH4102 | `RH4102EventNameCasingAnalyzer`, `RH4102EventNameCasingCodeFixProvider`, `RH4102CustomEventNameCasingCodeFixProvider` |
| [analyzer/RH4103.md](analyzer/RH4103.md) | RH4103 | `RH4103MethodNameCasingAnalyzer`, `RH4103MethodNameCasingCodeFixProvider` |
| [analyzer/RH4104.md](analyzer/RH4104.md) | RH4104 | `RH4104LocalFunctionNameCasingAnalyzer`, `RH4104LocalFunctionNameCasingCodeFixProvider` |
| [analyzer/RH4105.md](analyzer/RH4105.md) | RH4105 | `RH4105MethodParameterCasingAnalyzer`, `RH4105MethodParameterCasingCodeFixProvider` |
| [analyzer/RH4106.md](analyzer/RH4106.md) | RH4106 | `RH4106PrivateFieldCasingAnalyzer`, `RH4106PrivateFieldCasingCodeFixProvider` |
| [analyzer/RH4107.md](analyzer/RH4107.md) | RH4107 | `RH4107ProtectedFieldCasingAnalyzer`, `RH4107ProtectedFieldCasingCodeFixProvider` |
| [analyzer/RH4108.md](analyzer/RH4108.md) | RH4108 | `RH4108InternalFieldCasingAnalyzer`, `RH4108InternalFieldCasingCodeFixProvider` |
| [analyzer/RH4109.md](analyzer/RH4109.md) | RH4109 | `RH4109PublicFieldCasingAnalyzer`, `RH4109PublicFieldCasingCodeFixProvider` |
| [analyzer/RH4110.md](analyzer/RH4110.md) | RH4110 | `RH4110ConstFieldCasingAnalyzer`, `RH4110ConstFieldCasingCodeFixProvider` |
| [analyzer/RH4111.md](analyzer/RH4111.md) | RH4111 | `RH4111PrivatePropertyCasingAnalyzer`, `RH4111PrivatePropertyCasingCodeFixProvider` |
| [analyzer/RH4112.md](analyzer/RH4112.md) | RH4112 | `RH4112ProtectedPropertyCasingAnalyzer`, `RH4112ProtectedPropertyCasingCodeFixProvider` |
| [analyzer/RH4113.md](analyzer/RH4113.md) | RH4113 | `RH4113InternalPropertyCasingAnalyzer`, `RH4113InternalPropertyCasingCodeFixProvider` |
| [analyzer/RH4114.md](analyzer/RH4114.md) | RH4114 | `RH4114PublicPropertyCasingAnalyzer`, `RH4114PublicPropertyCasingCodeFixProvider` |
| [analyzer/RH4115.md](analyzer/RH4115.md) | RH4115 | `RH4115LocalVariableCasingAnalyzer`, `RH4115LocalVariableCasingCodeFixProvider` |
| [analyzer/RH4116.md](analyzer/RH4116.md) | RH4116 | `RH4116TupleElementCasingAnalyzer` |
| [analyzer/RH4117.md](analyzer/RH4117.md) | RH4117 | `RH4117DeconstructionVariableCasingAnalyzer`, `RH4117DeconstructionVariableCasingCodeFixProvider` |
| [analyzer/RH4118.md](analyzer/RH4118.md) | RH4118 | `RH4118TupleElementCasingAnalyzer` |
| [analyzer/RH4120.md](analyzer/RH4120.md) | RH4120 | `RH4120RecordPrimaryConstructorParameterCasingAnalyzer`, `RH4120RecordPrimaryConstructorParameterCasingCodeFixProvider` |
| [analyzer/RH4121.md](analyzer/RH4121.md) | RH4121 | `RH4121TypeParameterNameCasingAnalyzer`, `RH4121TypeParameterNameCasingCodeFixProvider` |
| [analyzer/RH4122.md](analyzer/RH4122.md) | RH4122 | `RH4122SingleLetterParameterNamesAnalyzer` |
| [analyzer/RH4123.md](analyzer/RH4123.md) | RH4123 | `RH4123SingleLetterLocalFunctionParameterNamesAnalyzer` |
| [analyzer/RH4124.md](analyzer/RH4124.md) | RH4124 | `RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer` |
| [analyzer/RH4125.md](analyzer/RH4125.md) | RH4125 | `RH4125SingleLetterLocalVariableNamesAnalyzer` |
| [analyzer/RH4126.md](analyzer/RH4126.md) | RH4126 | `RH4126SingleLetterForeachIterationVariableNamesAnalyzer` |
| [analyzer/RH4127.md](analyzer/RH4127.md) | RH4127 | `RH4127SingleLetterCatchExceptionVariableNamesAnalyzer` |
| [analyzer/RH5001.md](analyzer/RH5001.md) | RH5001 | `RH5001TryStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5001TryStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5002.md](analyzer/RH5002.md) | RH5002 | `RH5002IfStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5002IfStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5003.md](analyzer/RH5003.md) | RH5003 | `RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5003WhileStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5004.md](analyzer/RH5004.md) | RH5004 | `RH5004DoStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5004DoStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5005.md](analyzer/RH5005.md) | RH5005 | `RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5005UsingStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5006.md](analyzer/RH5006.md) | RH5006 | `RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5006ForeachStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5007.md](analyzer/RH5007.md) | RH5007 | `RH5007ForStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5007ForStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5008.md](analyzer/RH5008.md) | RH5008 | `RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5008ReturnStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5009.md](analyzer/RH5009.md) | RH5009 | `RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5009GotoStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5010.md](analyzer/RH5010.md) | RH5010 | `RH5010BreakStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5010BreakStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5011.md](analyzer/RH5011.md) | RH5011 | `RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzer`, `RH5011BreakStatementsShouldBeFollowedByABlankLineCodeFixProvider`, `StatementShouldBeFollowedByABlankLineAnalyzerBase<TStatement>` |
| [analyzer/RH5012.md](analyzer/RH5012.md) | RH5012 | `RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5013.md](analyzer/RH5013.md) | RH5013 | `RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5013ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5014.md](analyzer/RH5014.md) | RH5014 | `RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5014SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5015.md](analyzer/RH5015.md) | RH5015 | `RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5016.md](analyzer/RH5016.md) | RH5016 | `RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5017.md](analyzer/RH5017.md) | RH5017 | `RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5018.md](analyzer/RH5018.md) | RH5018 | `RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5019.md](analyzer/RH5019.md) | RH5019 | `RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5019YieldStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5020.md](analyzer/RH5020.md) | RH5020 | `RH5020CommentsShouldBePrecededByABlankLineAnalyzer`, `RH5020CommentsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5021.md](analyzer/RH5021.md) | RH5021 | `RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer`, `RH5021LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider` |
| [analyzer/RH5022.md](analyzer/RH5022.md) | RH5022 | `RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer`, `RH5022OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider` |
| [analyzer/RH5023.md](analyzer/RH5023.md) | RH5023 | `RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer`, `RH5023CodeMustNotContainMultipleBlankLinesInARowCodeFixProvider` |
| [analyzer/RH5024.md](analyzer/RH5024.md) | RH5024 | `RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer`, `RH5024ClosingBraceMustNotBePrecededByBlankLineCodeFixProvider` |
| [analyzer/RH5025.md](analyzer/RH5025.md) | RH5025 | `RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer`, `RH5025OpeningBraceMustNotBePrecededByBlankLineCodeFixProvider` |
| [analyzer/RH5026.md](analyzer/RH5026.md) | RH5026 | `RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer`, `RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineCodeFixProvider` |
| [analyzer/RH5027.md](analyzer/RH5027.md) | RH5027 | `RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzer`, `RH5027WhileDoFooterMustNotBePrecededByBlankLineCodeFixProvider` |
| [analyzer/RH5028.md](analyzer/RH5028.md) | RH5028 | `RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer`, `RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider` |
| [analyzer/RH5029.md](analyzer/RH5029.md) | RH5029 | `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer`, `RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5030.md](analyzer/RH5030.md) | RH5030 | `RH5030BlankLineAfterClosingBraceAnalyzer`, `RH5030BlankLineAfterClosingBraceCodeFixProvider` |
| [analyzer/RH5031.md](analyzer/RH5031.md) | RH5031 | `RH5031RegionDirectivesShouldBePrecededByABlankLineAnalyzer`, `RH5031RegionDirectivesShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5032.md](analyzer/RH5032.md) | RH5032 | `RH5032RegionDirectivesShouldBeFollowedByABlankLineAnalyzer`, `RH5032RegionDirectivesShouldBeFollowedByABlankLineCodeFixProvider` |
| [analyzer/RH5033.md](analyzer/RH5033.md) | RH5033 | `RH5033CommentsMustNotBePlacedBetweenAttributesAndDeclarationAnalyzer` |
| [analyzer/RH5034.md](analyzer/RH5034.md) | RH5034 | `RH5034CommentsMustNotBePlacedBetweenDocumentationAndDeclarationAnalyzer` |
| [analyzer/RH5035.md](analyzer/RH5035.md) | RH5035 | `RH5035CommentsMustNotBePlacedInsideParameterListAnalyzer` |
| [analyzer/RH5036.md](analyzer/RH5036.md) | RH5036 | `RH5036CommentsMustNotBePlacedBetweenStatementHeaderAndBodyAnalyzer` |
| [analyzer/RH5037.md](analyzer/RH5037.md) | RH5037 | `RH5037CommentsMustNotBePlacedBeforeContinuationClauseAnalyzer` |
| [analyzer/RH5038.md](analyzer/RH5038.md) | RH5038 | `RH5038CommentsMustNotBePlacedInsideArgumentListAnalyzer` |
| [analyzer/RH5039.md](analyzer/RH5039.md) | RH5039 | `RH5039CommentsMustNotBePlacedInsideStatementHeaderAnalyzer` |
| [analyzer/RH5040.md](analyzer/RH5040.md) | RH5040 | `RH5040CommentsMustNotBePlacedAroundExpressionBodyArrowAnalyzer` |
| [analyzer/RH5041.md](analyzer/RH5041.md) | RH5041 | `RH5041CommentsMustNotBePlacedInsideDeclaratorListAnalyzer` |
| [analyzer/RH5042.md](analyzer/RH5042.md) | RH5042 | `RH5042CommentsMustNotBePlacedAroundAssignmentOperatorAnalyzer` |
| [analyzer/RH5043.md](analyzer/RH5043.md) | RH5043 | `RH5043CommentsMustNotBePlacedInsideBaseListAnalyzer` |
| [analyzer/RH5044.md](analyzer/RH5044.md) | RH5044 | `RH5044CommentsMustNotBePlacedInsideAttributeListAnalyzer` |
| [analyzer/RH5045.md](analyzer/RH5045.md) | RH5045 | `RH5045CommentsMustNotBePlacedInsideConditionalExpressionAnalyzer` |
| [analyzer/RH5046.md](analyzer/RH5046.md) | RH5046 | `RH5046CommentsMustNotBePlacedInsideTypeParameterTypeArgumentOrTupleListAnalyzer` |
| [analyzer/RH5047.md](analyzer/RH5047.md) | RH5047 | `RH5047CommentsMustNotBePlacedInsideAutoPropertyAccessorListAnalyzer` |
| [analyzer/RH5049.md](analyzer/RH5049.md) | RH5049 | `RH5049CommentsMustNotBePlacedAfterBinaryOperatorAnalyzer` |
| [analyzer/RH5501.md](analyzer/RH5501.md) | RH5501 | `RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer`, `RH5501AssemblyAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5502.md](analyzer/RH5502.md) | RH5502 | `RH5502AssemblyAttributeListsMustFollowShapeRulesAnalyzer`, `RH5502AssemblyAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5503.md](analyzer/RH5503.md) | RH5503 | `RH5503ModuleAttributesMustFollowPlacementRulesAnalyzer`, `RH5503ModuleAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5504.md](analyzer/RH5504.md) | RH5504 | `RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer`, `RH5504ModuleAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5505.md](analyzer/RH5505.md) | RH5505 | `RH5505ClassAttributesMustFollowPlacementRulesAnalyzer`, `RH5505ClassAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5506.md](analyzer/RH5506.md) | RH5506 | `RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer`, `RH5506ClassAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5507.md](analyzer/RH5507.md) | RH5507 | `RH5507StructAttributesMustFollowPlacementRulesAnalyzer`, `RH5507StructAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5508.md](analyzer/RH5508.md) | RH5508 | `RH5508StructAttributeListsMustFollowShapeRulesAnalyzer`, `RH5508StructAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5509.md](analyzer/RH5509.md) | RH5509 | `RH5509EnumAttributesMustFollowPlacementRulesAnalyzer`, `RH5509EnumAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5510.md](analyzer/RH5510.md) | RH5510 | `RH5510EnumAttributeListsMustFollowShapeRulesAnalyzer`, `RH5510EnumAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5511.md](analyzer/RH5511.md) | RH5511 | `RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer`, `RH5511ConstructorAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5512.md](analyzer/RH5512.md) | RH5512 | `RH5512ConstructorAttributeListsMustFollowShapeRulesAnalyzer`, `RH5512ConstructorAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5513.md](analyzer/RH5513.md) | RH5513 | `RH5513MethodAttributesMustFollowPlacementRulesAnalyzer`, `RH5513MethodAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5514.md](analyzer/RH5514.md) | RH5514 | `RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer`, `RH5514MethodAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5515.md](analyzer/RH5515.md) | RH5515 | `RH5515PropertyAttributesMustFollowPlacementRulesAnalyzer`, `RH5515PropertyAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5516.md](analyzer/RH5516.md) | RH5516 | `RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer`, `RH5516PropertyAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5517.md](analyzer/RH5517.md) | RH5517 | `RH5517FieldAttributesMustFollowPlacementRulesAnalyzer`, `RH5517FieldAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5518.md](analyzer/RH5518.md) | RH5518 | `RH5518FieldAttributeListsMustFollowShapeRulesAnalyzer`, `RH5518FieldAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5519.md](analyzer/RH5519.md) | RH5519 | `RH5519EventAttributesMustFollowPlacementRulesAnalyzer`, `RH5519EventAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5520.md](analyzer/RH5520.md) | RH5520 | `RH5520EventAttributeListsMustFollowShapeRulesAnalyzer`, `RH5520EventAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5521.md](analyzer/RH5521.md) | RH5521 | `RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzer`, `RH5521InterfaceAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5522.md](analyzer/RH5522.md) | RH5522 | `RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzer`, `RH5522InterfaceAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5523.md](analyzer/RH5523.md) | RH5523 | `RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer`, `RH5523ParameterAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5524.md](analyzer/RH5524.md) | RH5524 | `RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer`, `RH5524ParameterAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5525.md](analyzer/RH5525.md) | RH5525 | `RH5525DelegateAttributesMustFollowPlacementRulesAnalyzer`, `RH5525DelegateAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5526.md](analyzer/RH5526.md) | RH5526 | `RH5526DelegateAttributeListsMustFollowShapeRulesAnalyzer`, `RH5526DelegateAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5527.md](analyzer/RH5527.md) | RH5527 | `RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer`, `RH5527ReturnValueAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5528.md](analyzer/RH5528.md) | RH5528 | `RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer`, `RH5528ReturnValueAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5529.md](analyzer/RH5529.md) | RH5529 | `RH5529GenericParameterAttributeListsMustFollowShapeRulesAnalyzer`, `RH5529GenericParameterAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5530.md](analyzer/RH5530.md) | RH5530 | `RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer`, `RH5530AccessorAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5531.md](analyzer/RH5531.md) | RH5531 | `RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer`, `RH5531AccessorAttributeListsMustFollowShapeRulesCodeFixProvider` |
| [analyzer/RH5601.md](analyzer/RH5601.md) | RH5601 | `RH5601UseTabsCorrectlyAnalyzer`, `RH5601UseTabsCorrectlyCodeFixProvider` |
| [analyzer/RH5602.md](analyzer/RH5602.md) | RH5602 | `RH5602CodeMustNotContainTrailingWhitespaceAnalyzer`, `RH5602CodeMustNotContainTrailingWhitespaceCodeFixProvider` |
| [analyzer/RH5603.md](analyzer/RH5603.md) | RH5603 | `RH5603FileMustNotEndWithANewlineAnalyzer`, `RH5603FileMustNotEndWithANewlineCodeFixProvider` |
| [analyzer/RH5604.md](analyzer/RH5604.md) | RH5604 | `RH5604CodeMustNotContainMixedLineEndingsAnalyzer`, `RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider` |
| [analyzer/RH6001.md](analyzer/RH6001.md) | RH6001 | `RH6001KeywordsMustBeSpacedCorrectlyAnalyzer`, `RH6001KeywordsMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH6002.md](analyzer/RH6002.md) | RH6002 | `RH6002CommasMustBeSpacedCorrectlyAnalyzer`, `RH6002CommasMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH6003.md](analyzer/RH6003.md) | RH6003 | `RH6003SemicolonsMustBeSpacedCorrectlyAnalyzer`, `RH6003SemicolonsMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH6004.md](analyzer/RH6004.md) | RH6004 | `RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzer`, `RH6004PreprocessorKeywordsMustNotBePrecededBySpaceCodeFixProvider` |
| [analyzer/RH6005.md](analyzer/RH6005.md) | RH6005 | `RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer`, `RH6005OperatorKeywordMustBeFollowedBySpaceCodeFixProvider` |
| [analyzer/RH6006.md](analyzer/RH6006.md) | RH6006 | `RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer`, `RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH6007.md](analyzer/RH6007.md) | RH6007 | `RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzer`, `RH6007OpeningSquareBracketsMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH6008.md](analyzer/RH6008.md) | RH6008 | `RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer`, `RH6008ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH6009.md](analyzer/RH6009.md) | RH6009 | `RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer`, `RH6009OpeningBracesMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH6010.md](analyzer/RH6010.md) | RH6010 | `RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer`, `RH6010ClosingBracesMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH6011.md](analyzer/RH6011.md) | RH6011 | `RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer`, `RH6011OpeningGenericBracketsMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH6012.md](analyzer/RH6012.md) | RH6012 | `RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer`, `RH6012ClosingGenericBracketsMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH7103.md](analyzer/RH7103.md) | RH7103 | `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer`, `RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider` |
| [analyzer/RH8001.md](analyzer/RH8001.md) | RH8001 | `RH8001NonPrivateClassesMustBeDocumentedAnalyzer` |
| [analyzer/RH8002.md](analyzer/RH8002.md) | RH8002 | `RH8002PrivateClassesMustBeDocumentedAnalyzer` |
| [analyzer/RH8003.md](analyzer/RH8003.md) | RH8003 | `RH8003NonPrivateStructsMustBeDocumentedAnalyzer` |
| [analyzer/RH8004.md](analyzer/RH8004.md) | RH8004 | `RH8004PrivateStructsMustBeDocumentedAnalyzer` |
| [analyzer/RH8005.md](analyzer/RH8005.md) | RH8005 | `RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer` |
| [analyzer/RH8006.md](analyzer/RH8006.md) | RH8006 | `RH8006PrivateInterfacesMustBeDocumentedAnalyzer` |
| [analyzer/RH8007.md](analyzer/RH8007.md) | RH8007 | `RH8007NonPrivateRecordsMustBeDocumentedAnalyzer` |
| [analyzer/RH8008.md](analyzer/RH8008.md) | RH8008 | `RH8008PrivateRecordsMustBeDocumentedAnalyzer` |
| [analyzer/RH8009.md](analyzer/RH8009.md) | RH8009 | `RH8009NonPrivateRecordStructsMustBeDocumentedAnalyzer` |
| [analyzer/RH8010.md](analyzer/RH8010.md) | RH8010 | `RH8010PrivateRecordStructsMustBeDocumentedAnalyzer` |
| [analyzer/RH8011.md](analyzer/RH8011.md) | RH8011 | `RH8011NonPrivateEnumsMustBeDocumentedAnalyzer` |
| [analyzer/RH8012.md](analyzer/RH8012.md) | RH8012 | `RH8012PrivateEnumsMustBeDocumentedAnalyzer` |
| [analyzer/RH8013.md](analyzer/RH8013.md) | RH8013 | `RH8013NonPrivateEnumMembersMustBeDocumentedAnalyzer` |
| [analyzer/RH8014.md](analyzer/RH8014.md) | RH8014 | `RH8014PrivateEnumMembersMustBeDocumentedAnalyzer` |
| [analyzer/RH8015.md](analyzer/RH8015.md) | RH8015 | `RH8015NonPrivateDelegatesMustBeDocumentedAnalyzer` |
| [analyzer/RH8016.md](analyzer/RH8016.md) | RH8016 | `RH8016PrivateDelegatesMustBeDocumentedAnalyzer` |
| [analyzer/RH8017.md](analyzer/RH8017.md) | RH8017 | `RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer` |
| [analyzer/RH8018.md](analyzer/RH8018.md) | RH8018 | `RH8018PrivateConstructorsMustBeDocumentedAnalyzer` |
| [analyzer/RH8019.md](analyzer/RH8019.md) | RH8019 | `RH8019DestructorsMustBeDocumentedAnalyzer` |
| [analyzer/RH8107.md](analyzer/RH8107.md) | RH8107 | `RH8107VoidReturnValueMustNotBeDocumentedAnalyzer`, `RH8107VoidReturnValueMustNotBeDocumentedCodeFixProvider`, `DocumentationCommentCodeFixUtilities` |
| [cli/reihitsu-format.md](cli/reihitsu-format.md) | CLI | `Program`, `ParseResult`, `FormatCommandHandler`, `ExitCodes`, `GeneratedFileUtilities`, `FileProcessResult`, `DefaultFileSystem`, `DefaultConsoleInput`, `DefaultSourceFormatter`, `DiffGenerator` |
| [playground/playground.md](playground/playground.md) | PLAY | `Home`, `MainLayout`, `PlaygroundFormatter`, `PlaygroundFormatResult`, `PlaygroundVersion` |
