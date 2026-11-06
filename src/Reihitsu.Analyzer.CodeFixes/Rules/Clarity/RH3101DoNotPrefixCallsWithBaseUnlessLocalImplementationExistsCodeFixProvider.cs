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

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider))]
public class RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Builds the replacement: <c>X</c> for <c>base.X</c>, and <c>this[…]</c> for <c>base[…]</c>
    /// </summary>
    /// <param name="node">Member access or element access with <c>base</c> as its receiver</param>
    /// <returns>The replacement expression</returns>
    private static ExpressionSyntax BuildReplacement(ExpressionSyntax node)
    {
        return node switch
               {
                   MemberAccessExpressionSyntax memberAccessExpression => memberAccessExpression.Name.WithTriviaFrom(memberAccessExpression),
                   ElementAccessExpressionSyntax elementAccessExpression => elementAccessExpression.WithExpression(SyntaxFactory.ThisExpression().WithTriviaFrom(elementAccessExpression.Expression)),
                   _ => node
               };
    }

    /// <summary>
    /// Determines whether the rewrite can be offered: no comment or directive stands between <c>base</c> and the member
    /// name, which the rewrite of a member access would delete; the access does not bind to a member that a type derived
    /// from the containing type can override, because without <c>base</c> the call dispatches virtually; and the code the
    /// rewrite writes parses as written, not as a generic name or a cast
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="node">Member access or element access with <c>base</c> as its receiver</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the fix can be offered; otherwise, <see langword="false"/></returns>
    private static bool CanRewrite(SyntaxNode root, SemanticModel semanticModel, ExpressionSyntax node, CancellationToken cancellationToken)
    {
        if (node is MemberAccessExpressionSyntax memberAccessExpression
            && SyntaxNodeUtilities.SpanContainsCommentOrDirective(memberAccessExpression, TextSpan.FromBounds(memberAccessExpression.Expression.Span.End, memberAccessExpression.Name.SpanStart)))
        {
            return false;
        }

        if (BindsToOverridableMember(semanticModel, node, cancellationToken))
        {
            return false;
        }

        return RewriteParseUtilities.ParsesAsWritten(root, root.ReplaceNode(node, BuildReplacement(node)), cancellationToken);
    }

    /// <summary>
    /// Determines whether the access binds to a virtual member — declared <see langword="virtual"/>, or an
    /// <see langword="override"/> that is not <see langword="sealed"/> — while the type that contains the access is not
    /// sealed. A type derived from the containing type can then override the member, and the call without
    /// <c>base</c> would reach that override instead of the base implementation
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="node">Member access or element access with <c>base</c> as its receiver</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if removing <c>base</c> could change which implementation runs</returns>
    private static bool BindsToOverridableMember(SemanticModel semanticModel, ExpressionSyntax node, CancellationToken cancellationToken)
    {
        if (semanticModel.GetEnclosingSymbol(node.SpanStart, cancellationToken)?.ContainingType is not { IsSealed: false })
        {
            return false;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(node, cancellationToken);
        var symbols = symbolInfo.Symbol != null
                          ? ImmutableArray.Create(symbolInfo.Symbol)
                          : symbolInfo.CandidateSymbols;

        return symbols.Any(symbol => symbol.IsVirtual
                                     || symbol.IsAbstract
                                     || (symbol.IsOverride && symbol.IsSealed == false));
    }

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, ExpressionSyntax node, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        return document.WithSyntaxRoot(root.ReplaceNode(node, BuildReplacement(node)));
    }

    /// <summary>
    /// Try to get the node from the diagnostic
    /// </summary>
    /// <param name="root">Root</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <returns>Matching node</returns>
    private static ExpressionSyntax TryGetNode(SyntaxNode root, Diagnostic diagnostic)
    {
        return root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<ExpressionSyntax>().FirstOrDefault(node => node is MemberAccessExpressionSyntax { Expression: BaseExpressionSyntax }
                                                                                                                                                      or ElementAccessExpressionSyntax { Expression: BaseExpressionSyntax });
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null
            && semanticModel != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var node = TryGetNode(root, diagnostic);

                if (node != null
                    && CanRewrite(root, semanticModel, node, context.CancellationToken))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3101Title,
                                                              token => ApplyCodeFixAsync(context.Document, node, token),
                                                              nameof(RH3101DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}