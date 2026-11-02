using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns the block body of lambda expressions so that the opening brace aligns
/// to the lambda's anchor token. The anchor is determined as follows:
/// <list type="bullet">
/// <item><description>Async lambda: the <c>async</c> keyword</description></item>
/// <item><description>Parenthesized lambda: the <c>(</c> token</description></item>
/// <item><description>Simple lambda: the parameter identifier</description></item>
/// </list>
/// The block of an anonymous method is aligned the same way, to the <c>async</c> keyword of an async anonymous method and
/// otherwise to the <c>delegate</c> keyword, but only where the scope depth cannot place it: in an interpolation hole of
/// a multi-line interpolated raw string, outside the block body of a lambda or anonymous method in that hole (FMT-INDENT-031)
/// </summary>
internal sealed class LambdaAlignmentContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Gets the anchor token for a simple lambda expression.
    /// For async lambdas, the anchor is the <c>async</c> keyword;
    /// otherwise, it is the parameter identifier
    /// </summary>
    /// <param name="lambda">The simple lambda expression</param>
    /// <returns>The anchor token</returns>
    private static SyntaxToken GetAnchorToken(SimpleLambdaExpressionSyntax lambda)
    {
        if (lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
        {
            return lambda.AsyncKeyword;
        }

        return lambda.Parameter.Identifier;
    }

    /// <summary>
    /// Gets the anchor token for a parenthesized lambda expression.
    /// For async lambdas, the anchor is the <c>async</c> keyword;
    /// otherwise, it is the open parenthesis of the parameter list
    /// </summary>
    /// <param name="lambda">The parenthesized lambda expression</param>
    /// <returns>The anchor token</returns>
    private static SyntaxToken GetAnchorToken(ParenthesizedLambdaExpressionSyntax lambda)
    {
        if (lambda.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
        {
            return lambda.AsyncKeyword;
        }

        return lambda.ParameterList.OpenParenToken;
    }

    /// <summary>
    /// Gets the anchor token for an anonymous method expression.
    /// For async anonymous methods, the anchor is the <c>async</c> keyword;
    /// otherwise, it is the <c>delegate</c> keyword, also behind a <c>static</c> modifier
    /// </summary>
    /// <param name="anonymousMethod">The anonymous method expression</param>
    /// <returns>The anchor token</returns>
    private static SyntaxToken GetAnchorToken(AnonymousMethodExpressionSyntax anonymousMethod)
    {
        if (anonymousMethod.AsyncKeyword.IsKind(SyntaxKind.AsyncKeyword))
        {
            return anonymousMethod.AsyncKeyword;
        }

        return anonymousMethod.DelegateKeyword;
    }

    /// <summary>
    /// Determines whether the block of an anonymous method is aligned to its anchor: the innermost interpolation hole around
    /// the anonymous method belongs to a multi-line interpolated raw string, and no block lies between the hole and the
    /// anonymous method. Outside such a hole the block takes its column from the scope depth, and inside the block body of a
    /// lambda or another anonymous method in the same hole it takes the scope depth relative to that body, which already
    /// moves with the literal
    /// </summary>
    /// <param name="anonymousMethod">The anonymous method expression</param>
    /// <returns><see langword="true"/> if the block is aligned to the anchor; otherwise, <see langword="false"/></returns>
    private static bool IsAlignedInRawStringHole(AnonymousMethodExpressionSyntax anonymousMethod)
    {
        foreach (var ancestor in anonymousMethod.Ancestors())
        {
            switch (ancestor)
            {
                case BlockSyntax:
                    {
                        return false;
                    }

                case InterpolationSyntax hole:
                    {
                        return hole.Parent != null
                               && RawStringLineShifts.IsMultiLineRawStringLiteral(hole.Parent);
                    }
            }
        }

        return false;
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, LayoutModel model, FormattingContext context)
    {
        BlockSyntax block;
        SyntaxToken anchorToken;

        switch (node)
        {
            case SimpleLambdaExpressionSyntax simpleLambda when simpleLambda.Block != null:
                {
                    block = simpleLambda.Block;
                    anchorToken = GetAnchorToken(simpleLambda);
                }
                break;

            case ParenthesizedLambdaExpressionSyntax parenLambda when parenLambda.Block != null:
                {
                    block = parenLambda.Block;
                    anchorToken = GetAnchorToken(parenLambda);
                }
                break;

            case AnonymousMethodExpressionSyntax anonymousMethod when IsAlignedInRawStringHole(anonymousMethod):
                {
                    block = anonymousMethod.Block;
                    anchorToken = GetAnchorToken(anonymousMethod);
                }
                break;

            default:
                {
                    return;
                }
        }

        var anchorColumn = LayoutComputer.GetAdjustedColumn(anchorToken, model);

        if (model.TryGetLayoutColumn(LayoutComputer.GetLine(block.OpenBraceToken), out var currentColumn) == false)
        {
            return;
        }

        var delta = anchorColumn - currentColumn;

        if (delta == 0)
        {
            return;
        }

        var startLine = LayoutComputer.GetLine(block.OpenBraceToken);
        var endLine = LayoutComputer.GetLine(block.CloseBraceToken);

        model.ShiftRange(startLine, endLine, delta, "LambdaBlockBody");
    }

    #endregion // ILayoutContributor
}