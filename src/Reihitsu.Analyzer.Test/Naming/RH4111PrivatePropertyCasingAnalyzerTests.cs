using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4111PrivatePropertyCasingAnalyzer"/> and <see cref="RH4111PrivatePropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4111PrivatePropertyCasingAnalyzerTests : BatchCodeFixTestsBase<RH4111PrivatePropertyCasingAnalyzer, RH4111PrivatePropertyCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for private properties that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-001")]
    [Requirement("RH4111-002")]
    public async Task VerifyDiagnosticsForPrivatePropertyAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        private int {|#0:resourceCount|} { get; set; }

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
                                         private int ResourceCount { get; set; }

                                         public int GetCount()
                                         {
                                             return ResourceCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4111PrivatePropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4111MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase private properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-001")]
    public async Task VerifyNoDiagnosticsForPascalCasePrivateProperty()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        private int ResourceCount { get; set; }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies expression-bodied private properties are also covered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-001")]
    [Requirement("RH4111-002")]
    public async Task VerifyDiagnosticsForExpressionBodiedPrivatePropertyWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        private int {|#0:resourceCount|} => 42;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceSettings
                                     {
                                         private int ResourceCount => 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4111PrivatePropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4111MessageFormat));
    }

    /// <summary>
    /// Verifies private protected properties are not claimed by the private property rule because they are handled by the protected property rule (RH4112)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-001")]
    public async Task VerifyNoDiagnosticsForPrivateProtectedProperty()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        private protected int resourceCount { get; set; }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a class property without an accessibility modifier is treated as effectively private
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-001")]
    [Requirement("RH4111-002")]
    public async Task VerifyImplicitlyPrivateClassPropertyIsDetectedAndFixed()
    {
        const string testCode = """
                                internal class ResourceSettings
                                {
                                    int {|#0:resourceCount|} { get; set; }
                                }
                                """;

        const string fixedCode = """
                                 internal class ResourceSettings
                                 {
                                     int ResourceCount { get; set; }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4111PrivatePropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4111MessageFormat));
    }

    /// <summary>
    /// Verifies that an implicit interface property is owned by the public property rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-001")]
    public async Task VerifyNoDiagnosticsForImplicitInterfaceProperty()
    {
        const string testCode = """
                                internal interface IResourceSettings
                                {
                                    int resourceCount { get; }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies an explicit-interface property implementation is not owned by the private property rule
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-001")]
    public async Task VerifyNoDiagnosticsForExplicitInterfacePropertyImplementation()
    {
        const string testCode = """
                                internal interface IResourceSettings
                                {
                                    int resourceCount { get; }
                                }

                                internal class ResourceSettings : IResourceSettings
                                {
                                    int IResourceSettings.resourceCount => 42;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a private property is checked, while an explicit interface property implementation belongs to no accessibility group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-004")]
    [Requirement("RH4111-001")]
    public async Task ExplicitInterfacePropertyImplementationBelongsToNoAccessibilityGroup()
    {
        const string testCode = """
                                public interface I
                                {
                                    int value { get; }
                                }

                                public class C : I
                                {
                                    private int {|RH4111:count|} { get; set; }

                                    int I.value => 0;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies static, expression-bodied, implicitly private, struct, and interface private properties are reported, while PascalCase names and other accessibility groups are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-001")]
    public async Task PrivatePropertiesOfEveryKindAreCheckedWhileOtherGroupsAreNot()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4111:count|} { get; set; }
                                    int {|RH4111:total|} => 0;
                                    private static int {|RH4111:cache_size|} { get; set; }
                                    private int Limit { get; set; }
                                    private protected int offset { get; set; }
                                    protected internal int scale { get; set; }
                                }

                                public struct S
                                {
                                    int {|RH4111:value|} { get; set; }
                                }

                                public interface I
                                {
                                    private int {|RH4111:hidden|} => 0;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies each part of a partial private property gets its own diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-001")]
    public async Task PartialPrivatePropertyIsReportedOnEachPart()
    {
        const string testCode = """
                                public partial class C
                                {
                                    private partial int {|RH4111:count|} { get; }
                                }

                                public partial class C
                                {
                                    private partial int {|RH4111:count|} => 0;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix renames private properties and every reference to them to the derived PascalCase name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4111-002")]
    public async Task FixRenamesPrivatePropertiesAndTheirReferencesToPascalCase()
    {
        const string testCode = """
                                public class C
                                {
                                    private int {|RH4111:count|} { get; set; }

                                    private int {|RH4111:max_size|} => 10;

                                    public int Sum => count + max_size;
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private int Count { get; set; }

                                     private int MaxSize => 10;

                                     public int Sum => Count + MaxSize;
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
                                    public class Cache
                                    {
                                        private int {|#0:itemCount|} { get; set; }

                                        private int {|#1:pageSize|} { get; set; }

                                        public int Total => itemCount * pageSize;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Cache
                                     {
                                         private int ItemCount { get; set; }

                                         private int PageSize { get; set; }

                                         public int Total => ItemCount * PageSize;
                                     }
                                 }
                                 """;

        // Both properties are read by the same expression body, so each rename has to reach a reference
        // outside its own declaration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4111PrivatePropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4111MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}