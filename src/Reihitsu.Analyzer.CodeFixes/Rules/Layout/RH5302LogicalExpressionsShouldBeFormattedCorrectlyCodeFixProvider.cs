using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider : ReihitsuCodeFixProviderBase
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="formattingNode">The outermost logical expression of the chain to reformat</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, BinaryExpressionSyntax formattingNode, CancellationToken cancellationToken)
    {
        return (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, formattingNode, cancellationToken)
                                       .ConfigureAwait(false)).Output;
    }

    /// <summary>
    /// Walks up through enclosing logical <c>&amp;&amp;</c>/<c>||</c> expressions to find the outermost expression of
    /// the chain, so a single fix reformats every operator in the chain instead of only the one that was reported
    /// </summary>
    /// <param name="binaryExpression">A binary expression within the chain</param>
    /// <returns>The outermost logical expression of the chain</returns>
    private static BinaryExpressionSyntax GetOutermostLogicalExpression(BinaryExpressionSyntax binaryExpression)
    {
        while (binaryExpression.Parent is BinaryExpressionSyntax parentBinary
               && (parentBinary.IsKind(SyntaxKind.LogicalAndExpression) || parentBinary.IsKind(SyntaxKind.LogicalOrExpression)))
        {
            binaryExpression = parentBinary;
        }

        return binaryExpression;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

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
                var operatorToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

                if (operatorToken.Parent is not BinaryExpressionSyntax binaryExpression)
                {
                    continue;
                }

                var formattingNode = GetOutermostLogicalExpression(binaryExpression);

                if (ReihitsuFormatter.FindUnsupportedSyntax(formattingNode) != null)
                {
                    continue;
                }

                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5302Title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, formattingNode, cancellationToken),
                                                          nameof(RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}