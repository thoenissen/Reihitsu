using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider))]
public class RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="chainRoot">The chain root to format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode chainRoot, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, chainRoot, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Finds the root expression of the chain containing the given token
    /// </summary>
    /// <param name="token">A token within the chain</param>
    /// <returns>The outermost chain node, or <c>null</c></returns>
    private static SyntaxNode FindChainRoot(SyntaxToken token)
    {
        var current = token.Parent;

        while (current != null)
        {
            var parent = current.Parent;

            if (parent is InvocationExpressionSyntax
                       or MemberAccessExpressionSyntax
                       or ConditionalAccessExpressionSyntax
                       or ElementAccessExpressionSyntax
                       or PostfixUnaryExpressionSyntax)
            {
                current = parent;
            }
            else
            {
                break;
            }
        }

        return current;
    }

    /// <summary>
    /// Gets the chain root for the diagnostic location
    /// </summary>
    /// <param name="root">The syntax root</param>
    /// <param name="diagnostic">The diagnostic to fix</param>
    /// <returns>The chain root, or <c>null</c></returns>
    private static SyntaxNode TryGetChainRoot(SyntaxNode root, Diagnostic diagnostic)
    {
        return FindChainRoot(root.FindToken(diagnostic.Location.SourceSpan.Start));
    }

    #endregion // Methods

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
                var chainRoot = TryGetChainRoot(root, diagnostic);

                if (chainRoot != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5112Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, chainRoot, cancellationToken),
                                                              nameof(RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}