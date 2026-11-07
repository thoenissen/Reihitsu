using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

/// <summary>
/// Shared helpers for expression-bodied member transforms
/// </summary>
internal static class ExpressionBodiedTransformUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether an expression-bodied member must keep its expression body because a
    /// directive in its span would not survive the rewrite
    /// </summary>
    /// <param name="member">The member declaration that owns the expression body</param>
    /// <param name="expressionBody">The expression body to inspect</param>
    /// <param name="semicolonToken">The member's terminating semicolon</param>
    /// <returns><see langword="true"/> if the conversion must be refused; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The decision lives in <see cref="ExpressionBodyRewriteUtilities.BlocksRewrite"/> so the
    /// analyzers that report these members apply exactly the same rule; a drifting copy would let an
    /// analyzer report a member whose code fix this transform then refuses to rewrite. Every
    /// expression-bodied transform consults this guard, so the whole member family is covered at once.
    /// </remarks>
    internal static bool RequiresExpressionBodyPreservation(SyntaxNode member,
                                                            ArrowExpressionClauseSyntax expressionBody,
                                                            SyntaxToken semicolonToken)
    {
        return ExpressionBodyRewriteUtilities.BlocksRewrite(member, expressionBody, semicolonToken);
    }

    /// <summary>
    /// Determines whether an expression-bodied method or local function must keep its expression body because it is
    /// <see langword="async"/> and its return type is neither <see langword="void"/> nor written as <c>Task</c>,
    /// <c>ValueTask</c>, <c>Task&lt;…&gt;</c>, or <c>ValueTask&lt;…&gt;</c>, with or without a nullable annotation. Behind
    /// an alias or a custom task-like type the syntax does not tell whether the task carries a result, so neither an
    /// expression statement nor a <c>return</c> statement is known to compile
    /// </summary>
    /// <param name="member">The member that owns the expression body</param>
    /// <returns><see langword="true"/> if the conversion must be refused; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The expression-body code fixes consult this guard as well, so they offer no fix for a member the formatter keeps.
    /// The analyzers do not: such a member is still reported, unlike one that <see cref="RequiresExpressionBodyPreservation"/>
    /// keeps, because the obstacle is what the return type means and not the code the rewrite touches
    /// </remarks>
    internal static bool RequiresAsyncExpressionBodyPreservation(SyntaxNode member)
    {
        return member switch
               {
                   MethodDeclarationSyntax method => RequiresAsyncExpressionBodyPreservation(method.ReturnType, method.Modifiers),
                   LocalFunctionStatementSyntax localFunction => RequiresAsyncExpressionBodyPreservation(localFunction.ReturnType, localFunction.Modifiers),
                   _ => false,
               };
    }

    /// <summary>
    /// Determines whether the converted body should use an expression statement: for a <see langword="void"/> return type,
    /// and for an <see langword="async"/> function whose return type is written as the non-generic <c>Task</c> or
    /// <c>ValueTask</c>, with or without a nullable annotation
    /// </summary>
    /// <param name="returnType">Return type</param>
    /// <param name="modifiers">Member modifiers</param>
    /// <returns><see langword="true"/> when an expression statement should be used; otherwise <see langword="false"/></returns>
    internal static bool UsesExpressionStatement(TypeSyntax returnType, SyntaxTokenList modifiers)
    {
        if (IsVoid(returnType))
        {
            return true;
        }

        return HasAsyncModifier(modifiers)
               && GetWrittenTaskTypeName(returnType) is IdentifierNameSyntax;
    }

    /// <summary>
    /// Determines whether an <see langword="async"/> function with the given return type keeps its expression body
    /// </summary>
    /// <param name="returnType">Return type</param>
    /// <param name="modifiers">Member modifiers</param>
    /// <returns><see langword="true"/> if the conversion must be refused; otherwise, <see langword="false"/></returns>
    private static bool RequiresAsyncExpressionBodyPreservation(TypeSyntax returnType, SyntaxTokenList modifiers)
    {
        return HasAsyncModifier(modifiers)
               && IsVoid(returnType) == false
               && GetWrittenTaskTypeName(returnType) == null;
    }

    /// <summary>
    /// Determines whether the return type is <see langword="void"/>
    /// </summary>
    /// <param name="returnType">Return type</param>
    /// <returns><see langword="true"/> if the return type is <see langword="void"/>; otherwise, <see langword="false"/></returns>
    private static bool IsVoid(TypeSyntax returnType)
    {
        return returnType is PredefinedTypeSyntax predefined
               && predefined.Keyword.IsKind(SyntaxKind.VoidKeyword);
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
    /// Gets the name of a return type written as <c>Task</c>, <c>ValueTask</c>, <c>Task&lt;…&gt;</c>, or
    /// <c>ValueTask&lt;…&gt;</c> — as a simple name or as the last part of a qualified or alias-qualified name, with or
    /// without a nullable annotation. The result is an <see cref="IdentifierNameSyntax"/> for a non-generic task type and
    /// a <see cref="GenericNameSyntax"/> for a generic one
    /// </summary>
    /// <param name="returnType">The return type syntax to check</param>
    /// <returns>The task type's name, or <see langword="null"/> when the return type is not written as a task type</returns>
    private static SimpleNameSyntax GetWrittenTaskTypeName(TypeSyntax returnType)
    {
        if (returnType is NullableTypeSyntax nullableType)
        {
            returnType = nullableType.ElementType;
        }

        var name = returnType switch
                   {
                       SimpleNameSyntax simpleName => simpleName,
                       QualifiedNameSyntax qualified => qualified.Right,
                       AliasQualifiedNameSyntax aliasQualified => aliasQualified.Name,
                       _ => null,
                   };

        return name != null && IsTaskLikeName(name.Identifier.ValueText)
                   ? name
                   : null;
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