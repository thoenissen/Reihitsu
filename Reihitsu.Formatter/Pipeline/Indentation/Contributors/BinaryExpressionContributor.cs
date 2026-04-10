using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Aligns binary operators on continuation lines to the left operand's first token column.
/// Applies to all binary operators including <c>&amp;&amp;</c>, <c>||</c>, <c>??</c>,
/// arithmetic, bitwise, equality, and pattern <c>or</c>/<c>and</c> operators.
/// </summary>
internal sealed class BinaryExpressionContributor : ILayoutContributor
{
    #region Methods

    /// <summary>
    /// Processes a binary expression chain, aligning operators to the leftmost operand's column.
    /// Only processes the outermost binary expression in a chain of the same kind.
    /// </summary>
    /// <param name="binary">The binary expression.</param>
    /// <param name="model">The layout model.</param>
    private static void ProcessBinaryExpression(BinaryExpressionSyntax binary, LayoutModel model)
    {
        if (binary.Parent is BinaryExpressionSyntax parentBinary
            && parentBinary.Kind() == binary.Kind())
        {
            return;
        }

        var alignColumn = LayoutComputer.GetAdjustedColumn(binary.Left.GetFirstToken(), model);

        AlignBinaryChain(binary, alignColumn, model);
    }

    /// <summary>
    /// Recursively aligns operators in a binary expression chain.
    /// </summary>
    /// <param name="binary">The binary expression.</param>
    /// <param name="alignColumn">The column to align operators to.</param>
    /// <param name="model">The layout model.</param>
    private static void AlignBinaryChain(BinaryExpressionSyntax binary, int alignColumn, LayoutModel model)
    {
        if (binary.Left is BinaryExpressionSyntax leftBinary
            && leftBinary.Kind() == binary.Kind())
        {
            AlignBinaryChain(leftBinary, alignColumn, model);
        }

        LayoutComputer.SetIfFirstOnLine(binary.OperatorToken, alignColumn, "BinaryExpression", model);

        if (binary.Right is BinaryExpressionSyntax rightBinary
            && rightBinary.Kind() == binary.Kind())
        {
            AlignBinaryChain(rightBinary, alignColumn, model);
        }
    }

    /// <summary>
    /// Processes a binary pattern chain (or/and), aligning operators to the anchor column.
    /// When the pattern is inside an <c>is</c> expression, operators align to the <c>is</c> keyword column.
    /// Otherwise, they align to the leftmost pattern's first token column.
    /// Only processes the outermost binary pattern in a chain of the same kind.
    /// </summary>
    /// <param name="pattern">The binary pattern.</param>
    /// <param name="model">The layout model.</param>
    private static void ProcessBinaryPattern(BinaryPatternSyntax pattern, LayoutModel model)
    {
        if (pattern.Parent is BinaryPatternSyntax parentPattern
            && parentPattern.Kind() == pattern.Kind())
        {
            return;
        }

        var alignColumn = LayoutComputer.GetAdjustedColumn(pattern.Left.GetFirstToken(), model);

        AlignPatternChain(pattern, alignColumn, model);
    }

    /// <summary>
    /// Processes an <c>is</c> pattern expression, aligning the <c>is</c> keyword to the
    /// expression's first token column when it appears on a continuation line.
    /// </summary>
    /// <param name="isPattern">The <c>is</c> pattern expression.</param>
    /// <param name="model">The layout model.</param>
    private static void ProcessIsPattern(IsPatternExpressionSyntax isPattern, LayoutModel model)
    {
        var alignColumn = LayoutComputer.GetAdjustedColumn(isPattern.Expression.GetFirstToken(), model);

        LayoutComputer.SetIfFirstOnLine(isPattern.IsKeyword, alignColumn, "IsPattern", model);
    }

    /// <summary>
    /// Recursively aligns operators in a binary pattern chain.
    /// </summary>
    /// <param name="pattern">The binary pattern.</param>
    /// <param name="alignColumn">The column to align operators to.</param>
    /// <param name="model">The layout model.</param>
    private static void AlignPatternChain(BinaryPatternSyntax pattern, int alignColumn, LayoutModel model)
    {
        if (pattern.Left is BinaryPatternSyntax leftPattern
            && leftPattern.Kind() == pattern.Kind())
        {
            AlignPatternChain(leftPattern, alignColumn, model);
        }

        LayoutComputer.SetIfFirstOnLine(pattern.OperatorToken, alignColumn, "BinaryPattern", model);

        if (pattern.Right is BinaryPatternSyntax rightPattern
            && rightPattern.Kind() == pattern.Kind())
        {
            AlignPatternChain(rightPattern, alignColumn, model);
        }
    }

    #endregion // Methods

    #region ILayoutContributor

    /// <inheritdoc/>
    public void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context)
    {
        switch (node)
        {
            case BinaryExpressionSyntax binary:
                ProcessBinaryExpression(binary, model);
                break;

            case BinaryPatternSyntax binaryPattern:
                ProcessBinaryPattern(binaryPattern, model);
                break;

            case IsPatternExpressionSyntax isPattern:
                ProcessIsPattern(isPattern, model);
                break;
        }
    }

    #endregion // ILayoutContributor
}