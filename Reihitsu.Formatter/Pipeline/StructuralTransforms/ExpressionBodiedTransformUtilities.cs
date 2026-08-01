using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Shared helpers for expression-bodied member transforms
/// </summary>
internal static class ExpressionBodiedTransformUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether an expression-bodied member must keep its expression body because a
    /// conditional directive splits it
    /// </summary>
    /// <param name="member">The member declaration that owns the expression body</param>
    /// <param name="expressionBody">The expression body to inspect</param>
    /// <param name="semicolonToken">The member's terminating semicolon</param>
    /// <returns><see langword="true"/> if the conversion must be refused; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// A conditional directive delimits alternative source text, not a node, so it cannot travel with
    /// the expression when the arrow body is rebuilt as a block. Converting anyway moved <c>#if</c>
    /// into the middle of the generated statement and pushed <c>#endif</c> outside the member, which
    /// turned source that parsed cleanly into source that no longer compiles. Every expression-bodied
    /// transform consults this guard, so the refusal covers the whole member family at once. Comments
    /// are deliberately not covered: the converter routes those onto the generated braces correctly.
    /// </remarks>
    internal static bool RequiresExpressionBodyPreservation(SyntaxNode member,
                                                            ArrowExpressionClauseSyntax expressionBody,
                                                            SyntaxToken semicolonToken)
    {
        if (member == null || expressionBody == null)
        {
            return false;
        }

        var spanEnd = semicolonToken.IsKind(SyntaxKind.None) || semicolonToken.IsMissing
                          ? expressionBody.Span.End
                          : semicolonToken.Span.End;

        return SyntaxTriviaUtilities.ContainsConditionalDirectives(member, TextSpan.FromBounds(expressionBody.ArrowToken.SpanStart, spanEnd));
    }

    /// <summary>
    /// Determines whether the converted body should use an expression statement
    /// </summary>
    /// <param name="returnType">Return type</param>
    /// <param name="modifiers">Member modifiers</param>
    /// <returns><see langword="true"/> when an expression statement should be used; otherwise <see langword="false"/></returns>
    internal static bool UsesExpressionStatement(TypeSyntax returnType, SyntaxTokenList modifiers)
    {
        if (returnType is PredefinedTypeSyntax predefined
            && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            return true;
        }

        return HasAsyncModifier(modifiers) && IsNonGenericTaskLikeReturnType(returnType);
    }

    /// <summary>
    /// Determines whether the provided modifiers include <see langword="async"/>
    /// </summary>
    /// <param name="modifiers">The modifiers to inspect</param>
    /// <returns><see langword="true"/> if an async modifier is present; otherwise, <see langword="false"/></returns>
    private static bool HasAsyncModifier(SyntaxTokenList modifiers)
    {
        return modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.AsyncKeyword));
    }

    /// <summary>
    /// Determines whether the given return type represents a non-generic task-like type
    /// (<see cref="System.Threading.Tasks.Task"/> or <see cref="System.Threading.Tasks.ValueTask"/>)
    /// </summary>
    /// <param name="returnType">The return type syntax to check</param>
    /// <returns><see langword="true"/> if the return type is a non-generic task-like type; otherwise, <see langword="false"/></returns>
    private static bool IsNonGenericTaskLikeReturnType(TypeSyntax returnType)
    {
        return returnType switch
               {
                   IdentifierNameSyntax identifier => IsTaskLikeName(identifier.Identifier.ValueText),
                   QualifiedNameSyntax qualified => IsTaskLikeName(qualified.Right.Identifier.ValueText) && qualified.Right is GenericNameSyntax == false,
                   AliasQualifiedNameSyntax aliasQualified => IsTaskLikeName(aliasQualified.Name.Identifier.ValueText) && aliasQualified.Name is GenericNameSyntax == false,
                   _ => false,
               };
    }

    /// <summary>
    /// Determines whether the given simple type name is a task-like name
    /// </summary>
    /// <param name="name">The type name to check</param>
    /// <returns><see langword="true"/> if the name is <c>Task</c> or <c>ValueTask</c>; otherwise, <see langword="false"/></returns>
    private static bool IsTaskLikeName(string name)
    {
        return name is "Task" or "ValueTask";
    }

    #endregion // Methods
}