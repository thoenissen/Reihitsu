using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns comments to the indentation of the code they precede.
/// For each comment trivia that starts its own line, the indentation is set to match
/// the next non-comment token's computed indentation
/// </summary>
internal sealed class CommentIndentationContributor : ILayoutContributor
{
    #region Fields

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public CommentIndentationContributor(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Aligns comment trivia in a token's leading trivia to the token's own indentation. Only a comment that starts its line
    /// is aligned: a documentation comment that Roslyn files as leading trivia of the token but that is written behind the
    /// preceding code shares that code's line, whose indentation belongs to the code rather than to the comment
    /// </summary>
    /// <param name="token">The token whose leading trivia to inspect</param>
    /// <param name="model">The layout model</param>
    /// <param name="context">The formatting context</param>
    private static void AlignCommentsBeforeToken(SyntaxToken token, LayoutModel model, FormattingContext context)
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

        var leadingTrivia = token.LeadingTrivia;

        for (var triviaIndex = 0; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            var trivia = leadingTrivia[triviaIndex];

            if (SyntaxTriviaUtilities.IsCommentTrivia(trivia) == false)
            {
                continue;
            }

            var commentLine = trivia.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (commentLine != tokenLine
                && StartsLine(token, triviaIndex, context))
            {
                model.Set(commentLine, new TokenLayout(alignColumn, "CommentAlignment"));
            }
        }
    }

    /// <summary>
    /// Determines whether the trivia at the specified index of a token's leading trivia starts its line, so that only
    /// whitespace precedes it on that line. The leading trivia before it is inspected first; a line break, a single-line
    /// documentation comment, which carries its own line break, or a directive ends the previous line. When only whitespace
    /// precedes it in the leading trivia, the token before decides: the trivia starts its line when there is none or when
    /// that token ends its line. The first token of a node formatted on its own has no previous token in its tree, so its
    /// preceding token in the document is read from <see cref="FormattingContext.RootPrecedingToken"/>
    /// </summary>
    /// <param name="token">The token whose leading trivia holds the trivia</param>
    /// <param name="triviaIndex">The index of the trivia in the leading trivia</param>
    /// <param name="context">The formatting context</param>
    /// <returns><see langword="true"/> if the trivia starts its line; otherwise, <see langword="false"/></returns>
    private static bool StartsLine(SyntaxToken token, int triviaIndex, FormattingContext context)
    {
        var leadingTrivia = token.LeadingTrivia;

        for (var previousIndex = triviaIndex - 1; previousIndex >= 0; previousIndex--)
        {
            var previousTrivia = leadingTrivia[previousIndex];

            if (previousTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            return previousTrivia.IsKind(SyntaxKind.EndOfLineTrivia)
                   || previousTrivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                   || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(previousTrivia);
        }

        var previousToken = PrecedingTokenFacts.Resolve(token.GetPreviousToken(), context);

        return previousToken.Exists == false
               || previousToken.EndsLine;
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        foreach (var token in node.DescendantTokens())
        {
            _cancellationToken.ThrowIfCancellationRequested();

            AlignCommentsBeforeToken(token, model, context);
        }
    }

    #endregion // ILayoutContributor
}