using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// The one owner of which declared variables are deconstruction variables: RH4117 checks exactly these variables, and
/// RH4115 leaves exactly these to it
/// </summary>
internal static class DeconstructionVariableUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a variable designation declares a variable of a deconstruction
    /// </summary>
    /// <param name="designation">Variable designation</param>
    /// <returns><see langword="true"/> if the designation declares a deconstruction variable; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// A deconstruction variable is a variable of a parenthesized variable designation, <c>var (a, b)</c>, in a declaration
    /// or a pattern; a variable declared by a declaration expression that is an element of a tuple expression, <c>
    /// (var a,
    /// int b) = p
    /// </c>, which the compiler allows only on the left of a deconstruction and in front of <c>in</c> of a
    /// <c>foreach</c> statement; and the variable that a pattern declares for the value it tests when that pattern
    /// receives a whole element of a positional pattern, <c>p is (var a, int b and &gt; 0)</c>: the pattern is the pattern
    /// of a subpattern of a positional pattern, or stands inside it only behind <c>and</c> combinators and parentheses
    /// </remarks>
    internal static bool IsDeconstructionVariable(SingleVariableDesignationSyntax designation)
    {
        return designation.Parent switch
               {
                   ParenthesizedVariableDesignationSyntax => true,
                   DeclarationExpressionSyntax declarationExpression => declarationExpression.Parent is ArgumentSyntax { Parent: TupleExpressionSyntax },
                   PatternSyntax pattern => ReceivesPositionalElement(pattern),
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether a pattern receives a whole element of a positional pattern
    /// </summary>
    /// <param name="pattern">Pattern that declares a variable for the value it tests</param>
    /// <returns><see langword="true"/> if the pattern receives a whole element; otherwise, <see langword="false"/></returns>
    private static bool ReceivesPositionalElement(PatternSyntax pattern)
    {
        SyntaxNode current = pattern;

        while (current.Parent is ParenthesizedPatternSyntax or BinaryPatternSyntax { RawKind: (int)SyntaxKind.AndPattern })
        {
            current = current.Parent;
        }

        return current.Parent is SubpatternSyntax { Parent: PositionalPatternClauseSyntax };
    }

    #endregion // Methods
}