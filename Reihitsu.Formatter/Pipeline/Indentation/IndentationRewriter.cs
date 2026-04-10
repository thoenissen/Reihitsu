using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation;

/// <summary>
/// Applies the computed layout model to a syntax tree by rewriting leading trivia
/// on first-on-line tokens to produce the correct indentation.
/// </summary>
internal static class IndentationRewriter
{
    #region Methods

    /// <summary>
    /// Applies indentation from the layout model to the syntax tree.
    /// </summary>
    /// <param name="root">The syntax tree root.</param>
    /// <param name="model">The computed layout model.</param>
    /// <returns>The syntax tree with corrected indentation.</returns>
    public static SyntaxNode Apply(SyntaxNode root, LayoutModel model)
    {
        return root.ReplaceTokens(root.DescendantTokens(), (original, rewritten) => ApplyIndentation(original, rewritten, model));
    }

    #endregion // Methods

    #region Private methods

    /// <summary>
    /// Applies indentation to a single token based on the layout model.
    /// </summary>
    /// <param name="original">The original token from the syntax tree.</param>
    /// <param name="rewritten">The rewritten token.</param>
    /// <param name="model">The computed layout model.</param>
    /// <returns>The token with corrected indentation.</returns>
    private static SyntaxToken ApplyIndentation(SyntaxToken original, SyntaxToken rewritten, LayoutModel model)
    {
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
    /// on each line that has a layout entry in the model.
    /// </summary>
    /// <param name="trivia">The original leading trivia list.</param>
    /// <param name="tokenLine">The 0-based line number of the token.</param>
    /// <param name="model">The layout model.</param>
    /// <returns>The rebuilt trivia list.</returns>
    private static SyntaxTriviaList RebuildLeadingTrivia(SyntaxTriviaList trivia, int tokenLine, LayoutModel model)
    {
        var eolCount = 0;

        foreach (var t in trivia)
        {
            if (t.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                eolCount++;
            }
            else if (t.HasStructure && t.GetStructure() is DirectiveTriviaSyntax)
            {
                // Directive trivia contains an embedded end-of-line
                eolCount++;
            }
        }

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
                // Preserve BOM character (\uFEFF) which Roslyn classifies as WhitespaceTrivia
                var text = t.ToFullString();
                var hasBom = text.Length > 0 && text[0] == '\uFEFF';

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
                    result.Add(t);
                }

                atLineStart = false;

                continue;
            }

            if (atLineStart)
            {
                if (model.TryGetLayout(currentLine, out var lineLayout) && lineLayout.Column > 0)
                {
                    result.Add(SyntaxFactory.Whitespace(new string(' ', lineLayout.Column)));
                }

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

        if (atLineStart && model.TryGetLayout(currentLine, out var finalLayout))
        {
            if (finalLayout.Column > 0)
            {
                result.Add(SyntaxFactory.Whitespace(new string(' ', finalLayout.Column)));
            }
        }

        return SyntaxFactory.TriviaList(result);
    }

    #endregion // Private methods
}