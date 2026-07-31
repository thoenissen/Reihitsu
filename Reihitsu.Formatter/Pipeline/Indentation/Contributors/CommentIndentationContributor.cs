using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns comments to the indentation of the code they precede.
/// For each comment trivia on its own line, the indentation is set to match
/// the next non-comment token's computed indentation
/// </summary>
internal sealed class CommentIndentationContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Aligns comment trivia in a token's leading trivia to the token's own indentation
    /// </summary>
    /// <param name="token">The token whose leading trivia to inspect</param>
    /// <param name="model">The layout model</param>
    private static void AlignCommentsBeforeToken(SyntaxToken token, LayoutModel model)
    {
        var tokenLine = LayoutComputer.GetLine(token);

        if (model.TryGetLayout(tokenLine, out var tokenLayout) == false)
        {
            return;
        }

        var alignColumn = tokenLayout.Column;

        // Comments before a closing brace should be indented inside the block
        if (token.IsKind(SyntaxKind.CloseBraceToken))
        {
            alignColumn += FormattingContext.IndentSize;
        }

        foreach (var trivia in token.LeadingTrivia)
        {
            if (SyntaxTriviaUtilities.IsCommentTrivia(trivia) == false)
            {
                continue;
            }

            var commentLine = trivia.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (commentLine != tokenLine)
            {
                model.Set(commentLine, new TokenLayout(alignColumn, "CommentAlignment"));
            }
        }
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        foreach (var token in node.DescendantTokens())
        {
            AlignCommentsBeforeToken(token, model);
        }
    }

    #endregion // ILayoutContributor
}