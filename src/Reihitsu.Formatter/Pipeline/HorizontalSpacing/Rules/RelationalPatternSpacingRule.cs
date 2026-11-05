using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space between the operator of a relational pattern (<c>&lt;</c>, <c>&lt;=</c>, <c>&gt;</c>, <c>&gt;=</c>)
/// and its operand (FMT-SPACE-023). The space in front of the operator is not decided by this rule
/// </summary>
internal sealed class RelationalPatternSpacingRule : ISpacingRule
{
    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (left.Parent is RelationalPatternSyntax relationalPattern
            && left == relationalPattern.OperatorToken)
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}