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
    [Requirement("RH4112-001")]
    [Requirement("RH4112-002")]
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
    [Requirement("RH4112-001")]
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
    [Requirement("RH4112-001")]
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
    [Requirement("RH4112-001")]
    [Requirement("RH4112-002")]
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
    [Requirement("RH4112-001")]
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

    /// <summary>
    /// Verifies protected, private protected, and static protected properties are reported, while PascalCase names and other accessibility groups are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4112-001")]
    public async Task ProtectedPropertiesOfEveryKindAreCheckedWhileOtherGroupsAreNot()
    {
        const string testCode = """
                                public class C
                                {
                                    protected int {|RH4112:count|} { get; set; }
                                    private protected int {|RH4112:offset|} { get; set; }
                                    protected static int {|RH4112:instance_count|} => 0;
                                    protected int Limit { get; set; }
                                    protected internal int scale { get; set; }
                                    int total { get; set; }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies abstract and virtual protected properties are reported, and so is an override whose original definition is declared in source
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4112-001")]
    public async Task AbstractAndVirtualProtectedPropertiesAreChecked()
    {
        const string testCode = """
                                public abstract class C
                                {
                                    protected abstract int {|RH4112:row_count|} { get; }
                                    protected virtual int {|RH4112:column_count|} => 0;
                                }

                                public class D : C
                                {
                                    protected override int {|RH4112:row_count|} => 1;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies each part of a partial protected property gets its own diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4112-001")]
    public async Task PartialProtectedPropertyIsReportedOnEachPart()
    {
        const string testCode = """
                                public partial class C
                                {
                                    protected partial int {|RH4112:count|} { get; }
                                }

                                public partial class C
                                {
                                    protected partial int {|RH4112:count|} => 0;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix renames protected properties and every reference to them to the derived PascalCase name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4112-002")]
    public async Task FixRenamesProtectedPropertiesAndTheirReferencesToPascalCase()
    {
        const string testCode = """
                                public class BaseView
                                {
                                    protected int {|RH4112:item_count|} { get; set; }

                                    protected int {|RH4112:offset|} => 0;

                                    public int Sum => item_count + offset;
                                }
                                """;

        const string fixedCode = """
                                 public class BaseView
                                 {
                                     protected int ItemCount { get; set; }

                                     protected int Offset => 0;

                                     public int Sum => ItemCount + Offset;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies renaming a protected property also renames the property it overrides and the property that overrides it in source
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4112-002")]
    public async Task FixRenamesOverriddenAndOverridingProtectedPropertiesInSource()
    {
        const string testCode = """
                                public class BaseView
                                {
                                    protected virtual int {|RH4112:row_count|} => 0;
                                }

                                public class GridView : BaseView
                                {
                                    protected override int {|RH4112:row_count|} => 1;
                                }
                                """;

        const string fixedCode = """
                                 public class BaseView
                                 {
                                     protected virtual int RowCount => 0;
                                 }

                                 public class GridView : BaseView
                                 {
                                     protected override int RowCount => 1;
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