using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5404ElementMustNotBeOnSingleLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5404ElementMustNotBeOnSingleLineCodeFixProvider))]
public class RH5404ElementMustNotBeOnSingleLineCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="declaration">Single-line type declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, BaseTypeDeclarationSyntax declaration, CancellationToken cancellationToken)
    {
        return (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, declaration, cancellationToken).ConfigureAwait(false)).Output;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId];

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
                // No fix is offered for an extension block: the formatter does not place its braces on their own lines.
                if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>() is { } declaration
                    && declaration is not ExtensionBlockDeclarationSyntax
                    && ReihitsuFormatter.FindUnsupportedSyntax(declaration) == null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5404Title,
                                                              token => ApplyCodeFixAsync(context.Document, declaration, token),
                                                              nameof(RH5404ElementMustNotBeOnSingleLineCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}