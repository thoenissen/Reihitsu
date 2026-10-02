using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider))]
public class RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="localFunctionStatement">Local function statement</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, LocalFunctionStatementSyntax localFunctionStatement, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, localFunctionStatement, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedAnalyzer.DiagnosticId];

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
                var localFunctionStatement = root.FindNode(diagnostic.Location.SourceSpan)
                                                 .FirstAncestorOrSelf<LocalFunctionStatementSyntax>();

                if (localFunctionStatement != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3208Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, localFunctionStatement, cancellationToken),
                                                              nameof(RH3208ExpressionStyleLocalFunctionsShouldNotBeUsedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}