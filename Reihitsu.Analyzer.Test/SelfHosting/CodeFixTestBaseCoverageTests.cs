using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.SelfHosting.Utilities;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Testing that every test class of an analyzer and its code fix declares, through its base class, whether the
/// code fix supports Fix All. <see cref="BatchCodeFixTestsBase{TAnalyzer, TCodeFix}"/> then requires a scenario
/// that corrects more than one diagnostic in one document, and <see cref="SingleCodeFixTestsBase{TAnalyzer, TCodeFix}"/>
/// records that the provider deliberately corrects one occurrence at a time
/// </summary>
[TestClass]
public class CodeFixTestBaseCoverageTests
{
    #region Fields

    /// <summary>
    /// Migration ledger of the test classes that still derive from
    /// <see cref="AnalyzerTestsBase{TAnalyzer, TCodeFix}"/> directly, because the compiler cannot prevent that
    /// inside the same assembly.
    /// <para>
    /// This list is temporary. It is scheduled for deletion together with this field's last entry: every
    /// migration pull request removes the classes it migrates, and the final one removes the list and this
    /// remark with it. Nothing may ever be added here — a new test class picks one of the two code-fix test
    /// bases from the start
    /// </para>
    /// </summary>
    private static readonly IReadOnlySet<string> _notYetMigratedTestClasses = new HashSet<string>(StringComparer.Ordinal)
                                                                              {
                                                                                  "Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Design.RH2004AccessModifierMustBeDeclaredAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Design.RH2005FieldsMustBePrivateAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Design.RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Design.RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Documentation.RH8204DoNotUsePlaceholderElementsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Documentation.RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Documentation.RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Documentation.RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Documentation.RH8308NoContentShouldAppearAfterClosingXmlTagsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Documentation.RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Documentation.RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Documentation.RH8402FileMustStartWithConfiguredXmlStyleCopyrightHeaderAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5001TryStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5002IfStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5003WhileStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5004DoStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5005UsingStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5006ForeachStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5007ForStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5008ReturnStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5009GotoStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5011BreakStatementsShouldBeFollowedByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5012ContinueStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5013ThrowStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5014SwitchStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5015CheckedStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5016UncheckedStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5017FixedStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5018LockStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5019YieldStatementsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5021LocalDeclarationsShouldBeFollowedByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5027WhileDoFooterMustNotBePrecededByBlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5029LocalDeclarationsShouldBePrecededByABlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5101FirstArgumentShouldBeOnSameLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5104CommentsMustBeOnTheirOwnLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5108ParameterListMustFollowDeclarationAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5202RawStringLiteralsShouldBeFormattedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5203MultiLineArgumentsShouldBeAlignedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5205StatementLambdaOpeningBraceShouldBeAlignedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5206SwitchExpressionBracesShouldBeAnchoredAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5301ObjectInitializerShouldBeFormattedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5302LogicalExpressionsShouldBeFormattedCorrectlyRepeatedFixTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5402BracesForMultiLineStatementsMustNotShareLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5403StatementMustNotBeOnSingleLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5404ElementMustNotBeOnSingleLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5405BracesMustNotBeOmittedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5407UseBracesConsistentlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5410FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5411FinalCollectionInitializerItemsMustNotHaveTrailingCommasAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5501AssemblyAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5502AssemblyAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5503ModuleAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5505ClassAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5506ClassAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5507StructAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5508StructAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5509EnumAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5510EnumAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5512ConstructorAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5514MethodAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5515PropertyAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5517FieldAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5518FieldAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5519EventAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5520EventAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5522InterfaceAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5523ParameterAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5525DelegateAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5526DelegateAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5529GenericParameterAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5530AccessorAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5601UseTabsCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5602CodeMustNotContainTrailingWhitespaceAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5603FileMustNotEndWithANewlineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6021ColonsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7101DoNotCombineFieldsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7303DoNotPlaceRegionsWithinElementsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7304RegionDirectivesMustUseConsistentIndentationAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7309RegionsShouldFollowCategoryOrderAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4002ClassNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4003StructNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4004EnumNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4005InterfaceNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4006DelegateNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4010RecordNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4011RecordStructNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4101EnumMemberCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4103MethodNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4104LocalFunctionNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4105MethodParameterCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4106PrivateFieldCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4107ProtectedFieldCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4108InternalFieldCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4109PublicFieldCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4110ConstFieldCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4111PrivatePropertyCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4112ProtectedPropertyCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4113InternalPropertyCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4114PublicPropertyCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4120RecordPrimaryConstructorParameterCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Naming.RH4121TypeParameterNameCasingAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7102ConstantsMustAppearBeforeFieldsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7104PartialElementsMustDeclareAccessModifierAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7105DeclarationKeywordsMustFollowOrderAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7106ProtectedMustComeBeforeInternalAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7107PropertyAccessorsMustFollowOrderAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7108EventAccessorsMustFollowOrderAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7205UsingStaticDirectivesMustBePlacedAtCorrectPositionAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Ordering.RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzerTests"
                                                                              };

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Gets the repository-relative source path a test class is expected to live in
    /// </summary>
    /// <param name="testClassType">Test class type</param>
    /// <returns>The repository-relative source path</returns>
    private static string GetSourcePath(Type testClassType)
    {
        const string projectNamespace = "Reihitsu.Analyzer.Test";

        var subNamespace = testClassType.Namespace?.Length > projectNamespace.Length
                               ? testClassType.Namespace[projectNamespace.Length..].TrimStart('.').Replace('.', '/')
                               : string.Empty;

        return subNamespace.Length > 0
                   ? $"{projectNamespace}/{subNamespace}/{testClassType.Name}.cs"
                   : $"{projectNamespace}/{testClassType.Name}.cs";
    }

