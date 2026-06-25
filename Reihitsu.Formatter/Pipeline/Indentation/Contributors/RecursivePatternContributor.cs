using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents recursive (property) pattern subpatterns +1 level from the anchor column
/// and aligns the opening and closing braces to the anchor column.
/// The anchor column is resolved by <see cref="PatternAnchor"/>
/// </summary>
internal sealed class RecursivePatternContributor : ILayoutContributor
{
    #region Constants

    /// <summary>
    /// The source identifier for layout entries contributed by this class
    /// </summary>
    private const string RecursivePatternSource = "RecursivePattern";

    #endregion // Constants

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

        var anchorColumn = PatternAnchor.GetColumn(recursivePattern, model);

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