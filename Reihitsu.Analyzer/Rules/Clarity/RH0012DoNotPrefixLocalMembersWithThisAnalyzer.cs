using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH0012: Do not prefix local members with this.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0012DoNotPrefixLocalMembersWithThisAnalyzer : DiagnosticAnalyzerBase<RH0012DoNotPrefixLocalMembersWithThisAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0012";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0012DoNotPrefixLocalMembersWithThisAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH0012Title), nameof(AnalyzerResources.RH0012MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the symbol infos represent the same target.
    /// </summary>
    /// <param name="leftSymbolInfo">Left symbol info</param>
    /// <param name="rightSymbolInfo">Right symbol info</param>
    /// <returns><see langword="true"/> if the symbol infos match</returns>
    private static bool AreEquivalent(SymbolInfo leftSymbolInfo, SymbolInfo rightSymbolInfo)
    {
        if (leftSymbolInfo.Symbol != null
            && rightSymbolInfo.Symbol != null)
        {
            return SymbolEqualityComparer.Default.Equals(leftSymbolInfo.Symbol.OriginalDefinition, rightSymbolInfo.Symbol.OriginalDefinition);
        }

        if (leftSymbolInfo.CandidateSymbols.Length != rightSymbolInfo.CandidateSymbols.Length)
        {
            return false;
        }

        for (var candidateIndex = 0; candidateIndex < leftSymbolInfo.CandidateSymbols.Length; candidateIndex++)
        {
            if (SymbolEqualityComparer.Default.Equals(leftSymbolInfo.CandidateSymbols[candidateIndex].OriginalDefinition, rightSymbolInfo.CandidateSymbols[candidateIndex].OriginalDefinition) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Build the comparison expressions.
    /// </summary>
    /// <param name="memberAccessExpression">Member access expression</param>
    /// <param name="originalExpression">Original expression</param>
    /// <param name="replacementExpression">Replacement expression</param>
    private static void BuildComparisonExpressions(MemberAccessExpressionSyntax memberAccessExpression, out ExpressionSyntax originalExpression, out ExpressionSyntax replacementExpression)
    {
        var currentNode = (SyntaxNode)memberAccessExpression;
        replacementExpression = memberAccessExpression.Name.WithTriviaFrom(memberAccessExpression);

        while (currentNode.Parent is ExpressionSyntax parentExpression)
        {
            switch (parentExpression)
            {
                case InvocationExpressionSyntax invocationExpression when invocationExpression.Expression == currentNode:
                    {
                        currentNode = invocationExpression;
                        replacementExpression = invocationExpression.WithExpression(replacementExpression);

                        continue;
                    }

                case MemberAccessExpressionSyntax nestedMemberAccessExpression when nestedMemberAccessExpression.Expression == currentNode:
                    {
                        currentNode = nestedMemberAccessExpression;
                        replacementExpression = nestedMemberAccessExpression.WithExpression(replacementExpression);

                        continue;
                    }

                case ElementAccessExpressionSyntax elementAccessExpression when elementAccessExpression.Expression == currentNode:
                    {
                        currentNode = elementAccessExpression;
                        replacementExpression = elementAccessExpression.WithExpression(replacementExpression);

                        continue;
                    }
            }

            break;
        }

        originalExpression = (ExpressionSyntax)currentNode;
    }

    /// <summary>
    /// Analyze member access expressions.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnMemberAccessExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccessExpression)
        {
            return;
        }

        BuildComparisonExpressions(memberAccessExpression, out var originalExpression, out var replacementExpression);

        var originalSymbolInfo = context.SemanticModel.GetSymbolInfo(originalExpression, context.CancellationToken);
        var speculativeSymbolInfo = context.SemanticModel.GetSpeculativeSymbolInfo(originalExpression.SpanStart, replacementExpression, SpeculativeBindingOption.BindAsExpression);

        if ((originalSymbolInfo.Symbol != null || originalSymbolInfo.CandidateSymbols.Length > 0)
            && (speculativeSymbolInfo.Symbol != null || speculativeSymbolInfo.CandidateSymbols.Length > 0)
            && AreEquivalent(originalSymbolInfo, speculativeSymbolInfo))
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