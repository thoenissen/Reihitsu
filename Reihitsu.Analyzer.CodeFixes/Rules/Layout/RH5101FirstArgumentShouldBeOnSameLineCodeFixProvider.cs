using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5101FirstArgumentShouldBeOnSameLineAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5101FirstArgumentShouldBeOnSameLineCodeFixProvider))]
public class RH5101FirstArgumentShouldBeOnSameLineCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix by delegating the layout to the shared formatter
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="argumentList">Argument list with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ArgumentListSyntax argumentList, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, argumentList, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether a comment or a directive sits in the region the rewrite actually writes. That region runs
    /// from the argument list's full span start to the end of its own span, which releases the trailing side and
    /// keeps the leading one: <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> restores the last token's
    /// trailing trivia unconditionally, so a comment written behind the closing parenthesis is never crossed and
    /// must not withhold the fix, while it restores the first token's leading trivia only when that token does not
    /// start a line, so a comment written before the opening parenthesis can still be rewritten and stays guarded
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="argumentList">Argument list to inspect</param>
    /// <returns><see langword="true"/> if the rewritten region carries a comment or directive; otherwise <see langword="false"/></returns>
    private static bool CarriesCommentOrDirectiveInRewrittenRegion(SyntaxNode root, ArgumentListSyntax argumentList)
    {
        return SyntaxNodeUtilities.SpanContainsCommentOrDirective(root, TextSpan.FromBounds(argumentList.FullSpan.Start, argumentList.Span.End));
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5101FirstArgumentShouldBeOnSameLineAnalyzer.DiagnosticId];

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
                var node = root.FindNode(diagnostic.Location.SourceSpan);
                var argumentList = node.FirstAncestorOrSelf<ArgumentListSyntax>();

                if (argumentList != null && CarriesCommentOrDirectiveInRewrittenRegion(root, argumentList) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5101Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, argumentList, cancellationToken),
                                                              nameof(RH5101FirstArgumentShouldBeOnSameLineCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}