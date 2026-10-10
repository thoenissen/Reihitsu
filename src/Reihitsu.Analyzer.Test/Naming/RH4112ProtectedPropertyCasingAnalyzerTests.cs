using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4112ProtectedPropertyCasingAnalyzer"/> and <see cref="RH4112ProtectedPropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4112ProtectedPropertyCasingAnalyzerTests : BatchCodeFixTestsBase<RH4112ProtectedPropertyCasingAnalyzer, RH4112ProtectedPropertyCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for protected properties that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForProtectedPropertyAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        protected int {|#0:resourceCount|} { get; set; }

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
                                         protected int ResourceCount { get; set; }

                                         public int GetCount()
                                         {
                                             return ResourceCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4112ProtectedPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4112MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase protected properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseProtectedProperty()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        protected int ResourceCount { get; set; }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies protected internal properties are not claimed by the protected property rule because they are handled by the internal property rule (RH4113)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForProtectedInternalProperty()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        protected internal int resourceCount { get; set; }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies private protected properties are covered by the protected property rule and renamed to PascalCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForPrivateProtectedPropertyWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        private protected int {|#0:resourceCount|} { get; set; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceSettings
                                     {
                                         private protected int ResourceCount { get; set; }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4112ProtectedPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4112MessageFormat));
    }

    /// <summary>
    /// Verifies that implicit class and interface properties are owned by the private and public rules
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForImplicitProperties()
    {
        const string testCode = """
                                internal class ResourceSettings
                                {
                                    int classProperty { get; set; }
                                }

                                internal interface IResourceSettings
                                {
                                    int interfaceProperty { get; }
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
                                    public class BaseView
                                    {
                                        protected int {|#0:headerHeight|} { get; set; }

                                        protected int {|#1:footerHeight|} { get; set; }

                                        public int Total => headerHeight + footerHeight;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class BaseView
                                     {
                                         protected int HeaderHeight { get; set; }

                                         protected int FooterHeight { get; set; }

                                         public int Total => HeaderHeight + FooterHeight;
                                     }
                                 }
                                 """;

        // Both properties are read by the same expression body, so each rename has to reach a reference
        // outside its own declaration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4112ProtectedPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4112MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}