using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.SelfHosting.Test.Utilities;

/// <summary>
/// Runs one analyzer over a set of compilations and renders each source diagnostic it reports as one line naming the
/// file, the line, the rule, and the analyzer
/// </summary>
internal static class AnalyzerViolationReport
{
    #region Fields

    /// <summary>
    /// Diagnostic IDs that are temporarily excluded from the self-hosting gate
    /// </summary>
    private static readonly HashSet<string> _ignoredDiagnosticIds = new(StringComparer.Ordinal);

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Runs the analyzer over every compilation and collects its violations
    /// </summary>
    /// <param name="analyzer">The analyzer to run</param>
    /// <param name="compilations">The compilations to analyze</param>
    /// <param name="solutionRoot">Root that the reported file paths are made relative to</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>One line per violation, ordered by file and then by position, in the form <c>path:line [ID] (Analyzer) message</c></returns>
    internal static IReadOnlyList<string> Collect(DiagnosticAnalyzer analyzer,
                                                  IEnumerable<CSharpCompilation> compilations,
                                                  string solutionRoot,
                                                  CancellationToken cancellationToken)
    {
        var analyzerName = analyzer.GetType().Name;
        var diagnostics = new List<Diagnostic>();

        foreach (var compilation in compilations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var compilationWithAnalyzers = compilation.WithAnalyzers(ImmutableArray.Create(analyzer));

            diagnostics.AddRange(compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync(cancellationToken)
                                                         .GetAwaiter()
                                                         .GetResult()
                                                         .Where(diagnostic => diagnostic.Location.SourceTree != null
                                                                              && _ignoredDiagnosticIds.Contains(diagnostic.Id) == false));
        }

        return diagnostics.OrderBy(diagnostic => diagnostic.Location.SourceTree.FilePath, StringComparer.OrdinalIgnoreCase)
                          .ThenBy(diagnostic => diagnostic.Location.SourceSpan.Start)
                          .Select(diagnostic => Describe(diagnostic, analyzerName, solutionRoot))
                          .ToList();
    }

    /// <summary>
    /// Renders one diagnostic as a report line
    /// </summary>
    /// <param name="diagnostic">Diagnostic with a source location</param>
    /// <param name="analyzerName">Name of the analyzer that reported it</param>
    /// <param name="solutionRoot">Root that the file path is made relative to</param>
    /// <returns>The report line</returns>
    private static string Describe(Diagnostic diagnostic, string analyzerName, string solutionRoot)
    {
        var relativePath = Path.GetRelativePath(solutionRoot, diagnostic.Location.SourceTree.FilePath);
        var lineNumber = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1;

        return $"{relativePath}:{lineNumber} [{diagnostic.Id}] ({analyzerName}) {diagnostic.GetMessage()}";
    }

    #endregion // Methods
}