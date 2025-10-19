using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Verifying analyzer and code fixers
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public class AnalyzerTestsBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    /// <summary>
    /// Creates a <see cref="DiagnosticResult"/> representing an expected diagnostic for the <em>single</em>
    /// </summary>
    /// <returns>A <see cref="DiagnosticResult"/> initialized using the single descriptor supported by the analyzer.</returns>
    protected static DiagnosticResult Diagnostic()
    {
        return CSharpAnalyzerVerifier<TAnalyzer, DefaultVerifier>.Diagnostic();
    }

    /// <summary>
    /// Creation of multiple diagnostics with <see cref="DiagnosticLocationOptions.InterpretAsMarkupKey"/>
    /// </summary>
    /// <param name="count">Count</param>
    /// <param name="message">Message</param>
    /// <returns>Diagnostics</returns>
    protected static DiagnosticResult[] Diagnostics(int count, string message)
    {
        var diagnostics = new DiagnosticResult[count];

        for (var index = 0; index < count; index++)
        {
            diagnostics[index] = Diagnostic().WithLocation(index, DiagnosticLocationOptions.InterpretAsMarkupKey)
                                             .WithMessage(message);
        }

        return diagnostics;
    }

    /// <summary>
    /// Verifies the analyzer produces the specified diagnostics for the given source text.
    /// </summary>
    /// <param name="source">The source text to test, which may include markup syntax.</param>
    /// <param name="expected">The expected diagnostics. These diagnostics are in addition to any diagnostics defined in markup.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected static async Task VerifyCodeFixAsync(string source, params DiagnosticResult[] expected)
    {
        await VerifyCodeFixAsync(source, null, expected);
    }

    /// <summary>
    /// Verifies the analyzer produces the specified diagnostics for the given source text.
    /// </summary>
    /// <param name="source">The source text to test, which may include markup syntax.</param>
    /// <param name="onConfigure">Additional configuration of the test</param>
    /// <param name="expected">The expected diagnostics. These diagnostics are in addition to any diagnostics defined in markup.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected static async Task VerifyCodeFixAsync(string source, Action<CSharpAnalyzerVerifierTest<TAnalyzer>> onConfigure, params DiagnosticResult[] expected)
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
}