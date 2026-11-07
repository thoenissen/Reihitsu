using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Core;

/// <summary>
/// Shared line and column policy for position-sensitive analyzers and formatting phases. The indentation surfaces on
/// both sides of the package — the RH5204 analyzer and the formatter's layout pass — have to agree on which token owns
/// a line before they can agree on where that line starts, so the ownership question is answered once here rather than
/// once per engine
/// </summary>
public static class SyntaxTokenPositionUtilities
{
    #region Methods

    /// <summary>
    /// Gets the 0-based line number of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Line number</returns>
    public static int GetLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Gets the 0-based column of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Column</returns>
    public static int GetColumn(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Character;
    }

    /// <summary>
    /// Determines whether a token is the first token on its line: a token with text that no other token with text precedes
    /// on its line. A line break separates it from the token with text in front of it (<see cref="SyntaxTriviaUtilities.AreSeparatedByEndOfLine"/>),
    /// whichever trivia carries that line break — an end-of-line trivia, or the line break that ends a <c>///</c> comment,
    /// a directive, or disabled text — or no token precedes it. A comment may stand in front of it on its line; a line break
    /// inside the text of a multi-line comment does not count, so a token behind the closing line of such a comment is not
    /// first on its line. A token without text, such as an omitted type argument or the end of the file, is never first on
    /// its line, and a token behind one is judged against the token with text in front of it (SHARED-BLANK-008). The answer
    /// depends on trivia kinds and on the line terminators of the trivia text, never on the line ending of the file
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token is the first token on its line; otherwise, <see langword="false"/></returns>
    public static bool IsFirstOnLine(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.None)
            || token.Span.IsEmpty)
        {
            return false;
        }

        var previousToken = token.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None))
        {
            return true;
        }

        return SyntaxTriviaUtilities.AreSeparatedByEndOfLine(previousToken, token);
    }

    /// <summary>
    /// Determines whether a token starts its line (SHARED-BLANK-008): it has text, and nothing but whitespace stands between
    /// it and the last line break in front of it, whichever trivia carries that line break. A comment in front of it on
    /// its line, or the closing line of a multi-line comment, means the token does not start its line. Without a preceding
    /// token and without a line break in front of it, the token starts its line when nothing but whitespace precedes it
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token starts its line; otherwise, <see langword="false"/></returns>
    public static bool StartsLine(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.None)
            || token.Span.IsEmpty)
        {
            return false;
        }

        return TokenGapAnalysis.StartsLineAtLeadingTriviaIndex(token, token.LeadingTrivia.Count);
    }

    #endregion // Methods
}