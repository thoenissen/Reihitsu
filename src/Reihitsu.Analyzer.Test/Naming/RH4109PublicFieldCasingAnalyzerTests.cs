using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4109PublicFieldCasingAnalyzer"/> and <see cref="RH4109PublicFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4109PublicFieldCasingAnalyzerTests : BatchCodeFixTestsBase<RH4109PublicFieldCasingAnalyzer, RH4109PublicFieldCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for public fields that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
    [Requirement("RH4109-002")]
    public async Task VerifyDiagnosticsForPublicFieldAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        public int {|#0:cacheCount|};

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
                                         public int CacheCount;

                                         public int GetCount()
                                         {
                                             return CacheCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4109PublicFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4109MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase public fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
    public async Task VerifyNoDiagnosticsForPascalCasePublicField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        public int CacheCount;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies public readonly fields are also covered by the public field rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
    [Requirement("RH4109-002")]
    public async Task VerifyDiagnosticsForPublicReadonlyFieldWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceCache
                                    {
                                        public readonly int {|#0:cacheLimit|} = 10;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceCache
                                     {
                                         public readonly int CacheLimit = 10;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4109PublicFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4109MessageFormat));
    }

    /// <summary>
    /// Verifies that an implicitly private field is not claimed by the public field rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
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
    /// Verifies that a public field of a nested private class and a static interface field without modifier belong to the public group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-004")]
    [Requirement("RH4109-001")]
    public async Task ContainingTypeDoesNotChangeTheAccessibilityGroup()
    {
        const string testCode = """
                                public class C
                                {
                                    private class Nested
                                    {
                                        public int {|RH4109:value|};
                                    }
                                }

                                public interface I
                                {
                                    static int {|RH4109:shared|};
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that every declarator of public, public readonly, and public static fields is checked, while constants and implicitly private fields are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-001")]
    public async Task FieldsOfThePublicGroupThatAreNotConstantsAreChecked()
    {
        const string testCode = """
                                public class C
                                {
                                    public int CacheCount;
                                    public int {|RH4109:itemCount|}, {|RH4109:pageSize|};
                                    public readonly int {|RH4109:cacheLimit|} = 10;
                                    public static int {|RH4109:instances|};
                                    public const int limit = 2;
                                    int cacheValue;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames a public field to PascalCase together with its use, and is withheld for a field whose new name is already taken
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4109-002")]
    [Requirement("SHARED-CASING-009")]
    public async Task FixRenamesPublicFieldAndWithholdsTheFixWhenTheNewNameIsTaken()
    {
        const string testCode = """
                                public class Invoice
                                {
                                    public int Amount;

                                    public int {|RH4109:amount|};

                                    public int {|RH4109:taxRate|};

                                    public int Total => Amount + amount + taxRate;
                                }
                                """;

        const string fixedCode = """
                                 public class Invoice
                                 {
                                     public int Amount;

                                     public int {|RH4109:amount|};

                                     public int TaxRate;

                                     public int Total => Amount + amount + TaxRate;
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         // The diagnostic that keeps no fix is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;

                         // The withheld field keeps the Fix All loop running for a second pass that changes nothing
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
                                    public class Invoice
                                    {
                                        public int Amount;

                                        public int {|#0:amount|};

                                        public int {|#1:taxRate|};

                                        public int Total => Amount + amount + taxRate;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Invoice
                                     {
                                         public int Amount;

                                         public int {|#0:amount|};

                                         public int TaxRate;

                                         public int Total => Amount + amount + TaxRate;
                                     }
                                 }
                                 """;

        // The target name of the first reported field is already taken by a correctly cased sibling, so that
        // rename is refused while the unrelated second field is still corrected
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4109PublicFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4109MessageFormat, 2),
                                  config =>
                                  {
                                      // "amount" cannot be corrected because the correctly cased sibling already owns the target name,
                                      // so its diagnostic survives. Both paths refuse it through the same conflict check, so the batch
                                      // result needs no separate expectation
                                      config.FixedState.ExpectedDiagnostics.Add(Diagnostic(RH4109PublicFieldCasingAnalyzer.DiagnosticId).WithLocation(0, DiagnosticLocationOptions.InterpretAsMarkupKey).WithMessage(AnalyzerResources.RH4109MessageFormat));

                                      // The refused candidate keeps the batch loop running for a second, no-op pass
                                      config.NumberOfFixAllIterations = -2;
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}