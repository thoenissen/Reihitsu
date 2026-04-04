using System.Collections.Generic;

namespace Reihitsu.Analyzer.Data;

/// <summary>
/// Category: Naming
/// </summary>
internal class ConfigurationCategoryNaming
{
    /// <summary>
    /// Allowed namespaces
    /// </summary>
    public IReadOnlyList<string> AllowedNamespaceDeclarations { get; set; }
}