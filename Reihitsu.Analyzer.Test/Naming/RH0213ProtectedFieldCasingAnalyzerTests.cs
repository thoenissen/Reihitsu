using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0213ProtectedFieldCasingAnalyzer"/> and <see cref="RH0213ProtectedFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0213ProtectedFieldCasingAnalyzerTests : AnalyzerTestsBase<RH0213ProtectedFieldCasingAnalyzer, RH0213ProtectedFieldCasingCodeFixProvider>
{
    /// <summary>
    /// Verifies diagnostics are reported for protected fields that do not use _camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForProtectedFieldAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceBase
                                    {
                                        protected int {|#0:CacheCount|};

                                        public int GetCount()
                                        {
                                            return CacheCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceBase
                                     {
                                         protected int _cacheCount;

                                         public int GetCount()
                                         {
                                             return _cacheCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0213ProtectedFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0213MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for protected fields that already use _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForUnderlineCamelCaseProtectedField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceBase
                                    {
                                        protected int _cacheCount;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies protected internal fields are also covered by the protected field rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForProtectedInternalFieldWithoutUnderlinePrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceBase
                                    {
                                        protected internal int {|#0:CacheLimit|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceBase
                                     {
                                         protected internal int _cacheLimit;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0213ProtectedFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0213MessageFormat));
    }
}