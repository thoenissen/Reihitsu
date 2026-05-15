using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation;

/// <summary>
/// Applies the computed layout model to a syntax tree by rewriting leading trivia
/// on first-on-line tokens to produce the correct indentation
/// </summary>
internal static class IndentationRewriter
{
    #region Methods

    /// <summary>
    /// Applies indentation from the layout model to the syntax tree
    /// </summary>
    /// <param name="root">The syntax tree root</param>
    /// <param name="model">The computed layout model</param>
    /// <returns>The syntax tree with corrected indentation</returns>
    public static SyntaxNode Apply(SyntaxNode root, LayoutModel model)
    {
        return root.ReplaceTokens(root.DescendantTokens(), (original, rewritten) => ApplyIndentation(original, rewritten, model));
    }

    #endregion // Methods

    #region Private methods

    /// <summary>
    /// Applies indentation to a single token based on the layout model
    /// </summary>
    /// <param name="original">The original token from the syntax tree</param>
    /// <param name="rewritten">The rewritten token</param>
    /// <param name="model">The computed layout model</param>
    /// <returns>The token with corrected indentation</returns>
    private static SyntaxToken ApplyIndentation(SyntaxToken original, SyntaxToken rewritten, LayoutModel model)
    {
        if (original.IsMissing)
        {
            return rewritten;
        }

        if (LayoutComputer.IsFirstOnLine(original) == false)
        {
            return rewritten;
        }

        var tokenLine = original.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (model.TryGetLayout(tokenLine, out _) == false)
        {
            return rewritten;
        }

        return rewritten.WithLeadingTrivia(RebuildLeadingTrivia(rewritten.LeadingTrivia, tokenLine, model));
    }

