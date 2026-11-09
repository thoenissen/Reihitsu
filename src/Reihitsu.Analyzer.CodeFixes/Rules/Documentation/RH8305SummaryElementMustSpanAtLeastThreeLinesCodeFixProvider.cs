using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider))]
public class RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider : CodeFixProvider
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

        if (root == null)
        {
            return document;
        }

        var declaration = FindDeclaration(root, diagnosticSpan);

        if (declaration == null)
        {
            return document;
        }

        return (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, declaration, cancellationToken).ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Finds the declaration the fix formats: the member or enum member that owns the reported documentation comment
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="diagnosticSpan">The diagnostic's span</param>
    /// <returns>The declaration, or <see langword="null"/> when there is none</returns>
    private static SyntaxNode FindDeclaration(SyntaxNode root, TextSpan diagnosticSpan)
    {
        var diagnosticNode = root.FindNode(diagnosticSpan, findInsideTrivia: true, getInnermostNodeForTie: true);

        return diagnosticNode.AncestorsAndSelf().FirstOrDefault(static obj => obj is MemberDeclarationSyntax or EnumMemberDeclarationSyntax);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId];

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
            var declaration = FindDeclaration(root, diagnostic.Location.SourceSpan);

            if (declaration != null
                && ReihitsuFormatter.FindUnsupportedSyntax(declaration) == null)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8305Title,
                                                          token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                          nameof(RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}