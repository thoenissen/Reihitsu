using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Helper methods for accessor ordering analyzers and code fixes.
/// </summary>
internal static class AccessorOrderingUtilities
{
    /// <summary>
    /// Tries to find an out-of-order accessor and the target accessor it should precede.
    /// </summary>
    /// <param name="accessorList">Accessor list</param>
    /// <param name="accessorKindToMove">Accessor that should come first</param>
    /// <param name="accessorKindsThatMustFollow">Accessors that must come after the first accessor</param>
    /// <param name="accessorToMove">Accessor to move</param>
    /// <param name="targetAccessor">Target accessor</param>
    /// <returns><see langword="true"/> if an out-of-order accessor was found</returns>
    internal static bool TryGetAccessorMove(AccessorListSyntax accessorList, SyntaxKind accessorKindToMove, IReadOnlyCollection<SyntaxKind> accessorKindsThatMustFollow, out AccessorDeclarationSyntax accessorToMove, out AccessorDeclarationSyntax targetAccessor)
    {
        accessorToMove = null;
        targetAccessor = null;

        var accessorToMoveIndex = -1;
        var targetAccessorIndex = -1;

        for (var accessorIndex = 0; accessorIndex < accessorList.Accessors.Count; accessorIndex++)
        {
            var accessor = accessorList.Accessors[accessorIndex];

            if (accessor.Kind() == accessorKindToMove)
            {
                accessorToMove = accessor;
                accessorToMoveIndex = accessorIndex;
            }

            if (targetAccessorIndex < 0 && accessorKindsThatMustFollow.Contains(accessor.Kind()))
            {
                targetAccessor = accessor;
                targetAccessorIndex = accessorIndex;
            }
        }

        return accessorToMove != null
               && targetAccessor != null
               && accessorToMoveIndex > targetAccessorIndex;
    }

    /// <summary>
    /// Moves an accessor before another accessor.
    /// </summary>
    /// <param name="accessorList">Accessor list</param>
    /// <param name="accessorToMove">Accessor to move</param>
    /// <param name="targetAccessor">Target accessor</param>
    /// <returns>The updated accessor list</returns>
    internal static AccessorListSyntax MoveAccessorBefore(AccessorListSyntax accessorList, AccessorDeclarationSyntax accessorToMove, AccessorDeclarationSyntax targetAccessor)
    {
        var accessorDeclarations = accessorList.Accessors;
        var accessorToMoveIndex = accessorDeclarations.IndexOf(accessorToMove);
        var targetAccessorIndex = accessorDeclarations.IndexOf(targetAccessor);

        if (accessorToMoveIndex < 0
            || targetAccessorIndex < 0
            || accessorToMoveIndex <= targetAccessorIndex)
        {
            return accessorList;
        }

        var updatedAccessors = accessorDeclarations.RemoveAt(accessorToMoveIndex)
                                                   .Insert(targetAccessorIndex, accessorToMove);

        return accessorList.WithAccessors(updatedAccessors);
    }
}