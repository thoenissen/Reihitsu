using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// Discovers the shipped Reihitsu analyzers that the self-hosting scan runs, one test row each
/// </summary>
internal static class AnalyzerCatalog
{
    #region Methods

    /// <summary>
    /// Discovers every concrete <see cref="DiagnosticAnalyzer"/> type of the Reihitsu.Analyzer assembly that carries
    /// <see cref="DiagnosticAnalyzerAttribute"/>
    /// </summary>
    /// <returns>The analyzer types, ordered by name</returns>
    internal static IEnumerable<Type> DiscoverAnalyzerTypes()
    {
        return typeof(DiagnosticAnalyzerBase).Assembly
                                             .GetTypes()
                                             .Where(type => type.IsAbstract is false
                                                            && type.IsInterface is false
                                                            && typeof(DiagnosticAnalyzer).IsAssignableFrom(type)
                                                            && type.GetCustomAttribute<DiagnosticAnalyzerAttribute>() is not null)
                                             .OrderBy(type => type.Name, StringComparer.Ordinal);
    }

    /// <summary>
    /// Creates the analyzer whose type has the given simple name
    /// </summary>
    /// <param name="analyzerName">Simple name of the analyzer type</param>
    /// <returns>The created analyzer</returns>
    internal static DiagnosticAnalyzer CreateAnalyzer(string analyzerName)
    {
        var analyzerType = DiscoverAnalyzerTypes().Single(type => string.Equals(type.Name, analyzerName, StringComparison.Ordinal));

        return Activator.CreateInstance(analyzerType) as DiagnosticAnalyzer
                   ?? throw new InvalidOperationException($"Failed to create analyzer type '{analyzerType.FullName}'.");
    }

    #endregion // Methods
}