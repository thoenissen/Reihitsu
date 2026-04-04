using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0205EnumMemberCasingAnalyzer"/> and <see cref="RH0205EnumMemberCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0205EnumMemberCasingAnalyzerTests : AnalyzerTestsBase<RH0205EnumMemberCasingAnalyzer, RH0205EnumMemberCasingCodeFixProvider>
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
                /// Test enum
                /// </summary>
                public enum TestEnum
                {
                    /// <summary>
                    /// Member
                    /// </summary>
                    {|#0:member|}
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
                /// Test enum
                /// </summary>
                public enum TestEnum
                {
                    /// <summary>
                    /// Member
                    /// </summary>
                    Member
                }
            }
            """;

        await Verify(testCode, fixedCode, Diagnostics(RH0205EnumMemberCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0205MessageFormat));
    }
}