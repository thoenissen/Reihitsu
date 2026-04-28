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
            var t = trivia[triviaIndex];

            if (t.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                result.Add(t);
                currentLine++;
                atLineStart = true;

                continue;
            }

            if (atLineStart && t.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                var text = t.ToFullString();
                var hasBom = text.Length > 0 && text[0] == '\uFEFF';

                AddLineStartWhitespace(result, t, hasBom, currentLine, model);

                atLineStart = false;

                continue;
            }

            if (atLineStart)
            {
                AddLayoutWhitespaceIfConfigured(result, currentLine, model);

                atLineStart = false;
            }

            result.Add(t);

            // Directive trivia contains an embedded end-of-line; advance to the next line
            if (t.HasStructure && t.GetStructure() is DirectiveTriviaSyntax)
            {
                currentLine++;
                atLineStart = true;
            }
        }

        if (atLineStart && model.TryGetLayout(currentLine, out var finalLayout) && finalLayout.Column > 0)
        {
            result.Add(SyntaxFactory.Whitespace(new string(' ', finalLayout.Column)));
        }

        return SyntaxFactory.TriviaList(result);
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