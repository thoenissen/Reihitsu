using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0217PrivatePropertyCasingAnalyzer"/> and <see cref="RH0217PrivatePropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0217PrivatePropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0217PrivatePropertyCasingAnalyzer, RH0217PrivatePropertyCasingCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported for private properties that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForPrivatePropertyAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        private int {|#0:resourceCount|} { get; set; }

                                        public int GetCount()
                                        {
                                            return resourceCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceSettings
                                     {
                                         private int ResourceCount { get; set; }

                                         public int GetCount()
                                         {
                                             return ResourceCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0217PrivatePropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0217MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase private properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCasePrivateProperty()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        private int ResourceCount { get; set; }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies expression-bodied private properties are also covered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForExpressionBodiedPrivatePropertyWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        private int {|#0:resourceCount|} => 42;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceSettings
                                     {
                                         private int ResourceCount => 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0217PrivatePropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0217MessageFormat));
    }
}