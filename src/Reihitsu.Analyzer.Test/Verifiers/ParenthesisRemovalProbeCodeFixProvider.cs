using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.Test.Verifiers;

/// <summary>
/// Probe code fix for <see cref="ParenthesisRemovalProbeAnalyzer"/>: it replaces the reported parenthesized expression
/// with the text of the expression inside, without checking whether the parentheses were needed, and lets the
/// document parse the result again, as an edit in the editor would
/// </summary>
public sealed class ParenthesisRemovalProbeCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Replaces the text of the parenthesized expression with the text of the expression inside
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Parenthesized expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The changed document</returns>
    private static async Task<Document> RemoveParenthesesAsync(Document document, ParenthesizedExpressionSyntax node, CancellationToken cancellationToken)
    {
        var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

        return document.WithText(text.WithChanges(new TextChange(node.Span, node.Expression.ToString())));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => [RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root?.FindNode(context.Span, getInnermostNodeForTie: true) is ParenthesizedExpressionSyntax node)
        {
            context.RegisterCodeFix(CodeAction.Create("Remove parentheses",
                                                      token => RemoveParenthesesAsync(context.Document, node, token),
                                                      nameof(ParenthesisRemovalProbeCodeFixProvider)),
                                    context.Diagnostics);
        }
    }

    #endregion // CodeFixProvider
}