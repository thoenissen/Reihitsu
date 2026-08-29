using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Providing fixes for <see cref="RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider))]
public class RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="formattingNode">The outermost logical expression of the chain to reformat</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, BinaryExpressionSyntax formattingNode, CancellationToken cancellationToken)
    {
        return await ReihitsuFormatter.FormatNodeInDocumentAsync(document, formattingNode, cancellationToken)
                                      .ConfigureAwait(false);
    }

    /// <summary>
    /// Determines whether the operator token trails at the end of its left operand's last line, the shape that
    /// requires moving a line break rather than only realigning the operator's own indentation
    /// </summary>
    /// <param name="binaryExpression">The binary expression whose operator is being checked</param>
    /// <returns><see langword="true"/> if the operator immediately trails the left operand; otherwise, <see langword="false"/></returns>
    private static bool IsTrailingOperator(BinaryExpressionSyntax binaryExpression)
    {
        var leftLineSpan = binaryExpression.Left.SyntaxTree.GetLineSpan(binaryExpression.Left.Span);
        var operatorLineSpan = binaryExpression.OperatorToken.SyntaxTree.GetLineSpan(binaryExpression.OperatorToken.Span);

        return operatorLineSpan.StartLinePosition.Line == leftLineSpan.EndLinePosition.Line;
    }

    /// <summary>
    /// Walks up through enclosing logical <c>&amp;&amp;</c>/<c>||</c> expressions to find the outermost expression of
    /// the chain, so a single fix reformats every operator in the chain instead of only the one that was reported
    /// </summary>
    /// <param name="binaryExpression">A binary expression within the chain</param>
    /// <returns>The outermost logical expression of the chain</returns>
    private static BinaryExpressionSyntax GetOutermostLogicalExpression(BinaryExpressionSyntax binaryExpression)
    {
        while (binaryExpression.Parent is BinaryExpressionSyntax parentBinary
               && (parentBinary.IsKind(SyntaxKind.LogicalAndExpression) || parentBinary.IsKind(SyntaxKind.LogicalOrExpression)))
        {
            binaryExpression = parentBinary;
        }

        return binaryExpression;
    }

    /// <summary>
    /// Determines whether any comment inside <paramref name="formattingNode"/>'s span occupies its own line —
    /// nothing but whitespace precedes it on that line, regardless of which token owns the comment as trivia.
    /// Reformatting the chain would carry the general-purpose formatting pipeline's own blank-line placement
    /// around that comment along with the operator move, a side effect outside what this diagnostic reports, so
    /// the fix withholds itself rather than risk relocating a user-authored comment. The check inspects the
    /// whole span rather than only the operator tokens' own leading trivia, because an own-line comment above an
    /// operand — not attached to any operator — triggers the same pipeline behavior
    /// </summary>
    /// <param name="formattingNode">The outermost logical expression of the chain</param>
    /// <returns><see langword="true"/> if an own-line comment sits anywhere within the chain's span; otherwise, <see langword="false"/></returns>
    private static bool ContainsOwnLineComment(BinaryExpressionSyntax formattingNode)
    {
        var text = formattingNode.SyntaxTree.GetText();

        foreach (var trivia in formattingNode.DescendantTrivia())
        {
            if (SyntaxTriviaUtilities.IsCommentTrivia(trivia) == false)
            {
                continue;
            }

            var line = text.Lines.GetLineFromPosition(trivia.SpanStart);
            var beforeComment = text.ToString(TextSpan.FromBounds(line.Start, trivia.SpanStart));

            if (string.IsNullOrWhiteSpace(beforeComment))
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH5302LogicalExpressionsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId];

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

                if (operatorToken.Parent is not BinaryExpressionSyntax binaryExpression)
                {
                    continue;
                }

                if (IsTrailingOperator(binaryExpression)
                    && SyntaxTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(operatorToken, binaryExpression.Right.GetFirstToken()))
                {
                    continue;
                }

                var formattingNode = GetOutermostLogicalExpression(binaryExpression);

                if (ContainsOwnLineComment(formattingNode))
                {
                    continue;
                }

                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH5302Title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, formattingNode, cancellationToken),
                                                          nameof(RH5302LogicalExpressionsShouldBeFormattedCorrectlyCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}