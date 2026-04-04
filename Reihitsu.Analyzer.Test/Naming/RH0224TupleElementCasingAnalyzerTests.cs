using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0224TupleElementCasingAnalyzer"/> and <see cref="RH0224TupleElementCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0224TupleElementCasingAnalyzerTests : AnalyzerTestsBase<RH0224TupleElementCasingAnalyzer, RH0224TupleElementCasingCodeFixProvider>
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
                    public void TestMethod()
                    {
                        var tuple = ({|#0:firstElement|}: 1, SecondElement: 2);
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
                    public void TestMethod()
                    {
                        var tuple = (FirstElement: 1, SecondElement: 2);
                    }
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0224TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0224MessageFormat));
    }
}