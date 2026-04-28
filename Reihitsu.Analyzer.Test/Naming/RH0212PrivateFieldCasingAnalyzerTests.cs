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
    /// Verifies diagnostics are reported for private fields that do not use _camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForPrivateFieldAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private int {|#0:cacheCount|};

                                        public int GetCount()
                                        {
                                            return cacheCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         private int _cacheCount;

                                         public int GetCount()
                                         {
                                             return _cacheCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0212PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0212MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a private field that already uses _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForUnderlineCamelCasePrivateField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private int _cacheCount;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies diagnostics are reported for private readonly fields that do not use _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForPrivateReadonlyFieldWithoutUnderlinePrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private readonly int {|#0:cacheLimit|} = 10;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         private readonly int _cacheLimit = 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0212PrivateFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0212MessageFormat));
    }

    /// <summary>
    /// Verifies private static readonly fields do not report diagnostics because RH0228 handles them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPrivateStaticReadonlyFieldsDoNotReportDiagnostics()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        private static readonly int CacheLimit = 10;
                                    }
                                }
                                """;

        await Verify(testCode);
    }
}