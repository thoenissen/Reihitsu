using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0217PrivatePropertyCasingAnalyzer"/> and <see cref="RH0217PrivatePropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0217PrivatePropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0217PrivatePropertyCasingAnalyzer, RH0217PrivatePropertyCasingCodeFixProvider>
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
                /// Test class
                /// </summary>
                public class TestClass
                {
                    /// <summary>
                    /// Test property
                    /// </summary>
                    private int {|#0:property|} { get; set; }
                }
            }
            """;

        const string fixedCode = """
            using System;

            namespace Reihitsu.Analyzer.Test.Naming.Resources
            {
                /// <summary>
                /// Test class
                /// </summary>
                public class TestClass
                {
                    /// <summary>
                    /// Test property
                    /// </summary>
                    private int Property { get; set; }
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0217PrivatePropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0217MessageFormat));
    }
}