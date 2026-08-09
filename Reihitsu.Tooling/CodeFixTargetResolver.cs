using System.Collections.Immutable;
using System.Reflection;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;

namespace Reihitsu.Tooling;

/// <summary>
/// Resolves a diagnostic ID to the shipped analyzer and code fix provider that own it
/// </summary>
public static class CodeFixTargetResolver
{
    #region Methods

    /// <summary>
    /// Resolves the analyzers and the code fix provider that own the provided diagnostic ID. The ID is the only
    /// stable public handle on these types: analyzer and code fix type names carry no compatibility contract,
    /// and the formatter's structural transforms are internal
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID to resolve</param>
    /// <param name="target">The resolved target when resolution succeeds</param>
    /// <param name="error">The reason resolution failed, naming every candidate that was considered</param>
    /// <returns><see langword="true"/> when exactly one code fix provider owns the ID; otherwise, <see langword="false"/></returns>
    public static bool TryResolve(string diagnosticId, out CodeFixTarget target, out string error)
    {
        target = null;
        error = null;

        var analyzers = CreateInstances<DiagnosticAnalyzer>(typeof(RH7101DoNotCombineFieldsAnalyzer).Assembly).Where(analyzer => analyzer.SupportedDiagnostics.Any(descriptor => descriptor.Id == diagnosticId))
                                                                                                              .ToImmutableArray();

        var providers = CreateInstances<CodeFixProvider>(typeof(RH7101DoNotCombineFieldsCodeFixProvider).Assembly).Where(provider => provider.FixableDiagnosticIds.Contains(diagnosticId))
                                                                                                                  .ToList();

        if (analyzers.Length == 0)
        {
            error = $"no analyzer reports '{diagnosticId}'.";

            return false;
        }

        if (providers.Count == 0)
        {
            error = $"no code fix provider declares '{diagnosticId}' as fixable.";

            return false;
        }

        if (providers.Count > 1)
        {
            var names = string.Join(", ", providers.Select(provider => provider.GetType().Name).OrderBy(name => name, StringComparer.Ordinal));

            error = $"'{diagnosticId}' is claimed by more than one code fix provider: {names}.";

            return false;
        }

        target = new CodeFixTarget(diagnosticId, analyzers, providers[0]);

        return true;
    }

    /// <summary>
    /// Creates one instance of every concrete public type in the assembly that derives from the requested type
    /// </summary>
    /// <typeparam name="T">Base type the instances derive from</typeparam>
    /// <param name="assembly">Assembly to scan</param>
    /// <returns>The created instances</returns>
    private static IEnumerable<T> CreateInstances<T>(Assembly assembly)
        where T : class
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract
                || type.IsPublic == false
                || typeof(T).IsAssignableFrom(type) == false
                || type.GetConstructor(Type.EmptyTypes) == null)
            {
                continue;
            }

            yield return (T)Activator.CreateInstance(type);
        }
    }

    #endregion // Methods
}