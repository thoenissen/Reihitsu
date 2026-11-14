using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Rules.Clarity;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3006ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider))]
public class RH3006ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        // The parentheses carry the operand's own trivia and none else: a token without trivia leaves no elastic
        // trivia behind, which would make the code action re-format the gaps around the operand and write the host's
        // line break into them
        var openParenthesis = SyntaxFactory.Token(node.GetLeadingTrivia(), SyntaxKind.OpenParenToken, SyntaxFactory.TriviaList());
        var closeParenthesis = SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.CloseParenToken, node.GetTrailingTrivia());

        var replacementNode = node switch
                              {
                                  ExpressionSyntax expressionSyntax => SyntaxFactory.ParenthesizedExpression(openParenthesis, expressionSyntax.WithoutTrivia(), closeParenthesis),
                                  PatternSyntax patternSyntax => SyntaxFactory.ParenthesizedPattern(openParenthesis, patternSyntax.WithoutTrivia(), closeParenthesis),
                                  _ => node,
                              };

        var updatedRoot = root.ReplaceNode(node, replacementNode);

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3006ConditionalExpressionsMustDeclarePrecedenceAnalyzer.DiagnosticId];

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
                var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

                if (node is ExpressionSyntax or PatternSyntax)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3006Title,
                                                              token => ApplyCodeFixAsync(context.Document, node, token),
                                                              nameof(RH3006ConditionalExpressionsMustDeclarePrecedenceCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}