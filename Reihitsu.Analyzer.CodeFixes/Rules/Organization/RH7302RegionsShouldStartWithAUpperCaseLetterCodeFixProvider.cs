using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Organization;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7302RegionsShouldStartWithAUpperCaseLetterCodeFixProvider))]
public class RH7302RegionsShouldStartWithAUpperCaseLetterCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnostic">Diagnostic to fix</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var tokenParent = root.FindToken(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true).Parent;
        var regionDirective = root.FindTrivia(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true).GetStructure() as RegionDirectiveTriviaSyntax
                                  ?? tokenParent?.AncestorsAndSelf()
                                                .OfType<RegionDirectiveTriviaSyntax>()
                                                .FirstOrDefault();

        if (regionDirective == null
            || TryCapitalizeRegionName(regionDirective, out var updatedRegionDirective, out var originalRegionName, out var updatedRegionName) == false)
        {
            return document;
        }

        var replacements = new List<(TextSpan Span, string Text)>
                           {
                               (regionDirective.ParentTrivia.FullSpan, updatedRegionDirective.ToFullString())
                           };
        var endRegionDirective = regionDirective.GetRelatedDirectives()
                                                .OfType<EndRegionDirectiveTriviaSyntax>()
                                                .FirstOrDefault();

        if (endRegionDirective != null)
        {
            var currentEndRegionText = sourceText.ToString(endRegionDirective.ParentTrivia.FullSpan);
            var updatedEndRegionText = ReplaceExactText(currentEndRegionText, originalRegionName, updatedRegionName);

            if (updatedEndRegionText != currentEndRegionText)
            {
                replacements.Add((endRegionDirective.ParentTrivia.FullSpan, updatedEndRegionText));
            }
        }

        var updatedText = sourceText;

        foreach (var replacement in replacements.OrderByDescending(static obj => obj.Span.Start))
        {
            updatedText = updatedText.Replace(replacement.Span, replacement.Text);
        }

        return document.WithText(updatedText);
    }

    /// <summary>
    /// Replaces all exact text occurrences using ordinal comparison
    /// </summary>
    /// <param name="text">Text to search</param>
    /// <param name="oldValue">Old value</param>
    /// <param name="newValue">New value</param>
    /// <returns>Updated text</returns>
    private static string ReplaceExactText(string text, string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(oldValue))
        {
            return text;
        }

        var result = text;
        var startIndex = result.IndexOf(oldValue, StringComparison.Ordinal);

        while (startIndex >= 0)
        {
            result = $"{result.Substring(0, startIndex)}{newValue}{result.Substring(startIndex + oldValue.Length)}";
            startIndex = result.IndexOf(oldValue, startIndex + newValue.Length, StringComparison.Ordinal);
        }

        return result;
    }

    /// <summary>
    /// Capitalizes the region name in the directive
    /// </summary>
    /// <param name="regionDirective">Region directive</param>
    /// <param name="updatedRegionDirective">Updated directive</param>
    /// <param name="originalRegionName">Original region name</param>
    /// <param name="updatedRegionName">Updated region name</param>
    /// <returns><see langword="true"/> if the region name changed</returns>
    private static bool TryCapitalizeRegionName(RegionDirectiveTriviaSyntax regionDirective,
                                                out RegionDirectiveTriviaSyntax updatedRegionDirective,
                                                out string originalRegionName,
                                                out string updatedRegionName)
    {
        var leadingTrivia = regionDirective.EndOfDirectiveToken.LeadingTrivia;

        for (var triviaIndex = 0; triviaIndex < leadingTrivia.Count; triviaIndex++)
        {
            if (leadingTrivia[triviaIndex].IsKind(SyntaxKind.PreprocessingMessageTrivia) == false)
            {
                continue;
            }

            var messageText = leadingTrivia[triviaIndex].ToString();

            if (TryCapitalizeFirstNonWhitespaceLetter(messageText, out var updatedMessageText, out originalRegionName, out updatedRegionName) == false)
            {
                break;
            }

            leadingTrivia = leadingTrivia.Replace(leadingTrivia[triviaIndex], SyntaxFactory.PreprocessingMessage(updatedMessageText));
            updatedRegionDirective = regionDirective.WithEndOfDirectiveToken(regionDirective.EndOfDirectiveToken.WithLeadingTrivia(leadingTrivia));

            return true;
        }

        updatedRegionDirective = regionDirective;
        originalRegionName = string.Empty;
        updatedRegionName = string.Empty;

        return false;
    }

    /// <summary>
    /// Capitalizes the first non-whitespace letter in the text
    /// </summary>
    /// <param name="text">Text to update</param>
    /// <param name="updatedText">Updated text</param>
    /// <param name="originalName">Original name without leading whitespace</param>
    /// <param name="updatedName">Updated name without leading whitespace</param>
    /// <returns><see langword="true"/> if the text changed</returns>
    private static bool TryCapitalizeFirstNonWhitespaceLetter(string text, out string updatedText, out string originalName, out string updatedName)
    {
        for (var charIndex = 0; charIndex < text.Length; charIndex++)
        {
            if (char.IsWhiteSpace(text[charIndex]))
            {
                continue;
            }

            originalName = text.Substring(charIndex);

            if (char.IsUpper(text[charIndex]))
            {
                updatedText = text;
                updatedName = originalName;

                return false;
            }

            updatedText = $"{text.Substring(0, charIndex)}{char.ToUpperInvariant(text[charIndex])}{text.Substring(charIndex + 1)}";
            updatedName = updatedText.Substring(charIndex);

            return true;
        }

        updatedText = text;
        originalName = string.Empty;
        updatedName = string.Empty;

        return false;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7302Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic, token),
                                                      nameof(RH7302RegionsShouldStartWithAUpperCaseLetterCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}