using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

/// <summary>
/// Shared helpers for target-based attribute formatting
/// </summary>
internal static class AttributeTargetFormattingShared
{
    #region Methods

    /// <summary>
    /// Resolves the expected placement mode for an attribute list based on its layout target
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Expected placement mode</returns>
    internal static TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList)
    {
        return UsesParameterLayout(attributeList)
                   ? TargetAttributePlacementMode.SingleLine
                   : TargetAttributePlacementMode.SeparateLine;
    }

    /// <summary>
    /// Resolves the expected list-shape mode for an attribute list based on its layout target
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Expected list-shape mode</returns>
    internal static TargetAttributeListShapeMode ResolveListShapeMode(AttributeListSyntax attributeList)
    {
        return UsesParameterLayout(attributeList)
                   ? TargetAttributeListShapeMode.MergedList
                   : TargetAttributeListShapeMode.SplitLists;
    }

    /// <summary>
    /// Determines whether an attribute list follows the parameter layout: it stays on its owner's line and keeps its
    /// attributes in one list. That applies to every list whose layout target is a parameter or a type parameter,
    /// and to a list on a parameter or type parameter whose specifier is not recognized at all, because a parameter
    /// or type-parameter list has no valid layout for an attribute list on a line of its own
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> when the list follows the parameter layout; otherwise, <see langword="false"/></returns>
    private static bool UsesParameterLayout(AttributeListSyntax attributeList)
    {
        return AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target)
                   ? target is AttributeTargets.Parameter or AttributeTargets.GenericParameter
                   : attributeList.Parent is ParameterSyntax or TypeParameterSyntax;
    }

    #endregion // Methods
}