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
}