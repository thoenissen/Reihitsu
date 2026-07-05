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
/// Providing fixes for <see cref="RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH5308ConditionalExpressionsShouldBeFormattedCorrectlyCodeFixProvider : CodeFixProvider
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

        // Delegate to the formatter using the enclosing statement (or member) as the target: those
        // start on their own line, so the formatter's indentation anchor stays consistent and the
        // whole ternary chain, including nested conditionals, is laid out just like a full format run.
        var target = conditional.FirstAncestorOrSelf<StatementSyntax>()
                         ?? (SyntaxNode)conditional.FirstAncestorOrSelf<MemberDeclarationSyntax>();

        if (target == null)
        {
            return document;
        }

        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, cancellationToken).ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

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
                var operatorToken = root.FindToken(diagnostic.Location.SourceSpan.Start);

                if (operatorToken.Parent is ConditionalExpressionSyntax)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5308Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, operatorToken, cancellationToken),
                                                              nameof(RH5308ConditionalExpressionsShouldBeFormattedCorrectlyCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}