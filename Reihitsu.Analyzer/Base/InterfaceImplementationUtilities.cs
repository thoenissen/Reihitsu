using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Shared helpers for interface-implementation analysis
/// </summary>
internal static class InterfaceImplementationUtilities
{
    #region Methods

    /// <summary>
    /// Gets the interface region name for the given member
    /// </summary>
    /// <param name="memberSymbol">Member symbol</param>
    /// <returns>Name of the implemented interface, or an empty string when the member is an override or does not implement an interface member</returns>
    internal static string GetInterfaceRegionName(ISymbol memberSymbol)
    {
        return memberSymbol.IsOverride ? string.Empty : GetImplementedInterfaceName(memberSymbol);
    }

    /// <summary>
    /// Gets the name of the interface whose member is implemented by the given member
    /// </summary>
    /// <param name="memberSymbol">Member symbol</param>
    /// <returns>Name of the implemented interface, or an empty string when the member does not implement an interface member</returns>
    internal static string GetImplementedInterfaceName(ISymbol memberSymbol)
    {
        var explicitInterfaceName = GetExplicitInterfaceName(memberSymbol);

        if (string.IsNullOrEmpty(explicitInterfaceName) == false)
        {
            return explicitInterfaceName;
        }

        if (memberSymbol.ContainingType is not { } containingType)
        {
            return string.Empty;
        }

        foreach (var interfaceType in containingType.AllInterfaces)
        {
            foreach (var interfaceMember in interfaceType.GetMembers())
            {
                if (interfaceMember.Kind != memberSymbol.Kind)
                {
                    continue;
                }

                if (SymbolEqualityComparer.Default.Equals(containingType.FindImplementationForInterfaceMember(interfaceMember), memberSymbol))
                {
                    return interfaceType.Name;
                }
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Gets the name of the interface that is implemented explicitly by the given member
    /// </summary>
    /// <param name="memberSymbol">Member symbol</param>
    /// <returns>Name of the explicitly implemented interface, or an empty string when the member is not an explicit implementation</returns>
    private static string GetExplicitInterfaceName(ISymbol memberSymbol)
    {
        return memberSymbol switch
               {
                   IMethodSymbol { ExplicitInterfaceImplementations.Length: > 0 } methodSymbol => methodSymbol.ExplicitInterfaceImplementations[0].ContainingType.Name,
                   IPropertySymbol { ExplicitInterfaceImplementations.Length: > 0 } propertySymbol => propertySymbol.ExplicitInterfaceImplementations[0].ContainingType.Name,
                   IEventSymbol { ExplicitInterfaceImplementations.Length: > 0 } eventSymbol => eventSymbol.ExplicitInterfaceImplementations[0].ContainingType.Name,
                   _ => string.Empty
               };
    }

    #endregion // Methods
}