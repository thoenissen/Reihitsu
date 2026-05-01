using System;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

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
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var line = text.Lines.GetLineFromPosition(diagnosticSpan.Start);
        var lineText = text.ToString(TextSpan.FromBounds(line.Start, line.End));
        var updatedLineText = RemoveForbiddenSuffixFromLine(lineText);

        return updatedLineText == lineText
                   ? document
                   : document.WithText(text.Replace(new TextSpan(line.Start, line.End - line.Start), updatedLineText));
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

        if (removalStart >= 2 && lineText.Substring(0, removalStart).EndsWith("//", StringComparison.Ordinal))
        {
            removalStart -= 2;

            while (removalStart > 0 && char.IsWhiteSpace(lineText[removalStart - 1]))
            {
                removalStart--;
            }
        }

        return lineText.Remove(removalStart, trimmedLength - removalStart);
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
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0388RegionDescriptionsShouldNotEndWithImplementationCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}