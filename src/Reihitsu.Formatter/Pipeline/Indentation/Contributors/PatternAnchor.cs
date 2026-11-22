using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Computes the column a multi-line pattern's delimiters should align to, based on the construct
/// that introduces the pattern
/// </summary>
internal static class PatternAnchor
{
    #region Methods

    /// <summary>
    /// Computes the column the given pattern's delimiters should align to (FMT-INDENT-026). Combinator (<c>and</c>/<c>or</c>)
    /// and <c>not</c> wrappers are skipped so the anchor matches the introducing construct: the <c>is</c>
    /// keyword for <c>is</c> expressions, the containing subpattern for nested patterns, the open
    /// parenthesis (plus one level) for parenthesized patterns, and otherwise the first token of the outermost
    /// pattern the wrappers lead to — in a switch-expression arm, an element of a list pattern, or a <c>case</c> label,
    /// the <c>P</c> of <c>P and { … }</c> or the <c>not</c> of <c>not { … }</c>, and the pattern's own first token where
    /// no wrapper surrounds it
    /// </summary>
    /// <param name="pattern">The pattern whose anchor column is requested</param>
    /// <param name="model">The layout model</param>
    /// <returns>The anchor column</returns>
    public static int GetColumn(PatternSyntax pattern, LayoutModel model)
    {
        var current = pattern;

        while (current.Parent is BinaryPatternSyntax or UnaryPatternSyntax)
        {
            current = (PatternSyntax)current.Parent;
        }

        switch (current.Parent)
        {
            case IsPatternExpressionSyntax isExpression:
                return LayoutComputer.GetAdjustedColumn(isExpression.IsKeyword, model);

            case SubpatternSyntax subpattern:
                return LayoutComputer.GetAdjustedColumn(subpattern.GetFirstToken(), model);

            case ParenthesizedPatternSyntax parenthesized:
                return LayoutComputer.GetAdjustedColumn(parenthesized.OpenParenToken, model) + FormattingContext.IndentSize;
        }

        return LayoutComputer.GetAdjustedColumn(current.GetFirstToken(), model);
    }

    #endregion // Methods
}