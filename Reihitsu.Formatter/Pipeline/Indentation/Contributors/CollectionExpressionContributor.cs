using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents collection expression elements +1 level from the opening bracket
/// and aligns the closing bracket to the opening bracket's column.
/// </summary>
internal sealed class CollectionExpressionContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        if (node is not CollectionExpressionSyntax collection)
        {
            return;
        }

        var openBracket = collection.OpenBracketToken;
        var closeBracket = collection.CloseBracketToken;

        var firstLine = LayoutComputer.GetLine(openBracket);
        var lastLine = LayoutComputer.GetLine(closeBracket);

        if (firstLine == lastLine)
        {
            return;
        }

        var bracketColumn = LayoutComputer.GetAdjustedColumn(openBracket, model);
        var elementColumn = bracketColumn + FormattingContext.IndentSize;

        LayoutComputer.SetIfFirstOnLine(closeBracket, bracketColumn, "CollectionExpression", model);

        foreach (var element in collection.Elements)
        {
            var firstToken = element.GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(firstToken, elementColumn, "CollectionExpression", model);
        }
    }

    #endregion // ILayoutContributor
}