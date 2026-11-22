using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns the parentheses of a multi-line parenthesized pattern to the introducing construct
/// so the wrapped pattern is indented one level inside the parentheses
/// </summary>
internal sealed class ParenthesizedPatternContributor : ILayoutContributor
{
    #region Constants

    /// <summary>
    /// The source identifier for layout entries contributed by this class
    /// </summary>
    private const string ParenthesizedPatternSource = "ParenthesizedPattern";

    #endregion // Constants

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        if (node is not ParenthesizedPatternSyntax parenthesizedPattern)
        {
            return;
        }

        var openParen = parenthesizedPattern.OpenParenToken;
        var closeParen = parenthesizedPattern.CloseParenToken;

        if (LayoutComputer.GetLine(openParen) == LayoutComputer.GetLine(closeParen))
        {
            return;
        }

        var anchorColumn = PatternAnchor.GetColumn(parenthesizedPattern, model);

        LayoutComputer.SetIfFirstOnLine(openParen, anchorColumn, ParenthesizedPatternSource, model);
        LayoutComputer.SetIfFirstOnLine(closeParen, anchorColumn, ParenthesizedPatternSource, model);

        // The pattern inside the parentheses, whatever kind of pattern it is, stands one level right of the opening
        // parenthesis (FMT-INDENT-026); the opening brace of a multi-line property pattern there is the recursive pattern
        // contributor's, which gives it the same column
        var contentFirstToken = parenthesizedPattern.Pattern.GetFirstToken();

        if (contentFirstToken.Parent is PropertyPatternClauseSyntax propertyClause
            && LayoutComputer.GetLine(propertyClause.OpenBraceToken) != LayoutComputer.GetLine(propertyClause.CloseBraceToken))
        {
            return;
        }

        LayoutComputer.SetIfFirstOnLine(contentFirstToken,
                                        LayoutComputer.GetAdjustedColumn(openParen, model) + FormattingContext.IndentSize,
                                        ParenthesizedPatternSource,
                                        model);
    }

    #endregion // ILayoutContributor
}