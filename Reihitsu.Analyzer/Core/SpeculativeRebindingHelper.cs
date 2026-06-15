using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared logic for analyzers that verify a member access can be rewritten without changing the bound symbol,
/// by speculatively rebinding the rewritten expression
/// </summary>
internal static class SpeculativeRebindingHelper
{
    #region Methods

    /// <summary>
    /// Determines whether the symbol infos represent the same target
    /// </summary>
    /// <param name="leftSymbolInfo">Left symbol info</param>
    /// <param name="rightSymbolInfo">Right symbol info</param>
    /// <returns><see langword="true"/> if the symbol infos match</returns>
    internal static bool AreEquivalent(SymbolInfo leftSymbolInfo, SymbolInfo rightSymbolInfo)
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
    /// Rebuilds the surrounding expression chain so the original and rewritten expressions can be compared by speculative binding
    /// </summary>
    /// <param name="node">The node whose expression is being rewritten</param>
    /// <param name="replacementExpression">The replacement for <paramref name="node"/></param>
    /// <param name="originalExpression">The outermost original expression</param>
    /// <param name="updatedExpression">The outermost expression with the replacement applied</param>
    internal static void BuildComparisonExpressions(SyntaxNode node, ExpressionSyntax replacementExpression, out ExpressionSyntax originalExpression, out ExpressionSyntax updatedExpression)
    {
        var currentNode = node;
        updatedExpression = replacementExpression;

        while (currentNode.Parent is ExpressionSyntax parentExpression)
        {
            switch (parentExpression)
            {
                case InvocationExpressionSyntax invocationExpression when invocationExpression.Expression == currentNode:
                    {
                        currentNode = invocationExpression;
                        updatedExpression = invocationExpression.WithExpression(updatedExpression);

                        continue;
                    }

                case MemberAccessExpressionSyntax memberAccessExpression when memberAccessExpression.Expression == currentNode:
                    {
                        currentNode = memberAccessExpression;
                        updatedExpression = memberAccessExpression.WithExpression(updatedExpression);

                        continue;
                    }

                case ElementAccessExpressionSyntax elementAccessExpression when elementAccessExpression.Expression == currentNode:
                    {
                        currentNode = elementAccessExpression;
                        updatedExpression = elementAccessExpression.WithExpression(updatedExpression);

                        continue;
                    }
            }

            break;
        }

        originalExpression = (ExpressionSyntax)currentNode;
    }

    #endregion // Methods
}