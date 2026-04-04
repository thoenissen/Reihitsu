using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0210LocalFunctionNameCasingAnalyzer"/> and <see cref="RH0210LocalFunctionNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0210LocalFunctionNameCasingAnalyzerTests : AnalyzerTestsBase<RH0210LocalFunctionNameCasingAnalyzer, RH0210LocalFunctionNameCasingCodeFixProvider>
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
                    public void Method()
                    {
                        void {|#0:testLocalFunction|}()
                        {
                        }
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
                    public void Method()
                    {
                        void TestLocalFunction()
                        {
                        }
                    }
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0210LocalFunctionNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0210MessageFormat));
    }
}