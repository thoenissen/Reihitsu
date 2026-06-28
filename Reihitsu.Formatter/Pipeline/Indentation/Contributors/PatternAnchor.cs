using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Computes the column a multi-line pattern's delimiters should align to, based on the construct
/// that introduces the pattern
/// </summary>
internal static class PatternAnchor
{
    #region Methods

    /// <summary>
    /// Computes the column the given pattern's delimiters should align to. Combinator (<c>and</c>/<c>or</c>)
    /// and <c>not</c> wrappers are skipped so the anchor matches the introducing construct: the <c>is</c>
    /// keyword for <c>is</c> expressions, the containing subpattern for nested patterns, the open
    /// parenthesis (plus one level) for parenthesized patterns, and the pattern's own first token
    /// otherwise (for example switch expression arms and case patterns, whose brace stays on the label line)
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

        return LayoutComputer.GetAdjustedColumn(pattern.GetFirstToken(), model);
    }

    #endregion // Methods
}