using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH0006UseStringEmptyForEmptyStringsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0006UseStringEmptyForEmptyStringsCodeFixProvider))]
public class RH0006UseStringEmptyForEmptyStringsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="literalExpression">Literal expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, LiteralExpressionSyntax literalExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var replacementExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)), SyntaxFactory.IdentifierName(nameof(string.Empty))).WithTriviaFrom(literalExpression);
        var updatedRoot = root.ReplaceNode(literalExpression, replacementExpression);

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0006UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId];

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
                var literalExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<LiteralExpressionSyntax>().FirstOrDefault();

                if (literalExpression != null)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0006Title,
                                                              token => ApplyCodeFixAsync(context.Document, literalExpression, token),
                                                              nameof(RH0006UseStringEmptyForEmptyStringsCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}