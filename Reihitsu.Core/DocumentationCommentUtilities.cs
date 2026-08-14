using System;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Provides shared helpers for working with XML documentation comment lines
/// </summary>
public static class DocumentationCommentUtilities
{
    #region Constants

    /// <summary>
    /// The exterior marker that introduces a single-line documentation comment
    /// </summary>
    public const string DocumentationExterior = "///";

    /// <summary>
    /// The exterior marker that introduces an ordinary single-line comment
    /// </summary>
    public const string SingleLineCommentExterior = "//";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Determines whether every continuation <c>///</c> marker uses the requested indentation column
    /// </summary>
    /// <param name="documentationTrivia">Single-line documentation comment trivia</param>
    /// <param name="column">Required continuation-marker column</param>
    /// <returns><see langword="true"/> when every continuation marker is aligned</returns>
    public static bool AreContinuationExteriorMarkersAligned(SyntaxTrivia documentationTrivia, int column)
    {
        foreach (var exteriorTrivia in GetContinuationExteriorMarkers(documentationTrivia))
        {
            var text = exteriorTrivia.ToFullString();
            var marker = text.TrimStart(' ', '\t');

            if (text != new string(' ', column) + marker)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Aligns every continuation <c>///</c> marker to an absolute column
    /// </summary>
    /// <param name="documentationTrivia">Single-line documentation comment trivia</param>
    /// <param name="column">Target continuation-marker column</param>
    /// <returns>The documentation trivia with aligned continuation markers</returns>
    public static SyntaxTrivia AlignContinuationExteriorMarkers(SyntaxTrivia documentationTrivia, int column)
    {
        return RewriteContinuationExteriorMarkers(documentationTrivia,
                                                  exteriorTrivia => CreateExteriorTrivia(exteriorTrivia, Math.Max(0, column)));
    }

    /// <summary>
    /// Normalizes the separator after every <c>///</c> exterior in documentation comment text
    /// </summary>
    /// <param name="commentText">Full text of a single-line documentation comment trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The comment text with a canonical separator after every exterior</returns>
    /// <exception cref="ArgumentNullException"><paramref name="commentText"/> is <see langword="null"/></exception>
    /// <remarks>
    /// The separator policy itself belongs to <see cref="NormalizeExteriorSuffix"/>; this method only decides
    /// which span of each line is the suffix
    /// </remarks>
    public static string NormalizeDocumentationLinePrefixes(string commentText, CancellationToken cancellationToken = default)
    {
        return RewriteDocumentationLines(commentText,
                                         (builder, exterior, suffix) =>
                                         {
                                             builder.Append(exterior);
                                             builder.Append(NormalizeExteriorSuffix(suffix));
                                         },
                                         cancellationToken);
    }

    /// <summary>
    /// Replaces every <c>///</c> exterior in documentation comment text with an ordinary <c>//</c> comment marker
    /// </summary>
    /// <param name="commentText">Full text of a single-line documentation comment trivia</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The comment text with every exterior reduced to two slashes</returns>
    /// <exception cref="ArgumentNullException"><paramref name="commentText"/> is <see langword="null"/></exception>
    /// <remarks>
    /// Everything after the exterior is content and is preserved verbatim, so a second <c>///</c> further along
    /// the same line is left alone
    /// </remarks>
    public static string ConvertDocumentationExteriorsToComments(string commentText, CancellationToken cancellationToken = default)
    {
        return RewriteDocumentationLines(commentText,
                                         (builder, _, suffix) =>
                                         {
                                             builder.Append(SingleLineCommentExterior);
                                             builder.Append(suffix);
                                         },
                                         cancellationToken);
    }

    /// <summary>
    /// Determines whether the text consists of whitespace and documentation comment exterior markers only.
    /// Both the <c>///</c> exterior of a single-line comment and the <c>*</c> continuation exterior of a
    /// <c>/** … */</c> comment are recognized. The <c>/**</c> opener is deliberately not an exterior, so
    /// callers never treat the line that starts the comment as content-free
    /// </summary>
    /// <param name="text">Text</param>
    /// <returns><see langword="true"/> if the text carries no documentation content</returns>
    /// <remarks>
    /// This is intentionally wider than <see cref="GetContinuationPrefix"/>, which recognizes <c>///</c> only. The
    /// two differ because they answer different questions: this one classifies existing text and must accept every
    /// exterior form, while the other emits new text and must not invent a <c>*</c> continuation
    /// </remarks>
    public static bool IsExteriorOnly(string text)
    {
        var trimmed = text.Trim();

        if (trimmed.Length == 0)
        {
            return true;
        }

        var isSlashOnly = true;
        var isAsteriskOnly = true;

        foreach (var character in trimmed)
        {
            isSlashOnly &= character == '/';
            isAsteriskOnly &= character == '*';
        }

        return isSlashOnly || isAsteriskOnly;
    }

    /// <summary>
    /// Normalizes the suffix after a single-line documentation exterior without removing content indentation
    /// </summary>
    /// <param name="suffix">Text after the <c>///</c> exterior</param>
    /// <returns>The suffix with a canonical separator</returns>
    /// <exception cref="ArgumentNullException"><paramref name="suffix"/> is <see langword="null"/></exception>
    /// <remarks>
    /// The first character is the separator. An ordinary space is preserved, another whitespace character is
    /// replaced, and a missing separator is inserted. Whitespace after that first character belongs to the
    /// documentation content. A whitespace-only suffix is reduced to either one canonical space or no suffix
    /// </remarks>
    public static string NormalizeExteriorSuffix(string suffix)
    {
        if (suffix == null)
        {
            throw new ArgumentNullException(nameof(suffix));
        }

        if (suffix.Length == 0)
        {
            return suffix;
        }

        if (string.IsNullOrWhiteSpace(suffix))
        {
            return suffix.Length == 1 && suffix[0] == ' ' ? suffix : string.Empty;
        }

        if (suffix[0] == ' ')
        {
            return suffix;
        }

        var contentStartIndex = char.IsWhiteSpace(suffix[0]) ? 1 : 0;

        return string.Concat(" ", suffix.Substring(contentStartIndex));
    }

    /// <summary>
    /// Gets the continuation prefix for the specified documentation comment line. The prefix consists of
    /// the leading indentation, the <c>///</c> exterior marker and a single trailing space. Any sentence
    /// text that follows the exterior on the line is intentionally excluded so that rebuilt continuation
    /// lines never duplicate it
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="line">Line</param>
    /// <returns>The continuation prefix, or an empty string when the line has no documentation exterior</returns>
    /// <remarks>
    /// This recognizes the <c>///</c> exterior only, so a <c>/** … */</c> comment yields an empty prefix and the
    /// caller keeps the line as it is. That is intentionally narrower than <see cref="IsExteriorOnly"/>: emitting a
    /// <c>*</c> continuation would rewrite delimited comments, which no caller is prepared for today
    /// </remarks>
    public static string GetContinuationPrefix(SourceText sourceText, TextLine line)
    {
        var lineText = sourceText.ToString(line.Span);
        var exteriorIndex = lineText.IndexOf(DocumentationExterior, StringComparison.Ordinal);

        if (exteriorIndex < 0)
        {
            return string.Empty;
        }

        return string.Concat(lineText.Substring(0, exteriorIndex + DocumentationExterior.Length), " ");
    }

    /// <summary>
    /// Shifts every continuation <c>///</c> marker by a containing node's column offset
    /// </summary>
    /// <param name="documentationTrivia">Single-line documentation comment trivia</param>
    /// <param name="columnOffset">Column offset to apply</param>
    /// <returns>The documentation trivia with shifted continuation markers</returns>
    public static SyntaxTrivia ShiftContinuationExteriorMarkers(SyntaxTrivia documentationTrivia, int columnOffset)
    {
        return RewriteContinuationExteriorMarkers(documentationTrivia,
                                                  exteriorTrivia =>
                                                  {
                                                      var text = exteriorTrivia.ToFullString();
                                                      var marker = text.TrimStart(' ', '\t');
                                                      var currentColumn = text.Length - marker.Length;

                                                      return CreateExteriorTrivia(exteriorTrivia, Math.Max(0, currentColumn + columnOffset));
                                                  });
    }

    /// <summary>
    /// Determines whether the character terminates a line for the purposes of documentation comment scanning.
    /// This is the C# line-terminator set, which is what <see cref="SourceText.Lines"/> recognizes as well
    /// </summary>
    /// <param name="character">Character</param>
    /// <returns><see langword="true"/> when the character terminates a line</returns>
    private static bool IsLineTerminator(char character)
    {
        return character is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029';
    }

    /// <summary>
    /// Rewrites the exterior and its suffix on every documentation line of the given comment text
    /// </summary>
    /// <param name="commentText">Full text of a single-line documentation comment trivia</param>
    /// <param name="rewriteExterior">Appends the replacement for an exterior and its suffix</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The rewritten comment text</returns>
    /// <exception cref="ArgumentNullException"><paramref name="commentText"/> is <see langword="null"/></exception>
    /// <remarks>
    /// The scan is deterministic and single-pass, so how long it takes depends only on the length of the input.
    /// Line-leading indentation, content after the exterior, the line terminators themselves and every line that
    /// carries no exterior are copied verbatim, which keeps the rewrite confined to the exteriors
    /// </remarks>
    private static string RewriteDocumentationLines(string commentText, Action<StringBuilder, string, string> rewriteExterior, CancellationToken cancellationToken)
    {
        if (commentText == null)
        {
            throw new ArgumentNullException(nameof(commentText));
        }

        var builder = new StringBuilder(commentText.Length);
        var index = 0;

        while (index < commentText.Length)
        {
            cancellationToken.ThrowIfCancellationRequested();
            index = RewriteDocumentationLine(commentText, index, builder, rewriteExterior);
            index = AppendLineTerminator(commentText, index, builder);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Rewrites one documentation line and returns the index of its terminator
    /// </summary>
    /// <param name="commentText">Complete documentation comment text</param>
    /// <param name="lineStart">Index at which the line starts</param>
    /// <param name="builder">Destination builder</param>
    /// <param name="rewriteExterior">Exterior rewrite callback</param>
    /// <returns>The index of the line terminator, or the text length for the final line</returns>
    private static int RewriteDocumentationLine(string commentText,
                                                int lineStart,
                                                StringBuilder builder,
                                                Action<StringBuilder, string, string> rewriteExterior)
    {
        var contentStart = lineStart;

        while (contentStart < commentText.Length
               && char.IsWhiteSpace(commentText[contentStart])
               && IsLineTerminator(commentText[contentStart]) == false)
        {
            contentStart++;
        }

        var lineEnd = FindLineEnd(commentText, contentStart);

        if (string.CompareOrdinal(commentText, contentStart, DocumentationExterior, 0, DocumentationExterior.Length) != 0)
        {
            builder.Append(commentText, lineStart, lineEnd - lineStart);

            return lineEnd;
        }

        var suffixStart = contentStart + DocumentationExterior.Length;

        builder.Append(commentText, lineStart, contentStart - lineStart);
        rewriteExterior(builder, DocumentationExterior, commentText.Substring(suffixStart, lineEnd - suffixStart));

        return lineEnd;
    }

    /// <summary>
    /// Finds the end of the current physical line
    /// </summary>
    /// <param name="text">Text to inspect</param>
    /// <param name="startIndex">Index from which to scan</param>
    /// <returns>The index of the line terminator, or the text length when none remains</returns>
    private static int FindLineEnd(string text, int startIndex)
    {
        while (startIndex < text.Length && IsLineTerminator(text[startIndex]) == false)
        {
            startIndex++;
        }

        return startIndex;
    }

    /// <summary>
    /// Copies the current line terminator and returns the next line's start index
    /// </summary>
    /// <param name="text">Text containing the terminator</param>
    /// <param name="terminatorIndex">Index of the terminator</param>
    /// <param name="builder">Destination builder</param>
    /// <returns>The index immediately after the terminator</returns>
    private static int AppendLineTerminator(string text, int terminatorIndex, StringBuilder builder)
    {
        if (terminatorIndex >= text.Length)
        {
            return terminatorIndex;
        }

        var terminatorLength = text[terminatorIndex] == '\r'
                               && terminatorIndex + 1 < text.Length
                               && text[terminatorIndex + 1] == '\n'
                                   ? 2
                                   : 1;

        builder.Append(text, terminatorIndex, terminatorLength);

        return terminatorIndex + terminatorLength;
    }

    /// <summary>
    /// Creates an exterior marker with the requested indentation
    /// </summary>
    /// <param name="exteriorTrivia">Original exterior trivia</param>
    /// <param name="column">Target marker column</param>
    /// <returns>The aligned exterior trivia</returns>
    private static SyntaxTrivia CreateExteriorTrivia(SyntaxTrivia exteriorTrivia, int column)
    {
        var marker = exteriorTrivia.ToFullString().TrimStart(' ', '\t');

        return SyntaxFactory.DocumentationCommentExterior(new string(' ', column) + marker);
    }

    /// <summary>
    /// Gets every continuation exterior marker after the first documentation line
    /// </summary>
    /// <param name="documentationTrivia">Documentation comment trivia</param>
    /// <returns>The continuation exterior markers</returns>
    private static SyntaxTrivia[] GetContinuationExteriorMarkers(SyntaxTrivia documentationTrivia)
    {
        if (documentationTrivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) == false
            || documentationTrivia.GetStructure() is not DocumentationCommentTriviaSyntax structure)
        {
            return [];
        }

        return structure.DescendantTokens()
                        .SelectMany(token => token.LeadingTrivia)
                        .Where(trivia => trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia))
                        .Skip(1)
                        .ToArray();
    }

    /// <summary>
    /// Rewrites every continuation exterior marker inside a documentation comment
    /// </summary>
    /// <param name="documentationTrivia">Documentation comment trivia</param>
    /// <param name="rewrite">Exterior-trivia rewrite</param>
    /// <returns>The documentation trivia with rewritten continuation markers</returns>
    private static SyntaxTrivia RewriteContinuationExteriorMarkers(SyntaxTrivia documentationTrivia, Func<SyntaxTrivia, SyntaxTrivia> rewrite)
    {
        if (documentationTrivia.GetStructure() is not DocumentationCommentTriviaSyntax structure)
        {
            return documentationTrivia;
        }

        var continuationExteriorMarkers = GetContinuationExteriorMarkers(documentationTrivia);

        if (continuationExteriorMarkers.Length == 0)
        {
            return documentationTrivia;
        }

        var updatedStructure = structure.ReplaceTrivia(continuationExteriorMarkers, (original, _) => rewrite(original));

        return SyntaxFactory.Trivia(updatedStructure);
    }

    #endregion // Methods
}