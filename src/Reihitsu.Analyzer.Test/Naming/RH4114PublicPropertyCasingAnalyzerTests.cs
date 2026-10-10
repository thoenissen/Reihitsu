using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4114PublicPropertyCasingAnalyzer"/> and <see cref="RH4114PublicPropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4114PublicPropertyCasingAnalyzerTests : BatchCodeFixTestsBase<RH4114PublicPropertyCasingAnalyzer, RH4114PublicPropertyCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for public properties that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForPublicPropertyAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        public int {|#0:resourceCount|} { get; set; }

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
                                         public int ResourceCount { get; set; }

                                         public int GetCount()
                                         {
                                             return ResourceCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4114PublicPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4114MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase public properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCasePublicProperty()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        public int ResourceCount { get; set; }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies expression-bodied public properties are also covered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForExpressionBodiedPublicPropertyWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        public int {|#0:resourceCount|} => 42;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceSettings
                                     {
                                         public int ResourceCount => 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4114PublicPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4114MessageFormat));
    }

    /// <summary>
    /// Verifies that an interface property without an accessibility modifier is treated as effectively public
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyImplicitlyPublicInterfacePropertyIsDetectedAndFixed()
    {
        const string testCode = """
                                internal interface IResourceSettings
                                {
                                    int {|#0:resourceCount|} { get; }
                                }
                                """;

        const string fixedCode = """
                                 internal interface IResourceSettings
                                 {
                                     int ResourceCount { get; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4114PublicPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4114MessageFormat));
    }

    /// <summary>
    /// Verifies that an implicit class property is owned by the private property rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForImplicitClassProperty()
    {
        const string testCode = """
                                internal class ResourceSettings
                                {
                                    int resourceCount { get; set; }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the interface declaration owns casing and its explicit implementation is renamed through the symbol
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInterfacePropertyOwnsExplicitImplementationCasing()
    {
        const string testCode = """
                                internal interface IResourceSettings
                                {
                                    int {|#0:resourceCount|} { get; }
                                }

                                internal class ResourceSettings : IResourceSettings
                                {
                                    int IResourceSettings.resourceCount => 42;
                                }
                                """;

        const string fixedCode = """
                                 internal interface IResourceSettings
                                 {
                                     int ResourceCount { get; }
                                 }

                                 internal class ResourceSettings : IResourceSettings
                                 {
                                     int IResourceSettings.ResourceCount => 42;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4114PublicPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4114MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class Message
                                    {
                                        public string {|#0:subject|} { get; set; }

                                        public string {|#1:body|} { get; set; }

                                        public int Length => subject.Length + body.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Message
                                     {
                                         public string Subject { get; set; }

                                         public string Body { get; set; }

                                         public int Length => Subject.Length + Body.Length;
                                     }
                                 }
                                 """;

        // Both properties are read by the same expression body, so each rename has to reach a reference
        // outside its own declaration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4114PublicPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4114MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}