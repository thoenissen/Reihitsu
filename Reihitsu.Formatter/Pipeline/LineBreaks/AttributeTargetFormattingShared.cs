using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Shared helpers for target-based attribute formatting
/// </summary>
internal static class AttributeTargetFormattingShared
{
    #region Methods

    /// <summary>
    /// Resolves the expected placement mode for an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Expected placement mode</returns>
    internal static TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList)
    {
        if (attributeList.Parent is AccessorDeclarationSyntax accessorDeclaration
            && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
            && SyntaxNodeUtilities.IsSingleLine(basePropertyDeclaration))
        {
            return TargetAttributePlacementMode.SingleLine;
        }

        return AttributeTargetUtilities.TryResolveTarget(attributeList, out var target)
               && (target == AttributeTargets.Parameter || target == AttributeTargets.GenericParameter)
                   ? TargetAttributePlacementMode.SingleLine
                   : TargetAttributePlacementMode.SeparateLine;
    }

    /// <summary>
    /// Resolves the expected list-shape mode for an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Expected list-shape mode</returns>
    internal static TargetAttributeListShapeMode ResolveListShapeMode(AttributeListSyntax attributeList)
    {
        if (attributeList.Parent is AccessorDeclarationSyntax accessorDeclaration
            && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
            && SyntaxNodeUtilities.IsSingleLine(basePropertyDeclaration))
        {
            return TargetAttributeListShapeMode.MergedList;
        }

        return AttributeTargetUtilities.TryResolveTarget(attributeList, out var target)
               && (target == AttributeTargets.Parameter || target == AttributeTargets.GenericParameter)
                   ? TargetAttributeListShapeMode.MergedList
                   : TargetAttributeListShapeMode.SplitLists;
    }

    #endregion // Methods
}