using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Compares the tree a formatter phase returned with the tree its text gives when it is parsed again with the parse options
/// of the tree the formatter was given: the same nodes in the same structure, the same tokens with the same kinds, texts,
/// and places, no syntax error, and every comment, documentation comment, directive, and disabled-text span as trivia of
/// the same token on the same side. Whitespace and line breaks are not compared beyond their places, which equal texts
/// already fix
/// </summary>
internal static class PhaseTreeRoundTrip
{
    #region Methods

    /// <summary>
    /// Runs the production pipeline on the root with an observer and describes every phase whose returned tree differs from
    /// the tree its text parses back to
    /// </summary>
    /// <param name="root">The root of the tree the formatter was given</param>
    /// <param name="context">The formatting context</param>
    /// <param name="parseOptions">The parse options of the tree the formatter was given</param>
    /// <returns>The final output of the pipeline and one description per phase that does not round-trip</returns>
    public static (SyntaxNode Result, IReadOnlyList<string> Failures) ExecuteAndCompare(SyntaxNode root, FormattingContext context, CSharpParseOptions parseOptions)
    {
        var failures = new List<string>();

        var result = FormattingPipeline.Execute(root,
                                                context,
                                                (phaseName, _, after) =>
                                                {
                                                    var difference = FindDifference(after, parseOptions);

                                                    if (difference != null)
                                                    {
                                                        failures.Add($"The output of {phaseName} does not parse back to its tree (FMT-PIPE-021): {difference}");
                                                    }
                                                },
                                                CancellationToken.None);

        return (result, failures);
    }

    /// <summary>
    /// Parses the text of the phase's tree again and describes the first difference between the two trees
    /// </summary>
    /// <param name="phaseOutput">The tree a phase returned</param>
    /// <param name="parseOptions">The parse options of the tree the formatter was given</param>
    /// <returns>A description of the first difference, or <see langword="null"/> when the trees are the same</returns>
    public static string FindDifference(SyntaxNode phaseOutput, CSharpParseOptions parseOptions)
    {
        var text = phaseOutput.ToFullString();
        var reparsed = CSharpSyntaxTree.ParseText(text, parseOptions).GetRoot();

        foreach (var diagnostic in reparsed.GetDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                return $"the text has a syntax error: {diagnostic}{Environment.NewLine}{text}";
            }
        }

        var expected = phaseOutput.DescendantNodesAndTokensAndSelf().ToList();
        var actual = reparsed.DescendantNodesAndTokensAndSelf().ToList();

        for (var index = 0; index < Math.Min(expected.Count, actual.Count); index++)
        {
            var difference = CompareElements(expected[index], actual[index]);

            if (difference != null)
            {
                return $"{difference} at line {GetLine(text, expected[index].Span.Start)}{Environment.NewLine}{text}";
            }
        }

        if (expected.Count != actual.Count)
        {
            return $"the tree has {expected.Count} nodes and tokens, the reparsed text {actual.Count}{Environment.NewLine}{text}";
        }

        return null;
    }

    /// <summary>
    /// Compares two elements at the same position of the pre-order walk of both trees
    /// </summary>
    /// <param name="expected">The element of the phase's tree</param>
    /// <param name="actual">The element of the reparsed tree</param>
    /// <returns>A description of the difference, or <see langword="null"/></returns>
    private static string CompareElements(SyntaxNodeOrToken expected, SyntaxNodeOrToken actual)
    {
        if (expected.IsToken != actual.IsToken
            || expected.RawKind != actual.RawKind)
        {
            return $"the tree holds {Describe(expected)} where the reparsed text holds {Describe(actual)}";
        }

        if (expected.Span != actual.Span)
        {
            return $"{Describe(expected)} spans {expected.Span} in the tree and {actual.Span} in the reparsed text";
        }

        if (expected.IsNode)
        {
            var expectedChildren = expected.AsNode().ChildNodesAndTokens().Count;
            var actualChildren = actual.AsNode().ChildNodesAndTokens().Count;

            return expectedChildren == actualChildren
                       ? null
                       : $"{Describe(expected)} has {expectedChildren} children in the tree and {actualChildren} in the reparsed text";
        }

        var expectedToken = expected.AsToken();
        var actualToken = actual.AsToken();

        if (expectedToken.Text != actualToken.Text
            || expectedToken.IsMissing != actualToken.IsMissing)
        {
            return $"the tree holds {Describe(expected)} where the reparsed text holds {Describe(actual)}";
        }

        var leading = CompareTrivia(expectedToken.LeadingTrivia, actualToken.LeadingTrivia);

        if (leading != null)
        {
            return $"the leading trivia of {Describe(expected)} differ: {leading}";
        }

        var trailing = CompareTrivia(expectedToken.TrailingTrivia, actualToken.TrailingTrivia);

        return trailing == null
                   ? null
                   : $"the trailing trivia of {Describe(expected)} differ: {trailing}";
    }

    /// <summary>
    /// Compares the comments, documentation comments, directives, disabled-text spans, and skipped tokens of one side of
    /// a token in both trees
    /// </summary>
    /// <param name="expected">The trivia of the phase's tree</param>
    /// <param name="actual">The trivia of the reparsed tree</param>
    /// <returns>A description of the difference, or <see langword="null"/></returns>
    private static string CompareTrivia(SyntaxTriviaList expected, SyntaxTriviaList actual)
    {
        var expectedItems = GetOwnedTrivia(expected);
        var actualItems = GetOwnedTrivia(actual);

        return expectedItems.SequenceEqual(actualItems)
                   ? null
                   : $"[{string.Join(", ", expectedItems)}] in the tree, [{string.Join(", ", actualItems)}] in the reparsed text";
    }

    /// <summary>
    /// Returns the trivia whose owner FMT-PIPE-021 compares: every trivia that is neither whitespace nor a line break
    /// </summary>
    /// <param name="triviaList">The trivia of one side of a token</param>
    /// <returns>The kind and text of each such trivia</returns>
    private static List<string> GetOwnedTrivia(SyntaxTriviaList triviaList)
    {
        return triviaList.Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                                          && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
                         .Select(trivia => $"{(SyntaxKind)trivia.RawKind} '{trivia.ToFullString().Replace("\r", "\\r").Replace("\n", "\\n")}'")
                         .ToList();
    }

    /// <summary>
    /// Describes a node or token by its kind and, for a token, its text
    /// </summary>
    /// <param name="element">The node or token</param>
    /// <returns>The description</returns>
    private static string Describe(SyntaxNodeOrToken element)
    {
        return element.IsToken
                   ? $"token {(SyntaxKind)element.RawKind} '{element.AsToken().Text}'"
                   : $"node {(SyntaxKind)element.RawKind}";
    }

    /// <summary>
    /// Returns the one-based line of a position in the text
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="position">The position</param>
    /// <returns>The line</returns>
    private static int GetLine(string text, int position)
    {
        var line = 1;

        for (var index = 0; index < position && index < text.Length; index++)
        {
            if (text[index] == '\n')
            {
                line++;
            }
        }

        return line;
    }

    #endregion // Methods
}