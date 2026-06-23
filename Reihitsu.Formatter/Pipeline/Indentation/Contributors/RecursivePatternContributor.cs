using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents recursive (property) pattern subpatterns +1 level from the anchor column
/// and aligns the opening and closing braces to the anchor column.
/// The anchor is the <c>is</c> keyword for <c>is</c> expressions, the <c>case</c> keyword for
/// case patterns, the containing subpattern for nested patterns, and the pattern's own first
/// token otherwise (for example switch expression arms)
/// </summary>
internal sealed class RecursivePatternContributor : ILayoutContributor
{
    #region Constants

    /// <summary>
    /// The source identifier for layout entries contributed by this class
    /// </summary>
    private const string RecursivePatternSource = "RecursivePattern";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Computes the column the recursive pattern's braces should align to
    /// </summary>
    /// <param name="pattern">The recursive pattern</param>
    /// <param name="model">The layout model</param>
    /// <returns>The anchor column</returns>
    private static int GetAnchorColumn(RecursivePatternSyntax pattern, LayoutModel model)
    {
        PatternSyntax current = pattern;

        // Walk out of enclosing binary patterns (and/or) so the anchor matches the introducing construct
        while (current.Parent is BinaryPatternSyntax binaryParent)
        {
            current = binaryParent;
        }

        switch (current.Parent)
        {
            case IsPatternExpressionSyntax isExpression:
                return LayoutComputer.GetAdjustedColumn(isExpression.IsKeyword, model);

            case CasePatternSwitchLabelSyntax caseLabel:
                return LayoutComputer.GetAdjustedColumn(caseLabel.Keyword, model);

            case SubpatternSyntax subpattern:
                return LayoutComputer.GetAdjustedColumn(subpattern.GetFirstToken(), model);
        }

        return LayoutComputer.GetAdjustedColumn(pattern.GetFirstToken(), model);
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node is not RecursivePatternSyntax recursivePattern)
        {
            return;
        }

        var propertyClause = recursivePattern.PropertyPatternClause;

        if (propertyClause == null)
        {
            return;
        }

        var openBrace = propertyClause.OpenBraceToken;
        var closeBrace = propertyClause.CloseBraceToken;

        if (LayoutComputer.GetLine(openBrace) == LayoutComputer.GetLine(closeBrace))
        {
            return;
        }

        var anchorColumn = GetAnchorColumn(recursivePattern, model);

        LayoutComputer.SetIfFirstOnLine(openBrace, anchorColumn, RecursivePatternSource, model);
        LayoutComputer.SetIfFirstOnLine(closeBrace, anchorColumn, RecursivePatternSource, model);

        var subpatternColumn = anchorColumn + FormattingContext.IndentSize;

        foreach (var subpattern in propertyClause.Subpatterns)
        {
            var firstToken = subpattern.GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(firstToken, subpatternColumn, RecursivePatternSource, model);
        }
    }

    #endregion // ILayoutContributor
}