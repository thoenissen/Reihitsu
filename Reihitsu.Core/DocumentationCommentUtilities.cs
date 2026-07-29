using System;
using System.Linq;

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