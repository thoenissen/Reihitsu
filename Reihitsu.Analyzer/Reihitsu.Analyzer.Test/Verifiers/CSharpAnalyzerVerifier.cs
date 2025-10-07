using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace Reihitsu.Analyzer.Test.Verifiers;

/// <summary>
/// Verifying analyzers
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public static class CSharpAnalyzerVerifier<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    #region Methods

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic()"/>
    public static DiagnosticResult Diagnostic() => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic();

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId) => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="AnalyzerVerifier{TAnalyzer, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor) => CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic(descriptor);

    /// <summary>
    /// Verifies the analyzer produces the specified diagnostics for the given source text.
    /// </summary>
    /// <param name="source">The source text to test, which may include markup syntax.</param>
    /// <param name="onConfigure">Additional configuration of the test</param>
    /// <param name="expected">The expected diagnostics. These diagnostics are in addition to any diagnostics defined in markup.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task VerifyAnalyzerAsync(string source, Action<CSharpAnalyzerVerifierTest<TAnalyzer>> onConfigure = null, params DiagnosticResult[] expected)
    {
        var test = new CSharpAnalyzerVerifierTest<TAnalyzer>
                   {
                       TestCode = source,
                       ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                   };

        test.ExpectedDiagnostics.AddRange(expected);

        onConfigure?.Invoke(test);

        await test.RunAsync(CancellationToken.None);
    }

    #endregion // Methods
}