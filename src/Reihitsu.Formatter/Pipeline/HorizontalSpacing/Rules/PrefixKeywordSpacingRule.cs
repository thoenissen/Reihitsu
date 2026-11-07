using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

/// <summary>
/// Puts one space behind a modifier or prefix keyword in front of its operand (FMT-SPACE-024): a lambda modifier, a
/// modifier of an argument, a parameter, or a function pointer's parameter, the <c>ref</c> and <c>readonly</c> of a
/// lambda's return type, the <c>ref</c> of a ref expression, <c>await</c>, <c>stackalloc</c>, <c>var</c> in front of a
/// parenthesized designation, the pattern <c>not</c>, and the <c>in</c> of <c>foreach</c>, <c>from</c>, and
/// <c>join</c>. A keyword followed by its own parenthesis, such as <c>typeof</c>, and a modifier of a member or a local
/// declaration are not decided by this rule
/// </summary>
internal sealed class PrefixKeywordSpacingRule : ISpacingRule
{
    #region Methods

    /// <summary>
    /// Determines whether the token is a modifier or prefix keyword this rule spaces
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <returns><see langword="true"/> if one space follows the token; otherwise, <see langword="false"/></returns>
    private static bool IsPrefixKeyword(SyntaxToken token)
    {
        return token.Parent switch
               {
                   AnonymousFunctionExpressionSyntax anonymousFunction => anonymousFunction.Modifiers.Contains(token),
                   ArgumentSyntax argument => token == argument.RefKindKeyword,
                   ParameterSyntax parameter => parameter.Modifiers.Contains(token),
                   FunctionPointerParameterSyntax functionPointerParameter => functionPointerParameter.Modifiers.Contains(token),
                   RefTypeSyntax refType => IsRefOfALambdaReturnType(token, refType),
                   RefExpressionSyntax refExpression => token == refExpression.RefKeyword,
                   AwaitExpressionSyntax awaitExpression => token == awaitExpression.AwaitKeyword,
                   StackAllocArrayCreationExpressionSyntax stackAlloc => token == stackAlloc.StackAllocKeyword,
                   ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAlloc => token == implicitStackAlloc.StackAllocKeyword,
                   VarPatternSyntax varPattern => varPattern.Designation is ParenthesizedVariableDesignationSyntax,
                   IdentifierNameSyntax varType => IsVarOfADeconstruction(varType),
                   UnaryPatternSyntax unaryPattern => token == unaryPattern.OperatorToken,
                   CommonForEachStatementSyntax forEach => token == forEach.InKeyword,
                   FromClauseSyntax fromClause => token == fromClause.InKeyword,
                   JoinClauseSyntax joinClause => token == joinClause.InKeyword,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether the token is the <c>ref</c> or the <c>readonly</c> of a lambda's return type. The <c>ref</c> type
    /// of any other declaration keeps the author's spacing
    /// </summary>
    /// <param name="token">The token to inspect</param>
    /// <param name="refType">The ref type the token belongs to</param>
    /// <returns><see langword="true"/> if the token modifies a lambda's return type; otherwise, <see langword="false"/></returns>
    private static bool IsRefOfALambdaReturnType(SyntaxToken token, RefTypeSyntax refType)
    {
        if (refType.Parent is not ParenthesizedLambdaExpressionSyntax lambda
            || lambda.ReturnType != refType)
        {
            return false;
        }

        return token == refType.RefKeyword
               || token == refType.ReadOnlyKeyword;
    }

    /// <summary>
    /// Determines whether the name is the <c>var</c> in front of the parenthesized designation of a deconstruction, as in
    /// <c>var (x, y) = t;</c>
    /// </summary>
    /// <param name="name">The name to inspect</param>
    /// <returns><see langword="true"/> if the name is such a <c>var</c>; otherwise, <see langword="false"/></returns>
    private static bool IsVarOfADeconstruction(IdentifierNameSyntax name)
    {
        return name.IsVar
               && name.Parent is DeclarationExpressionSyntax { Designation: ParenthesizedVariableDesignationSyntax } declarationExpression
               && declarationExpression.Type == name;
    }

    #endregion // Methods

    #region ISpacingRule

    /// <inheritdoc/>
    public int? DesiredSpacesAfter(SyntaxToken left, SyntaxToken right)
    {
        if (IsPrefixKeyword(left))
        {
            return 1;
        }

        return null;
    }

    #endregion // ISpacingRule
}