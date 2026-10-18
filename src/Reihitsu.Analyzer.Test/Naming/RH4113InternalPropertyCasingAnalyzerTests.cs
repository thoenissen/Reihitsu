using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4113InternalPropertyCasingAnalyzer"/> and <see cref="RH4113InternalPropertyCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4113InternalPropertyCasingAnalyzerTests : BatchCodeFixTestsBase<RH4113InternalPropertyCasingAnalyzer, RH4113InternalPropertyCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for internal properties that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-001")]
    [Requirement("RH4113-002")]
    public async Task VerifyDiagnosticsForInternalPropertyAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        internal int {|#0:resourceCount|} { get; set; }

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
                                         internal int ResourceCount { get; set; }

                                         public int GetCount()
                                         {
                                             return ResourceCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4113InternalPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4113MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase internal properties
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-001")]
    public async Task VerifyNoDiagnosticsForPascalCaseInternalProperty()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        internal int ResourceCount { get; set; }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies expression-bodied internal properties are also covered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-001")]
    [Requirement("RH4113-002")]
    public async Task VerifyDiagnosticsForExpressionBodiedInternalPropertyWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        internal int {|#0:resourceCount|} => 42;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceSettings
                                     {
                                         internal int ResourceCount => 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4113InternalPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4113MessageFormat));
    }

    /// <summary>
    /// Verifies protected internal properties are covered by the internal property rule and renamed to PascalCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-001")]
    [Requirement("RH4113-002")]
    public async Task VerifyDiagnosticsForProtectedInternalPropertyWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class ResourceSettings
                                    {
                                        protected internal int {|#0:resourceCount|} { get; set; }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class ResourceSettings
                                     {
                                         protected internal int ResourceCount { get; set; }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4113InternalPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4113MessageFormat));
    }

    /// <summary>
    /// Verifies that implicit class and interface properties are owned by the private and public rules
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-001")]
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
    /// Verifies internal, protected internal, and static internal properties are reported, while PascalCase names and other accessibility groups are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-001")]
    public async Task InternalPropertiesOfEveryKindAreCheckedWhileOtherGroupsAreNot()
    {
        const string testCode = """
                                public class C
                                {
                                    internal int {|RH4113:count|} { get; set; }
                                    protected internal int {|RH4113:scale|} { get; set; }
                                    internal static int {|RH4113:instance_count|} => 0;
                                    internal int Limit { get; set; }
                                    private protected int offset { get; set; }
                                    int total { get; set; }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies abstract and virtual internal properties are reported, and so is an override whose original definition is declared in source
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-001")]
    public async Task AbstractAndVirtualInternalPropertiesAreChecked()
    {
        const string testCode = """
                                public abstract class C
                                {
                                    internal abstract int {|RH4113:row_count|} { get; }
                                    internal virtual int {|RH4113:column_count|} => 0;
                                }

                                public class D : C
                                {
                                    internal override int {|RH4113:row_count|} => 1;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies each part of a partial internal property gets its own diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-001")]
    public async Task PartialInternalPropertyIsReportedOnEachPart()
    {
        const string testCode = """
                                public partial class C
                                {
                                    internal partial int {|RH4113:count|} { get; }
                                }

                                public partial class C
                                {
                                    internal partial int {|RH4113:count|} => 0;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix renames internal properties and every reference to them to the derived PascalCase name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-002")]
    public async Task FixRenamesInternalPropertiesAndTheirReferencesToPascalCase()
    {
        const string testCode = """
                                public class Settings
                                {
                                    internal int {|RH4113:retry_count|} { get; set; }

                                    protected internal int {|RH4113:timeout|} => 0;

                                    public int Sum => retry_count + timeout;
                                }
                                """;

        const string fixedCode = """
                                 public class Settings
                                 {
                                     internal int RetryCount { get; set; }

                                     protected internal int Timeout => 0;

                                     public int Sum => RetryCount + Timeout;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies renaming an internal property also renames the property it overrides and the property that overrides it in source
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4113-002")]
    public async Task FixRenamesOverriddenAndOverridingInternalPropertiesInSource()
    {
        const string testCode = """
                                public class BaseStore
                                {
                                    internal virtual int {|RH4113:row_count|} => 0;
                                }

                                public class FileStore : BaseStore
                                {
                                    internal override int {|RH4113:row_count|} => 1;
                                }
                                """;

        const string fixedCode = """
                                 public class BaseStore
                                 {
                                     internal virtual int RowCount => 0;
                                 }

                                 public class FileStore : BaseStore
                                 {
                                     internal override int RowCount => 1;
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
                                    public class Settings
                                    {
                                        internal int {|#0:retryCount|} { get; set; }

                                        internal int {|#1:timeoutSeconds|} { get; set; }

                                        public int Total => retryCount * timeoutSeconds;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Settings
                                     {
                                         internal int RetryCount { get; set; }

                                         internal int TimeoutSeconds { get; set; }

                                         public int Total => RetryCount * TimeoutSeconds;
                                     }
                                 }
                                 """;

        // Both properties are read by the same expression body, so each rename has to reach a reference
        // outside its own declaration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4113InternalPropertyCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4113MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}