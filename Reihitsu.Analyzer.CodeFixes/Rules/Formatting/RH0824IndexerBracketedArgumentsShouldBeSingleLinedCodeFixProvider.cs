using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Providing fixes for <see cref="RH0824IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0824IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider))]
public class RH0824IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider : CodeFixProvider
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
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, formatTarget, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether the list belongs to indexer/element access syntax
    /// </summary>
    /// <param name="bracketedArgumentList">Bracketed argument list</param>
    /// <returns><see langword="true"/> when the parent is indexer/element access syntax; otherwise, <see langword="false"/></returns>
    private static bool IsIndexerArgumentList(BracketedArgumentListSyntax bracketedArgumentList)
    {
        return bracketedArgumentList.Parent is ElementAccessExpressionSyntax or ImplicitElementAccessSyntax;
    }

    /// <summary>
    /// Determines whether a bracketed argument list can be safely collapsed to one line
    /// </summary>
    /// <param name="bracketedArgumentList">Bracketed argument list</param>
    /// <returns><see langword="true"/> if collapsing is safe; otherwise, <see langword="false"/></returns>
    private static bool CanSafelyCollapseToSingleLine(BracketedArgumentListSyntax bracketedArgumentList)
    {
        foreach (var trivia in bracketedArgumentList.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsDirective || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                return false;
            }
        }

        foreach (var argument in bracketedArgumentList.Arguments)
        {
            var lineSpan = argument.GetLocation().GetLineSpan();

            if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Try to get the formatting scope that should be formatted
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>The formatting scope</returns>
    private static SyntaxNode TryGetFormatTarget(SyntaxNode root, Diagnostic diagnostic)
    {
        var tokenParent = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
        var bracketedArgumentList = tokenParent?.AncestorsAndSelf()
                                               .OfType<BracketedArgumentListSyntax>()
                                               .FirstOrDefault();

        if (bracketedArgumentList == null)
        {
            bracketedArgumentList = tokenParent?.AncestorsAndSelf()
                                               .OfType<ElementAccessExpressionSyntax>()
                                               .Select(node => node.ArgumentList)
                                               .FirstOrDefault();
        }

        if (bracketedArgumentList == null
            || IsIndexerArgumentList(bracketedArgumentList) == false
            || CanSafelyCollapseToSingleLine(bracketedArgumentList) == false)
        {
            return null;
        }

        return bracketedArgumentList.AncestorsAndSelf()
                                    .FirstOrDefault(node => node is StatementSyntax
                                                                 or EqualsValueClauseSyntax
                                                                 or ArrowExpressionClauseSyntax
                                                                 or ArgumentSyntax)
                   ?? bracketedArgumentList;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0824IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId];

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
                var formatTarget = TryGetFormatTarget(root, diagnostic);

                if (formatTarget != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0824Title,
                                                              c => ApplyCodeFixAsync(context.Document, formatTarget, c),
                                                              nameof(RH0824IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}