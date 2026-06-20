using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarCodeFixProvider))]
public class RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Removes all interpolation markers ($) from the interpolated string at the given span
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> RemoveInterpolationMarkersAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        if (root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is not InterpolatedStringExpressionSyntax node)
        {
            return document;
        }

        var replacementNode = StringInterpolationUtilities.RemoveInterpolationMarkers(node);

        if (replacementNode == node)
        {
            return document;
        }

        return document.WithSyntaxRoot(root.ReplaceNode(node, replacementNode));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3204Title,
                                                      token => RemoveInterpolationMarkersAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}