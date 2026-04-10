using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing;

/// <summary>
/// Syntax rewriter that visits each token and normalizes horizontal spacing
/// between adjacent tokens on the same line.
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
    /// <param name="cancellationToken">Cancellation token.</param>
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

        if (HorizontalSpacingPhase.AreSeparatedByEndOfLine(token, nextToken))
        {
            return token;
        }

        var desiredSpaces = HorizontalSpacingPhase.GetDesiredSpacesAfter(token, nextToken);

        if (desiredSpaces.HasValue)
        {
            return HorizontalSpacingPhase.SetTrailingWhitespace(token, desiredSpaces.Value);
        }

        // Collapse multiple consecutive spaces to a single space
        return HorizontalSpacingPhase.CollapseMultipleTrailingSpaces(token);
    }

    #endregion // CSharpSyntaxRewriter
}