using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5201MethodChainsShouldBeAlignedAnalyzer"/>. The whole chain is laid out by the
/// formatter, so every link gets the line breaks and columns that formatting the document gives it, and an operator such as
/// <c>?.</c> or <c>!.</c> is never split
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5201MethodChainsShouldBeAlignedCodeFixProvider))]
public class RH5201MethodChainsShouldBeAlignedCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5201MethodChainsShouldBeAlignedAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool FormatsThroughFormatter => true;

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var chainNode = FluentChainCodeFixHelper.FindChainNode(root, diagnostic);

                if (chainNode != null
                    && FluentChainCodeFixHelper.IsFormattingRefused(chainNode) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5201Title,
                                                              cancellationToken => FluentChainCodeFixHelper.FormatChainAsync(context.Document, chainNode, cancellationToken),
                                                              nameof(RH5201MethodChainsShouldBeAlignedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}