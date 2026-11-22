using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    /// <summary>
    /// Class of the element of one rendered line
    /// </summary>
    private const string LineClass = "playground-line";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Colours a text by its syntax tree, with an error mark at each given span. With the highlighting of the text's previous
    /// version, only the part of the text that the edit changes, or whose syntax the edit changes, is parsed, coloured, and
    /// rendered again; the result is the same as without it
    /// </summary>
    /// <param name="text">The C# source text</param>
    /// <param name="marks">The spans to mark, each already one character wide where it was empty and followed by an ordinary character (see <see cref="GetMarkSpan"/>)</param>
    /// <param name="previous">The highlighting of the text's previous version, or <see langword="null"/></param>
    /// <returns>The highlighting</returns>
    public static PlaygroundHighlighting Highlight(string text, IReadOnlyList<TextSpan> marks, PlaygroundHighlighting previous = null)
    {
        SyntaxTree tree;
        List<(TextSpan Span, string ClassName)> categories;
        List<PlaygroundHighlightedLine> lines;

        if (previous == null)
        {
            tree = PlaygroundFormatter.Parse(text);
            categories = Classify(tree.GetRoot());
        }
        else
        {
            var change = GetChange(previous.Text, text);

            tree = PlaygroundFormatter.Reparse(previous.Tree, change);

            var changeEnd = change.Span.Start + change.NewText.Length;
            var regionStart = change.Span.Start;
            var regionEnd = changeEnd;

            categories = [];

            ClassifyChanged(tree.GetRoot(), previous, change, categories, ref regionStart, ref regionEnd);

            var oldRegionEnd = regionEnd - (changeEnd - change.Span.End);

            if (marks.Count == 0 && previous.IsMarked == false)
            {
                lines = RenderChangedLines(text, categories, previous.Lines, regionStart, oldRegionEnd);

                return new PlaygroundHighlighting(text, tree, categories, lines, false, CreateContent(lines));
            }
        }

        lines = marks.Count > 0
                    ? [RenderMarkedText(text, categories, marks)]
                    : RenderLines(text, categories, 0, text.Length, true);

        return new PlaygroundHighlighting(text, tree, categories, lines, marks.Count > 0, CreateContent(lines));
    }

    /// <summary>
    /// Gets the span an error mark covers in a text (PLAY-014): a span with characters as reported; an empty span the
    /// character behind it when that character is an ordinary one, both code units of a surrogate pair included, and
    /// otherwise, at a line break as the parser counts it or at the end of the text, the empty span itself, which the
    /// stylesheet shows one character wide
    /// </summary>
    /// <param name="span">The reported span</param>
    /// <param name="text">The text the span lies in</param>
    /// <returns>The span to mark</returns>
    public static TextSpan GetMarkSpan(TextSpan span, string text)
    {
        if (span.IsEmpty
            && span.Start < text.Length
            && SyntaxFacts.IsNewLine(text[span.Start]) == false)
        {
            return new TextSpan(span.Start, char.IsSurrogatePair(text, span.Start) ? 2 : 1);
        }

        return span;
    }

    /// <summary>
    /// Gets the change that turns one text into another: the part between their longest common start and their longest
    /// common end
    /// </summary>
    /// <param name="oldText">The text before the change</param>
    /// <param name="newText">The text after the change</param>
    /// <returns>The change, in positions of <paramref name="oldText"/></returns>
    private static TextChange GetChange(string oldText, string newText)
    {
        var prefix = oldText.AsSpan().CommonPrefixLength(newText);
        var maximumSuffix = Math.Min(oldText.Length, newText.Length) - prefix;
        var suffix = 0;

        while (suffix < maximumSuffix
               && oldText[oldText.Length - 1 - suffix] == newText[newText.Length - 1 - suffix])
        {
            suffix++;
        }

        return new TextChange(TextSpan.FromBounds(prefix, oldText.Length - suffix), newText.Substring(prefix, newText.Length - prefix - suffix));
    }

    /// <summary>
    /// Classifies the children of a node of a reparsed tree. A child outside the changed text that is the same syntax as the
    /// child at its place in the earlier tree keeps the colours it had there, shifted by the change in length; every other
    /// token is classified again and widens the region whose colours can differ from those of the earlier text
    /// </summary>
    /// <param name="node">The node of the reparsed tree</param>
    /// <param name="previous">The highlighting of the earlier text</param>
    /// <param name="change">The change from the earlier text, in its positions</param>
    /// <param name="spans">The coloured spans, in text order</param>
    /// <param name="regionStart">Start of the region, in positions of the reparsed text</param>
    /// <param name="regionEnd">End of the region, in positions of the reparsed text</param>
    private static void ClassifyChanged(SyntaxNode node, PlaygroundHighlighting previous, TextChange change, List<(TextSpan Span, string ClassName)> spans, ref int regionStart, ref int regionEnd)
    {
        foreach (var child in node.ChildNodesAndTokens())
        {
            var span = child.FullSpan;
            var previousSpan = GetPreviousSpan(span, change);

            if (previousSpan != null
                && IsUnchanged(previous.Tree.GetRoot(), child, previousSpan.Value))
            {
                AddPreviousCategories(spans, previous.Categories, previousSpan.Value, span.Start - previousSpan.Value.Start);
            }
            else if (child.AsNode() is { } childNode)
            {
                ClassifyChanged(childNode, previous, change, spans, ref regionStart, ref regionEnd);
            }
            else
            {
                AddToken(spans, child.AsToken(), false);

                // An empty token, such as the end of the file without trivia, has no colour that could differ
                if (span.IsEmpty == false)
                {
                    regionStart = Math.Min(regionStart, span.Start);
                    regionEnd = Math.Max(regionEnd, span.End);
                }
            }
        }
    }

    /// <summary>
    /// Gets the span of the earlier text that a span of the reparsed text outside the changed text had
    /// </summary>
    /// <param name="span">The span of the reparsed text</param>
    /// <param name="change">The change from the earlier text, in its positions</param>
    /// <returns>The span of the earlier text, or <see langword="null"/> when the span touches the changed text</returns>
    private static TextSpan? GetPreviousSpan(TextSpan span, TextChange change)
    {
        var changeEnd = change.Span.Start + change.NewText.Length;

        if (span.End <= change.Span.Start)
        {
            return span;
        }

        if (span.Start >= changeEnd)
        {
            return new TextSpan(span.Start - (changeEnd - change.Span.End), span.Length);
        }

        return null;
    }

    /// <summary>
    /// Adds the coloured spans of the earlier text that lie in a span of it, shifted to their place in the reparsed text
    /// </summary>
    /// <param name="spans">The coloured spans of the reparsed text</param>
    /// <param name="previousCategories">The coloured spans of the earlier text</param>
    /// <param name="previousSpan">The span of the earlier text, which holds whole tokens</param>
    /// <param name="shift">The distance from the earlier place to the new one</param>
    private static void AddPreviousCategories(List<(TextSpan Span, string ClassName)> spans, List<(TextSpan Span, string ClassName)> previousCategories, TextSpan previousSpan, int shift)
    {
        for (var index = FindFirstSpanEndingAfter(previousCategories, previousSpan.Start); index < previousCategories.Count && previousCategories[index].Span.End <= previousSpan.End; index++)
        {
            var (span, className) = previousCategories[index];

            spans.Add((new TextSpan(span.Start + shift, span.Length), className));
        }
    }

    /// <summary>
    /// Determines whether a node or token of a reparsed tree is the same syntax, reused by the parser, as the node or token at
    /// a span of the earlier tree
    /// </summary>
    /// <param name="previousRoot">The root of the earlier tree</param>
    /// <param name="child">The node or token of the reparsed tree</param>
    /// <param name="previousSpan">The full span in the earlier tree</param>
    /// <returns>Whether it is the same syntax; an empty one is never treated as the same</returns>
    private static bool IsUnchanged(SyntaxNode previousRoot, SyntaxNodeOrToken child, TextSpan previousSpan)
    {
        if (previousSpan.IsEmpty)
        {
            return false;
        }

        if (child.AsNode() is { } node)
        {
            for (var candidate = previousRoot.FindNode(previousSpan, getInnermostNodeForTie: true); candidate != null && candidate.FullSpan == previousSpan; candidate = candidate.Parent)
            {
                if (candidate.IsIncrementallyIdenticalTo(node))
                {
                    return true;
                }
            }

            return false;
        }

        var token = previousRoot.FindToken(previousSpan.Start);

        return token.FullSpan == previousSpan && token.IsIncrementallyIdenticalTo(child.AsToken());
    }

    /// <summary>
    /// Classifies a syntax tree
    /// </summary>
    /// <param name="root">The root of the syntax tree, parsed by <see cref="PlaygroundFormatter.Parse"/></param>
    /// <returns>The coloured spans in text order, each with its class; text outside them keeps the normal colour</returns>
    private static List<(TextSpan Span, string ClassName)> Classify(SyntaxNode root)
    {
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
    /// Renders the lines of a text after an edit, reusing every line of the earlier text that lies wholly outside the changed
    /// region, its line break included; the lines in between are rendered from the new text
    /// </summary>
    /// <param name="text">The new text</param>
    /// <param name="categories">The coloured spans of the new text</param>
    /// <param name="previousLines">The lines of the earlier text, which carries no marks</param>
    /// <param name="regionStart">Start of the changed region, the same in both texts</param>
    /// <param name="oldRegionEnd">End of the changed region in the earlier text</param>
    /// <returns>The lines of the new text</returns>
    private static List<PlaygroundHighlightedLine> RenderChangedLines(string text, List<(TextSpan Span, string ClassName)> categories, List<PlaygroundHighlightedLine> previousLines, int regionStart, int oldRegionEnd)
    {
        // Every line but the last ends with its line break, so a leading line ending in front of the region is unchanged
        var leadingCount = 0;
        var leadingLength = 0;

        while (leadingCount < previousLines.Count - 1
               && leadingLength + previousLines[leadingCount].Length <= regionStart)
        {
            leadingLength += previousLines[leadingCount].Length;
            leadingCount++;
        }

        // A trailing line is unchanged when the line break in front of it lies behind the region too
        var trailingCount = 0;
        var oldTrailingStart = previousLines.Sum(line => line.Length);

        while (previousLines.Count - trailingCount - 1 > leadingCount
               && oldTrailingStart - previousLines[previousLines.Count - trailingCount - 1].Length > oldRegionEnd)
        {
            oldTrailingStart -= previousLines[previousLines.Count - trailingCount - 1].Length;
            trailingCount++;
        }

        var newTrailingStart = text.Length - (previousLines.Sum(line => line.Length) - oldTrailingStart);

        return [
                   .. previousLines.Take(leadingCount),
                   .. RenderLines(text, categories, leadingLength, newTrailingStart, trailingCount == 0),
                   .. previousLines.Skip(previousLines.Count - trailingCount)
               ];
    }

    /// <summary>
    /// Renders a range of a text without marks as lines, each ending behind its line feed
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="categories">The coloured spans of the text</param>
    /// <param name="start">Start of the range, the start of a line</param>
    /// <param name="end">End of the range</param>
    /// <param name="isLast">Whether the range ends the text, so that its last line is rendered even when it is empty</param>
    /// <returns>The lines</returns>
    private static List<PlaygroundHighlightedLine> RenderLines(string text, List<(TextSpan Span, string ClassName)> categories, int start, int end, bool isLast)
    {
        var lines = new List<PlaygroundHighlightedLine>();
        var lineStart = start;

        while (lineStart < end || (isLast && lineStart == end))
        {
            var lineFeed = text.IndexOf('\n', lineStart, end - lineStart);
            var lineEnd = lineFeed < 0 ? end : lineFeed + 1;
            var markup = new StringBuilder();
            var frames = HoldsNullCharacter(text, lineStart, lineEnd) ? new List<Action<RenderTreeBuilder>>() : null;

            RenderText(markup, frames, text, categories, lineStart, lineEnd);

            lines.Add(CreateLine(lineEnd - lineStart, markup, frames));

            if (lineFeed < 0)
            {
                break;
            }

            lineStart = lineEnd;
        }

        return lines;
    }

    /// <summary>
    /// Renders a text with marks as one line, so that a mark across a line break stays one element
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="categories">The coloured spans of the text</param>
    /// <param name="marks">The spans to mark</param>
    /// <returns>The line</returns>
    private static PlaygroundHighlightedLine RenderMarkedText(string text, List<(TextSpan Span, string ClassName)> categories, IReadOnlyList<TextSpan> marks)
    {
        // A longer mark encloses the shorter ones that start with it
        var orderedMarks = marks.OrderBy(mark => mark.Start)
                                .ThenByDescending(mark => mark.Length)
                                .ToList();

        var markup = new StringBuilder(text.Length * 2);
        var frames = HoldsNullCharacter(text, 0, text.Length) ? new List<Action<RenderTreeBuilder>>() : null;
        var markIndex = 0;

        RenderRange(markup, frames, text, categories, orderedMarks, ref markIndex, 0, text.Length, true);

        return CreateLine(text.Length, markup, frames);
    }

    /// <summary>
    /// Determines whether a range of a text holds a null character. Markup parsing replaces a null character, also one
    /// written as a character reference, so a line that holds one is rendered as frames instead of markup
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="start">Start of the range</param>
    /// <param name="end">End of the range</param>
    /// <returns><see langword="true"/> when the range holds a null character</returns>
    private static bool HoldsNullCharacter(string text, int start, int end)
    {
        return text.IndexOf('\0', start, end - start) >= 0;
    }

    /// <summary>
    /// Creates a rendered line from what was written for it: its frames when it was rendered as frames, and otherwise its
    /// markup. An empty line gets no content, because the page shows empty markup as a space
    /// </summary>
    /// <param name="length">The number of characters of the text the line covers, its line break included</param>
    /// <param name="markup">The markup written for the line</param>
    /// <param name="frames">The frames written for the line, or <see langword="null"/> when it was rendered as markup</param>
    /// <returns>The line</returns>
    private static PlaygroundHighlightedLine CreateLine(int length, StringBuilder markup, List<Action<RenderTreeBuilder>> frames)
    {
        if (frames != null)
        {
            return new PlaygroundHighlightedLine(length,
                                                 builder =>
                                                 {
                                                     foreach (var frame in frames)
                                                     {
                                                         frame(builder);
                                                     }
                                                 });
        }

        if (markup.Length == 0)
        {
            return new PlaygroundHighlightedLine(length, null);
        }

        var lineMarkup = markup.ToString();

        return new PlaygroundHighlightedLine(length, builder => builder.AddMarkupContent(0, lineMarkup));
    }

    /// <summary>
    /// Creates the content that shows rendered lines: one element per line, keyed by the line, so that the page replaces only
    /// the elements of the lines an edit changes
    /// </summary>
    /// <param name="lines">The lines</param>
    /// <returns>The content</returns>
    private static RenderFragment CreateContent(List<PlaygroundHighlightedLine> lines)
    {
        return builder =>
               {
                   foreach (var line in lines)
                   {
                       builder.OpenElement(0, "span");
                       builder.SetKey(line);
                       builder.AddAttribute(1, "class", LineClass);

                       if (line.Content != null)
                       {
                           builder.AddContent(2, line.Content);
                       }

                       builder.CloseElement();
                   }
               };
    }

    /// <summary>
    /// Renders a range of the text with the marks that start in it. A mark encloses the marks that start inside it; one that
    /// starts inside it and reaches beyond its end is cut at that end, so every mark stays one element
    /// </summary>
    /// <param name="markup">The markup being written</param>
    /// <param name="frames">The frames being written instead of markup, or <see langword="null"/></param>
    /// <param name="text">The text</param>
    /// <param name="categories">The coloured spans of the text</param>
    /// <param name="marks">The marks, ordered by start and, at one start, from the longest</param>
    /// <param name="markIndex">Index of the next mark to render</param>
    /// <param name="start">Start of the range</param>
    /// <param name="end">End of the range</param>
    /// <param name="isTopLevel">Whether the range is the whole text, which takes every remaining mark</param>
    private static void RenderRange(StringBuilder markup, List<Action<RenderTreeBuilder>> frames, string text, List<(TextSpan Span, string ClassName)> categories, List<TextSpan> marks, ref int markIndex, int start, int end, bool isTopLevel)
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

            RenderText(markup, frames, text, categories, position, markStart);

            markIndex++;

            OpenSpan(markup, frames, MarkClass);

            RenderRange(markup, frames, text, categories, marks, ref markIndex, markStart, markEnd, false);

            CloseSpan(markup, frames);

            position = markEnd;
        }

        RenderText(markup, frames, text, categories, position, end);
    }

    /// <summary>
    /// Renders a range of the text without marks, each coloured part in an element with its class
    /// </summary>
    /// <param name="markup">The markup being written</param>
    /// <param name="frames">The frames being written instead of markup, or <see langword="null"/></param>
    /// <param name="text">The text</param>
    /// <param name="categories">The coloured spans of the text</param>
    /// <param name="start">Start of the range</param>
    /// <param name="end">End of the range</param>
    private static void RenderText(StringBuilder markup, List<Action<RenderTreeBuilder>> frames, string text, List<(TextSpan Span, string ClassName)> categories, int start, int end)
    {
        var position = start;

        // The spans are in text order and do not overlap, so the first one that starts behind the range ends the loop
        for (var index = FindFirstSpanEndingAfter(categories, start); index < categories.Count && categories[index].Span.Start < end; index++)
        {
            var (span, className) = categories[index];

            var spanStart = Math.Max(span.Start, position);
            var spanEnd = Math.Min(span.End, end);

            AppendText(markup, frames, text, position, spanStart);

            OpenSpan(markup, frames, className);

            AppendText(markup, frames, text, spanStart, spanEnd);

            CloseSpan(markup, frames);

            position = spanEnd;
        }

        AppendText(markup, frames, text, position, end);
    }

    /// <summary>
    /// Opens an element with a class
    /// </summary>
    /// <param name="markup">The markup being written</param>
    /// <param name="frames">The frames being written instead of markup, or <see langword="null"/></param>
    /// <param name="className">The class</param>
    private static void OpenSpan(StringBuilder markup, List<Action<RenderTreeBuilder>> frames, string className)
    {
        if (frames != null)
        {
            frames.Add(builder =>
                       {
                           builder.OpenElement(0, "span");
                           builder.AddAttribute(1, "class", className);
                       });
        }
        else
        {
            markup.Append("<span class=\"").Append(className).Append("\">");
        }
    }

    /// <summary>
    /// Closes the element opened last
    /// </summary>
    /// <param name="markup">The markup being written</param>
    /// <param name="frames">The frames being written instead of markup, or <see langword="null"/></param>
    private static void CloseSpan(StringBuilder markup, List<Action<RenderTreeBuilder>> frames)
    {
        if (frames != null)
        {
            frames.Add(builder => builder.CloseElement());
        }
        else
        {
            markup.Append("</span>");
        }
    }

    /// <summary>
    /// Appends a range of the text: as a text frame, which the page shows as exactly these characters, when frames are
    /// written, and otherwise as markup text
    /// </summary>
    /// <param name="markup">The markup being written</param>
    /// <param name="frames">The frames being written instead of markup, or <see langword="null"/></param>
    /// <param name="text">The text</param>
    /// <param name="start">Start of the range</param>
    /// <param name="end">End of the range</param>
    private static void AppendText(StringBuilder markup, List<Action<RenderTreeBuilder>> frames, string text, int start, int end)
    {
        if (frames != null)
        {
            if (end > start)
            {
                var part = text.Substring(start, end - start);

                frames.Add(builder => builder.AddContent(2, part));
            }
        }
        else
        {
            AppendEncoded(markup, text, start, end);
        }
    }

    /// <summary>
    /// Appends a range of the text as markup text that the page shows as exactly these characters: the characters markup
    /// reads as syntax are escaped, and a carriage return is written as a character reference, because markup parsing
    /// would otherwise fold it into a line feed. The range holds no null character: markup parsing replaces one, written
    /// raw or as a character reference, so a line that holds one is rendered as frames (see <see cref="HoldsNullCharacter"/>)
    /// </summary>
    /// <param name="markup">The markup being written</param>
    /// <param name="text">The text</param>
    /// <param name="start">Start of the range</param>
    /// <param name="end">End of the range</param>
    private static void AppendEncoded(StringBuilder markup, string text, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            var character = text[index];

            switch (character)
            {
                case '&':
                    {
                        markup.Append("&amp;");
                    }
                    break;

                case '<':
                    {
                        markup.Append("&lt;");
                    }
                    break;

                case '>':
                    {
                        markup.Append("&gt;");
                    }
                    break;

                case '\r':
                    {
                        markup.Append("&#13;");
                    }
                    break;

                default:
                    {
                        markup.Append(character);
                    }
                    break;
            }
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