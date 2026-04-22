using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH0012DoNotPrefixLocalMembersWithThisAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0012DoNotPrefixLocalMembersWithThisCodeFixProvider))]
public class RH0012DoNotPrefixLocalMembersWithThisCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="memberAccessExpression">Member access expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, MemberAccessExpressionSyntax memberAccessExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var updatedRoot = root.ReplaceNode(memberAccessExpression, memberAccessExpression.Name.WithTriviaFrom(memberAccessExpression));

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0012DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId];

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
                var memberAccessExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<MemberAccessExpressionSyntax>().FirstOrDefault(node => node.Expression is ThisExpressionSyntax);

                if (memberAccessExpression != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0012Title,
                                                              token => ApplyCodeFixAsync(context.Document, memberAccessExpression, token),
                                                              nameof(RH0012DoNotPrefixLocalMembersWithThisCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}