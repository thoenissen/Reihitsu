using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Core;
using Reihitsu.Formatter.Pipeline.HorizontalSpacing.Utilities;

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
    /// Applies the spacing decision for the gap in front of a token to the whitespace the token carries between the last
    /// comment in its own leading trivia and the token itself: removed when no space is allowed in front of the token, and
    /// set to the decided number of spaces otherwise
    /// </summary>
    /// <param name="token">The token whose leading trivia should be normalized</param>
    /// <returns>The token with the gap behind the comment in its leading trivia normalized</returns>
    /// <remarks>
    /// The gap in front of a token is split across two trivia lists once a comment sits in it: the part before the
    /// comment belongs to the previous token's trailing trivia, the part after it to this token's leading trivia.
    /// The whitespace in front of the comment is kept as written (FMT-SPACE-014), so the rule's decision is applied
    /// to the part behind the comment: <c>[Obsolete /** why */ ]</c> loses the space RH6014 reports, and
    /// <c>case (1, 2) /** why */when b:</c> gets the space in front of <c>when</c>. A gap without a decision keeps its
    /// whitespace. A gap that holds a line break — also the one that ends a <c>///</c> comment — is indentation and stays
    /// as it is (FMT-SPACE-001)
    /// </remarks>
    private static SyntaxToken NormalizeGapBehindLeadingComment(SyntaxToken token)
    {
        var leadingTrivia = token.LeadingTrivia;
        var whitespaceCount = leadingTrivia.Count > 0 && leadingTrivia[leadingTrivia.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia) ? 1 : 0;
        var commentIndex = leadingTrivia.Count - whitespaceCount - 1;

        if (commentIndex < 0
            || SyntaxTriviaUtilities.IsCommentTrivia(leadingTrivia[commentIndex]) == false)
        {
            return token;
        }

        var previousToken = token.GetPreviousToken();

        if (previousToken.RawKind == 0
            || SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token))
        {
            return token;
        }

        var desiredSpaces = SpacingPolicy.GetDesiredSpacesAfter(previousToken, token);

        if (desiredSpaces.HasValue == false)
        {
            return token;
        }

        var desiredWhitespace = new string(' ', desiredSpaces.Value);
        var currentWhitespace = whitespaceCount == 1 ? leadingTrivia[leadingTrivia.Count - 1].ToFullString() : string.Empty;

        if (currentWhitespace == desiredWhitespace)
        {
            return token;
        }

        var newLeadingTrivia = whitespaceCount == 1
                                   ? leadingTrivia.RemoveAt(leadingTrivia.Count - 1)
                                   : leadingTrivia;

        if (desiredSpaces.Value > 0)
        {
            newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.Whitespace(desiredWhitespace));
        }

        return token.WithLeadingTrivia(newLeadingTrivia);
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

        // The gap behind the token is decided from the token as it stands in the tree: a rewritten token is detached, so
        // it has no parent and no next token for the rules to inspect
        var originalToken = token;
        var nextToken = originalToken.GetNextToken();

        token = NormalizeGapBehindLeadingComment(token);

        if (nextToken.RawKind == 0)
        {
            return token;
        }

        if (SyntaxTriviaUtilities.AreSeparatedByEndOfLine(originalToken, nextToken))
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
        if (nextToken.LeadingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia))
        {
            return token;
        }

        var desiredSpaces = SpacingPolicy.GetDesiredSpacesAfter(originalToken, nextToken);

        if (desiredSpaces.HasValue)
        {
            return TrailingWhitespaceWriter.SetTrailingWhitespace(token, desiredSpaces.Value);
        }

        // Collapse multiple consecutive spaces to a single space
        return TrailingWhitespaceWriter.CollapseMultipleTrailingSpaces(token);
    }

    #endregion // CSharpSyntaxRewriter
}