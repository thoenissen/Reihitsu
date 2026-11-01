using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space between a type or a pattern and the name it declares (FMT-SPACE-021): the type of a parameter, a tuple
/// element, a variable declaration (a field, an event field, a local, or the declaration of a <c>for</c>, <c>fixed</c>,
/// or <c>using</c> statement), a declaration expression, a declaration pattern, a <c>foreach</c> variable, a
/// <c>catch</c> declaration, or a <c>from</c> or <c>join</c> range variable and the name behind it, and a recursive or
/// list pattern and its designation. The return type of a method, a local function, or a delegate, the type of a
/// property or an indexer, and the type of an event with an accessor list are not decided by this rule
/// </summary>
internal sealed class DeclaredNameSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines whether the right token is a declared name or designation and the left token ends the type or pattern
    /// in front of it
    /// </summary>
    /// <param name="left">The token in front of the gap</param>
    /// <param name="right">The token behind the gap</param>
    /// <returns><see langword="true"/> if the gap separates a type or pattern from the name it declares; otherwise, <see langword="false"/></returns>
    private static bool IsDeclaredName(SyntaxToken left, SyntaxToken right)
    {
        var typeOrPattern = right.Parent switch
                            {
                                ParameterSyntax parameter when right == parameter.Identifier => parameter.Type,
                                TupleElementSyntax tupleElement when right == tupleElement.Identifier => tupleElement.Type,
                                VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax declaration } declarator when right == declarator.Identifier => declaration.Type,
                                ForEachStatementSyntax forEach when right == forEach.Identifier => forEach.Type,
                                CatchDeclarationSyntax catchDeclaration when right == catchDeclaration.Identifier => catchDeclaration.Type,
                                FromClauseSyntax fromClause when right == fromClause.Identifier => fromClause.Type,
                                JoinClauseSyntax joinClause when right == joinClause.Identifier => joinClause.Type,
                                _ => null
                            };

        if (typeOrPattern != null)
        {
            return left == typeOrPattern.GetLastToken();
        }

        return right.Parent is SingleVariableDesignationSyntax or DiscardDesignationSyntax
               && IsDesignationBehindATypeOrPattern(right.Parent)
               && left == right.GetPreviousToken();
    }

    /// <summary>
    /// Determines whether a designation stands behind a type or a pattern: the type of a declaration expression or a
    /// declaration pattern, or the recursive or list pattern the designation belongs to
    /// </summary>
    /// <param name="designation">The designation</param>
    /// <returns><see langword="true"/> if a type or a pattern precedes the designation; otherwise, <see langword="false"/></returns>
    private static bool IsDesignationBehindATypeOrPattern(SyntaxNode designation)
    {
        return designation.Parent switch
               {
                   DeclarationExpressionSyntax declarationExpression => declarationExpression.Designation == designation,
                   DeclarationPatternSyntax declarationPattern => declarationPattern.Designation == designation,
                   RecursivePatternSyntax recursivePattern => recursivePattern.Designation == designation,
                   ListPatternSyntax listPattern => listPattern.Designation == designation,
                   _ => false
               };
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (IsDeclaredName(left, right))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}