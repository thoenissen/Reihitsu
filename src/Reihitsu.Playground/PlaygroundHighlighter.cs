using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Playground;

/// <summary>
/// Colours C# source text by the kinds of the tokens and trivia of its syntax tree (PLAY-010) and renders it with error
/// marks (PLAY-014)
/// </summary>
public static class PlaygroundHighlighter
{
    #region Constants

    /// <summary>
    /// Class of a keyword
    /// </summary>
    private const string KeywordClass = "hl-keyword";

    /// <summary>
    /// Class of an identifier
    /// </summary>
    private const string IdentifierClass = "hl-identifier";

    /// <summary>
    /// Class of a string or character literal and of the text parts and delimiters of an interpolated or raw string
    /// </summary>
    private const string StringClass = "hl-string";

    /// <summary>
    /// Class of a numeric literal
    /// </summary>
    private const string NumberClass = "hl-number";

    /// <summary>
    /// Class of a comment, documentation comments included
    /// </summary>
    private const string CommentClass = "hl-comment";

    /// <summary>
    /// Class of a preprocessor directive's tokens and message
    /// </summary>
    private const string DirectiveClass = "hl-directive";

    /// <summary>
    /// Class of the text of a branch the conditional directives exclude
    /// </summary>
    private const string DisabledClass = "hl-disabled";

    /// <summary>
    /// Class of an error mark
    /// </summary>
    private const string MarkClass = "playground-mark";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Creates the content that shows a text coloured by its syntax tree, with an error mark at each given span
    /// </summary>
    /// <param name="text">The C# source text</param>
    /// <param name="marks">The spans to mark, each already one character wide where it was empty and followed by an ordinary character (see <see cref="GetMarkSpan"/>)</param>
    /// <returns>The content</returns>
    public static RenderFragment Render(string text, IReadOnlyList<TextSpan> marks)
    {
        var categories = Classify(text);

        // A longer mark encloses the shorter ones that start with it
        var orderedMarks = marks.OrderBy(mark => mark.Start)
                                .ThenByDescending(mark => mark.Length)
                                .ToList();

        return builder =>
               {
                   var markIndex = 0;

                   RenderRange(builder, text, categories, orderedMarks, ref markIndex, 0, text.Length, true);
               };
    }

    /// <summary>
    /// Gets the span an error mark covers in a text (PLAY-014): a span with characters as reported; an empty span the
    /// character behind it when that character is an ordinary one, and otherwise, at a line break or at the end of the
    /// text, the empty span itself, which the stylesheet shows one character wide
    /// </summary>
    /// <param name="span">The reported span</param>
    /// <param name="text">The text the span lies in</param>
    /// <returns>The span to mark</returns>
    public static TextSpan GetMarkSpan(TextSpan span, string text)
    {
        if (span.IsEmpty
            && span.Start < text.Length
            && text[span.Start] is not ('\r' or '\n'))
        {
            return new TextSpan(span.Start, 1);
        }

        return span;
    }

    /// <summary>
    /// Classifies a text by its syntax tree, parsed with the options of PLAY-005
    /// </summary>
    /// <param name="text">The C# source text</param>
    /// <returns>The coloured spans in text order, each with its class; text outside them keeps the normal colour</returns>
    private static List<(TextSpan Span, string ClassName)> Classify(string text)
    {
        var root = CSharpSyntaxTree.ParseText(text).GetRoot();
        var spans = new List<(TextSpan Span, string ClassName)>();

        foreach (var token in root.DescendantTokens())
        {
            AddToken(spans, token, false);
        }

        return spans;
    }

    /// <summary>
    /// Adds a token and its trivia
    /// </summary>
    /// <param name="spans">The coloured spans</param>
    /// <param name="token">The token</param>
    /// <param name="inDirective">Whether the token belongs to a preprocessor directive</param>
    private static void AddToken(List<(TextSpan Span, string ClassName)> spans, SyntaxToken token, bool inDirective)
    {
        foreach (var trivia in token.LeadingTrivia)
        {
            AddTrivia(spans, trivia, inDirective);
        }

        Add(spans, token.Span, inDirective ? DirectiveClass : GetTokenClass(token));

        foreach (var trivia in token.TrailingTrivia)
        {
            AddTrivia(spans, trivia, inDirective);
        }
    }

