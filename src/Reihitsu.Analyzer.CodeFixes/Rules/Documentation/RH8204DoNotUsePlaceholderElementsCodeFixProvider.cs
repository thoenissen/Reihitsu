using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Documentation;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8204DoNotUsePlaceholderElementsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8204DoNotUsePlaceholderElementsCodeFixProvider))]
public class RH8204DoNotUsePlaceholderElementsCodeFixProvider : CodeFixProvider
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
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var placeholderText = text.ToString(diagnosticSpan);
        var replacementText = ExtractInnerContent(placeholderText);

        return document.WithText(text.Replace(diagnosticSpan, replacementText));
    }

    /// <summary>
    /// Extracts the inner content of a placeholder element, unwrapping the placeholder tags
    /// </summary>
    /// <param name="placeholderText">The placeholder element text</param>
    /// <returns>The inner content with the placeholder tags removed</returns>
    private static string ExtractInnerContent(string placeholderText)
    {
        try
        {
            var placeholderElement = XElement.Parse(placeholderText);

            return string.Concat(placeholderElement.Nodes().Select(obj => obj.ToString(SaveOptions.DisableFormatting)));
        }
        catch (XmlException)
        {
            // Documentation content is not always well-formed XML (for example unescaped '&' or '<'). Strip the
            // placeholder tags textually instead of letting the parse failure surface as an unhandled exception
            return StripPlaceholderTags(placeholderText);
        }
    }

    /// <summary>
    /// Removes the outer placeholder tags textually, keeping the inner content untouched
    /// </summary>
    /// <param name="placeholderText">The placeholder element text</param>
    /// <returns>The inner content with the surrounding tags removed</returns>
    private static string StripPlaceholderTags(string placeholderText)
    {
        var openingEnd = placeholderText.IndexOf('>');

        if (openingEnd < 0)
        {
            return placeholderText;
        }

        if (openingEnd > 0 && placeholderText[openingEnd - 1] == '/')
        {
            return string.Empty;
        }

        var closingStart = placeholderText.LastIndexOf('<');

        if (closingStart <= openingEnd)
        {
            return string.Empty;
        }

        return placeholderText.Substring(openingEnd + 1, closingStart - openingEnd - 1);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8204Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH8204DoNotUsePlaceholderElementsCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}