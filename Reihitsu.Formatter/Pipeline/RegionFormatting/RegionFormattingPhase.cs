using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Formatter.Pipeline.RegionFormatting;

/// <summary>
/// Region formatting — capitalizes region descriptions and synchronizes
/// endregion comments to match their corresponding region name
/// </summary>
internal static class RegionFormattingPhase
{
    #region Methods

    /// <summary>
    /// Applies region formatting rules to the given syntax node
    /// </summary>
    /// <param name="root">The syntax node to format</param>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The formatted syntax node</returns>
    public static SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        var regions = new Stack<SyntaxTrivia>();
        var pairs = new List<(SyntaxTrivia Region, SyntaxTrivia EndRegion)>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                regions.Push(trivia);
            }
            else if (trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) && regions.Count > 0)
            {
                pairs.Add((regions.Pop(), trivia));
            }
        }

        var replacements = new Dictionary<SyntaxTrivia, SyntaxTrivia>();

        foreach (var (region, endRegion) in pairs)
        {
            var regionDirective = (RegionDirectiveTriviaSyntax)region.GetStructure();
            var regionName = GetRegionName(regionDirective);

            if (string.IsNullOrWhiteSpace(regionName))
            {
                continue;
            }

            var capitalizedName = CapitalizeFirstLetter(regionName);

            if (regionName != capitalizedName)
            {
                replacements[region] = BuildRegionTrivia(regionDirective, capitalizedName);
            }

            var endRegionDirective = (EndRegionDirectiveTriviaSyntax)endRegion.GetStructure();
            var expectedComment = " // " + capitalizedName;
            var currentComment = GetEndRegionComment(endRegionDirective);

            if (currentComment != expectedComment)
            {
                replacements[endRegion] = BuildEndRegionTrivia(endRegionDirective, expectedComment);
            }
        }

        if (replacements.Count > 0)
        {
            root = root.ReplaceTrivia(replacements.Keys, (oldTrivia, _) => replacements[oldTrivia]);
        }

        return RemoveNestedRegions(root, cancellationToken);
    }

    /// <summary>
    /// Determines whether the directive is located within an element body
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <returns><see langword="true"/> if the directive is inside an element body</returns>
    private static bool IsWithinElementBody(SyntaxTrivia directiveTrivia)
    {
        var currentNode = directiveTrivia.Token.Parent;

        while (currentNode != null)
        {
            switch (currentNode)
            {
                case BlockSyntax { Parent: not TypeDeclarationSyntax and not NamespaceDeclarationSyntax and not FileScopedNamespaceDeclarationSyntax and not CompilationUnitSyntax }:
                case AccessorListSyntax:
                case AnonymousFunctionExpressionSyntax:
                case LocalFunctionStatementSyntax:
                case StatementSyntax:
                    return true;

                case TypeDeclarationSyntax:
                case NamespaceDeclarationSyntax:
                case FileScopedNamespaceDeclarationSyntax:
                case CompilationUnitSyntax:
                    return false;

                default:
                    currentNode = currentNode.Parent;

                    break;
            }
        }

        return false;
    }

    /// <summary>
    /// Removes region directives placed within element bodies
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated root</returns>
    private static SyntaxNode RemoveNestedRegions(SyntaxNode root, CancellationToken cancellationToken)
    {
        var sourceText = root.SyntaxTree?.GetText(cancellationToken) ?? SourceText.From(root.ToFullString());
        var removalSpans = new List<TextSpan>();

        foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if ((trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
                && IsWithinElementBody(trivia))
            {
                var line = sourceText.Lines.GetLineFromPosition(trivia.Span.Start);
                var removalEnd = line.EndIncludingLineBreak > line.End
                                     ? line.EndIncludingLineBreak
                                     : line.End;

                removalSpans.Add(TextSpan.FromBounds(line.Start, removalEnd));
            }
        }

        if (removalSpans.Count == 0)
        {
            return root;
        }

        var updatedText = sourceText;

        foreach (var removalSpan in removalSpans.OrderByDescending(static span => span.Start))
        {
            updatedText = updatedText.Replace(removalSpan, string.Empty);
        }

        return root.SyntaxTree.WithChangedText(updatedText).GetRoot(cancellationToken);
    }

    /// <summary>
    /// Extracts the region name from a <c>#region</c> directive
    /// </summary>
    /// <param name="directive">The region directive syntax</param>
    /// <returns>The region name, or <see langword="null"/> if no name is found</returns>
    private static string GetRegionName(RegionDirectiveTriviaSyntax directive)
    {
        var leadingTrivia = directive.EndOfDirectiveToken.LeadingTrivia;
        var messageTrivia = leadingTrivia.FirstOrDefault(static trivia => trivia.IsKind(SyntaxKind.PreprocessingMessageTrivia));

        return messageTrivia == default
                   ? null
                   : messageTrivia.ToString().TrimStart();
    }

    /// <summary>
    /// Gets the trailing comment text from an <c>#endregion</c> directive
    /// </summary>
    /// <param name="directive">The endregion directive syntax</param>
    /// <returns>The trailing comment text</returns>
    private static string GetEndRegionComment(EndRegionDirectiveTriviaSyntax directive)
    {
        var combined = directive.EndRegionKeyword.TrailingTrivia.ToFullString()
                       + directive.EndOfDirectiveToken.LeadingTrivia.ToFullString();

        return combined;
    }

    /// <summary>
    /// Returns the given text with its first letter capitalized
    /// </summary>
    /// <param name="text">The text to capitalize</param>
    /// <returns>The text with the first letter in upper case</returns>
    private static string CapitalizeFirstLetter(string text)
    {
        if (text.Length == 0 || char.IsUpper(text[0]))
        {
            return text;
        }

        return char.ToUpperInvariant(text[0]) + text.Substring(1);
    }

    /// <summary>
    /// Builds a replacement <c>#region</c> trivia with the specified name
    /// </summary>
    /// <param name="original">The original region directive</param>
    /// <param name="newName">The new region name</param>
    /// <returns>The replacement trivia</returns>
    private static SyntaxTrivia BuildRegionTrivia(RegionDirectiveTriviaSyntax original, string newName)
    {
        var cleanKeyword = original.RegionKeyword.WithTrailingTrivia(SyntaxTriviaList.Empty);
        var endToken = original.EndOfDirectiveToken;
        var newEndToken = endToken.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(" " + newName)));
        var newDirective = original.WithRegionKeyword(cleanKeyword).WithEndOfDirectiveToken(newEndToken);

        return SyntaxFactory.Trivia(newDirective);
    }

    /// <summary>
    /// Builds a replacement <c>#endregion</c> trivia with the specified comment
    /// </summary>
    /// <param name="original">The original endregion directive</param>
    /// <param name="comment">The comment to attach</param>
    /// <returns>The replacement trivia</returns>
    private static SyntaxTrivia BuildEndRegionTrivia(EndRegionDirectiveTriviaSyntax original, string comment)
    {
        var cleanKeyword = original.EndRegionKeyword.WithTrailingTrivia(SyntaxTriviaList.Empty);
        var endToken = original.EndOfDirectiveToken;
        var newEndToken = endToken.WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(comment)));
        var newDirective = original.WithEndRegionKeyword(cleanKeyword).WithEndOfDirectiveToken(newEndToken);

        return SyntaxFactory.Trivia(newDirective);
    }

    #endregion // Methods
}