    /// <summary>
    /// Adds a trivia by its kind; a directive and skipped tokens are added by their tokens
    /// </summary>
    /// <param name="spans">The coloured spans</param>
    /// <param name="trivia">The trivia</param>
    /// <param name="inDirective">Whether the trivia belongs to a preprocessor directive</param>
    private static void AddTrivia(List<(TextSpan Span, string ClassName)> spans, SyntaxTrivia trivia, bool inDirective)
    {
        switch (trivia.Kind())
        {
            case SyntaxKind.SingleLineCommentTrivia:
            case SyntaxKind.MultiLineCommentTrivia:
                {
                    Add(spans, trivia.Span, CommentClass);
                }
                break;

            // The exterior of a documentation comment, its "///" or "/**", lies outside the trivia's span
            case SyntaxKind.SingleLineDocumentationCommentTrivia:
            case SyntaxKind.MultiLineDocumentationCommentTrivia:
                {
                    Add(spans, trivia.FullSpan, CommentClass);
                }
                break;

            case SyntaxKind.DisabledTextTrivia:
                {
                    Add(spans, trivia.Span, DisabledClass);
                }
                break;

            case SyntaxKind.PreprocessingMessageTrivia:
                {
                    Add(spans, trivia.Span, DirectiveClass);
                }
                break;

            default:
                {
                    if (trivia.GetStructure() is { } structure)
                    {
                        var isDirective = inDirective || structure is DirectiveTriviaSyntax;

                        foreach (var token in structure.DescendantTokens())
                        {
                            AddToken(spans, token, isDirective);
                        }
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Gets the class of a token outside a directive by its kind
    /// </summary>
    /// <param name="token">The token</param>
    /// <returns>The class, or <see langword="null"/> for the normal colour</returns>
    private static string GetTokenClass(SyntaxToken token)
    {
        var kind = token.Kind();

        if (SyntaxFacts.IsKeywordKind(kind))
        {
            return KeywordClass;
        }

        switch (kind)
        {
            case SyntaxKind.IdentifierToken:
                return IdentifierClass;

            case SyntaxKind.NumericLiteralToken:
                return NumberClass;

            case SyntaxKind.CharacterLiteralToken:
            case SyntaxKind.StringLiteralToken:
            case SyntaxKind.Utf8StringLiteralToken:
            case SyntaxKind.SingleLineRawStringLiteralToken:
            case SyntaxKind.Utf8SingleLineRawStringLiteralToken:
            case SyntaxKind.MultiLineRawStringLiteralToken:
            case SyntaxKind.Utf8MultiLineRawStringLiteralToken:
            case SyntaxKind.InterpolatedStringToken:
            case SyntaxKind.InterpolatedStringStartToken:
            case SyntaxKind.InterpolatedVerbatimStringStartToken:
            case SyntaxKind.InterpolatedSingleLineRawStringStartToken:
            case SyntaxKind.InterpolatedMultiLineRawStringStartToken:
            case SyntaxKind.InterpolatedStringTextToken:
            case SyntaxKind.InterpolatedStringEndToken:
            case SyntaxKind.InterpolatedRawStringEndToken:
                return StringClass;

            default:
                return null;
        }
    }

    /// <summary>
    /// Adds a coloured span, unless it is empty or keeps the normal colour
    /// </summary>
    /// <param name="spans">The coloured spans</param>
    /// <param name="span">The span</param>
    /// <param name="className">The class, or <see langword="null"/> for the normal colour</param>
    private static void Add(List<(TextSpan Span, string ClassName)> spans, TextSpan span, string className)
    {
        if (className != null && span.IsEmpty == false)
        {
            spans.Add((span, className));
        }
    }

    /// <summary>
    /// Renders a range of the text with the marks that start in it. A mark encloses the marks that start inside it; one that
    /// starts inside it and reaches beyond its end is cut at that end, so every mark stays one element
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="text">The text</param>
    /// <param name="categories">The coloured spans of the text</param>
    /// <param name="marks">The marks, ordered by start and, at one start, from the longest</param>
    /// <param name="markIndex">Index of the next mark to render</param>
    /// <param name="start">Start of the range</param>
    /// <param name="end">End of the range</param>
    /// <param name="isTopLevel">Whether the range is the whole text, which takes every remaining mark</param>
    private static void RenderRange(RenderTreeBuilder builder, string text, List<(TextSpan Span, string ClassName)> categories, List<TextSpan> marks, ref int markIndex, int start, int end, bool isTopLevel)
    {
        var position = start;

        while (markIndex < marks.Count)
        {
            var mark = marks[markIndex];

            if (isTopLevel == false && mark.Start >= end)
            {
                break;
            }

            var markStart = Math.Clamp(mark.Start, position, end);
            var markEnd = Math.Clamp(mark.End, markStart, end);

            RenderText(builder, text, categories, position, markStart);

            markIndex++;

            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", MarkClass);

            RenderRange(builder, text, categories, marks, ref markIndex, markStart, markEnd, false);

            builder.CloseElement();

            position = markEnd;
        }

        RenderText(builder, text, categories, position, end);
    }

    /// <summary>
    /// Renders a range of the text without marks, each coloured part in an element with its class
    /// </summary>
    /// <param name="builder">The render tree builder</param>
    /// <param name="text">The text</param>
    /// <param name="categories">The coloured spans of the text</param>
    /// <param name="start">Start of the range</param>
    /// <param name="end">End of the range</param>
    private static void RenderText(RenderTreeBuilder builder, string text, List<(TextSpan Span, string ClassName)> categories, int start, int end)
    {
        var position = start;

        for (var index = FindFirstSpanEndingAfter(categories, start); index < categories.Count && position < end; index++)
        {
            var (span, className) = categories[index];

            var spanStart = Math.Max(span.Start, position);
            var spanEnd = Math.Min(span.End, end);

            if (spanStart >= spanEnd)
            {
                continue;
            }

            if (spanStart > position)
            {
                builder.AddContent(2, text[position..spanStart]);
            }

            builder.OpenElement(3, "span");
            builder.AddAttribute(4, "class", className);
            builder.AddContent(5, text[spanStart..spanEnd]);
            builder.CloseElement();

            position = spanEnd;
        }

        if (position < end)
        {
            builder.AddContent(6, text[position..end]);
        }
    }

    /// <summary>
    /// Finds the first coloured span that ends behind a position
    /// </summary>
    /// <param name="categories">The coloured spans, in text order</param>
    /// <param name="position">The position</param>
    /// <returns>The index of the span, or the number of spans when there is none</returns>
    private static int FindFirstSpanEndingAfter(List<(TextSpan Span, string ClassName)> categories, int position)
    {
        var low = 0;
        var high = categories.Count;

        while (low < high)
        {
            var middle = (low + high) / 2;

            if (categories[middle].Span.End <= position)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        return low;
    }

    #endregion // Methods
}