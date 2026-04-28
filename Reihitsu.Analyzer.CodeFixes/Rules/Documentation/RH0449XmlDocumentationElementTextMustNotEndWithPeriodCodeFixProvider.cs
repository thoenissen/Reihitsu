using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH0449XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0449XmlDocumentationElementTextMustNotEndWithPeriodCodeFixProvider))]
public class RH0449XmlDocumentationElementTextMustNotEndWithPeriodCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix.
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var updatedDocument = document.WithText(sourceText.Replace(diagnosticSpan, string.Empty));
        var root = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var declaration = root?.FindToken(diagnosticSpan.Start, findInsideTrivia: true)
                              .Parent?.AncestorsAndSelf()
                              .FirstOrDefault(static obj => obj is MemberDeclarationSyntax or EnumMemberDeclarationSyntax);

        return declaration == null
                   ? updatedDocument
                   : await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, declaration, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0449XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0449Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0449XmlDocumentationElementTextMustNotEndWithPeriodCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}