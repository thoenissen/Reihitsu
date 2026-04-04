using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0222TupleElementCasingAnalyzer"/> and <see cref="RH0222TupleElementCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0222TupleElementCasingAnalyzerTests : AnalyzerTestsBase<RH0222TupleElementCasingAnalyzer, RH0222TupleElementCasingCodeFixProvider>
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
                    /// Test method
                    /// </summary>
                    public (int {|#0:firstElement|}, int SecondElement) TestMethod()
                    {
                        return default;
                    }
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
                    /// Test method
                    /// </summary>
                    public (int FirstElement, int SecondElement) TestMethod()
                    {
                        return default;
                    }
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0222TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0222MessageFormat));
    }
}