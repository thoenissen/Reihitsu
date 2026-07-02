using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Shared helpers for override-member analysis
/// </summary>
internal static class OverrideMemberUtilities
{
    #region Methods

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