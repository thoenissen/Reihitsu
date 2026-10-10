using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider))]
public class RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider : CodeFixProvider
{
    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var chainNode = FluentChainCodeFixHelper.FindChainNode(root, diagnostic);

                if (chainNode != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5112Title,
                                                              cancellationToken => FluentChainCodeFixHelper.FormatChainAsync(context.Document, chainNode, cancellationToken),
                                                              nameof(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}