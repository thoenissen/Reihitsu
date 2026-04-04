using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0212PrivateFieldCasingAnalyzer"/> and <see cref="RH0212PrivateFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0212PrivateFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0212PrivateFieldCasingAnalyzer, RH0212PrivateFieldCasingCodeFixProvider>
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
                    /// Test private field
                    /// </summary>
                    private int {|#0:TestField|};
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
                    /// Test private field
                    /// </summary>
                    private int _testField;
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0212PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0212MessageFormat));
    }
}