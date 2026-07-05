using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using Reihitsu.Core.Enumerations;

namespace Reihitsu.Core;

/// <summary>
/// Provides helpers for classifying top-level regions into their category
/// </summary>
public static class RegionCategoryUtilities
{
    #region Methods

    /// <summary>
    /// Determines the category of a top-level region based on its description and the declaring type
    /// </summary>
    /// <param name="typeSymbol">Symbol of the type that declares the region</param>
    /// <param name="regionDescription">Description of the region</param>
    /// <returns>Category of the region</returns>
    public static RegionCategory GetRegionCategory(INamedTypeSymbol typeSymbol, string regionDescription)
    {
        if (typeSymbol == null || string.IsNullOrEmpty(regionDescription))
        {
            return RegionCategory.Custom;
        }

        if (typeSymbol.AllInterfaces.Any(interfaceType => string.Equals(interfaceType.Name, regionDescription, StringComparison.Ordinal)))
        {
            return RegionCategory.InterfaceImplementation;
        }

        for (var baseType = typeSymbol.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            if (string.Equals(baseType.Name, regionDescription, StringComparison.Ordinal))
            {
                return RegionCategory.BaseTypeOverride;
            }
        }

        return RegionCategory.Custom;
    }

    #endregion // Methods
}