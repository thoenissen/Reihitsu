using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared helpers for override-member analysis
/// </summary>
internal static class OverrideMemberUtilities
{
    #region Methods

    /// <summary>
    /// Gets the names of the regions an override belongs in: the name of the type that introduced the member it overrides
    /// (SHARED-REGGROUP-003)
    /// </summary>
    /// <param name="memberSymbol">Symbol of a method, property, indexer, or event</param>
    /// <returns>The one name, or no name when the member overrides nothing</returns>
    internal static IReadOnlyList<string> GetOverrideRegionNames(ISymbol memberSymbol)
    {
        return memberSymbol switch
               {
                   IMethodSymbol { OverriddenMethod: not null } methodSymbol => [GetOriginalDeclaringTypeName(methodSymbol)],
                   IPropertySymbol { OverriddenProperty: not null } propertySymbol => [GetOriginalDeclaringTypeName(propertySymbol)],
                   IEventSymbol { OverriddenEvent: not null } eventSymbol => [GetOriginalDeclaringTypeName(eventSymbol)],
                   _ => []
               };
    }

    /// <summary>
    /// Gets the type name that introduced the original method in the override chain
    /// </summary>
    /// <param name="methodSymbol">Method symbol</param>
    /// <returns>Containing type name of the first declaration in the chain</returns>
    internal static string GetOriginalDeclaringTypeName(IMethodSymbol methodSymbol)
    {
        while (methodSymbol.OverriddenMethod != null)
        {
            methodSymbol = methodSymbol.OverriddenMethod;
        }

        return methodSymbol.ContainingType.Name;
    }

    /// <summary>
    /// Gets the type name that introduced the original property or indexer in the override chain
    /// </summary>
    /// <param name="propertySymbol">Property symbol</param>
    /// <returns>Containing type name of the first declaration in the chain</returns>
    internal static string GetOriginalDeclaringTypeName(IPropertySymbol propertySymbol)
    {
        while (propertySymbol.OverriddenProperty != null)
        {
            propertySymbol = propertySymbol.OverriddenProperty;
        }

        return propertySymbol.ContainingType.Name;
    }

    /// <summary>
    /// Gets the type name that introduced the original event in the override chain
    /// </summary>
    /// <param name="eventSymbol">Event symbol</param>
    /// <returns>Containing type name of the first declaration in the chain</returns>
    internal static string GetOriginalDeclaringTypeName(IEventSymbol eventSymbol)
    {
        while (eventSymbol.OverriddenEvent != null)
        {
            eventSymbol = eventSymbol.OverriddenEvent;
        }

        return eventSymbol.ContainingType.Name;
    }

    #endregion // Methods
}