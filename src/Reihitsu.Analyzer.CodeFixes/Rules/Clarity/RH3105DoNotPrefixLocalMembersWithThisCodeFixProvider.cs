using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3105DoNotPrefixLocalMembersWithThisAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3105DoNotPrefixLocalMembersWithThisCodeFixProvider))]
public class RH3105DoNotPrefixLocalMembersWithThisCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="memberAccessExpression">Member access expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
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
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3105DoNotPrefixLocalMembersWithThisAnalyzer.DiagnosticId];

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

                // A comment or a directive around the removed dot would be deleted, and the member name alone can
                // read as part of a generic name or as the type of a cast
                if (memberAccessExpression != null
                    && SyntaxNodeUtilities.SpanContainsCommentOrDirective(memberAccessExpression, TextSpan.FromBounds(memberAccessExpression.Expression.Span.End, memberAccessExpression.Name.SpanStart)) == false
                    && RewriteParseUtilities.ParsesAsWritten(root, root.ReplaceNode(memberAccessExpression, memberAccessExpression.Name.WithTriviaFrom(memberAccessExpression)), context.CancellationToken))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3105Title,
                                                              token => ApplyCodeFixAsync(context.Document, memberAccessExpression, token),
                                                              nameof(RH3105DoNotPrefixLocalMembersWithThisCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}