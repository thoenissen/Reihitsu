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
/// Providing fixes for <see cref="RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH5308ConditionalExpressionsShouldBeFormattedCorrectlyCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="operatorToken">Operator token with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxToken operatorToken, CancellationToken cancellationToken)
    {
        if (operatorToken.Parent is not ConditionalExpressionSyntax conditional)
        {
            return document;
        }

        return (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, GetFormatTarget(conditional), cancellationToken).ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Gets the node the fix formats: the conditional expression, or the outermost conditional it is directly nested in as a
    /// condition or a branch, so that a nested conditional is laid out with the conditionals around it, as one stair. Its
    /// first token keeps its line and column, so the code around it stays as written (RH5308-004, SHARED-FIX-006)
    /// </summary>
    /// <param name="conditional">Conditional expression</param>
    /// <returns>The node to format</returns>
    private static ConditionalExpressionSyntax GetFormatTarget(ConditionalExpressionSyntax conditional)
    {
        while (conditional.Parent is ConditionalExpressionSyntax parent)
        {
            conditional = parent;
        }

        return conditional;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        // The fix changes only the reported construct and lays it out from the column its first token has, so two constructs
        // on one line are fixed one after another, each from the column the fix in front of it left (SHARED-FIX-002)
        return DocumentOrderFixAllProvider.Create(new RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer());
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
                var operatorToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

                if (operatorToken.Parent is ConditionalExpressionSyntax conditional
                    && ReihitsuFormatter.FindUnsupportedSyntax(GetFormatTarget(conditional)) == null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5308Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, operatorToken, cancellationToken),
                                                              nameof(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}