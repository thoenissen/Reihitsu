using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Helpers for the byte order mark, which the parser reads as white-space trivia at the start of the text. The phases that
/// remove white space by trivia kind use them to keep the encoding signature of the text in its place
/// </summary>
internal static class ByteOrderMarkUtilities
{
    #region Constants

    /// <summary>
    /// The byte order mark, which the parser reads as white-space trivia at the start of the text
    /// </summary>
    private const string ByteOrderMark = "\uFEFF";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Determines whether a trivia is white space that starts with the byte order mark. The parser reads the byte order
    /// mark as white space, together with any spaces or tabs that follow it on the first line, so a step that removes
    /// white space by trivia kind would drop the encoding signature of the text with it
    /// </summary>
    /// <param name="trivia">The trivia to check</param>
    /// <returns><see langword="true"/> if the trivia is white space whose text starts with the byte order mark</returns>
    internal static bool StartsWithByteOrderMark(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.WhitespaceTrivia)
               && trivia.ToFullString().StartsWith(ByteOrderMark, StringComparison.Ordinal);
    }

    /// <summary>
    /// Creates a white-space trivia that holds only the byte order mark, which a step that removes the white space it
    /// was read with puts back in its place
    /// </summary>
    /// <returns>The byte order mark trivia</returns>
    internal static SyntaxTrivia CreateByteOrderMarkTrivia()
    {
        return SyntaxFactory.Whitespace(ByteOrderMark);
    }

    /// <summary>
    /// Creates one white-space trivia that holds the byte order mark followed by the given white space, the shape the
    /// parser gives a byte order mark that the indentation of the first line follows
    /// </summary>
    /// <param name="followingWhitespace">White-space trivia that follows the byte order mark on its line</param>
    /// <returns>The byte order mark trivia, joined with the following white space</returns>
    internal static SyntaxTrivia CreateByteOrderMarkTrivia(IEnumerable<SyntaxTrivia> followingWhitespace)
    {
        return SyntaxFactory.Whitespace(string.Concat(ByteOrderMark, string.Concat(followingWhitespace.Select(static trivia => trivia.ToFullString()))));
    }

    #endregion // Methods
}