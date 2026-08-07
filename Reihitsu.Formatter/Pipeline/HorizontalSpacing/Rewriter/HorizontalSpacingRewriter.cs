using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.HorizontalSpacing.Utilities;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rewriter;

/// <summary>
/// Syntax rewriter that visits each token and normalizes horizontal spacing
/// between adjacent tokens on the same line. It delegates the spacing decision to
/// <see cref="SpacingPolicy"/> and the trivia edit to <see cref="TrailingWhitespaceWriter"/>
/// </summary>
internal sealed class HorizontalSpacingRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public HorizontalSpacingRewriter(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Removes the whitespace a token carries between the last comment in its own leading trivia and the token
    /// itself, when no space is allowed in front of it
    /// </summary>
    /// <param name="token">The token whose leading trivia should be trimmed</param>
    /// <returns>The token with the trailing gap of its leading trivia removed</returns>
    /// <remarks>
    /// The gap in front of a token is split across two trivia lists once a comment sits in it: the part before the
    /// comment belongs to the previous token's trailing trivia, the part after it to this token's leading trivia.
    /// The spacing rules only ever rewrite the first, so <c>[Obsolete /** why */ ]</c> keeps a space RH6014 reports
    /// however the previous token is normalized. Trimming here is what the exemption above owes the analyzers
    /// (issue #591)
    /// </remarks>
    private static SyntaxToken TrimGapBeforeToken(SyntaxToken token)
    {
        var leadingTrivia = token.LeadingTrivia;

        if (leadingTrivia.Count < 2
            || leadingTrivia[leadingTrivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia) == false
            || ReihitsuFormatterHelpers.IsCommentTrivia(leadingTrivia[leadingTrivia.Count - 2]) == false)
        {
            return token;
        }

        var previousToken = token.GetPreviousToken();

        if (previousToken.RawKind == 0
            || SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token)
            || SpacingPolicy.GetDesiredSpacesAfter(previousToken, token) != 0)
        {
            return token;
        }

        return token.WithLeadingTrivia(leadingTrivia.RemoveAt(leadingTrivia.Count - 1));
    }

    #endregion // Methods

    #region CSharpSyntaxRewriter

    /// <inheritdoc/>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        token = base.VisitToken(token);

        if (token.RawKind == 0)
        {
            return token;
        }

        token = TrimGapBeforeToken(token);

        var nextToken = token.GetNextToken();

        if (nextToken.RawKind == 0)
        {
            return token;
        }

        if (SyntaxTriviaUtilities.AreSeparatedByEndOfLine(token, nextToken))
        {
            return token;
        }

        // Spacing is decided from the token pair alone, so a comment the next token carries in its leading trivia is
        // invisible to every rule below. The rules answer for the gap in front of that token, but the whitespace they
        // rewrite sits in front of the comment, so NoSpaceSpacingRule collapses `_a /** x */;` to `_a/** x */;` -
        // it only ever sees `_a` and `;`. Exempting that gap keeps the author's space in front of the comment.
        //
        // The exemption is deliberately one-sided. A comment in the left token's own trailing trivia does not move
        // the whitespace the rules target, so exempting that side as well would suppress the normalization every
        // "no space before X" rule performs and leave the formatter emitting output RH6002, RH6003 and RH6014 report.
        if (nextToken.LeadingTrivia.Any(ReihitsuFormatterHelpers.IsCommentTrivia))
        {
            return token;
        }

        var desiredSpaces = SpacingPolicy.GetDesiredSpacesAfter(token, nextToken);

        if (desiredSpaces.HasValue)
        {
            return TrailingWhitespaceWriter.SetTrailingWhitespace(token, desiredSpaces.Value);
        }

        // Collapse multiple consecutive spaces to a single space
        return TrailingWhitespaceWriter.CollapseMultipleTrailingSpaces(token);
    }

    #endregion // CSharpSyntaxRewriter
}