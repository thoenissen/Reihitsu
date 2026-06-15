using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3101: Do not prefix calls with base unless local implementation exists
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer : DiagnosticAnalyzerBase<RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3101";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3101Title), nameof(AnalyzerResources.RH3101MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the base access targets the member currently being overridden
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="baseAccessExpression">Base access expression</param>
    /// <returns><see langword="true"/> when the base access targets the overridden local implementation</returns>
    private static bool TargetsOverriddenLocalImplementation(SyntaxNodeAnalysisContext context, ExpressionSyntax baseAccessExpression)
    {
        var accessedSymbol = context.SemanticModel.GetSymbolInfo(baseAccessExpression, context.CancellationToken).Symbol;

        if (accessedSymbol == null)
        {
            return false;
        }

        var containingMember = baseAccessExpression.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault();

        return containingMember switch
               {
                   MethodDeclarationSyntax methodDeclaration when context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken) is IMethodSymbol methodSymbol
                                                                  && methodSymbol.IsOverride
                                                                  && accessedSymbol is IMethodSymbol accessedMethodSymbol
                                                                  && SymbolEqualityComparer.Default.Equals(methodSymbol.OverriddenMethod?.OriginalDefinition, accessedMethodSymbol.OriginalDefinition) => true,
                   PropertyDeclarationSyntax propertyDeclaration when context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken) is IPropertySymbol propertySymbol
                                                                      && propertySymbol.IsOverride
                                                                      && accessedSymbol is IPropertySymbol accessedPropertySymbol
                                                                      && SymbolEqualityComparer.Default.Equals(propertySymbol.OverriddenProperty?.OriginalDefinition, accessedPropertySymbol.OriginalDefinition) => true,
                   EventDeclarationSyntax eventDeclaration when context.SemanticModel.GetDeclaredSymbol(eventDeclaration, context.CancellationToken) is IEventSymbol eventSymbol
                                                                && eventSymbol.IsOverride
                                                                && accessedSymbol is IEventSymbol accessedEventSymbol
                                                                && SymbolEqualityComparer.Default.Equals(eventSymbol.OverriddenEvent?.OriginalDefinition, accessedEventSymbol.OriginalDefinition) => true,
                   IndexerDeclarationSyntax indexerDeclaration when context.SemanticModel.GetDeclaredSymbol(indexerDeclaration, context.CancellationToken) is IPropertySymbol indexerSymbol
                                                                    && indexerSymbol.IsOverride
                                                                    && accessedSymbol is IPropertySymbol accessedIndexerSymbol
                                                                    && accessedIndexerSymbol.IsIndexer
                                                                    && SymbolEqualityComparer.Default.Equals(indexerSymbol.OverriddenProperty?.OriginalDefinition, accessedIndexerSymbol.OriginalDefinition) => true,
                   _ => false
               };
    }

    /// <summary>
    /// Analyze element access expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnElementAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ElementAccessExpressionSyntax { Expression: BaseExpressionSyntax } elementAccessExpression)
        {
            return;
        }

        if (TargetsOverriddenLocalImplementation(context, elementAccessExpression))
        {
            return;
        }

        var replacementExpression = elementAccessExpression.WithExpression(SyntaxFactory.ThisExpression().WithTriviaFrom(elementAccessExpression.Expression));

        ReportIfEquivalent(context, elementAccessExpression, replacementExpression, elementAccessExpression.Expression.GetLocation());
    }

    /// <summary>
    /// Analyze member access expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMemberAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax } memberAccessExpression)
        {
            return;
        }

        if (TargetsOverriddenLocalImplementation(context, memberAccessExpression))
        {
            return;
        }

        ReportIfEquivalent(context, memberAccessExpression, memberAccessExpression.Name.WithTriviaFrom(memberAccessExpression), memberAccessExpression.Expression.GetLocation());
    }

    /// <summary>
    /// Report the diagnostic when the speculative binding is unchanged
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="originalNode">Original node</param>
    /// <param name="replacementExpression">Replacement expression</param>
    /// <param name="location">Diagnostic location</param>
    private void ReportIfEquivalent(SyntaxNodeAnalysisContext context, ExpressionSyntax originalNode, ExpressionSyntax replacementExpression, Location location)
    {
        SpeculativeRebindingHelper.BuildComparisonExpressions(originalNode, replacementExpression, out var originalExpression, out var updatedExpression);

        var originalSymbolInfo = context.SemanticModel.GetSymbolInfo(originalExpression, context.CancellationToken);
        var speculativeSymbolInfo = context.SemanticModel.GetSpeculativeSymbolInfo(originalExpression.SpanStart, updatedExpression, SpeculativeBindingOption.BindAsExpression);

        if ((originalSymbolInfo.Symbol != null || originalSymbolInfo.CandidateSymbols.Length > 0)
            && (speculativeSymbolInfo.Symbol != null || speculativeSymbolInfo.CandidateSymbols.Length > 0)
            && SpeculativeRebindingHelper.AreEquivalent(originalSymbolInfo, speculativeSymbolInfo))
        {
            context.ReportDiagnostic(CreateDiagnostic(location));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterSyntaxNodeAction(OnElementAccessExpression, SyntaxKind.ElementAccessExpression);
    }

    #endregion // DiagnosticAnalyzer
}