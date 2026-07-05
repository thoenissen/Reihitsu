using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

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
    /// Determines whether two adjacent tokens are separated by an end-of-line trivia,
    /// meaning they are on different lines and spacing should not be adjusted. This is a traversal
    /// guard rather than a spacing rule: when it holds, the rewriter leaves the token untouched and
    /// does not even collapse multiple spaces, so it lives with the rewriter and not in
    /// <see cref="SpacingPolicy"/>
    /// </summary>
    /// <param name="token">The first token</param>
    /// <param name="nextToken">The second token</param>
    /// <returns><see langword="true"/> if the tokens are on different lines; otherwise, <see langword="false"/></returns>
    private static bool AreSeparatedByEndOfLine(SyntaxToken token, SyntaxToken nextToken)
    {
        return token.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
               || nextToken.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
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

        var nextToken = token.GetNextToken();

        if (nextToken.RawKind == 0)
        {
            return token;
        }

        if (AreSeparatedByEndOfLine(token, nextToken))
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