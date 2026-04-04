using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Rules.Regions;

/// <summary>
/// Ensures region directive consistency.
/// Covers RH0301 (endregion comments must match their corresponding region names)
/// and RH0328 (region names must start with an uppercase letter).
/// </summary>
internal sealed class RegionFormattingRule : FormattingRuleBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public RegionFormattingRule(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collects all matching region/endregion pairs from the syntax tree using a stack-based approach.
    /// </summary>
    /// <param name="node">The root syntax node.</param>
    /// <returns>A list of matched region/endregion pairs.</returns>
    private static List<RegionPair> CollectRegionPairs(SyntaxNode node)
    {
        var pairs = new List<RegionPair>();
        var stack = new Stack<SyntaxTrivia>();

        foreach (var token in node.DescendantTokens(descendIntoTrivia: true))
        {
            ProcessTriviaList(token.LeadingTrivia, stack, pairs);
            ProcessTriviaList(token.TrailingTrivia, stack, pairs);
        }

        return pairs;
    }

    /// <summary>
    /// Processes a trivia list, pushing region directives onto the stack
    /// and pairing endregion directives with the most recent region.
    /// </summary>
    /// <param name="triviaList">The trivia list to process.</param>
    /// <param name="stack">The stack of unmatched region directives.</param>
    /// <param name="pairs">The list of matched pairs to populate.</param>
    private static void ProcessTriviaList(SyntaxTriviaList triviaList, Stack<SyntaxTrivia> stack, List<RegionPair> pairs)
    {
        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                stack.Push(trivia);
            }
            else if (trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) && stack.Count > 0)
            {
                var matchingRegion = stack.Pop();

                pairs.Add(new RegionPair(matchingRegion, trivia));
            }
        }
    }

    /// <summary>
    /// Extracts the region name from a <c>#region</c> directive trivia.
    /// The name is the text after <c>#region </c> (position 8 in the full trivia text).
    /// </summary>
    /// <param name="regionTrivia">The region directive trivia.</param>
    /// <returns>The region name, or <c>null</c> if the trivia is malformed.</returns>
    private static string GetRegionName(SyntaxTrivia regionTrivia)
    {
        var fullText = regionTrivia.ToFullString();
        var regionIndex = fullText.IndexOf("#region ", StringComparison.Ordinal);

        if (regionIndex < 0)
        {
            return null;
        }

        var nameStart = regionIndex + 8;
        var name = fullText.Substring(nameStart).TrimEnd('\r', '\n', ' ');

        if (name.Length == 0)
        {
            return null;
        }

        return name;
    }

    /// <summary>
    /// Extracts the comment text from an <c>#endregion</c> directive trivia.
    /// Returns the text after <c>#endregion</c> (position 10 in the full trivia text), trimmed of trailing whitespace.
    /// </summary>
    /// <param name="endRegionTrivia">The endregion directive trivia.</param>
    /// <returns>The comment text after <c>#endregion</c>, or an empty string if none exists.</returns>
    private static string GetEndRegionComment(SyntaxTrivia endRegionTrivia)
    {
        var fullText = endRegionTrivia.ToFullString();
        var endRegionIndex = fullText.IndexOf("#endregion", StringComparison.Ordinal);

        if (endRegionIndex < 0)
        {
            return string.Empty;
        }

        var commentStart = endRegionIndex + 10;
        var comment = fullText.Substring(commentStart).TrimEnd('\r', '\n', ' ');

        return comment;
    }

    /// <summary>
    /// Ensures that the first character of the given name is uppercase.
    /// </summary>
    /// <param name="name">The region name.</param>
    /// <returns>The name with its first character converted to uppercase.</returns>
    private static string EnsureUppercaseStart(string name)
    {
        if (name.Length == 0 || char.IsUpper(name[0]))
        {
            return name;
        }

        return char.ToUpperInvariant(name[0]) + name.Substring(1);
    }

    /// <summary>
    /// Builds a replacement <c>#region</c> directive trivia with the specified name,
    /// preserving the original directive's indentation and trailing trivia.
    /// </summary>
    /// <param name="regionName">The corrected region name.</param>
    /// <param name="originalRegionTrivia">The original region directive trivia to preserve formatting from.</param>
    /// <returns>The replacement trivia.</returns>
    private static SyntaxTrivia BuildRegionTrivia(string regionName, SyntaxTrivia originalRegionTrivia)
    {
        var originalDirective = (RegionDirectiveTriviaSyntax)originalRegionTrivia.GetStructure();
        var originalHashLeadingTrivia = originalDirective!.HashToken.LeadingTrivia;
        var originalEndOfDirectiveTrailingTrivia = originalDirective.EndOfDirectiveToken.TrailingTrivia;

        var endOfDirectiveToken = SyntaxFactory.Token(SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(" " + regionName)),
                                                      SyntaxKind.EndOfDirectiveToken,
                                                      originalEndOfDirectiveTrailingTrivia);

        var hashToken = SyntaxFactory.Token(originalHashLeadingTrivia,
                                            SyntaxKind.HashToken,
                                            SyntaxFactory.TriviaList());

        var regionDirective = SyntaxFactory.RegionDirectiveTrivia(true)
                                           .WithHashToken(hashToken)
                                           .WithEndOfDirectiveToken(endOfDirectiveToken);

        return SyntaxFactory.Trivia(regionDirective);
    }

    /// <summary>
    /// Builds a replacement <c>#endregion</c> directive trivia with a comment matching the specified region name,
    /// preserving the original directive's indentation and trailing trivia.
    /// The format is <c>#endregion // &lt;region_name&gt;</c>.
    /// </summary>
    /// <param name="regionName">The region name to include in the endregion comment.</param>
    /// <param name="originalEndRegionTrivia">The original endregion directive trivia to preserve formatting from.</param>
    /// <returns>The replacement trivia.</returns>
    private static SyntaxTrivia BuildEndRegionTrivia(string regionName, SyntaxTrivia originalEndRegionTrivia)
    {
        var originalDirective = (EndRegionDirectiveTriviaSyntax)originalEndRegionTrivia.GetStructure();
        var originalHashLeadingTrivia = originalDirective!.HashToken.LeadingTrivia;
        var originalEndOfDirectiveTrailingTrivia = originalDirective.EndOfDirectiveToken.TrailingTrivia;

        var endRegionKeyword = SyntaxFactory.Token(SyntaxFactory.TriviaList(),
                                                   SyntaxKind.EndRegionKeyword,
                                                   SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage($" // {regionName}")));

        var hashToken = SyntaxFactory.Token(originalHashLeadingTrivia,
                                            SyntaxKind.HashToken,
                                            SyntaxFactory.TriviaList());

        var endOfDirectiveToken = SyntaxFactory.Token(SyntaxFactory.TriviaList(),
                                                      SyntaxKind.EndOfDirectiveToken,
                                                      originalEndOfDirectiveTrailingTrivia);

        var endRegionDirective = SyntaxFactory.EndRegionDirectiveTrivia(true)
                                              .WithHashToken(hashToken)
                                              .WithEndRegionKeyword(endRegionKeyword)
                                              .WithEndOfDirectiveToken(endOfDirectiveToken);

        return SyntaxFactory.Trivia(endRegionDirective);
    }

    /// <summary>
    /// Processes all region and endregion directives in the syntax tree,
    /// applying RH0328 (uppercase region names) and RH0301 (matching endregion comments).
    /// </summary>
    /// <param name="node">The root syntax node.</param>
    /// <returns>The syntax node with corrected region directives.</returns>
    private SyntaxNode ProcessRegions(SyntaxNode node)
    {
        var regionPairs = CollectRegionPairs(node);
        var replacements = new Dictionary<SyntaxTrivia, SyntaxTrivia>();

        foreach (var pair in regionPairs)
        {
            var regionTrivia = pair.Region;
            var endRegionTrivia = pair.EndRegion;

            var regionName = GetRegionName(regionTrivia);

            if (regionName == null)
            {
                continue;
            }

            var correctedName = EnsureUppercaseStart(regionName);

            if (correctedName != regionName)
            {
                var newRegionTrivia = BuildRegionTrivia(correctedName, regionTrivia);

                replacements[regionTrivia] = newRegionTrivia;
            }

            var expectedEndRegionComment = " // " + correctedName;
            var currentEndRegionComment = GetEndRegionComment(endRegionTrivia);

            if (currentEndRegionComment != expectedEndRegionComment)
            {
                var newEndRegionTrivia = BuildEndRegionTrivia(correctedName, endRegionTrivia);

                replacements[endRegionTrivia] = newEndRegionTrivia;
            }
        }

        if (replacements.Count == 0)
        {
            return node;
        }

        return node.ReplaceTrivia(replacements.Keys, (original, _) => replacements[original]);
    }

    #endregion // Methods

    #region FormattingRuleBase

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        if (node == null)
        {
            return null;
        }

        return ProcessRegions(node);
    }

    #endregion // FormattingRuleBase

    #region IFormattingRule

    /// <inheritdoc/>
    public override FormattingPhase Phase => FormattingPhase.RegionFormatting;

    #endregion // IFormattingRule
}