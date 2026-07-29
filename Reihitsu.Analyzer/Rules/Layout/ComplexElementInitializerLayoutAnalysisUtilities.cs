using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// Shared layout analysis for multi-line complex-element initializers
/// </summary>
internal static class ComplexElementInitializerLayoutAnalysisUtilities
{
    #region Constants

    /// <summary>
    /// Required indentation between a complex element's braces and its expressions
    /// </summary>
    private const int IndentSize = 4;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Finds the first brace or expression that does not match recursive complex-element layout
    /// </summary>
    /// <param name="complexElement">Complex-element initializer to inspect</param>
    /// <returns>The first misaligned location, or <see langword="null"/> when the element is valid</returns>
    public static Location FindFirstMisalignment(InitializerExpressionSyntax complexElement)
    {
        var openBracePosition = complexElement.OpenBraceToken
                                              .GetLocation()
                                              .GetLineSpan()
                                              .StartLinePosition;
        var closeBracePosition = complexElement.CloseBraceToken
                                               .GetLocation()
                                               .GetLineSpan()
                                               .StartLinePosition;

        if (openBracePosition.Line == closeBracePosition.Line)
        {
            return null;
        }

        if (openBracePosition.Character != closeBracePosition.Character)
        {
            return complexElement.GetLocation();
        }

        var expressionLines = new HashSet<int>();

        foreach (var expression in complexElement.Expressions)
        {
            var expressionPosition = expression.GetFirstToken()
                                               .GetLocation()
                                               .GetLineSpan()
                                               .StartLinePosition;

            if (expressionPosition.Line <= openBracePosition.Line
                || expressionPosition.Line >= closeBracePosition.Line
                || expressionLines.Add(expressionPosition.Line) == false
                || expressionPosition.Character != openBracePosition.Character + IndentSize)
            {
                return expression.GetLocation();
            }

            if (expression is InitializerExpressionSyntax nestedComplexElement
                && nestedComplexElement.IsKind(SyntaxKind.ComplexElementInitializerExpression))
            {
                var nestedMisalignment = FindFirstMisalignment(nestedComplexElement);

                if (nestedMisalignment != null)
                {
                    return nestedMisalignment;
                }
            }
        }

        return null;
    }

    #endregion // Methods
}