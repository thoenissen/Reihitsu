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
    /// Determine whether swapping the operands preserves the semantics of the comparison. The swap is only safe for
    /// built-in operators or when the swapped expression rebinds to the very same user-defined operator, otherwise the
    /// output may fail to compile or silently change behavior
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

        var originalMethod = semanticModel.GetSymbolInfo(binaryExpression, cancellationToken).Symbol as IMethodSymbol;
        var swappedSymbolInfo = semanticModel.GetSpeculativeSymbolInfo(binaryExpression.SpanStart, BuildSwappedExpression(binaryExpression), SpeculativeBindingOption.BindAsExpression);

        if (swappedSymbolInfo.Symbol is not IMethodSymbol swappedMethod)
        {
            // The swapped expression does not bind to an operator; this is only safe when both sides use a built-in operator
            return originalMethod == null
                   && swappedSymbolInfo.CandidateSymbols.IsEmpty;
        }

        if (originalMethod is null or { MethodKind: MethodKind.BuiltinOperator })
        {
            // Built-in comparison operators always have a valid mirrored operator
            return swappedMethod.MethodKind == MethodKind.BuiltinOperator;
        }

        // A user-defined operator is only safe when the swapped expression rebinds to the identical operator, which
        // requires symmetric operand types (for example the equality operators of a single type)
        return swappedMethod.MethodKind == MethodKind.UserDefinedOperator
               && SymbolEqualityComparer.Default.Equals(originalMethod.OriginalDefinition, swappedMethod.OriginalDefinition);
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
                    && SyntaxNodeUtilities.HasCommentsOrDirectives(binaryExpression) == false
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