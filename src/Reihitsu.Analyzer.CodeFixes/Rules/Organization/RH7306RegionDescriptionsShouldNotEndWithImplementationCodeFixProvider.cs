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

        if (root == null
            || TryGetDirectivesToFix(root, diagnosticSpan, out var directives) == false)
        {
            return document;
        }

        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var replacements = new List<(TextSpan Span, string Text)>();

        foreach (var directive in directives)
        {
            var line = text.Lines.GetLineFromPosition(directive.SpanStart);
            var lineSpan = TextSpan.FromBounds(line.Start, line.End);
            var lineText = text.ToString(lineSpan);

            replacements.Add((lineSpan, RemoveForbiddenSuffixFromLine(lineText)));
        }

        var updatedText = text;

        foreach (var replacement in replacements.OrderByDescending(static obj => obj.Span.Start))
        {
            updatedText = updatedText.Replace(replacement.Span, replacement.Text);
        }

        return document.WithText(updatedText);
    }

    /// <summary>
    /// Gets the reported directive
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <returns>The reported region or endregion directive, or <see langword="null"/></returns>
    private static DirectiveTriviaSyntax GetReportedDirective(SyntaxNode root, TextSpan diagnosticSpan)
    {
        var directive = root.FindTrivia(diagnosticSpan.Start, findInsideTrivia: true).GetStructure() as DirectiveTriviaSyntax;

        if (directive == null)
        {
            var tokenParent = root.FindToken(diagnosticSpan.Start, findInsideTrivia: true).Parent;

            directive = tokenParent?.AncestorsAndSelf()
                                   .OfType<DirectiveTriviaSyntax>()
                                   .FirstOrDefault();
        }

        return directive is RegionDirectiveTriviaSyntax or EndRegionDirectiveTriviaSyntax
                   ? directive
                   : null;
    }

    /// <summary>
    /// Gets the reported directive and its paired directive whose descriptions end with the forbidden suffix, judged
    /// by the analyzer's own predicate on each directive's description. The fix is withheld when nothing would remain
    /// of one of these descriptions
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="directives">Directives whose line loses the suffix</param>
    /// <returns><see langword="true"/> if the fix can remove the suffix from every one of them</returns>
    private static bool TryGetDirectivesToFix(SyntaxNode root, TextSpan diagnosticSpan, out List<DirectiveTriviaSyntax> directives)
    {
        directives = [];

        var directive = GetReportedDirective(root, diagnosticSpan);

        if (directive == null)
        {
            return false;
        }

        var candidates = new List<DirectiveTriviaSyntax>
                         {
                             directive
                         };

        candidates.AddRange(directive.GetRelatedDirectives()
                                     .Where(related => related != directive
                                                       && (related.IsKind(SyntaxKind.RegionDirectiveTrivia) || related.IsKind(SyntaxKind.EndRegionDirectiveTrivia))));

        foreach (var candidate in candidates)
        {
            var description = RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.GetDescription(candidate);

            if (RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.EndsWithForbiddenSuffix(description) == false)
            {
                continue;
            }

            if (RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.IsForbiddenSuffixAlone(description))
            {
                directives.Clear();

                return false;
            }

            directives.Add(candidate);
        }

        return directives.Count > 0;
    }

    /// <summary>
    /// Removes the forbidden suffix and the whitespace in front of it from a directive line whose description ends
    /// with the suffix. The description is the end of the line, so the suffix is the last word of the line
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <returns>The updated line text</returns>
    private static string RemoveForbiddenSuffixFromLine(string lineText)
    {
        var trimmedLength = lineText.TrimEnd().Length;
        var removalStart = trimmedLength - RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzer.ForbiddenSuffix.Length;

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
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            if (TryGetDirectivesToFix(root, diagnostic.Location.SourceSpan, out _) == false)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH7306Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH7306RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}