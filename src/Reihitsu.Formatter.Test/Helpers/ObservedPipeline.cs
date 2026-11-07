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
    #region Constants

    /// <summary>
    /// Name under which the pipeline observer reports the TabReplacement sub-phase of the preparation phase
    /// </summary>
    public const string TabReplacementPhaseName = "PreparationPhase/TabReplacement";

    /// <summary>
    /// Name under which the pipeline observer reports the LineEndings sub-phase of the preparation phase
    /// </summary>
    public const string LineEndingsPhaseName = "PreparationPhase/LineEndings";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Formats the input through the production pipeline and records every phase
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <returns>The phases in the order they ran</returns>
    public static IReadOnlyList<ObservedPhase> Run(string input, string endOfLine)
    {
        return Run(input, endOfLine, null);
    }

    /// <summary>
    /// Formats the input, parsed with the given parse options, through the production pipeline and records every phase
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <returns>The phases in the order they ran</returns>
    public static IReadOnlyList<ObservedPhase> Run(string input, string endOfLine, CSharpParseOptions parseOptions)
    {
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions);
        var context = new FormattingContext(endOfLine, languageVersion: LanguageVersionResolver.Resolve(tree.Options));
        var phases = new List<ObservedPhase>();

        FormattingPipeline.Execute(tree.GetRoot(),
                                   context,
                                   (phaseName, before, after) => phases.Add(new ObservedPhase(phaseName, before, after)),
                                   CancellationToken.None);

        return phases;
    }

    /// <summary>
    /// Formats the input through the production pipeline and returns the tree the named phase returned
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <param name="phaseName">The name under which the pipeline observer reports the phase</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <returns>The tree the phase returned</returns>
    public static SyntaxNode GetPhaseOutput(string input, string endOfLine, string phaseName, CSharpParseOptions parseOptions = null)
    {
        var phase = Run(input, endOfLine, parseOptions).SingleOrDefault(observedPhase => observedPhase.Name == phaseName);

        Assert.IsNotNull(phase, $"The pipeline should run the phase {phaseName}.");

        return phase.Output;
    }

    /// <summary>
    /// Returns the output of the first phase and asserts that the first phase is the TabReplacement sub-phase of the
    /// preparation phase
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <returns>The source text the first phase returned</returns>
    public static string RunTabReplacementPhase(string input, string endOfLine)
    {
        var phases = Run(input, endOfLine);

        Assert.AreEqual(TabReplacementPhaseName, phases[0].Name, "The first phase of the pipeline should be the TabReplacement sub-phase of the preparation phase.");

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
    /// Describes every line break that the line-ending rules cover and that differs from the given line ending: end-of-line
    /// trivia between tokens, the line break that ends a directive, and the newlines of a documentation comment. Line breaks
    /// inside literals, inside comments that are not documentation comments, and inside disabled text are not reported,
    /// because they are part of the text of a token or a trivia and not line breaks of their own
    /// </summary>
    /// <param name="root">The syntax root to scan</param>
    /// <param name="endOfLine">The line ending every reported line break should have</param>
    /// <returns>One description per line break that differs from the line ending</returns>
    public static IReadOnlyList<string> FindForeignLineBreaks(SyntaxNode root, string endOfLine)
    {
        var findings = new List<string>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia)
                && trivia.ToFullString() != endOfLine)
            {
                findings.Add(DescribeLineBreak(trivia.ToFullString(), trivia.GetLocation(), trivia.IsPartOfStructuredTrivia() ? "directive or documentation trivia" : "end-of-line trivia"));
            }
        }

        foreach (var token in root.DescendantTokens(descendIntoTrivia: true))
        {
            if (token.IsKind(SyntaxKind.XmlTextLiteralNewLineToken)
                && token.Text != endOfLine)
            {
                findings.Add(DescribeLineBreak(token.Text, token.GetLocation(), "documentation newline"));
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
    /// Describes a line break by its characters, its carrier, and its position
    /// </summary>
    /// <param name="text">The characters of the line break</param>
    /// <param name="location">The location of the line break</param>
    /// <param name="carrier">What carries the line break</param>
    /// <returns>The description</returns>
    private static string DescribeLineBreak(string text, Location location, string carrier)
    {
        var position = location.GetLineSpan().StartLinePosition;
        var characters = string.Join(" ", text.Select(character => $"U+{(int)character:X4}"));

        return $"{characters} in {carrier} at line {position.Line + 1}, column {position.Character + 1}";
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