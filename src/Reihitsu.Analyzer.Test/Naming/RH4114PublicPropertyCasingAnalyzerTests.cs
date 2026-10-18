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
    [Requirement("RH4114-001")]
    [Requirement("RH4114-002")]
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
    [Requirement("RH4114-001")]
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
    [Requirement("RH4114-001")]
    [Requirement("RH4114-002")]
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
    [Requirement("RH4114-001")]
    [Requirement("RH4114-002")]
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
    [Requirement("RH4114-001")]
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
    [Requirement("RH4114-001")]
    [Requirement("RH4114-002")]
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

    /// <summary>
    /// Verifies interface, static interface, public, and static public properties are reported, while PascalCase names, explicit implementations, and other accessibility groups are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4114-001")]
    public async Task PublicPropertiesOfEveryKindAreCheckedWhileOtherGroupsAreNot()
    {
        const string testCode = """
                                public interface I
                                {
                                    int {|RH4114:value|} { get; }
                                    static int {|RH4114:shared|} { get; set; }
                                }

                                public class C : I
                                {
                                    public int {|RH4114:count|} { get; set; }
                                    public static int {|RH4114:instance_count|} => 0;
                                    public int Limit { get; set; }
                                    int I.value => 0;
                                    int total { get; set; }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies abstract and virtual public properties are reported, and so are overrides and implicit interface implementations whose original definition is declared in source
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4114-001")]
    public async Task AbstractVirtualOverridingAndImplementingPublicPropertiesAreChecked()
    {
        const string testCode = """
                                public interface I
                                {
                                    int {|RH4114:size|} { get; }
                                }

                                public abstract class C
                                {
                                    public abstract int {|RH4114:row_count|} { get; }
                                    public virtual int {|RH4114:column_count|} => 0;
                                }

                                public class D : C, I
                                {
                                    public override int {|RH4114:row_count|} => 1;
                                    public int {|RH4114:size|} => 2;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies each part of a partial public property gets its own diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4114-001")]
    public async Task PartialPublicPropertyIsReportedOnEachPart()
    {
        const string testCode = """
                                public partial class C
                                {
                                    public partial int {|RH4114:count|} { get; }
                                }

                                public partial class C
                                {
                                    public partial int {|RH4114:count|} => 0;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix renames public properties, every reference to them, and the explicit implementations of an interface property to the derived PascalCase name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4114-002")]
    public async Task FixRenamesPublicPropertiesTheirReferencesAndExplicitImplementationsToPascalCase()
    {
        const string testCode = """
                                public interface IResourceSettings
                                {
                                    int {|RH4114:resourceCount|} { get; }
                                }

                                public class ResourceSettings : IResourceSettings
                                {
                                    int IResourceSettings.resourceCount => 42;

                                    public string {|RH4114:display_name|} { get; set; }

                                    public int Length => display_name.Length;
                                }
                                """;

        const string fixedCode = """
                                 public interface IResourceSettings
                                 {
                                     int ResourceCount { get; }
                                 }

                                 public class ResourceSettings : IResourceSettings
                                 {
                                     int IResourceSettings.ResourceCount => 42;

                                     public string DisplayName { get; set; }

                                     public int Length => DisplayName.Length;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies renaming a public property also renames its implicit implementation or interface property and its overridden or overriding property in source
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4114-002")]
    public async Task FixRenamesImplicitImplementationsAndOverridesOfPublicPropertiesInSource()
    {
        const string testCode = """
                                public interface I
                                {
                                    int {|RH4114:value|} { get; }
                                }

                                public class C : I
                                {
                                    public int {|RH4114:value|} => 0;
                                }

                                public class B
                                {
                                    public virtual int {|RH4114:size|} => 0;
                                }

                                public class D : B
                                {
                                    public override int {|RH4114:size|} => 1;
                                }
                                """;

        const string fixedCode = """
                                 public interface I
                                 {
                                     int Value { get; }
                                 }

                                 public class C : I
                                 {
                                     public int Value => 0;
                                 }

                                 public class B
                                 {
                                     public virtual int Size => 0;
                                 }

                                 public class D : B
                                 {
                                     public override int Size => 1;
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