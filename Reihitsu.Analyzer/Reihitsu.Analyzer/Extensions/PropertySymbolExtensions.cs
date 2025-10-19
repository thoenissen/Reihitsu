using Microsoft.CodeAnalysis;

namespace Reihitsu.Analyzer.Extensions;

/// <summary>
/// Extension methods for <see cref="IPropertySymbol"/>
/// </summary>
internal static class PropertySymbolExtensions
{
    #region Methods

    /// <summary>
    /// Checking if the given property is an auto property
    /// </summary>
    /// <param name="property">Property</param>
    /// <returns>Is the given property an auto property?</returns>
    public static bool IsAutoProperty(this IPropertySymbol property)
    {
        return property.ContainingType.GetMembers().Any(member => member is IFieldSymbol field && SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, property));
    }

    #endregion // Methods
}