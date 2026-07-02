using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for reasoning about the spacing between a prefix unary sign operator and its operand
/// </summary>
public static class UnaryOperatorSpacingUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether removing the whitespace between a prefix unary sign operator and the first
    /// token of its operand would glue the two tokens into a different operator. For example, removing
    /// the space from <c>- -x</c> re-lexes the two minus tokens into the pre-decrement <c>--x</c>, and
    /// <c>+ +x</c> into the pre-increment <c>++x</c>, silently changing the semantics
    /// </summary>
    /// <param name="signOperator">The prefix unary sign operator token (<c>+</c> or <c>-</c>)</param>
    /// <param name="operandToken">The first token of the operand expression</param>
    /// <returns><see langword="true"/> if collapsing the spacing would glue the tokens into a different operator; otherwise, <see langword="false"/></returns>
    public static bool WouldGlueIntoDifferentOperator(SyntaxToken signOperator, SyntaxToken operandToken)
    {
        if (signOperator.IsKind(SyntaxKind.MinusToken))
        {
            return operandToken.IsKind(SyntaxKind.MinusToken)
                   || operandToken.IsKind(SyntaxKind.MinusMinusToken);
        }

        if (signOperator.IsKind(SyntaxKind.PlusToken))
        {
            return operandToken.IsKind(SyntaxKind.PlusToken)
                   || operandToken.IsKind(SyntaxKind.PlusPlusToken);
        }

        return false;
    }

    /// <summary>
    /// Determines whether removing the whitespace after the operator of a prefix unary expression would
    /// glue the operator into a different operator together with the start of its operand
    /// </summary>
    /// <param name="node">The prefix unary expression to inspect</param>
    /// <returns><see langword="true"/> if collapsing the spacing would glue the tokens into a different operator; otherwise, <see langword="false"/></returns>
    public static bool WouldGlueIntoDifferentOperator(PrefixUnaryExpressionSyntax node)
    {
        if (node?.Operand == null)
        {
            return false;
        }

        return WouldGlueIntoDifferentOperator(node.OperatorToken, node.Operand.GetFirstToken());
    }

    #endregion // Methods
}