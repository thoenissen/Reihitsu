using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0220PublicPropertyCasingAnalyzer"/> and <see cref="RH0220PublicPropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0220PublicPropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0220PublicPropertyCasingAnalyzer, RH0220PublicPropertyCasingCodeFixProvider>
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
                    public int {|#0:property|} { get; set; }
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
                    public int Property { get; set; }
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0220PublicPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0220MessageFormat));
    }
}