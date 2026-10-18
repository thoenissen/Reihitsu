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
| [shared/attribute-targets.md](shared/attribute-targets.md) | SHARED-ATTR | `AttributeTargetRuleAnalyzerBase`, `TargetAttributePlacementAnalyzerBase`, `TargetAttributePlacementCodeFixProviderBase` |
| [shared/blank-line-spacing.md](shared/blank-line-spacing.md) | SHARED-BLANK | `BlankLineSpacingPolicy`, `TokenGapAnalysis`, `RegionDirectiveBlankLineUtilities`, `FormattingTextAnalysisUtilities` |
| [shared/casing.md](shared/casing.md) | SHARED-CASING | `CasingAnalyzerBase`, `CasingCodeFixProviderBase<T>`, `NamingAccessibilityClassifier`, `NamespaceCasingHelper`, `CasingUtilities` |
| [shared/code-fixes.md](shared/code-fixes.md) | SHARED-FIX | — |
| [shared/diagnostics.md](shared/diagnostics.md) | SHARED-DIAG | `DiagnosticAnalyzerBase` |
| [shared/fluent-chain-layout.md](shared/fluent-chain-layout.md) | SHARED-CHAIN | `FluentChain`, `FluentChainLink`, `FluentChainAttachedPart`, `FluentChainUtilities` |
| [shared/ordering-move-safety.md](shared/ordering-move-safety.md) | SHARED-ORDER | `OrderingMoveSafety`, `OrderingDeclarationUtilities`, `AccessorOrderingUtilities`, `TypeMemberOrderingCodeFixProviderBase` |
| [shared/single-letter-names.md](shared/single-letter-names.md) | SHARED-SINGLE | `SingleLetterIdentifierUtilities` |
| [shared/statement-blank-lines.md](shared/statement-blank-lines.md) | SHARED-STMT | `StatementShouldBePrecededByABlankLineAnalyzerBase<TStatement>`, `StatementShouldBePrecededByABlankLineCodeFixProviderBase` |
| [shared/using-directive-ordering.md](shared/using-directive-ordering.md) | SHARED-USING | `UsingDirectiveOrderingUtilities`, `UsingDirectiveOrderingSafety`, `UsingDirectiveOrderingGroup` |
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
| [analyzer/RH5012.md](analyzer/RH5012.md) | RH5012 | `RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5012ContinueStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5013.md](analyzer/RH5013.md) | RH5013 | `RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5013ThrowStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5014.md](analyzer/RH5014.md) | RH5014 | `RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5014SwitchStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5015.md](analyzer/RH5015.md) | RH5015 | `RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5015CheckedStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5016.md](analyzer/RH5016.md) | RH5016 | `RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5016UncheckedStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5017.md](analyzer/RH5017.md) | RH5017 | `RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5017FixedStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5018.md](analyzer/RH5018.md) | RH5018 | `RH5018LockStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5018LockStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5019.md](analyzer/RH5019.md) | RH5019 | `RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzer`, `RH5019YieldStatementsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5021.md](analyzer/RH5021.md) | RH5021 | `RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzer`, `RH5021LocalDeclarationsShouldBeFollowedByABlankLineCodeFixProvider` |
| [analyzer/RH5029.md](analyzer/RH5029.md) | RH5029 | `RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzer`, `RH5029LocalDeclarationsShouldBePrecededByABlankLineCodeFixProvider` |
| [analyzer/RH5501.md](analyzer/RH5501.md) | RH5501 | `RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzer`, `RH5501AssemblyAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5503.md](analyzer/RH5503.md) | RH5503 | `RH5503ModuleAttributesMustFollowPlacementRulesAnalyzer`, `RH5503ModuleAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5505.md](analyzer/RH5505.md) | RH5505 | `RH5505ClassAttributesMustFollowPlacementRulesAnalyzer`, `RH5505ClassAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5507.md](analyzer/RH5507.md) | RH5507 | `RH5507StructAttributesMustFollowPlacementRulesAnalyzer`, `RH5507StructAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5509.md](analyzer/RH5509.md) | RH5509 | `RH5509EnumAttributesMustFollowPlacementRulesAnalyzer`, `RH5509EnumAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5511.md](analyzer/RH5511.md) | RH5511 | `RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzer`, `RH5511ConstructorAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5513.md](analyzer/RH5513.md) | RH5513 | `RH5513MethodAttributesMustFollowPlacementRulesAnalyzer`, `RH5513MethodAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5515.md](analyzer/RH5515.md) | RH5515 | `RH5515PropertyAttributesMustFollowPlacementRulesAnalyzer`, `RH5515PropertyAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5517.md](analyzer/RH5517.md) | RH5517 | `RH5517FieldAttributesMustFollowPlacementRulesAnalyzer`, `RH5517FieldAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5519.md](analyzer/RH5519.md) | RH5519 | `RH5519EventAttributesMustFollowPlacementRulesAnalyzer`, `RH5519EventAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5521.md](analyzer/RH5521.md) | RH5521 | `RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzer`, `RH5521InterfaceAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5523.md](analyzer/RH5523.md) | RH5523 | `RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer`, `RH5523ParameterAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5525.md](analyzer/RH5525.md) | RH5525 | `RH5525DelegateAttributesMustFollowPlacementRulesAnalyzer`, `RH5525DelegateAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5527.md](analyzer/RH5527.md) | RH5527 | `RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer`, `RH5527ReturnValueAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH5530.md](analyzer/RH5530.md) | RH5530 | `RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer`, `RH5530AccessorAttributesMustFollowPlacementRulesCodeFixProvider` |
| [analyzer/RH6002.md](analyzer/RH6002.md) | RH6002 | `RH6002CommasMustBeSpacedCorrectlyAnalyzer`, `RH6002CommasMustBeSpacedCorrectlyCodeFixProvider` |
| [analyzer/RH7103.md](analyzer/RH7103.md) | RH7103 | `RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer`, `RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider` |
| [analyzer/RH8107.md](analyzer/RH8107.md) | RH8107 | `RH8107VoidReturnValueMustNotBeDocumentedAnalyzer`, `RH8107VoidReturnValueMustNotBeDocumentedCodeFixProvider`, `DocumentationAnalysisUtilities`, `DirectDocumentationSyntaxChecker`, `DocumentationModeAnalysisContextExtensions`, `DocumentationCommentCodeFixUtilities` |
| [cli/reihitsu-format.md](cli/reihitsu-format.md) | CLI | `Program`, `ParseResult`, `FormatCommandHandler`, `ExitCodes`, `GeneratedFileUtilities`, `FileProcessResult`, `DefaultFileSystem`, `DefaultConsoleInput`, `DefaultSourceFormatter`, `DiffGenerator` |
| [playground/playground.md](playground/playground.md) | PLAY | `Home`, `MainLayout`, `PlaygroundFormatter`, `PlaygroundFormatResult`, `PlaygroundVersion` |
