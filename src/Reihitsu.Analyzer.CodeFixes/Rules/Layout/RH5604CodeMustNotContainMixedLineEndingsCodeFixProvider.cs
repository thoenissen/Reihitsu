using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5604CodeMustNotContainMixedLineEndingsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider))]
public class RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applies the code fix: rewrites every line break that differs from the predominant line ending through the rewrite the
    /// formatter's line-ending phase shares, without any of the formatter's checks, so the fix also applies to files the
    /// formatter refuses
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

        var normalizedRoot = LineEndingUtilities.NormalizeLineEndings(root, LineEndingUtilities.DetectEndOfLine(root));

        return normalizedRoot == root
                   ? document
                   : document.WithSyntaxRoot(normalizedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5604CodeMustNotContainMixedLineEndingsAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected sealed override Task RegisterCodeFixesCoreAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5604Title,
                                                      token => ApplyCodeFixAsync(context.Document, token),
                                                      nameof(RH5604CodeMustNotContainMixedLineEndingsCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // ReihitsuCodeFixProviderBase
}