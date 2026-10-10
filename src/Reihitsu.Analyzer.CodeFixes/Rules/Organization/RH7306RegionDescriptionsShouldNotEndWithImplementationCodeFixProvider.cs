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
/// Code fix provider for <see cref="RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7306RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider))]
public class RH7306RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider : CodeFixProvider
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
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var replacements = new List<(TextSpan Span, string Text)>();

        foreach (var directiveSpan in GetDirectiveLineStarts(root, diagnosticSpan))
        {
            var line = text.Lines.GetLineFromPosition(directiveSpan);
            var lineSpan = TextSpan.FromBounds(line.Start, line.End);
            var lineText = text.ToString(lineSpan);
            var updatedLineText = RemoveForbiddenSuffixFromLine(lineText);

            if (updatedLineText != lineText)
            {
                replacements.Add((lineSpan, updatedLineText));
            }
        }

        if (replacements.Count == 0)
        {
            return document;
        }

        var updatedText = text;

        foreach (var replacement in replacements.OrderByDescending(static obj => obj.Span.Start))
        {
            updatedText = updatedText.Replace(replacement.Span, replacement.Text);
        }

        return document.WithText(updatedText);
    }

    /// <summary>
    /// Gets the line start positions of the flagged directive and its matching paired directive
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <returns>The line start positions to inspect</returns>
    private static IEnumerable<int> GetDirectiveLineStarts(SyntaxNode root, TextSpan diagnosticSpan)
    {
        var directive = root.FindTrivia(diagnosticSpan.Start, findInsideTrivia: true).GetStructure() as DirectiveTriviaSyntax;

        if (directive == null)
        {
            var tokenParent = root.FindToken(diagnosticSpan.Start, findInsideTrivia: true).Parent;

            directive = tokenParent?.AncestorsAndSelf()
                                   .OfType<DirectiveTriviaSyntax>()
                                   .FirstOrDefault();
        }

        if (directive == null)
        {
            yield return diagnosticSpan.Start;

            yield break;
        }

        yield return directive.GetLocation().SourceSpan.Start;

        foreach (var related in directive.GetRelatedDirectives())
        {
            if (related != directive
                && (related.IsKind(SyntaxKind.RegionDirectiveTrivia) || related.IsKind(SyntaxKind.EndRegionDirectiveTrivia)))
            {
                yield return related.GetLocation().SourceSpan.Start;
            }
        }
    }

    /// <summary>
    /// Determines whether the specified description ends with the forbidden suffix
    /// </summary>
    /// <param name="description">Description to inspect</param>
    /// <returns><see langword="true"/> if the suffix is present</returns>
    private static bool EndsWithForbiddenSuffix(string description)
    {
        var trimmedDescription = description.TrimEnd();

        if (trimmedDescription.EndsWith(ForbiddenSuffix, StringComparison.OrdinalIgnoreCase) == false)
        {
            return false;
        }

        return trimmedDescription.Length == ForbiddenSuffix.Length
               || char.IsWhiteSpace(trimmedDescription[trimmedDescription.Length - ForbiddenSuffix.Length - 1]);
    }

    /// <summary>
    /// Removes the forbidden suffix from a directive line
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <returns>The updated line text</returns>
    private static string RemoveForbiddenSuffixFromLine(string lineText)
    {
        if (EndsWithForbiddenSuffix(lineText) == false)
        {
            return lineText;
        }

        var trimmedLength = lineText.TrimEnd().Length;
        var removalStart = trimmedLength - ForbiddenSuffix.Length;

        while (removalStart > 0 && char.IsWhiteSpace(lineText[removalStart - 1]))
        {
            removalStart--;
        }

        return lineText.Remove(removalStart, trimmedLength - removalStart);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7306Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH7306RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}