using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns the block body of lambda expressions so that the opening brace aligns
/// to the lambda's anchor token. The anchor is determined as follows:
/// <list type="bullet">
/// <item><description>Async lambda: the <c>async</c> keyword</description></item>
/// <item><description>Parenthesized lambda: the <c>(</c> token</description></item>
/// <item><description>Simple lambda: the parameter identifier</description></item>
/// </list>
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

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
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

            default:
                {
                    return;
                }
        }

        var anchorColumn = LayoutComputer.GetAdjustedColumn(anchorToken, model);

        if (model.TryGetLayout(block.OpenBraceToken, out var currentLayout) == false)
        {
            return;
        }

        var delta = anchorColumn - currentLayout.Column;

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