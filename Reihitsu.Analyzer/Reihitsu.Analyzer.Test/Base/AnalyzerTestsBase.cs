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
    public static DiagnosticResult Diagnostic() => CSharpAnalyzerVerifier<TAnalyzer>.Diagnostic();

    /// <inheritdoc cref="CSharpAnalyzerVerifier{TAnalyzer}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId) => CSharpAnalyzerVerifier<TAnalyzer>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="CSharpAnalyzerVerifier{TAnalyzer}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
    public static Task VerifyCodeFixAsync(string source, params DiagnosticResult[] expected) => CSharpAnalyzerVerifier<TAnalyzer>.VerifyAnalyzerAsync(source, expected);
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
    public static DiagnosticResult Diagnostic() => CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Diagnostic();

    /// <inheritdoc cref="CSharpCodeFixVerifier{TAnalyzer, TCodeFix}.Diagnostic(string)"/>
    public static DiagnosticResult Diagnostic(string diagnosticId) => CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Diagnostic(diagnosticId);

    /// <inheritdoc cref="CSharpCodeFixVerifier{TAnalyzer, TCodeFix}.VerifyCodeFixAsync(string, string, DiagnosticResult[])"/>
    public static Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected) => CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.VerifyCodeFixAsync(source, fixedSource, expected);

    #endregion // Methods
}