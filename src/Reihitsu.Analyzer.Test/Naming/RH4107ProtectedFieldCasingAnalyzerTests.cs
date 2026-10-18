using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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
    [Requirement("RH4107-001")]
    [Requirement("RH4107-002")]
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
    [Requirement("RH4107-001")]
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
    [Requirement("RH4107-001")]
    [Requirement("SHARED-CASING-004")]
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
    [Requirement("RH4107-001")]
    [Requirement("RH4107-002")]
    [Requirement("SHARED-CASING-004")]
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
    [Requirement("RH4107-001")]
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
    /// Verifies that every declarator of protected, private protected, and static readonly fields is checked, while protected internal fields, constants, and implicitly private fields are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4107-001")]
    public async Task FieldsOfTheProtectedGroupThatAreNotConstantsAreChecked()
    {
        const string testCode = """
                                public class C
                                {
                                    protected int _cacheCount;
                                    protected int {|RH4107:CacheCount|}, {|RH4107:retryOffset|};
                                    private protected int {|RH4107:CacheLimit|};
                                    protected static readonly int {|RH4107:MaxSize|} = 1;
                                    protected internal int RetryLimit;
                                    protected const int Limit = 2;
                                    int cacheValue;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames both declarators of one protected field declaration to _camelCase together with their uses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4107-002")]
    public async Task FixRenamesEveryDeclaratorOfAProtectedFieldAndEveryReference()
    {
        const string testCode = """
                                public class BaseService
                                {
                                    protected int {|RH4107:CacheCount|}, {|RH4107:RetryOffset|};

                                    public int Sum => CacheCount + RetryOffset;
                                }
                                """;

        const string fixedCode = """
                                 public class BaseService
                                 {
                                     protected int _cacheCount, _retryOffset;

                                     public int Sum => _cacheCount + _retryOffset;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that Fix All skips two fields that would both be renamed to the same name and renames the other field, while the single fixes rename the first of the two and are then withheld for the second
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-010")]
    [Requirement("RH4107-002")]
    public async Task FixAllSkipsFieldsThatWouldReceiveTheSameNameAndRenamesTheOthers()
    {
        const string testCode = """
                                public class C
                                {
                                    protected int {|RH4107:Total|};

                                    protected int {|RH4107:total|};

                                    protected int {|RH4107:Limit|};

                                    public int Sum => Total + total + Limit;
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     protected int _total;

                                     protected int {|RH4107:total|};

                                     protected int _limit;

                                     public int Sum => _total + total + _limit;
                                 }
                                 """;

        const string batchFixedCode = """
                                      public class C
                                      {
                                          protected int {|RH4107:Total|};

                                          protected int {|RH4107:total|};

                                          protected int _limit;

                                          public int Sum => Total + total + _limit;
                                      }
                                      """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.BatchFixedCode = batchFixedCode;

                         // The diagnostics that keep no fix are marked in the expected sources
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;

                         // The skipped group keeps the Fix All loop running for a second pass that changes nothing
                         test.NumberOfFixAllIterations = -2;
                     });
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