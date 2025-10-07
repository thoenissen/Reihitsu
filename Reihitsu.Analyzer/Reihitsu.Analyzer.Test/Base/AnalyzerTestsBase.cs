using System;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeFixes;
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
    /// <inheritdoc cref="CSharpAnalyzerVerifier{TAnalyzer}.Diagnostic()"/>
    protected static DiagnosticResult Diagnostic()
    {
        return CSharpAnalyzerVerifier<TAnalyzer>.Diagnostic();
    }

    /// <inheritdoc cref="CSharpAnalyzerVerifier{TAnalyzer}.Diagnostic(string)"/>
    protected static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return CSharpAnalyzerVerifier<TAnalyzer>.Diagnostic(diagnosticId);
    }

    /// <summary>
    /// Verifies the analyzer produces the specified diagnostics for the given source text.
    /// </summary>
    /// <param name="source">The source text to test, which may include markup syntax.</param>
    /// <param name="expected">The expected diagnostics. These diagnostics are in addition to any diagnostics defined in markup.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected static Task VerifyCodeFixAsync(string source, params DiagnosticResult[] expected)
    {
        return CSharpAnalyzerVerifier<TAnalyzer>.VerifyAnalyzerAsync(source, null, expected);
    }

    /// <summary>
    /// Verifies the analyzer produces the specified diagnostics for the given source text.
    /// </summary>
    /// <param name="source">The source text to test, which may include markup syntax.</param>
    /// <param name="onConfigure">Additional configuration of the test</param>
    /// <param name="expected">The expected diagnostics. These diagnostics are in addition to any diagnostics defined in markup.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected static Task VerifyCodeFixAsync(string source, Action<CSharpAnalyzerVerifierTest<TAnalyzer>> onConfigure, params DiagnosticResult[] expected)
    {
        return CSharpAnalyzerVerifier<TAnalyzer>.VerifyAnalyzerAsync(source, onConfigure, expected);
    }
}

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

    /// <inheritdoc cref="CSharpCodeFixVerifier{TAnalyzer, TCodeFix}.Diagnostic()"/>
    public static DiagnosticResult Diagnostic()
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Diagnostic();
    }

    /// <inheritdoc cref="CSharpCodeFixVerifier{TAnalyzer, TCodeFix}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Diagnostic(diagnosticId);
    }

    /// <inheritdoc cref="CSharpCodeFixVerifier{TAnalyzer, TCodeFix}.VerifyCodeFixAsync(string, string, DiagnosticResult[])"/>
    public static Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.VerifyCodeFixAsync(source, fixedSource, expected);
    }

    #endregion // Methods
}