using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer"/>
/// </summary>
[TestClass]
public class RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzerTests : AnalyzerTestsBase<RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing parameter name attribute
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8103-001")]
    [Requirement("RH8103-002")]
    [Requirement("SHARED-DOC-016")]
    public async Task VerifyDiagnosticForMissingParameterName()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<param>First value.</param>|}
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8103MessageFormat));
    }

    /// <summary>
    /// Verifies that unnamed parameter tags nested in remarks are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8103-001")]
    [Requirement("SHARED-DOC-016")]
    public async Task VerifyNoDiagnosticForUnnamedParameterNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <remarks><param>Nested parameter.</param></remarks>
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an operator parameter tag missing a name attribute
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8103-001")]
    [Requirement("RH8103-002")]
    public async Task VerifyDiagnosticForOperatorParameterMissingName()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  /// <summary>Adds two amounts.</summary>
                                  /// {|#0:<param>Left operand.</param>|}
                                  /// <param name="right">Right operand.</param>
                                  /// <returns>The sum.</returns>
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8103MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a conversion operator parameter tag missing a name attribute
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8103-001")]
    [Requirement("RH8103-002")]
    public async Task VerifyDiagnosticForConversionOperatorParameterMissingName()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;

                                  /// <summary>Converts to a plain amount.</summary>
                                  /// {|#0:<param>The amount.</param>|}
                                  /// <returns>The plain amount.</returns>
                                  public static implicit operator int(Money value) => value.Amount;
                              }
                              """;

        await Verify(source, Diagnostics(RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8103MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8103-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<param>First value.</param>|}
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: a param element without a name is reported on the whole element, whether or not the declaration has parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8103-001")]
    [Requirement("RH8103-002")]
    public async Task VerifySpecificationExampleReportsParameterElementsWithoutName()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              /// {|#0:<param>Unused</param>|}
                              public class C
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|#1:<param>Value</param>|}
                                  public void M(int value)
                                  {
                                  }

                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|#2:<param/>|}
                                  public void N()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8103MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the shared specification example: a top-level param element without a name attribute, with an empty or white-space name, or with only a differently cased Name attribute is reported, while a nested one is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8103-001")]
    [Requirement("RH8103-002")]
    [Requirement("SHARED-DOC-016")]
    public async Task VerifySpecificationExampleReportsMissingEmptyAndWhiteSpaceNames()
    {
        const string source = """
                              /// <summary>
                              /// Container
                              /// </summary>
                              /// <typeparam>Element</typeparam>
                              public class C<T>
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|#0:<param>First</param>|}
                                  /// {|#1:<param name="">Second</param>|}
                                  /// {|#2:<param name=" ">Third</param>|}
                                  /// {|#3:<param Name="d">Fourth</param>|}
                                  /// <param name="e">Fifth</param>
                                  /// <remarks>
                                  /// <param>Nested</param>
                                  /// </remarks>
                                  public void M(int a, int b, int c, int d, int e)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8103MessageFormat, 4));
    }

    /// <summary>
    /// Verifies unnamed param elements are reported on constructor, delegate, indexer, record, and explicit interface implementation declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8103-001")]
    [Requirement("RH8103-002")]
    public async Task VerifyDiagnosticForUnnamedParameterElementsOfOtherParameterOwners()
    {
        const string source = """
                              /// <summary>Handler</summary>
                              /// {|#0:<param>Value</param>|}
                              internal delegate void Handler(int value);

                              /// <summary>Point</summary>
                              /// {|#1:<param>X</param>|}
                              internal record Point(int X);

                              /// <summary>Contract</summary>
                              internal interface I
                              {
                                  /// <summary>Runs</summary>
                                  /// <param name="value">Value</param>
                                  void Run(int value);
                              }

                              /// <summary>Service</summary>
                              internal class C : I
                              {
                                  /// <summary>Creates</summary>
                                  /// {|#2:<param>Size</param>|}
                                  public C(int size)
                                  {
                                  }

                                  /// <summary>Item</summary>
                                  /// {|#3:<param>Index</param>|}
                                  public int this[int index] => 0;

                                  /// <summary>Runs</summary>
                                  /// {|#4:<param>Value</param>|}
                                  void I.Run(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8103ElementParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8103MessageFormat, 5));
    }

    #endregion // Tests
}