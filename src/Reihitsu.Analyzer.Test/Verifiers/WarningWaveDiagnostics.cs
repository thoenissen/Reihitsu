using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Reihitsu.Analyzer.Test.Verifiers;

/// <summary>
/// Finds the warning-wave diagnostics a code fix writes into its result. A warning wave is a set of compiler warnings
/// that a project only sees from a warning level above 4 (CS8848, CS8981, …). The verifiers compile every state at the
/// default warning level 4 and compare only errors, so without this check a fix that writes such a warning passes its
/// own tests. Only what the fix adds counts: a warning-wave diagnostic the source state already holds, and an ordinary
/// warning of level 1 to 4 a fix writes on purpose, pass
/// </summary>
internal static class WarningWaveDiagnostics
{
    #region Constants

    /// <summary>
    /// The warning level both states are compiled at: every warning wave the compiler knows, also the waves of a later
    /// compiler, as a project with <c>AnalysisLevel</c> <c>preview</c> sees them
    /// </summary>
    internal const int WarningLevel = 9999;

    /// <summary>
    /// The highest warning level that holds no warning wave; every diagnostic of a higher level belongs to a wave
    /// </summary>
    private const int HighestWarningLevelWithoutWaves = 4;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Gets the warning-wave diagnostics the fixed solution holds more often than the source solution, counted per
    /// diagnostic ID over every project
    /// </summary>
    /// <param name="sourceSolution">Solution of the source state</param>
    /// <param name="fixedSolution">Solution of the fixed state</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The diagnostics of every ID the fix adds, in the fixed solution; empty when the fix adds none</returns>
    internal static async Task<ImmutableArray<Diagnostic>> GetIntroducedAsync(Solution sourceSolution, Solution fixedSolution, CancellationToken cancellationToken)
    {
        var sourceDiagnostics = await GetAsync(sourceSolution, cancellationToken).ConfigureAwait(false);
        var fixedDiagnostics = await GetAsync(fixedSolution, cancellationToken).ConfigureAwait(false);
        var sourceCounts = sourceDiagnostics.GroupBy(diagnostic => diagnostic.Id)
                                            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        return [
                   ..fixedDiagnostics.GroupBy(diagnostic => diagnostic.Id)
                                     .Where(group => group.Count() > sourceCounts.GetValueOrDefault(group.Key))
                                     .SelectMany(group => group)
               ];
    }

    /// <summary>
    /// Creates the failure message for the warning-wave diagnostics a code fix adds
    /// </summary>
    /// <param name="state">Name of the fixed state</param>
    /// <param name="introduced">Diagnostics the fix adds</param>
    /// <returns>The message</returns>
    internal static string CreateMessage(string state, ImmutableArray<Diagnostic> introduced)
    {
        return $"The code fix writes warning-wave diagnostics into the {state} that the source state does not hold (warning level {WarningLevel}):{Environment.NewLine}"
               + string.Join(Environment.NewLine, introduced.Select(diagnostic => diagnostic.ToString()));
    }

    /// <summary>
    /// Gets the warning-wave diagnostics of every project of the solution, compiled at <see cref="WarningLevel"/>
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The warning-wave diagnostics</returns>
    private static async Task<List<Diagnostic>> GetAsync(Solution solution, CancellationToken cancellationToken)
    {
        var diagnostics = new List<Diagnostic>();

        foreach (var project in solution.Projects)
        {
            var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);

            if (compilation?.Options is CSharpCompilationOptions options)
            {
                diagnostics.AddRange(compilation.WithOptions(options.WithWarningLevel(WarningLevel))
                                                .GetDiagnostics(cancellationToken)
                                                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning
                                                                     && diagnostic.WarningLevel > HighestWarningLevelWithoutWaves
                                                                     && diagnostic.IsSuppressed == false));
            }
        }

        return diagnostics;
    }

    #endregion // Methods
}