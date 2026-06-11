using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Pipeline.StructuralTransforms;

/// <summary>
/// Shared helpers for expression-bodied member transforms
/// </summary>
internal static class ExpressionBodiedTransformUtilities
{
    #region Methods

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