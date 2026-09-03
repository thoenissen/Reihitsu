using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4108InternalFieldCasingAnalyzer"/> and <see cref="RH4108InternalFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4108InternalFieldCasingAnalyzerTests : BatchCodeFixTestsBase<RH4108InternalFieldCasingAnalyzer, RH4108InternalFieldCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for internal fields that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH4108InternalFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4108MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase internal fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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
    /// Verifies internal readonly fields are also covered by the internal field rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH4108InternalFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4108MessageFormat));
    }

    /// <summary>
    /// Verifies protected internal fields are covered by the internal field rule and renamed to PascalCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForProtectedInternalFieldWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        protected internal int {|#0:cacheLimit|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         protected internal int CacheLimit;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4108InternalFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4108MessageFormat));
    }

    /// <summary>
    /// Verifies that an implicitly private field is not claimed by the internal field rule
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
                                    public class Registry
                                    {
                                        internal int {|#0:itemCount|};

                                        internal int {|#1:pageSize|} = 10;

                                        public int Total => itemCount * pageSize;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Registry
                                     {
                                         internal int ItemCount;

                                         internal int PageSize = 10;

                                         public int Total => ItemCount * PageSize;
                                     }
                                 }
                                 """;

        // Both fields are read by the same expression body, so each rename has to reach a reference outside
        // its own declaration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4108InternalFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4108MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}