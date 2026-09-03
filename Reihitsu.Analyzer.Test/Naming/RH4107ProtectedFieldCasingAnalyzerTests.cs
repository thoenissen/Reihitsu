using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4107ProtectedFieldCasingAnalyzer"/> and <see cref="RH4107ProtectedFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4107ProtectedFieldCasingAnalyzerTests : BatchCodeFixTestsBase<RH4107ProtectedFieldCasingAnalyzer, RH4107ProtectedFieldCasingCodeFixProvider>
{
    #region Tests

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

        await Verify(testCode, fixedCode, Diagnostics(RH4107ProtectedFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4107MessageFormat));
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
    /// Verifies protected internal fields are not claimed by the protected field rule because they are handled by the internal field rule (RH4108)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForProtectedInternalField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceBase
                                    {
                                        protected internal int CacheLimit;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies private protected fields are covered by the protected field rule and renamed to _camelCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForPrivateProtectedFieldWithoutUnderlinePrefix()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceBase
                                    {
                                        private protected int {|#0:CacheLimit|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceBase
                                     {
                                         private protected int _cacheLimit;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4107ProtectedFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4107MessageFormat));
    }

    /// <summary>
    /// Verifies that an implicitly private field is not claimed by the protected field rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForImplicitlyPrivateField()
    {
        const string testCode = """
                                internal class ResourceCache
                                {
                                    int cacheCount;
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class BaseService
                                    {
                                        protected int {|#0:CacheCount|}, {|#1:RetryOffset|};

                                        public int Sum => CacheCount + RetryOffset;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class BaseService
                                     {
                                         protected int _cacheCount, _retryOffset;

                                         public int Sum => _cacheCount + _retryOffset;
                                     }
                                 }
                                 """;

        // Both fields are declarators of one field declaration, so the two renames are applied to the same
        // variable list
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4107ProtectedFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4107MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}