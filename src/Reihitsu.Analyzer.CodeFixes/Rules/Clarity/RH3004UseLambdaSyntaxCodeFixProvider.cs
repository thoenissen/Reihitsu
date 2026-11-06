using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3004UseLambdaSyntaxAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3004UseLambdaSyntaxCodeFixProvider))]
public class RH3004UseLambdaSyntaxCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Determines whether the anonymous method carries trivia that the rewrite would discard. The replacement is
    /// reassembled from the text of the parameter list and of the block, so only those two spans survive. Trivia
    /// between the leading keywords and the parameter list, or between the parameter list and the block, belongs
    /// to neither of them and would be dropped, which is why the fix is not offered for those shapes
    /// </summary>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <returns><see langword="true"/> if the rewrite would discard trivia; otherwise, <see langword="false"/></returns>
    private static bool ContainsDiscardedTrivia(AnonymousMethodExpressionSyntax anonymousMethodExpression)
    {
        var parameterList = anonymousMethodExpression.ParameterList;

        if (parameterList == null
            || anonymousMethodExpression.Body is not BlockSyntax block)
        {
            return true;
        }

        return SyntaxNodeUtilities.SpanContainsCommentOrDirective(anonymousMethodExpression, TextSpan.FromBounds(anonymousMethodExpression.Span.Start, parameterList.Span.Start))
               || SyntaxNodeUtilities.SpanContainsCommentOrDirective(anonymousMethodExpression, TextSpan.FromBounds(parameterList.Span.End, block.Span.Start));
    }

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, AnonymousMethodExpressionSyntax anonymousMethodExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || anonymousMethodExpression.ParameterList == null)
        {
            return document;
        }

        var asyncPrefix = anonymousMethodExpression.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword)
                              ? "async "
                              : string.Empty;
        var bodyText = anonymousMethodExpression.Block is { Statements.Count: 1 } blockSyntax
                       && blockSyntax.Statements[0] is ReturnStatementSyntax { Expression: not null } returnStatement
                       && SyntaxNodeUtilities.ContainsCommentOrDirective(anonymousMethodExpression.Block) == false
                           ? returnStatement.Expression.ToString()
                           : anonymousMethodExpression.Block.ToString();
        var formattingAnnotation = new SyntaxAnnotation();
        var replacementExpression = SyntaxFactory.ParseExpression($"{asyncPrefix}{anonymousMethodExpression.ParameterList} => {bodyText}")
                                                 .WithTriviaFrom(anonymousMethodExpression)
                                                 .WithAdditionalAnnotations(formattingAnnotation);
        var updatedDocument = document.WithSyntaxRoot(root.ReplaceNode(anonymousMethodExpression, replacementExpression));
        var updatedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (updatedRoot?.GetAnnotatedNodes(formattingAnnotation).FirstOrDefault() is not { } lambdaExpression)
        {
            return updatedDocument;
        }

        // The lambda is laid out as the formatter lays it out: a block body on lines of its own, aligned under the
        // lambda's anchor
        var formattedDocument = await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, lambdaExpression, cancellationToken).ConfigureAwait(false);

        return await RestoreDocumentationCommentsAsync(formattedDocument, formattingAnnotation, GetDocumentationComments(anonymousMethodExpression), cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the documentation comments inside a node, in document order
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>The documentation comment trivia</returns>
    private static List<SyntaxTrivia> GetDocumentationComments(SyntaxNode node)
    {
        return node.DescendantTrivia()
                   .Where(static trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                                           || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                   .ToList();
    }

    /// <summary>
    /// Writes the documentation comments of the block back as they were written. The fix keeps the block's statements
    /// unchanged and only lays out their lines; formatting the lambda would also rewrite the text of a documentation
    /// comment that stands in the block
    /// </summary>
    /// <param name="document">Document with the formatted lambda</param>
    /// <param name="formattingAnnotation">Annotation of the lambda</param>
    /// <param name="originalDocumentationComments">Documentation comments of the anonymous method, in document order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document with the documentation comments as written</returns>
    private static async Task<Document> RestoreDocumentationCommentsAsync(Document document, SyntaxAnnotation formattingAnnotation, List<SyntaxTrivia> originalDocumentationComments, CancellationToken cancellationToken)
    {
        if (originalDocumentationComments.Count == 0)
        {
            return document;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root?.GetAnnotatedNodes(formattingAnnotation).FirstOrDefault() is not { } lambdaExpression)
        {
            return document;
        }

        var formattedDocumentationComments = GetDocumentationComments(lambdaExpression);

        if (formattedDocumentationComments.Count != originalDocumentationComments.Count)
        {
            return document;
        }

        var replacements = new Dictionary<SyntaxTrivia, SyntaxTrivia>();

        for (var commentIndex = 0; commentIndex < formattedDocumentationComments.Count; commentIndex++)
        {
            replacements[formattedDocumentationComments[commentIndex]] = originalDocumentationComments[commentIndex];
        }

        return document.WithSyntaxRoot(root.ReplaceTrivia(replacements.Keys, (originalTrivia, _) => replacements[originalTrivia]));
    }

    /// <summary>
    /// Determines whether the anonymous method is directly the operand of a cast or an operand of a binary operator.
    /// A lambda is not a valid operand there without parentheses: in front of an operator, its body takes in the rest
    /// of the expression, and behind a cast or an operator, it does not parse as the operand
    /// </summary>
    /// <param name="anonymousMethodExpression">Anonymous method expression</param>
    /// <returns><see langword="true"/> if the anonymous method is the operand of a cast or a binary operator</returns>
    private static bool IsOperandOfCastOrBinaryOperator(AnonymousMethodExpressionSyntax anonymousMethodExpression)
    {
        return anonymousMethodExpression.Parent is CastExpressionSyntax or BinaryExpressionSyntax;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3004UseLambdaSyntaxAnalyzer.DiagnosticId];

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
                var anonymousMethodExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<AnonymousMethodExpressionSyntax>().FirstOrDefault();

                if (anonymousMethodExpression?.ParameterList != null
                    && ContainsDiscardedTrivia(anonymousMethodExpression) == false
                    && IsOperandOfCastOrBinaryOperator(anonymousMethodExpression) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3004Title,
                                                              token => ApplyCodeFixAsync(context.Document, anonymousMethodExpression, token),
                                                              nameof(RH3004UseLambdaSyntaxCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}