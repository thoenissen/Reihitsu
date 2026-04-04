using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0207EventNameCasingAnalyzer"/> and <see cref="RH0207EventNameCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0207EventNameCasingAnalyzerTests : AnalyzerTestsBase<RH0207EventNameCasingAnalyzer, RH0207EventNameCasingCodeFixProvider>
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
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;

            namespace Reihitsu.Analyzer.Test.Naming.Resources
            {
                /// <summary>
                /// Test class
                /// </summary>
                public class TestClass
                {
                    /// <summary>
                    /// Test event
                    /// </summary>
                    public event EventHandler {|#0:testEvent|};
                }
            }
            """;

        const string fixedCode = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            using System.Text;
            using System.Threading.Tasks;

            namespace Reihitsu.Analyzer.Test.Naming.Resources
            {
                /// <summary>
                /// Test class
                /// </summary>
                public class TestClass
                {
                    /// <summary>
                    /// Test event
                    /// </summary>
                    public event EventHandler TestEvent;
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0207EventNameCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0207MessageFormat));
    }
}