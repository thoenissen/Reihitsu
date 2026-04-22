using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0228StaticReadonlyFieldCasingAnalyzer"/> and <see cref="RH0228StaticReadonlyFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0228StaticReadonlyFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0228StaticReadonlyFieldCasingAnalyzer, RH0228StaticReadonlyFieldCasingCodeFixProvider>
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
                                        private static readonly int {|#0:_testField|} = 42;
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
                                         private static readonly int TestField = 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0228StaticReadonlyFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0228MessageFormat));
    }

    /// <summary>
    /// Verifying PascalCase static readonly fields are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPascalCaseStaticReadonlyFieldsDoNotReportDiagnostics()
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
                                        private static readonly int TestField = 42;
                                    }
                                }
                                """;

        await Verify(testCode, testCode);
    }
}