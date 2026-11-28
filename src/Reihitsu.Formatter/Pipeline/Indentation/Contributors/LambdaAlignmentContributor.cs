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
/// The block of an anonymous method is aligned the same way wherever the anonymous method stands, to the <c>async</c>
/// keyword of an async anonymous method and otherwise to the <c>delegate</c> keyword (FMT-INDENT-031)
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

            case AnonymousMethodExpressionSyntax anonymousMethod:
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

        // A block whose opening brace does not start its line, such as one that stays on the line of the lambda in an
        // interpolation that cannot hold a line break, has no line to move to the anchor
        if (LayoutComputer.IsFirstOnLine(block.OpenBraceToken) == false)
        {
            return;
        }

        var anchorColumn = LayoutComputer.GetAdjustedColumn(anchorToken, model);

        if (model.TryGetLayoutColumn(LayoutComputer.GetLine(block.OpenBraceToken), out var currentColumn) == false)
        {
            return;
        }

        // The block is placed also when it already stands under its anchor (a delta of 0), so its lines in an
        // interpolation hole of a multi-line raw string keep the layout the shift confirms
        var delta = anchorColumn - currentColumn;
        var startLine = LayoutComputer.GetLine(block.OpenBraceToken);
        var endLine = LayoutComputer.GetLine(block.CloseBraceToken);

        model.ShiftRange(startLine, endLine, delta, "LambdaBlockBody");
    }

    #endregion // ILayoutContributor
}