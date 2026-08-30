using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Helper methods for accessor ordering analyzers and code fixes
/// </summary>
public static class AccessorOrderingUtilities
{
    #region Methods

    /// <summary>
    /// Tries to find an out-of-order accessor and the target accessor it should precede
    /// </summary>
    /// <param name="accessorList">Accessor list</param>
    /// <param name="accessorKindToMove">Accessor that should come first</param>
    /// <param name="accessorKindsThatMustFollow">Accessors that must come after the first accessor</param>
    /// <param name="accessorToMove">Accessor to move</param>
    /// <param name="targetAccessor">Target accessor</param>
    /// <returns><see langword="true"/> if an out-of-order accessor was found</returns>
    public static bool TryGetAccessorMove(AccessorListSyntax accessorList, SyntaxKind accessorKindToMove, IReadOnlyCollection<SyntaxKind> accessorKindsThatMustFollow, out AccessorDeclarationSyntax accessorToMove, out AccessorDeclarationSyntax targetAccessor)
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
    /// Moves an accessor before another accessor. Blank-line separators are kept at the position they already
    /// occupied via <see cref="OrderingMoveSafety.MoveNodeBeforePreservingSeparators{TNode}"/>
    /// </summary>
    /// <param name="accessorList">Accessor list</param>
    /// <param name="accessorToMove">Accessor to move</param>
    /// <param name="targetAccessor">Target accessor</param>
    /// <returns>The updated accessor list</returns>
    public static AccessorListSyntax MoveAccessorBefore(AccessorListSyntax accessorList, AccessorDeclarationSyntax accessorToMove, AccessorDeclarationSyntax targetAccessor)
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

        var updatedAccessors = OrderingMoveSafety.MoveNodeBeforePreservingSeparators(accessorDeclarations, accessorToMove, targetAccessor);

        return accessorList.WithAccessors(updatedAccessors);
    }

    /// <summary>
    /// Determines whether moving an accessor before another accessor would relocate a preprocessor directive away
    /// from the code it governs, splitting a conditional-compilation pair or scrambling region structure.
    /// The analysis is shared with the type member guard through
    /// <see cref="OrderingMoveSafety.MoveRangeContainsDirectives{TNode}"/>
    /// </summary>
    /// <param name="accessorList">Accessor list</param>
    /// <param name="accessorToMove">Accessor to move</param>
    /// <param name="targetAccessor">Target accessor</param>
    /// <returns><see langword="true"/> if the move would relocate a preprocessor directive</returns>
    public static bool MoveRangeContainsDirectives(AccessorListSyntax accessorList, AccessorDeclarationSyntax accessorToMove, AccessorDeclarationSyntax targetAccessor)
    {
        return OrderingMoveSafety.MoveRangeContainsDirectives(accessorList, accessorList.Accessors, accessorToMove, targetAccessor);
    }

    #endregion // Methods
}