using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns comments to the indentation of the code they precede.
/// For each comment trivia on its own line, the indentation is set to match
/// the next non-comment token's computed indentation.
/// </summary>
internal sealed class CommentIndentationContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Aligns comment trivia in a token's leading trivia to the token's own indentation.
    /// </summary>
    /// <param name="token">The token whose leading trivia to inspect.</param>
    /// <param name="model">The layout model.</param>
    private static void AlignCommentsBeforeToken(SyntaxToken token, LayoutModel model)
    {
        var tokenLine = LayoutComputer.GetLine(token);

        if (model.TryGetLayout(tokenLine, out var tokenLayout) == false)
        {
            return;
        }

        foreach (var trivia in token.LeadingTrivia)
        {
            if (IsComment(trivia) == false)
            {
                continue;
            }

            var commentLine = trivia.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (commentLine != tokenLine)
            {
                model.Set(commentLine, new TokenLayout(tokenLayout.Column, "CommentAlignment"));
            }
        }
    }

    /// <summary>
    /// Determines whether a trivia is a comment (single-line, multi-line, or documentation).
    /// </summary>
    /// <param name="trivia">The trivia to check.</param>
    /// <returns><see langword="true"/> if the trivia is a comment; otherwise, <see langword="false"/>.</returns>
    private static bool IsComment(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        foreach (var token in node.DescendantTokens())
        {
            AlignCommentsBeforeToken(token, model);
        }
    }

    #endregion // ILayoutContributor
}