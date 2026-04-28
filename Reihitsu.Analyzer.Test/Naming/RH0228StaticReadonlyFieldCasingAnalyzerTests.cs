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
    /// Verifies diagnostics are reported for static readonly fields that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForStaticReadonlyFieldAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class CacheDefaults
                                    {
                                        private static readonly int {|#0:_cacheLimit|} = 10;

                                        public int GetLimit()
                                        {
                                            return _cacheLimit;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class CacheDefaults
                                     {
                                         private static readonly int CacheLimit = 10;

                                         public int GetLimit()
                                         {
                                             return CacheLimit;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0228StaticReadonlyFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0228MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase static readonly fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseStaticReadonlyField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class CacheDefaults
                                    {
                                        private static readonly int CacheLimit = 10;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies non-readonly static fields do not report diagnostics because other rules handle them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStaticFieldsWithoutReadonlyDoNotReportDiagnostics()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class CacheDefaults
                                    {
                                        private static int cacheLimit = 10;
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}