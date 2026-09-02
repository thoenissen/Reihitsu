using System.Linq;

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
    /// Determines whether a token is the first token on its line. The line break is looked for in both adjacent
    /// trivia lists because Roslyn attaches it to whichever side the source put it on; asking the text instead would
    /// make the answer depend on the line ending, which is exactly what LF and CRLF results must not do
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token starts a line; otherwise, <see langword="false"/></returns>
    public static bool IsFirstOnLine(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.None))
        {
            return false;
        }

        var previousToken = token.GetPreviousToken();

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
        {
            return true;
        }

        return token.LeadingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia))
               || previousToken.TrailingTrivia.Any(static trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    #endregion // Methods
}