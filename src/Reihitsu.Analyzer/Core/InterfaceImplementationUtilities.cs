using System.Collections.Generic;

using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Shared helpers for interface-implementation analysis
/// </summary>
internal static class InterfaceImplementationUtilities
{
    #region Methods

    /// <summary>
    /// Gets the names of the regions an interface implementation belongs in: the name of every interface whose member the
    /// member implements (SHARED-REGGROUP-004). The compiler binds the implemented interface member to the defining part
    /// of a partial member, so the interfaces of a part are those of its defining part
    /// </summary>
    /// <param name="memberSymbol">Member symbol, or the symbol of a part of a partial member</param>
    /// <returns>Names of the implemented interfaces, or no name when the member is an override or implements no interface member</returns>
    internal static IReadOnlyList<string> GetInterfaceRegionNames(ISymbol memberSymbol)
    {
        var member = PartialMemberUtilities.GetDefiningPartSymbol(memberSymbol);

        return member.IsOverride ? [] : GetImplementedInterfaceNames(member);
    }

    /// <summary>
    /// Gets the name of the first interface whose member is implemented by the given member
    /// </summary>
    /// <param name="memberSymbol">Member symbol</param>
    /// <returns>Name of the implemented interface, or an empty string when the member does not implement an interface member</returns>
    internal static string GetImplementedInterfaceName(ISymbol memberSymbol)
    {
        return GetImplementedInterfaceNames(memberSymbol).FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Gets the names of the interfaces whose members are implemented by the given member: the interface its name is
    /// qualified with for an explicit implementation, and every interface that declares a member it implements otherwise
    /// </summary>
    /// <param name="memberSymbol">Member symbol</param>
    /// <returns>Names of the implemented interfaces in the order of the type's interfaces</returns>
    private static IReadOnlyList<string> GetImplementedInterfaceNames(ISymbol memberSymbol)
    {
        var explicitInterfaceName = GetExplicitInterfaceName(memberSymbol);

        if (string.IsNullOrEmpty(explicitInterfaceName) == false)
        {
            return [explicitInterfaceName];
        }

        if (memberSymbol.ContainingType is not { } containingType)
        {
            return [];
        }

        return containingType.AllInterfaces
                             .Where(interfaceType => interfaceType.GetMembers()
                                                                  .Any(interfaceMember => interfaceMember.Kind == memberSymbol.Kind
                                                                                          && SymbolEqualityComparer.Default.Equals(containingType.FindImplementationForInterfaceMember(interfaceMember), memberSymbol)))
                             .Select(interfaceType => interfaceType.Name)
                             .Distinct(StringComparer.Ordinal)
                             .ToList();
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