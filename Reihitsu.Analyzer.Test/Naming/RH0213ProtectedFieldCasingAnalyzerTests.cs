using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0213ProtectedFieldCasingAnalyzer"/> and <see cref="RH0213ProtectedFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0213ProtectedFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0213ProtectedFieldCasingAnalyzer, RH0213ProtectedFieldCasingCodeFixProvider>
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
                    /// Test field
                    /// </summary>
                    protected int {|#0:TestField|};
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
                    /// Test field
                    /// </summary>
                    protected int _testField;
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0213ProtectedFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0213MessageFormat));
    }
}