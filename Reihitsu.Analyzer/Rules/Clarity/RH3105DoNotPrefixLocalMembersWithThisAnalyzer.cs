using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3105: Do not prefix local members with this
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3105DoNotPrefixLocalMembersWithThisAnalyzer : DiagnosticAnalyzerBase<RH3105DoNotPrefixLocalMembersWithThisAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3105";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3105DoNotPrefixLocalMembersWithThisAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3105Title), nameof(AnalyzerResources.RH3105MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze member access expressions
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMemberAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccessExpression)
        {
            return;
        }

        SpeculativeRebindingHelper.BuildComparisonExpressions(memberAccessExpression, memberAccessExpression.Name.WithTriviaFrom(memberAccessExpression), out var originalExpression, out var replacementExpression);

        var originalSymbolInfo = context.SemanticModel.GetSymbolInfo(originalExpression, context.CancellationToken);
        var speculativeSymbolInfo = context.SemanticModel.GetSpeculativeSymbolInfo(originalExpression.SpanStart, replacementExpression, SpeculativeBindingOption.BindAsExpression);

        if ((originalSymbolInfo.Symbol != null || originalSymbolInfo.CandidateSymbols.Length > 0)
            && (speculativeSymbolInfo.Symbol != null || speculativeSymbolInfo.CandidateSymbols.Length > 0)
            && SpeculativeRebindingHelper.AreEquivalent(originalSymbolInfo, speculativeSymbolInfo))
        {
            context.ReportDiagnostic(CreateDiagnostic(memberAccessExpression.Expression.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
    }

    #endregion // DiagnosticAnalyzer
}