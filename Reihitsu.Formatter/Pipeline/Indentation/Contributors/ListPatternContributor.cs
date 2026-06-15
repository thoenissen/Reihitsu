using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents list-pattern entries +1 level from the opening bracket
/// and aligns the closing bracket to the opening bracket's column
/// </summary>
internal sealed class ListPatternContributor : ILayoutContributor
{
    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node is not ListPatternSyntax listPattern)
        {
            return;
        }

        var openBracket = listPattern.OpenBracketToken;
        var closeBracket = listPattern.CloseBracketToken;

        var firstLine = LayoutComputer.GetLine(openBracket);
        var lastLine = LayoutComputer.GetLine(closeBracket);

        if (firstLine == lastLine)
        {
            return;
        }

        var bracketColumn = LayoutComputer.GetAdjustedColumn(openBracket, model);
        var patternColumn = bracketColumn + FormattingContext.IndentSize;

        LayoutComputer.SetIfFirstOnLine(closeBracket, bracketColumn, "ListPattern", model);

        foreach (var pattern in listPattern.Patterns)
        {
            var firstToken = pattern.GetFirstToken();

            LayoutComputer.SetIfFirstOnLine(firstToken, patternColumn, "ListPattern", model);
        }
    }

    #endregion // ILayoutContributor
}