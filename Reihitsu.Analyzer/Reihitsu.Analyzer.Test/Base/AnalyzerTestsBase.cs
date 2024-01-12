using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Base
{
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

        /// <inheritdoc cref="CodeFixVerifier{TAnalyzer,TCodeFix,TTest,TVerifier}.Diagnostic()"/>
        public static DiagnosticResult Diagnostic() => CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Diagnostic();

        /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(string)"/>
        public static DiagnosticResult Diagnostic(string diagnosticId) => CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.Diagnostic(diagnosticId);

        /// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)"/>
        public static Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected) => CSharpCodeFixVerifier<TAnalyzer, TCodeFix>.VerifyCodeFixAsync(source, fixedSource, expected);

        #endregion // Methods
    }
}
