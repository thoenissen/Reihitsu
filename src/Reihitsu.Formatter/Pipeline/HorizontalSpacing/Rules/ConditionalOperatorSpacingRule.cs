using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space on each side of the <c>?</c> and the <c>:</c> of a conditional expression (FMT-SPACE-018). The
/// question mark of a nullable type or a conditional access, and the <c>??</c> operator, are not a conditional operator
/// </summary>
internal sealed class ConditionalOperatorSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines whether the token is the <c>?</c> or the <c>:</c> of a conditional expression
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if the token is a conditional operator; otherwise, <see langword="false"/></returns>
    private static bool IsConditionalOperator(SyntaxToken token)
    {
        return token.Parent is ConditionalExpressionSyntax conditional
               && (token == conditional.QuestionToken || token == conditional.ColonToken);
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (IsConditionalOperator(left) || IsConditionalOperator(right))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}