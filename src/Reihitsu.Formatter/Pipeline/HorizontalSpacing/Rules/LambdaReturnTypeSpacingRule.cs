using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space between the explicit return type of a lambda and the <c>(</c> of its parameter list (FMT-SPACE-026)
/// </summary>
internal sealed class LambdaReturnTypeSpacingRule : ISpacingRule
{
    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (right.Parent is ParameterListSyntax { Parent: ParenthesizedLambdaExpressionSyntax { ReturnType: { } returnType } } parameterList
            && right == parameterList.OpenParenToken
            && left == returnType.GetLastToken())
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}