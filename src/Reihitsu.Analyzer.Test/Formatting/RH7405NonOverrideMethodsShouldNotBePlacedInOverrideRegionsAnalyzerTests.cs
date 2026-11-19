using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer"/>
/// </summary>
[TestClass]
public class RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzerTests : AnalyzerTestsBase<RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that non-override methods inside override regions produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    public async Task VerifyDiagnosticForNonOverrideMethodInOverrideRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override void Execute()
                                    {
                                    }

                                    private static string {|#0:BuildCacheKey|}()
                                    {
                                        return "report";
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData, Diagnostics(RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("BaseProcessor")));
    }

    /// <summary>
    /// Verifies that mixed override kinds in the same base-type region do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-002")]
    public async Task VerifyNoDiagnosticsForMixedOverrideKindsInSameRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }

                                    public virtual string Name => string.Empty;
                                }

                                internal class DerivedProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override void Execute()
                                    {
                                    }

                                    public override string Name => base.Name;

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that non-override methods in generic regions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-002")]
    public async Task VerifyNoDiagnosticsForNonOverrideMethodInGenericRegion()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                    public virtual void Execute()
                                    {
                                    }
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    public override void Execute()
                                    {
                                    }

                                    #endregion // BaseProcessor

                                    #region Methods

                                    private static string BuildCacheKey()
                                    {
                                        return "report";
                                    }

                                    #endregion // Methods
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that base-type named regions without override methods do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-002")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyNoDiagnosticsForBaseTypeNamedRegionWithoutOverrideMethods()
    {
        const string testData = """
                                internal abstract class BaseProcessor
                                {
                                }

                                internal sealed class ReportProcessor : BaseProcessor
                                {
                                    #region BaseProcessor

                                    private static string BuildCacheKey()
                                    {
                                        return "report";
                                    }

                                    #endregion // BaseProcessor
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a static method, a new method, and an interface implementation that override nothing are reported in
    /// an override region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    public async Task VerifyDiagnosticsForStaticNewAndInterfaceMethodsInOverrideRegion()
    {
        const string testData = """
                                interface IRunnable
                                {
                                    void Run();
                                }

                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B, IRunnable
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    public static void {|RH7405:S|}()
                                    {
                                    }

                                    public new string {|RH7405:ToString|}() => "";

                                    public void {|RH7405:Run|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an override method, a constructor, and an operator in an override region and a method in an ordinary
    /// region are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-002")]
    public async Task VerifyNoDiagnosticsForConstructorOperatorAndMethodInOrdinaryRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    public C()
                                    {
                                    }

                                    public static C operator +(C left, C right) => left;

                                    #endregion

                                    #region Methods

                                    private void N()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region named after the base type of an override is an override region also when the override lies
    /// outside it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-006")]
    [Requirement("RH7405-001")]
    public async Task VerifyRegionIsOverrideRegionWhenOverrideLiesOutsideIt()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B
                                {
                                    public override void M()
                                    {
                                    }

                                    #region B

                                    public void {|RH7405:N|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example in which an override property makes the region named after its base type an
    /// override region, so a method that overrides nothing in it is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyOverrideOfOtherMemberKindMakesOverrideRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual int P => 0;
                                }

                                class C : B
                                {
                                    #region B

                                    public override int P => 1;

                                    public void {|RH7405:N|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example in which an override in another declaration of the partial type makes the
    /// region named after its base type an override region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyOverrideInOtherPartialDeclarationMakesOverrideRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                partial class C : B
                                {
                                    public override void M()
                                    {
                                    }
                                }

                                partial class C
                                {
                                    #region B

                                    public void {|RH7405:N|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an override in a declaration of the partial type in another file makes the region named after its
    /// base type an override region
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyOverrideInOtherPartialDeclarationInAnotherFileMakesOverrideRegion()
    {
        const string overridePart = """
                                    class B
                                    {
                                        public virtual void M()
                                        {
                                        }
                                    }

                                    partial class C : B
                                    {
                                        public override void M()
                                        {
                                        }
                                    }
                                    """;

        const string regionPart = """
                                  partial class C
                                  {
                                      #region B

                                      public void {|RH7405:N|}()
                                      {
                                      }

                                      #endregion
                                  }
                                  """;

        await Verify(overridePart, test => test.TestState.Sources.Add(("/0/Test1.cs", regionPart)));
    }

    /// <summary>
    /// Verifies the specification example in which a method in a nested region is judged by the innermost region: the
    /// method in <c>B</c> inside <c>Methods</c> is reported, the method in <c>Methods</c> inside <c>B</c> is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    [Requirement("RH7405-002")]
    [Requirement("SHARED-REGGROUP-001")]
    [Requirement("SHARED-TYPEREGION-006")]
    public async Task VerifyMethodInNestedRegionIsJudgedByTheInnermostRegion()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    #region Methods

                                    public void O()
                                    {
                                    }

                                    #endregion

                                    #endregion

                                    #region Methods

                                    #region B

                                    public void {|RH7405:N|}()
                                    {
                                    }

                                    #endregion

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a method in a region without a description, nested in an override region, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-002")]
    public async Task VerifyMethodInRegionWithoutDescriptionNestedInOverrideRegionIsNotReported()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B
                                {
                                    #region B

                                    public override void M()
                                    {
                                    }

                                    #region

                                    public void O()
                                    {
                                    }

                                    #endregion

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a region nested in another region counts as an override region when an override in another region
    /// names it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    [Requirement("SHARED-REGGROUP-006")]
    public async Task VerifyNestedRegionIsOverrideRegionWhenAnOverrideOutsideItNamesIt()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void M()
                                    {
                                    }
                                }

                                class C : B
                                {
                                    public override void M()
                                    {
                                    }

                                    #region Methods

                                    #region B

                                    public void {|RH7405:N|}()
                                    {
                                    }

                                    #endregion

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example of a <see langword="partial"/> method that overrides nothing and both
    /// of whose parts lie in an override region: it is reported once, on its defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    [Requirement("SHARED-DIAG-004")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialMethodIsReportedOnceOnItsDefiningPart()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void Run()
                                    {
                                    }
                                }

                                partial class C : B
                                {
                                    #region B

                                    public override void Run()
                                    {
                                    }

                                    public partial void {|#0:N|}();

                                    public partial void N()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData, Diagnostics(RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("B")));
    }

    /// <summary>
    /// Verifies that a <see langword="partial"/> method that overrides nothing, whose implementing part stands
    /// first and whose parts both lie in an override region, is reported once, on its defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialNonOverrideMethodWithImplementingPartFirstIsReportedOnceOnItsDefiningPart()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void Run()
                                    {
                                    }
                                }

                                partial class C : B
                                {
                                    #region B

                                    public override void Run()
                                    {
                                    }

                                    public partial void N()
                                    {
                                    }

                                    public partial void {|#0:N|}();

                                    #endregion
                                }
                                """;

        await Verify(testData, Diagnostics(RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("B")));
    }

    /// <summary>
    /// Verifies that a <see langword="partial"/> non-override method whose defining part lies in generated code, with
    /// both parts in an override region, is reported once, on its implementing part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialNonOverrideMethodWithGeneratedDefiningPartIsReportedOnItsImplementingPart()
    {
        const string userPart = """
                                class B
                                {
                                    public virtual void Run()
                                    {
                                    }
                                }

                                partial class C : B
                                {
                                    #region B

                                    public override void Run()
                                    {
                                    }

                                    public partial void {|#0:N|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        const string generatedPart = """
                                     partial class C
                                     {
                                         #region B

                                         public partial void N();

                                         #endregion
                                     }
                                     """;

        await Verify(userPart,
                     test => test.TestState.Sources.Add(("/0/C.g.cs", generatedPart)),
                     Diagnostics(RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("B")));
    }

    /// <summary>
    /// Verifies that a <see langword="partial"/> non-override method whose implementing part lies in generated code,
    /// with both parts in an override region, is reported once, on its defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialNonOverrideMethodWithGeneratedImplementingPartIsReportedOnItsDefiningPart()
    {
        const string userPart = """
                                class B
                                {
                                    public virtual void Run()
                                    {
                                    }
                                }

                                partial class C : B
                                {
                                    #region B

                                    public override void Run()
                                    {
                                    }

                                    public partial void {|#0:N|}();

                                    #endregion
                                }
                                """;

        const string generatedPart = """
                                     partial class C
                                     {
                                         #region B

                                         public partial void N()
                                         {
                                         }

                                         #endregion
                                     }
                                     """;

        await Verify(userPart,
                     test => test.TestState.Sources.Add(("/0/C.g.cs", generatedPart)),
                     Diagnostics(RH7405NonOverrideMethodsShouldNotBePlacedInOverrideRegionsAnalyzer.DiagnosticId, CreateMessage("B")));
    }

    /// <summary>
    /// Verifies the specification example of a <see langword="partial"/> method that overrides nothing and of which only
    /// the implementing part lies in an override region: it is reported once, on that part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialMethodWithOnlyItsImplementingPartInOverrideRegionIsReportedOnThatPart()
    {
        const string testData = """
                                class B
                                {
                                    public virtual void Run()
                                    {
                                    }
                                }

                                partial class C : B
                                {
                                    #region Methods

                                    public partial void N();

                                    #endregion

                                    #region B

                                    public override void Run()
                                    {
                                    }

                                    public partial void {|RH7405:N|}()
                                    {
                                    }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a <see langword="partial"/> method that overrides nothing, whose defining part lies in no region in one
    /// file and whose implementing part lies in an override region in a second file, is reported once, on the
    /// implementing part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialMethodWithOnlyItsImplementingPartInOverrideRegionInAnotherFileIsReportedOnThatPart()
    {
        const string definingPart = """
                                    class B
                                    {
                                        public virtual void Run()
                                        {
                                        }
                                    }

                                    partial class C : B
                                    {
                                        public partial void N();
                                    }
                                    """;

        const string implementingPart = """
                                        partial class C
                                        {
                                            #region B

                                            public override void Run()
                                            {
                                            }

                                            public partial void {|RH7405:N|}()
                                            {
                                            }

                                            #endregion
                                        }
                                        """;

        await Verify(definingPart, test => test.TestState.Sources.Add(("/0/Test1.cs", implementingPart)));
    }

    /// <summary>
    /// Verifies that a <see langword="partial"/> method that overrides nothing, whose defining part lies in an override
    /// region in one file and whose implementing part lies in no region in a second file, is reported once, on the
    /// defining part
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7405-001")]
    [Requirement("SHARED-DIAG-012")]
    public async Task VerifyPartialMethodWithOnlyItsDefiningPartInOverrideRegionInAnotherFileIsReportedOnThatPart()
    {
        const string definingPart = """
                                    class B
                                    {
                                        public virtual void Run()
                                        {
                                        }
                                    }

                                    partial class C : B
                                    {
                                        #region B

                                        public override void Run()
                                        {
                                        }

                                        public partial void {|RH7405:N|}();

                                        #endregion
                                    }
                                    """;

        const string implementingPart = """
                                        partial class C
                                        {
                                            public partial void N()
                                            {
                                            }
                                        }
                                        """;

        await Verify(definingPart, test => test.TestState.Sources.Add(("/0/Test1.cs", implementingPart)));
    }

    /// <summary>
    /// Verifies that a non-override method in the Object region of a union that overrides ToString is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-REGGROUP-001")]
    [Requirement("RH7405-001")]
    public async Task UnionNonOverrideMethodInOverrideRegionIsReported()
    {
        const string testCode = """
                                union U(int, string)
                                {
                                    #region Object

                                    public override string ToString()
                                    {
                                        return "";
                                    }

                                    public int {|RH7405:Get|}()
                                    {
                                        return 1;
                                    }

                                    #endregion
                                }
                                """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(testCode, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates the expected diagnostic message for the given region
    /// </summary>
    /// <param name="regionName">Region name</param>
    /// <returns>Diagnostic message</returns>
    private static string CreateMessage(string regionName)
    {
        return string.Format(MessageFormat, regionName);
    }

    #endregion // Methods
}