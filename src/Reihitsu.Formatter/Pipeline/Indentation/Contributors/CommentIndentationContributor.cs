using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;
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
    /// preceding code shares that code's line, whose indentation belongs to the code rather than to the comment. The first
    /// token of a node formatted on its own has no previous token in its tree, so its comments are aligned as if no token
    /// stood in front of the node. The column is the one the token's line has in the output of
    /// the indentation phase, after the phase moved the lines of multi-line raw string literals
    /// (<see cref="LayoutModel.TryGetPlacedColumn"/>). A comment in front of a closing brace, of the closing bracket of a
    /// collection expression or a list pattern, or of the closing parenthesis of a positional or parenthesized pattern is
    /// indented one level deeper than that token, inside the content it closes; a comment in front of the closing brace of
    /// an interpolation that is not part of a multi-line raw string literal takes the brace's column
    /// </summary>
    /// <param name="token">The token whose leading trivia to inspect</param>
    /// <param name="model">The layout model</param>
    private static void AlignCommentsBeforeToken(SyntaxToken token, LayoutModel model)
    {
        var tokenLine = LayoutComputer.GetLine(token);

        if (model.TryGetPlacedColumn(tokenLine, out var alignColumn) == false)
        {
            return;
        }

        // Comments before a closing delimiter whose content is indented one level deeper than the delimiter are indented
        // into that content: a closing brace, and the closing delimiters of patterns and collection expressions, whose set
        // the blank-line phase reads from the same predicate (FMT-INDENT-006, FMT-PIPE-014). The closing brace of an
        // interpolation closes no such content unless its string is a multi-line raw string literal: the lines of any other
        // hole are continuation lines of the statement, so the comment takes the brace's column
        if ((token.IsKind(SyntaxKind.CloseBraceToken)
             && ClosesContentOneLevelDeeper(token))
            || BraceBlankLinePolicy.IsClosingTokenOfPatternOrCollectionExpression(token))
        {
            alignColumn += FormattingContext.IndentSize;
        }

        var previousToken = PrecedingTokenFacts.From(SyntaxTokenNavigationUtilities.GetPreviousToken(token));
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
                && TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, triviaIndex, previousToken.Exists, previousToken.TrailingTrivia))
            {
                model.Set(commentLine, new TokenLayout(alignColumn, "CommentAlignment"));
            }
        }
    }

    /// <summary>
    /// Determines whether a closing brace closes content that is indented one level deeper than the brace. Every closing
    /// brace does, except the closing brace of an interpolation whose interpolated string is not a multi-line raw string
    /// literal: the lines of such a hole and the brace itself are continuation lines of the statement (FMT-INDENT-006).
    /// The string the interpolation belongs to decides, not where that string stands
    /// </summary>
    /// <param name="closeBrace">The closing brace</param>
    /// <returns><see langword="true"/> if the brace closes content one level deeper; otherwise, <see langword="false"/></returns>
    private static bool ClosesContentOneLevelDeeper(SyntaxToken closeBrace)
    {
        return closeBrace.Parent is not InterpolationSyntax interpolation
               || RawStringLineUtilities.IsMultiLineRawStringLiteral(interpolation.Parent);
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        foreach (var token in node.DescendantTokens())
        {
            _cancellationToken.ThrowIfCancellationRequested();

            AlignCommentsBeforeToken(token, model);
        }
    }

    #endregion // ILayoutContributor
}