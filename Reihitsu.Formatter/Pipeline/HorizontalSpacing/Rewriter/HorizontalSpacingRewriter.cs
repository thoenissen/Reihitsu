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

        var nextToken = token.GetNextToken();

        if (nextToken.RawKind == 0)
        {
            return token;
        }

        if (SyntaxTriviaUtilities.AreSeparatedByEndOfLine(token, nextToken))
        {
            return token;
        }

        // Spacing is decided from the token pair alone, so a comment written between the two tokens is invisible to
        // every rule below. Normalizing the gap anyway deletes the space the author put in front of the comment -
        // NoSpaceSpacingRule collapses `_a /** x */;` to `_a/** x */;` because it only sees `_a` and `;`. RH5113
        // already documents this exemption from the analyzer side: the formatter does not collapse across a comment.
        if (SyntaxTriviaUtilities.AreSeparatedByComment(token, nextToken))
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