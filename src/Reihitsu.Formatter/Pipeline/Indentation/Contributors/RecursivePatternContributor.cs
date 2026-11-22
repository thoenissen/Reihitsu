using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Indents recursive (property) pattern subpatterns +1 level from the anchor column
/// and aligns the opening and closing braces to the anchor column.
/// The anchor column is resolved by <see cref="PatternAnchor"/>. The subpatterns of a multi-line positional pattern clause
/// are indented +1 level from its opening parenthesis, and its closing parenthesis is aligned to the opening one
/// (FMT-INDENT-025)
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
    /// Indents the subpatterns of a positional pattern clause that spans several lines one level deeper than its opening
    /// parenthesis and aligns its closing parenthesis to the opening one, as for a list pattern (FMT-INDENT-025)
    /// </summary>
    /// <param name="positionalClause">The positional pattern clause</param>
    /// <param name="model">The layout model</param>
    private static void ContributePositionalClause(PositionalPatternClauseSyntax positionalClause, LayoutModel model)
    {
        var openParen = positionalClause.OpenParenToken;
        var closeParen = positionalClause.CloseParenToken;

        if (LayoutComputer.GetLine(openParen) == LayoutComputer.GetLine(closeParen))
        {
            return;
        }

        var parenColumn = LayoutComputer.GetAdjustedColumn(openParen, model);
        var subpatternColumn = parenColumn + FormattingContext.IndentSize;

        LayoutComputer.SetIfFirstOnLine(closeParen, parenColumn, RecursivePatternSource, model);

        foreach (var subpattern in positionalClause.Subpatterns)
        {
            LayoutComputer.SetIfFirstOnLine(subpattern.GetFirstToken(), subpatternColumn, RecursivePatternSource, model);
        }
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

        if (recursivePattern.PositionalPatternClause != null)
        {
            ContributePositionalClause(recursivePattern.PositionalPatternClause, model);
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