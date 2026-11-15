using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5305CollectionExpressionsShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH5305CollectionExpressionsShouldBeFormattedCorrectlyCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="formatTarget">Node that should be formatted</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode formatTarget, CancellationToken cancellationToken)
    {
        return (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, formatTarget, cancellationToken).ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Gets the node the fix formats: the reported collection expression, whose first token keeps its line and column, so the
    /// code around it stays as written (RH5305-004, SHARED-FIX-006)
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>The collection expression, or <see langword="null"/> when there is none</returns>
    private static SyntaxNode TryGetFormatTarget(SyntaxNode root, Diagnostic diagnostic)
    {
        return root.FindToken(diagnostic.Location.SourceSpan.Start)
                   .Parent
                   ?.AncestorsAndSelf()
                   .OfType<CollectionExpressionSyntax>()
                   .FirstOrDefault();
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        // The fix changes only the reported construct and lays it out from the column its first token has, so two constructs
        // on one line are fixed one after another, each from the column the fix in front of it left (SHARED-FIX-002)
        return DocumentOrderFixAllProvider.Create(new RH5305CollectionExpressionsShouldBeFormattedCorrectlyAnalyzer());
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
                var formatTarget = TryGetFormatTarget(root, diagnostic);

                if (formatTarget != null
                    && ReihitsuFormatter.FindUnsupportedSyntax(formatTarget) == null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5305Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, formatTarget, cancellationToken),
                                                              nameof(RH5305CollectionExpressionsShouldBeFormattedCorrectlyCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}