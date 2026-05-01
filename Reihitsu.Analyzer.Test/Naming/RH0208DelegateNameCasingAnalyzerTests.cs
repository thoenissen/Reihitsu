using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0208DelegateNameCasingAnalyzer"/> and <see cref="RH0208DelegateNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0208DelegateNameCasingAnalyzerTests : AnalyzerTestsBase<RH0208DelegateNameCasingAnalyzer, RH0208DelegateNameCasingCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifying diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnostics()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    /// <summary>
                                    /// Test delegate
                                    /// </summary>
                                    /// <param name="sender">Sender</param>
                                    /// <param name="e">Event args</param>
                                    public delegate void {|#0:eventHandler|}(object sender, EventArgs e);
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     /// <summary>
                                     /// Test delegate
                                     /// </summary>
                                     /// <param name="sender">Sender</param>
                                     /// <param name="e">Event args</param>
                                     public delegate void EventHandler(object sender, EventArgs e);
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0208DelegateNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0208MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase delegate name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseDelegate()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public delegate void MessageHandler(string message);
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a delegate with a non-void return type and wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForDelegateWithReturnValueWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public delegate int {|#0:computeResult|}(int x, int y);
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public delegate int ComputeResult(int x, int y);
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0208DelegateNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0208MessageFormat));
    }

    /// <summary>
    /// Verifying no diagnostics for a PascalCase generic delegate
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForGenericDelegate()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public delegate void TransformAction<T>(T item);
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying diagnostics for a generic delegate with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForGenericDelegateWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public delegate void {|#0:transformAction|}<T>(T item);
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public delegate void TransformAction<T>(T item);
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0208DelegateNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0208MessageFormat));
    }

    /// <summary>
    /// Verifying diagnostics for an internal delegate with wrong casing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForInternalDelegateWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    internal delegate void {|#0:messageCallback|}(string msg);
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     internal delegate void MessageCallback(string msg);
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0208DelegateNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0208MessageFormat));
    }

    #endregion // Members
}