    /// <summary>
    /// Rebuilds the leading trivia for a token, replacing indentation whitespace
    /// on each line that has a layout entry in the model
    /// </summary>
    /// <param name="trivia">The original leading trivia list</param>
    /// <param name="tokenLine">The 0-based line number of the token</param>
    /// <param name="model">The layout model</param>
    /// <returns>The rebuilt trivia list</returns>
    private static SyntaxTriviaList RebuildLeadingTrivia(SyntaxTriviaList trivia, int tokenLine, LayoutModel model)
    {
        var eolCount = CountLineBreaks(trivia);

        var currentLine = tokenLine - eolCount;
        var result = new List<SyntaxTrivia>();
        var atLineStart = true;

        for (var triviaIndex = 0; triviaIndex < trivia.Count; triviaIndex++)
        {
            var triviaItem = trivia[triviaIndex];

            if (TryHandleEndOfLineTrivia(triviaItem, result, ref currentLine, ref atLineStart))
            {
                continue;
            }

            if (TryHandleLineStartWhitespace(trivia, triviaIndex, triviaItem, result, currentLine, model, ref atLineStart))
            {
                continue;
            }

            HandleNonWhitespaceTrivia(triviaItem, result, currentLine, model, ref atLineStart, ref currentLine);
        }

        AddFinalLineIndentation(result, atLineStart, currentLine, model);

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Handles an end-of-line trivia entry while rebuilding leading trivia
    /// </summary>
    /// <param name="triviaItem">The trivia entry to inspect</param>
    /// <param name="result">The rebuilt trivia list</param>
    /// <param name="currentLine">The current source line</param>
    /// <param name="atLineStart">Whether the builder is currently at line start</param>
    /// <returns><see langword="true"/> when the trivia was handled</returns>
    private static bool TryHandleEndOfLineTrivia(SyntaxTrivia triviaItem,
                                                 List<SyntaxTrivia> result,
                                                 ref int currentLine,
                                                 ref bool atLineStart)
    {
        if (triviaItem.IsKind(SyntaxKind.EndOfLineTrivia) == false)
        {
            return false;
        }

        result.Add(triviaItem);
        currentLine++;
        atLineStart = true;

        return true;
    }

    /// <summary>
    /// Handles whitespace that appears at the start of a line
    /// </summary>
    /// <param name="trivia">The full trivia list</param>
    /// <param name="triviaIndex">The current trivia index</param>
    /// <param name="triviaItem">The current trivia item</param>
    /// <param name="result">The rebuilt trivia list</param>
    /// <param name="currentLine">The current source line</param>
    /// <param name="model">The layout model</param>
    /// <param name="atLineStart">Whether the builder is currently at line start</param>
    /// <returns><see langword="true"/> when the whitespace was handled</returns>
    private static bool TryHandleLineStartWhitespace(SyntaxTriviaList trivia,
                                                     int triviaIndex,
                                                     SyntaxTrivia triviaItem,
                                                     List<SyntaxTrivia> result,
                                                     int currentLine,
                                                     LayoutModel model,
                                                     ref bool atLineStart)
    {
        if (atLineStart == false || triviaItem.IsKind(SyntaxKind.WhitespaceTrivia) == false)
        {
            return false;
        }

        var text = triviaItem.ToFullString();
        var hasBom = text.Length > 0 && text[0] == '\uFEFF';

        if (StartsWithNonRegionDirective(trivia, triviaIndex + 1))
        {
            if (hasBom)
            {
                result.Add(SyntaxFactory.Whitespace("\uFEFF"));
            }

            return true;
        }

        AddLineStartWhitespace(result, triviaItem, hasBom, currentLine, model);
        atLineStart = false;

        return true;
    }

    /// <summary>
    /// Handles non-whitespace trivia while rebuilding leading trivia
    /// </summary>
    /// <param name="triviaItem">The current trivia item</param>
    /// <param name="result">The rebuilt trivia list</param>
    /// <param name="layoutLine">The source line used for layout lookup</param>
    /// <param name="model">The layout model</param>
    /// <param name="atLineStart">Whether the builder is currently at line start</param>
    /// <param name="currentLine">The mutable current source line</param>
    private static void HandleNonWhitespaceTrivia(SyntaxTrivia triviaItem,
                                                  List<SyntaxTrivia> result,
                                                  int layoutLine,
                                                  LayoutModel model,
                                                  ref bool atLineStart,
                                                  ref int currentLine)
    {
        if (atLineStart)
        {
            if (IsNonRegionDirectiveTrivia(triviaItem) == false)
            {
                AddLayoutWhitespaceIfConfigured(result, layoutLine, model);
            }

            atLineStart = false;
        }

        result.Add(triviaItem);

        // Directive trivia contains an embedded end-of-line; advance to the next line
        if (triviaItem.HasStructure && triviaItem.GetStructure() is DirectiveTriviaSyntax)
        {
            currentLine++;
            atLineStart = true;
        }
    }

    /// <summary>
    /// Adds indentation for a trailing empty line when the layout model defines one
    /// </summary>
    /// <param name="result">The rebuilt trivia list</param>
    /// <param name="atLineStart">Whether the builder is currently at line start</param>
    /// <param name="currentLine">The current source line</param>
    /// <param name="model">The layout model</param>
    private static void AddFinalLineIndentation(List<SyntaxTrivia> result, bool atLineStart, int currentLine, LayoutModel model)
    {
        if (atLineStart && model.TryGetLayout(currentLine, out var finalLayout) && finalLayout.Column > 0)
        {
            result.Add(SyntaxFactory.Whitespace(new string(' ', finalLayout.Column)));
        }
    }

    /// <summary>
    /// Counts the number of effective line breaks represented by leading trivia
    /// </summary>
    /// <param name="trivia">The trivia to inspect</param>
    /// <returns>The effective line-break count</returns>
    private static int CountLineBreaks(SyntaxTriviaList trivia)
    {
        var endOfLineCount = 0;

        foreach (var entry in trivia)
        {
            if (entry.IsKind(SyntaxKind.EndOfLineTrivia)
                || (entry.HasStructure && entry.GetStructure() is DirectiveTriviaSyntax))
            {
                endOfLineCount++;
            }
        }

        return endOfLineCount;
    }

    /// <summary>
    /// Determines whether the trivia entry represents a non-region preprocessor directive
    /// </summary>
    /// <param name="trivia">The trivia entry to inspect</param>
    /// <returns><see langword="true"/> if the trivia is a non-region directive; otherwise, <see langword="false"/></returns>
    private static bool IsNonRegionDirectiveTrivia(SyntaxTrivia trivia)
    {
        return trivia.HasStructure
               && trivia.GetStructure() is DirectiveTriviaSyntax
               && trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) == false
               && trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) == false;
    }

    /// <summary>
    /// Determines whether the remaining trivia on the current line starts with a non-region preprocessor directive
    /// </summary>
    /// <param name="trivia">The leading trivia list</param>
    /// <param name="startIndex">The index to inspect from</param>
    /// <returns><see langword="true"/> if a non-region directive is next on the line; otherwise, <see langword="false"/></returns>
    private static bool StartsWithNonRegionDirective(SyntaxTriviaList trivia, int startIndex)
    {
        for (var triviaIndex = startIndex; triviaIndex < trivia.Count; triviaIndex++)
        {
            if (trivia[triviaIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                continue;
            }

            return IsNonRegionDirectiveTrivia(trivia[triviaIndex]);
        }

        return false;
    }

    /// <summary>
    /// Adds indentation trivia for the current line start, preserving BOM when present
    /// </summary>
    /// <param name="result">The target trivia list being built</param>
    /// <param name="originalTrivia">The original whitespace trivia</param>
    /// <param name="hasBom">Whether the original trivia starts with a BOM character</param>
    /// <param name="currentLine">The current source line index</param>
    /// <param name="model">The layout model</param>
    private static void AddLineStartWhitespace(List<SyntaxTrivia> result, SyntaxTrivia originalTrivia, bool hasBom, int currentLine, LayoutModel model)
    {
        // Preserve BOM character (\uFEFF) which Roslyn classifies as WhitespaceTrivia.
        if (hasBom)
        {
            result.Add(SyntaxFactory.Whitespace("\uFEFF"));
        }

        if (model.TryGetLayout(currentLine, out var layout))
        {
            if (layout.Column > 0)
            {
                result.Add(SyntaxFactory.Whitespace(new string(' ', layout.Column)));
            }
        }
        else if (hasBom == false)
        {
            result.Add(originalTrivia);
        }
    }

    /// <summary>
    /// Adds indentation whitespace for a line when layout data is available
    /// </summary>
    /// <param name="result">The target trivia list being built</param>
    /// <param name="currentLine">The current source line index</param>
    /// <param name="model">The layout model</param>
    private static void AddLayoutWhitespaceIfConfigured(List<SyntaxTrivia> result, int currentLine, LayoutModel model)
    {
        if (model.TryGetLayout(currentLine, out var lineLayout) && lineLayout.Column > 0)
        {
            result.Add(SyntaxFactory.Whitespace(new string(' ', lineLayout.Column)));
        }
    }

    #endregion // Private methods
}