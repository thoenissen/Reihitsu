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
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Clarity;

/// <summary>
/// Code fix provider for <see cref="RH3005UseReadableConditionsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH3005UseReadableConditionsCodeFixProvider))]
public class RH3005UseReadableConditionsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Get the replacement operator token kind
    /// </summary>
    /// <param name="kind">Binary expression kind</param>
    /// <returns>Operator token kind</returns>
    private static SyntaxKind GetReplacementOperatorTokenKind(SyntaxKind kind)
    {
        return kind switch
               {
                   SyntaxKind.LessThanExpression => SyntaxKind.GreaterThanToken,
                   SyntaxKind.GreaterThanExpression => SyntaxKind.LessThanToken,
                   SyntaxKind.LessThanOrEqualExpression => SyntaxKind.GreaterThanEqualsToken,
                   SyntaxKind.GreaterThanOrEqualExpression => SyntaxKind.LessThanEqualsToken,
                   SyntaxKind.EqualsExpression => SyntaxKind.EqualsEqualsToken,
                   SyntaxKind.NotEqualsExpression => SyntaxKind.ExclamationEqualsToken,
                   _ => SyntaxKind.None
               };
    }

    /// <summary>
    /// Get the replacement operator text
    /// </summary>
    /// <param name="kind">Binary expression kind</param>
    /// <returns>Operator text</returns>
    private static string GetReplacementOperatorText(SyntaxKind kind)
    {
        return GetReplacementOperatorTokenKind(kind) switch
               {
                   SyntaxKind.GreaterThanToken => ">",
                   SyntaxKind.LessThanToken => "<",
                   SyntaxKind.GreaterThanEqualsToken => ">=",
                   SyntaxKind.LessThanEqualsToken => "<=",
                   SyntaxKind.EqualsEqualsToken => "==",
                   SyntaxKind.ExclamationEqualsToken => "!=",
                   _ => string.Empty
               };
    }

    /// <summary>
    /// Build the comparison expression with the operands swapped that the fix would produce
    /// </summary>
    /// <param name="binaryExpression">Binary expression</param>
    /// <returns>The swapped comparison expression</returns>
    private static ExpressionSyntax BuildSwappedExpression(BinaryExpressionSyntax binaryExpression)
    {
        return SyntaxFactory.ParseExpression($"{binaryExpression.Right.WithoutTrivia()} {GetReplacementOperatorText(binaryExpression.Kind())} {binaryExpression.Left.WithoutTrivia()}");
    }

    /// <summary>
    /// Determine whether swapping the operands preserves the semantics of the comparison. Only built-in comparison
    /// operators are offered, because they are guaranteed to have a commutative mirrored operator. User-defined
    /// operators are excluded: the swapped expression may fail to compile, bind to a different operator, or bind to a
    /// non-commutative operator that silently changes behavior
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="binaryExpression">Binary expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the swap is semantics-preserving</returns>
    private static bool IsSwapSemanticsPreserving(SemanticModel semanticModel, BinaryExpressionSyntax binaryExpression, CancellationToken cancellationToken)
    {
        // Dynamic operands rebind at runtime, so a matching mirrored operator is not guaranteed to exist
        if (IsDynamic(semanticModel, binaryExpression.Left, cancellationToken)
            || IsDynamic(semanticModel, binaryExpression.Right, cancellationToken))
        {
            return false;
        }

        // Both the original comparison and the swapped comparison must resolve to a built-in operator
        return IsBuiltInComparison(semanticModel.GetSymbolInfo(binaryExpression, cancellationToken))
               && IsBuiltInComparison(semanticModel.GetSpeculativeSymbolInfo(binaryExpression.SpanStart, BuildSwappedExpression(binaryExpression), SpeculativeBindingOption.BindAsExpression));
    }

    /// <summary>
    /// Determine whether the symbol info represents a successfully bound built-in comparison operator
    /// </summary>
    /// <param name="symbolInfo">Symbol info</param>
    /// <returns><see langword="true"/> if the symbol info represents a built-in comparison operator</returns>
    private static bool IsBuiltInComparison(SymbolInfo symbolInfo)
    {
        if (symbolInfo.Symbol is IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.BuiltinOperator;
        }

        // Some Roslyn versions surface a successfully bound built-in operator as a null symbol; distinguish that from
        // a binding failure by the absence of candidate symbols and a resolved candidate reason
        return symbolInfo.CandidateReason == CandidateReason.None
               && symbolInfo.CandidateSymbols.IsEmpty;
    }

    /// <summary>
    /// Determine whether the expression is of the dynamic type
    /// </summary>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="expression">Expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the expression is dynamic</returns>
    private static bool IsDynamic(SemanticModel semanticModel, ExpressionSyntax expression, CancellationToken cancellationToken)
    {
        return semanticModel.GetTypeInfo(expression, cancellationToken).Type?.TypeKind == TypeKind.Dynamic;
    }

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="binaryExpression">Binary expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, BinaryExpressionSyntax binaryExpression, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var replacementExpression = BuildSwappedExpression(binaryExpression).WithTriviaFrom(binaryExpression);
        var updatedRoot = root.ReplaceNode(binaryExpression, replacementExpression);

        return document.WithSyntaxRoot(updatedRoot);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH3005UseReadableConditionsAnalyzer.DiagnosticId];

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
                var binaryExpression = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent?.AncestorsAndSelf().OfType<BinaryExpressionSyntax>().FirstOrDefault();

                if (binaryExpression != null
                    && SyntaxNodeUtilities.ContainsCommentOrDirective(binaryExpression) == false
                    && IsSwapSemanticsPreserving(semanticModel, binaryExpression, context.CancellationToken))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH3005Title,
                                                              token => ApplyCodeFixAsync(context.Document, binaryExpression, token),
                                                              nameof(RH3005UseReadableConditionsCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}