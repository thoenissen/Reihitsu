using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Runs the production pipeline with a phase observer and records the input and output of every phase, so a test can
/// assert what a single phase did, or what every phase after a given one received and returned
/// </summary>
public static class ObservedPipeline
{
    #region Methods

    /// <summary>
    /// Formats the input through the production pipeline and records every phase
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <returns>The phases in the order they ran</returns>
    public static IReadOnlyList<ObservedPhase> Run(string input, string endOfLine)
    {
        var tree = CSharpSyntaxTree.ParseText(input);
        var context = new FormattingContext(endOfLine, languageVersion: LanguageVersionResolver.Resolve(tree.Options));
        var phases = new List<ObservedPhase>();

        FormattingPipeline.Execute(tree.GetRoot(),
                                   context,
                                   (phaseName, before, after) => phases.Add(new ObservedPhase(phaseName, before, after)),
                                   CancellationToken.None);

        return phases;
    }

    /// <summary>
    /// Returns the output of the first phase and asserts that the first phase is the tab replacement
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <returns>The source text the first phase returned</returns>
    public static string RunTabReplacementPhase(string input, string endOfLine)
    {
        var phases = Run(input, endOfLine);

        Assert.AreEqual("TabReplacementPhase", phases[0].Name, "The first phase of the pipeline should be the tab replacement.");

        return phases[0].Output.ToFullString();
    }

    /// <summary>
    /// Describes every tab in the whitespace of the code — the whitespace trivia of tokens outside documentation comments
    /// and directives — and in the whitespace between the tokens of a directive. Tabs inside comments, documentation
    /// comments, literals, disabled text, and a directive's free text are not reported
    /// </summary>
    /// <param name="root">The syntax root to scan</param>
    /// <returns>One description per whitespace trivia that holds a tab</returns>
    public static IReadOnlyList<string> FindTabsInCodeAndDirectiveWhitespace(SyntaxNode root)
    {
        var findings = new List<string>();

        foreach (var token in root.DescendantTokens())
        {
            foreach (var trivia in token.LeadingTrivia.Concat(token.TrailingTrivia))
            {
                if (IsWhitespaceWithTab(trivia))
                {
                    findings.Add(Describe(trivia, "code whitespace"));
                }
                else if (trivia.IsDirective
                         && trivia.GetStructure() is DirectiveTriviaSyntax directive)
                {
                    foreach (var directiveToken in directive.DescendantTokens())
                    {
                        foreach (var directiveTrivia in directiveToken.LeadingTrivia.Concat(directiveToken.TrailingTrivia))
                        {
                            if (IsWhitespaceWithTab(directiveTrivia))
                            {
                                findings.Add(Describe(directiveTrivia, "directive whitespace"));
                            }
                        }
                    }
                }
            }
        }

        return findings;
    }

    /// <summary>
    /// Splits text into lines without their line-ending characters and without the whitespace that ends them, and drops
    /// the empty lines at the end of the text. Two texts with equal lines place every token, comment, and directive in the
    /// same column
    /// </summary>
    /// <param name="text">The text to split</param>
    /// <returns>The lines of the text</returns>
    public static IReadOnlyList<string> GetLinesWithoutTrailingWhitespace(string text)
    {
        var lines = text.Replace("\r\n", "\n")
                        .Split('\n')
                        .Select(line => line.TrimEnd())
                        .ToList();

        while (lines.Count > 0
               && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines;
    }

    /// <summary>
    /// Determines whether the trivia is whitespace that holds a tab
    /// </summary>
    /// <param name="trivia">The trivia to inspect</param>
    /// <returns><see langword="true"/> when the trivia is whitespace with a tab</returns>
    private static bool IsWhitespaceWithTab(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.WhitespaceTrivia)
               && trivia.ToFullString().Contains('\t');
    }

    /// <summary>
    /// Describes a trivia by its kind of whitespace and its position
    /// </summary>
    /// <param name="trivia">The trivia to describe</param>
    /// <param name="kind">The kind of whitespace</param>
    /// <returns>The description</returns>
    private static string Describe(SyntaxTrivia trivia, string kind)
    {
        var position = trivia.GetLocation().GetLineSpan().StartLinePosition;

        return $"tab in {kind} at line {position.Line + 1}, column {position.Character + 1}";
    }

    #endregion // Methods
}