    /// <summary>
    /// Gets the name of the code-fix test base a test class has to derive from
    /// </summary>
    /// <param name="testClass">Test class metadata</param>
    /// <returns>The name of the expected code-fix test base</returns>
    private static string GetExpectedBaseName(DiscoveredCodeFixTestClass testClass)
    {
        var expectedBase = testClass.SupportsFixAll
                               ? typeof(BatchCodeFixTestsBase<,>)
                               : typeof(SingleCodeFixTestsBase<,>);

        return expectedBase.Name[..expectedBase.Name.IndexOf('`', StringComparison.Ordinal)];
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifying every code-fix test class derives from one of the two code-fix test bases, so its Fix All
    /// coverage is decided in code instead of being left to whoever writes the next test
    /// </summary>
    [TestMethod]
    public void EveryCodeFixTestClassDerivesFromACodeFixTestsBase()
    {
        var testClasses = AnalyzerMetadataDiscovery.DiscoverCodeFixTestClasses();

        Assert.IsNotEmpty(testClasses);

        var findings = testClasses.Where(testClass => testClass.CodeFixTestsBaseDefinition is null
                                                      && _notYetMigratedTestClasses.Contains(testClass.TestClassType.FullName) is false)
                                  .Select(testClass => $"{testClass.TestClassType.FullName} must derive from {GetExpectedBaseName(testClass)} ({GetSourcePath(testClass.TestClassType)})")
                                  .ToArray();

        if (findings.Length > 0)
        {
            Assert.Fail($"The following changes are required:\n\n{string.Join(Environment.NewLine, findings)}");
        }
    }

    /// <summary>
    /// Verifying the migration ledger only lists test classes that exist and are still unmigrated, so it can
    /// only shrink and never hides a class that has already been migrated
    /// </summary>
    [TestMethod]
    public void MigrationLedgerListsOnlyUnmigratedTestClasses()
    {
        var testClasses = AnalyzerMetadataDiscovery.DiscoverCodeFixTestClasses();
        var unmigratedTestClasses = testClasses.Where(testClass => testClass.CodeFixTestsBaseDefinition is null)
                                               .Select(testClass => testClass.TestClassType.FullName)
                                               .ToHashSet(StringComparer.Ordinal);
        var findings = _notYetMigratedTestClasses.Where(testClass => unmigratedTestClasses.Contains(testClass) is false)
                                                 .OrderBy(testClass => testClass, StringComparer.Ordinal)
                                                 .Select(testClass => $"{testClass} is listed in the migration ledger, but it does not exist or already derives from a code-fix test base")
                                                 .ToArray();

        if (findings.Length > 0)
        {
            Assert.Fail($"The following entries have to be removed from the migration ledger:\n\n{string.Join(Environment.NewLine, findings)}");
        }
    }

    #endregion // Tests
}