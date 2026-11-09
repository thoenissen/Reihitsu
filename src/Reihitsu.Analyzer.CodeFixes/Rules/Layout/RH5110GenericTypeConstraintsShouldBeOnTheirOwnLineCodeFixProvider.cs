using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider))]
public class RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Finds the declaration the fix formats: the declaration that owns the reported constraint clause
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">The diagnostic's span</param>
    /// <returns>The declaration, or <see langword="null"/> when there is none</returns>
    private static SyntaxNode FindDeclaration(SyntaxNode root, TextSpan diagnosticSpan)
    {
        return root.FindToken(diagnosticSpan.Start).Parent?.FirstAncestorOrSelf<TypeParameterConstraintClauseSyntax>()?.Parent;
    }

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

        if (root == null)
        {
            return document;
        }

        var declaration = FindDeclaration(root, diagnosticSpan);

        return declaration == null
                   ? document
                   : (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, declaration, cancellationToken).ConfigureAwait(false)).Output;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineAnalyzer.DiagnosticId];

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
            if (FindDeclaration(root, diagnostic.Location.SourceSpan) is { } declaration
                && ReihitsuFormatter.FindUnsupportedSyntax(declaration) != null)
            {
                continue;
            }

            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5110Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH5110GenericTypeConstraintsShouldBeOnTheirOwnLineCodeFixProvider)),
                                    diagnostic);
        }
    }

    #endregion // CodeFixProvider
}