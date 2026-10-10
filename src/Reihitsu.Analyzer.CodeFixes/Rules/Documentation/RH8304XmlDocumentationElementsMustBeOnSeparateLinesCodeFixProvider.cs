using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Documentation;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Documentation;

/// <summary>
/// Code fix provider for <see cref="RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH8304XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider))]
public class RH8304XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        return DocumentationCommentCodeFixUtilities.MoveContentToNextDocumentationLineAsync(document, diagnosticSpan, cancellationToken);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH8304XmlDocumentationElementsMustBeOnSeparateLinesAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH8304Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH8304XmlDocumentationElementsMustBeOnSeparateLinesCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}