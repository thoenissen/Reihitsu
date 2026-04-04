using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0215PublicFieldCasingAnalyzer"/> and <see cref="RH0215PublicFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0215PublicFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0215PublicFieldCasingAnalyzer, RH0215PublicFieldCasingCodeFixProvider>
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
                    public int {|#0:testField|};
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
                    public int TestField;
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0215PublicFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0215MessageFormat));
    }
}