using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Verifying analyzer and code fixers
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
/// <typeparam name="TCodeFix">Type of the code fixer</typeparam>
public class AnalyzerTestsBase<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    #region Methods

    /// <summary>
    /// Creates a <see cref="DiagnosticResult"/> representing an expected diagnostic for the <em>single</em>
    /// </summary>
    /// <returns>A <see cref="DiagnosticResult"/> initialized using the single descriptor supported by the analyzer.</returns>
    protected static DiagnosticResult Diagnostic()
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic();
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
            diagnostics[index] = Diagnostic().WithLocation(index, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(message);
        }

        return diagnostics;
    }

    /// <summary>
    /// Verifies the analyzer provides diagnostics which, in combination with the code fix, produce the expected
    /// fixed code.
    /// </summary>
    /// <param name="source">The source text to test, which may include markup syntax.</param>
    /// <param name="fixedSource">The expected fixed source text. Any remaining diagnostics are defined in markup.</param>
    /// <param name="expected">The expected diagnostics. These diagnostics are in addition to any diagnostics defined in markup.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected static async Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        var test = new CSharpCodeFixVerifierTest<TAnalyzer, TCodeFix>
                   {
                       TestCode = source,
                       FixedCode = fixedSource,
                       ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                   };

        test.ExpectedDiagnostics.AddRange(expected);

        await test.RunAsync(CancellationToken.None);
    }

    #endregion // Methods
}