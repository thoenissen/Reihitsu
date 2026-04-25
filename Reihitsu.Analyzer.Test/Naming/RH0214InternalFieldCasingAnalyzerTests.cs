using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0214InternalFieldCasingAnalyzer"/> and <see cref="RH0214InternalFieldCasingCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0214InternalFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0214InternalFieldCasingAnalyzer, RH0214InternalFieldCasingCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported for internal fields that are not PascalCase and that references are renamed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForInternalFieldAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        internal int {|#0:cacheCount|};

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
                                         internal int CacheCount;

                                         public int GetCount()
                                         {
                                             return CacheCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0214InternalFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0214MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase internal fields.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseInternalField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        internal int CacheCount;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies internal readonly fields are also covered by the internal field rule.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForInternalReadonlyFieldWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        internal readonly int {|#0:cacheLimit|} = 10;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         internal readonly int CacheLimit = 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0214InternalFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0214MessageFormat));
    }
}