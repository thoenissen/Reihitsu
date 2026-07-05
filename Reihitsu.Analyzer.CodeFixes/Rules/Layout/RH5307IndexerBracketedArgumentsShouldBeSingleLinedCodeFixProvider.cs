using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider))]
public class RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider : CodeFixProvider
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
        return FormattingSafetyUtilities.HasCommentsOrDirectives(bracketedArgumentList) == false
               && FormattingSafetyUtilities.AreAllSingleLine(bracketedArgumentList.Arguments);
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
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId];

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
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5307Title,
                                                              cancellationToken => ApplyCodeFixAsync(context.Document, formatTarget, cancellationToken),
                                                              nameof(RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}