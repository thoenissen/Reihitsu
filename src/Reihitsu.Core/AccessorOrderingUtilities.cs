using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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
    /// Creates the text change that moves an accessor before another accessor (SHARED-ORDER-009): within the line when the
    /// whole accessor list stands on one line, otherwise as a move of whole lines, which
    /// <see cref="OrderingMoveSafety.TryCreateWholeLineMove{TNode}"/> refuses where it cannot be performed. The swap within
    /// the line is refused when a comment stands in the list outside every accessor, in front of the first, between two,
    /// or behind the last: the swap leaves the text between the accessors in place, so such a comment would end up next to
    /// another accessor
    /// </summary>
    /// <param name="accessorList">Accessor list</param>
    /// <param name="accessorToMove">Accessor to move</param>
    /// <param name="targetAccessor">Target accessor</param>
    /// <param name="change">The text change that performs the move</param>
    /// <returns><see langword="true"/> if the move can be performed</returns>
    public static bool TryCreateMove(AccessorListSyntax accessorList, AccessorDeclarationSyntax accessorToMove, AccessorDeclarationSyntax targetAccessor, out TextChange change)
    {
        var lines = accessorList.SyntaxTree.GetText().Lines;

        if (lines.GetLineFromPosition(accessorList.OpenBraceToken.SpanStart).LineNumber != lines.GetLineFromPosition(accessorList.CloseBraceToken.SpanStart).LineNumber)
        {
            return OrderingMoveSafety.TryCreateWholeLineMove(accessorList.Accessors, accessorToMove, targetAccessor, out change);
        }

        if (HoldsCommentOutsideEveryAccessor(accessorList))
        {
            change = default;

            return false;
        }

        return OrderingMoveSafety.TryCreateSwapWithinLine(accessorList.Accessors, accessorToMove, targetAccessor, out change);
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

    /// <summary>
    /// Determines whether a comment stands between the braces of an accessor list outside every accessor
    /// </summary>
    /// <param name="accessorList">Accessor list</param>
    /// <returns><see langword="true"/> if such a comment stands in the list</returns>
    private static bool HoldsCommentOutsideEveryAccessor(AccessorListSyntax accessorList)
    {
        return accessorList.DescendantTrivia(TextSpan.FromBounds(accessorList.OpenBraceToken.Span.End, accessorList.CloseBraceToken.SpanStart))
                           .Any(trivia => SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                                          && accessorList.Accessors.Any(accessor => accessor.Span.Contains(trivia.Span)) == false);
    }

    #endregion // Methods
}