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
    [Requirement("RH4108-001")]
    [Requirement("RH4108-002")]
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
    [Requirement("RH4108-001")]
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
    [Requirement("RH4108-001")]
    [Requirement("RH4108-002")]
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
    [Requirement("RH4108-001")]
    [Requirement("RH4108-002")]
    [Requirement("SHARED-CASING-004")]
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
    [Requirement("RH4108-001")]
    [Requirement("SHARED-CASING-004")]
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

    /// <summary>
    /// Verifies that internal and protected internal fields belong to the internal group, while private protected, protected, public, and implicitly private fields do not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-004")]
    [Requirement("RH4108-001")]
    public async Task AccessibilityGroupIsDecidedByTheModifiersOfTheField()
    {
        const string testCode = """
                                public class C
                                {
                                    internal int {|RH4108:a|};
                                    protected internal int {|RH4108:b|};
                                    private protected int c;
                                    protected int d;
                                    public int e;
                                    int f;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that every declarator of internal, internal readonly, and protected internal fields is checked, while constants and implicitly private fields are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4108-001")]
    public async Task FieldsOfTheInternalGroupThatAreNotConstantsAreChecked()
    {
        const string testCode = """
                                public class C
                                {
                                    internal int CacheCount;
                                    internal int {|RH4108:itemCount|}, {|RH4108:pageSize|};
                                    internal readonly int {|RH4108:cacheLimit|} = 10;
                                    protected internal int {|RH4108:retryLimit|};
                                    internal const int limit = 2;
                                    int cacheValue;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames internal fields to PascalCase together with their uses, joining words separated by an underscore
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4108-002")]
    public async Task FixRenamesInternalFieldsAndEveryReference()
    {
        const string testCode = """
                                public class Registry
                                {
                                    internal int {|RH4108:itemCount|};
                                    internal int {|RH4108:page_size|} = 10;

                                    public int Total => itemCount * page_size;
                                }
                                """;

        const string fixedCode = """
                                 public class Registry
                                 {
                                     internal int ItemCount;
                                     internal int PageSize = 10;

                                     public int Total => ItemCount * PageSize;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
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