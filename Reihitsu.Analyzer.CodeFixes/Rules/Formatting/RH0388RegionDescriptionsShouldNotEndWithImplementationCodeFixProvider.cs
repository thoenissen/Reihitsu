using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0388RegionDescriptionsShouldNotEndWithImplementationAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0388RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider))]
public class RH0388RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider : CodeFixProvider
{
    #region Constants

    /// <summary>
    /// Forbidden suffix
    /// </summary>
    private const string ForbiddenSuffix = "implementation";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

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
            var regionDescription = GetRegionDescription(regionDirective);
            var updatedRegionDescription = TrimForbiddenSuffix(regionDescription);

            if (updatedRegionDescription != regionDescription)
            {
                replacements[region] = BuildRegionTrivia(regionDirective, updatedRegionDescription);
            }

            var endRegionDirective = (EndRegionDirectiveTriviaSyntax)endRegion.GetStructure();
            var endRegionDescription = GetEndRegionDescription(endRegionDirective);
            var updatedEndRegionDescription = updatedRegionDescription != regionDescription
                                                  ? updatedRegionDescription
                                                  : TrimForbiddenSuffix(endRegionDescription);

            if (updatedEndRegionDescription != endRegionDescription)
            {
                replacements[endRegion] = BuildEndRegionTrivia(endRegionDirective, updatedEndRegionDescription);
            }
        }

        return replacements.Count == 0
                   ? document
                   : document.WithSyntaxRoot(root.ReplaceTrivia(replacements.Keys, (oldTrivia, _) => replacements[oldTrivia]));
    }

    /// <summary>
    /// Builds a replacement <c>#endregion</c> trivia with the specified comment
    /// </summary>
    /// <param name="original">The original endregion directive</param>
    /// <param name="description">The updated description</param>
    /// <returns>The replacement trivia</returns>
    private static SyntaxTrivia BuildEndRegionTrivia(EndRegionDirectiveTriviaSyntax original, string description)
    {
        var cleanKeyword = original.EndRegionKeyword.WithTrailingTrivia(SyntaxTriviaList.Empty);
        var endToken = original.EndOfDirectiveToken;
        var newLeadingTrivia = string.IsNullOrWhiteSpace(description)
                                   ? SyntaxTriviaList.Empty
                                   : SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(" // " + description));
        var newEndToken = endToken.WithLeadingTrivia(newLeadingTrivia);
        var newDirective = original.WithEndRegionKeyword(cleanKeyword)
                                   .WithEndOfDirectiveToken(newEndToken);

        return SyntaxFactory.Trivia(newDirective);
    }

    /// <summary>
    /// Builds a replacement <c>#region</c> trivia with the specified name
    /// </summary>
    /// <param name="original">The original region directive</param>
    /// <param name="description">The updated description</param>
    /// <returns>The replacement trivia</returns>
    private static SyntaxTrivia BuildRegionTrivia(RegionDirectiveTriviaSyntax original, string description)
    {
        var cleanKeyword = original.RegionKeyword.WithTrailingTrivia(SyntaxTriviaList.Empty);
        var endToken = original.EndOfDirectiveToken;
        var newLeadingTrivia = string.IsNullOrWhiteSpace(description)
                                   ? SyntaxTriviaList.Empty
                                   : SyntaxFactory.TriviaList(SyntaxFactory.PreprocessingMessage(" " + description));
        var newEndToken = endToken.WithLeadingTrivia(newLeadingTrivia);
        var newDirective = original.WithRegionKeyword(cleanKeyword)
                                   .WithEndOfDirectiveToken(newEndToken);

        return SyntaxFactory.Trivia(newDirective);
    }

    /// <summary>
    /// Determines whether the specified description ends with the forbidden suffix
    /// </summary>
    /// <param name="description">Description to inspect</param>
    /// <returns><see langword="true"/> if the suffix is present</returns>
    private static bool EndsWithForbiddenSuffix(string description)
    {
        var trimmedDescription = description.Trim();

        if (trimmedDescription.EndsWith(ForbiddenSuffix, StringComparison.OrdinalIgnoreCase) == false)
        {
            return false;
        }

        return trimmedDescription.Length == ForbiddenSuffix.Length
               || char.IsWhiteSpace(trimmedDescription[trimmedDescription.Length - ForbiddenSuffix.Length - 1]);
    }

    /// <summary>
    /// Gets the description of an endregion directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>Description text</returns>
    private static string GetEndRegionDescription(EndRegionDirectiveTriviaSyntax directive)
    {
        var description = (directive.EndRegionKeyword.TrailingTrivia.ToFullString()
                           + directive.EndOfDirectiveToken.LeadingTrivia.ToFullString()).Trim();

        if (description.StartsWith("//", StringComparison.Ordinal))
        {
            description = description.Substring(2).TrimStart();
        }

        return description;
    }

    /// <summary>
    /// Gets the description of a region directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns>Description text</returns>
    private static string GetRegionDescription(RegionDirectiveTriviaSyntax directive)
    {
        var messageTrivia = directive.EndOfDirectiveToken.LeadingTrivia.FirstOrDefault(static trivia => trivia.IsKind(SyntaxKind.PreprocessingMessageTrivia));

        return messageTrivia == default
                   ? string.Empty
                   : messageTrivia.ToString().Trim();
    }

    /// <summary>
    /// Trims the forbidden suffix from the end of a region description
    /// </summary>
    /// <param name="description">Description to update</param>
    /// <returns>The updated description</returns>
    private static string TrimForbiddenSuffix(string description)
    {
        if (EndsWithForbiddenSuffix(description) == false)
        {
            return description;
        }

        var trimmedDescription = description.Trim();

        return trimmedDescription.Substring(0, trimmedDescription.Length - ForbiddenSuffix.Length).TrimEnd();
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0388RegionDescriptionsShouldNotEndWithImplementationAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0388Title,
                                                      token => ApplyCodeFixAsync(context.Document, token),
                                                      nameof(RH0388RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}