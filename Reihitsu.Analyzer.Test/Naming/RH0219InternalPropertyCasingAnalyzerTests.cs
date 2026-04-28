using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0219InternalPropertyCasingAnalyzer"/> and <see cref="RH0219InternalPropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0219InternalPropertyCasingAnalyzerTests : AnalyzerTestsBase<RH0219InternalPropertyCasingAnalyzer, RH0219InternalPropertyCasingCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported for internal properties that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForInternalPropertyAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        internal int {|#0:resourceCount|} { get; set; }

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
                                         internal int ResourceCount { get; set; }

                                         public int GetCount()
                                         {
                                             return ResourceCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0219InternalPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0219MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase internal properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseInternalProperty()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        internal int ResourceCount { get; set; }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies expression-bodied internal properties are also covered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForExpressionBodiedInternalPropertyWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        internal int {|#0:resourceCount|} => 42;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceSettings
                                     {
                                         internal int ResourceCount => 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0219InternalPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0219MessageFormat));
    }
}