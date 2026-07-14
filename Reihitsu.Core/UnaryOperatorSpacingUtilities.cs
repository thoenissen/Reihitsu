using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for reasoning about the spacing between a prefix unary operator and its operand
/// </summary>
public static class UnaryOperatorSpacingUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether removing the whitespace between a prefix unary operator and the first token
    /// of its operand would glue the two tokens into a different operator. For example, removing the
    /// space from <c>- -x</c> re-lexes the two minus tokens into the pre-decrement <c>--x</c>, <c>+ +x</c>
    /// into the pre-increment <c>++x</c>, and <c>&amp; &amp;x</c> (nested address-of in unsafe code) into
    /// the logical-and operator <c>&amp;&amp;x</c>, silently changing the semantics or breaking parsing
    /// </summary>
    /// <param name="prefixOperator">The prefix unary operator token (<c>+</c>, <c>-</c>, or <c>&amp;</c>)</param>
    /// <param name="operandToken">The first token of the operand expression</param>
    /// <returns><see langword="true"/> if collapsing the spacing would glue the tokens into a different operator; otherwise, <see langword="false"/></returns>
    public static bool WouldGlueIntoDifferentOperator(SyntaxToken prefixOperator, SyntaxToken operandToken)
    {
        if (prefixOperator.IsKind(SyntaxKind.MinusToken))
        {
            return operandToken.IsKind(SyntaxKind.MinusToken)
                   || operandToken.IsKind(SyntaxKind.MinusMinusToken);
        }

        if (prefixOperator.IsKind(SyntaxKind.PlusToken))
        {
            return operandToken.IsKind(SyntaxKind.PlusToken)
                   || operandToken.IsKind(SyntaxKind.PlusPlusToken);
        }

        if (prefixOperator.IsKind(SyntaxKind.AmpersandToken))
        {
            return operandToken.IsKind(SyntaxKind.AmpersandToken)
                   || operandToken.IsKind(SyntaxKind.AmpersandAmpersandToken);
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