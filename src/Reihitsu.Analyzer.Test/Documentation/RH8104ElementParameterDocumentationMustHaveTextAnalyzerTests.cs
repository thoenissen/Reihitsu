using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8104ElementParameterDocumentationMustHaveTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH8104ElementParameterDocumentationMustHaveTextAnalyzerTests : AnalyzerTestsBase<RH8104ElementParameterDocumentationMustHaveTextAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for an empty parameter tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    [Requirement("RH8104-002")]
    [Requirement("SHARED-DOC-014")]
    public async Task VerifyDiagnosticForEmptyParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<param name="firstValue"></param>|}
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8104ElementParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8104MessageFormat));
    }

    /// <summary>
    /// Verifies that empty parameter tags nested in remarks are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    public async Task VerifyNoDiagnosticForEmptyParameterNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <remarks><param name="firstValue"></param></remarks>
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that direct parameter tags are analyzed without also analyzing nested matches
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    [Requirement("RH8104-002")]
    public async Task VerifyOnlyDirectEmptyParameterIsReportedBesideNestedMatch()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <remarks><param name="nestedValue"></param></remarks>
                                  /// {|#0:<param name="firstValue"></param>|}
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8104ElementParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8104MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an empty operator parameter tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    [Requirement("RH8104-002")]
    public async Task VerifyDiagnosticForEmptyOperatorParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  /// <summary>Adds two amounts.</summary>
                                  /// {|#0:<param name="left"></param>|}
                                  /// <param name="right">Right operand.</param>
                                  /// <returns>The sum.</returns>
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8104ElementParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8104MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an empty conversion operator parameter tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    [Requirement("RH8104-002")]
    public async Task VerifyDiagnosticForEmptyConversionOperatorParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;

                                  /// <summary>Converts to a plain amount.</summary>
                                  /// {|#0:<param name="value"></param>|}
                                  /// <returns>The plain amount.</returns>
                                  public static implicit operator int(Money value) => value.Amount;
                              }
                              """;

        await Verify(source, Diagnostics(RH8104ElementParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8104MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// {|#0:<param name="firstValue"></param>|}
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: an empty or white-space param element at the top level is reported on the whole element, on a primary constructor, a method, and an operator, while a nested one and one with text are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    [Requirement("RH8104-002")]
    public async Task VerifySpecificationExampleReportsParameterElementsWithoutText()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              /// {|#0:<param name="size"></param>|}
                              public class C(int size)
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|#1:<param name="a"/>|}
                                  /// <param name="b">B</param>
                                  /// <remarks>
                                  /// <param name="c"></param>
                                  /// </remarks>
                                  public void M(int a, int b)
                                  {
                                  }

                                  /// <summary>
                                  /// Adds
                                  /// </summary>
                                  /// {|#2:<param name="left">
                                  /// </param>|}
                                  /// <param name="right">Right</param>
                                  /// <returns>Sum</returns>
                                  public static C operator +(C left, C right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8104ElementParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8104MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the shared specification example: a param element that is empty, holds only a white-space character reference, or only line breaks of a delimited comment counts as without text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    [Requirement("RH8104-002")]
    [Requirement("SHARED-DOC-014")]
    public async Task VerifySpecificationExampleJudgesParameterTextLikeSummaryText()
    {
        const string source = """
                              /// <summary>
                              /// Container
                              /// </summary>
                              /// <typeparam name="T"></typeparam>
                              public class C<T>
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|#0:<param name="a"/>|}
                                  /// {|#1:<param name="b">&#32;</param>|}
                                  /// <param name="c">Value</param>
                                  /// <returns>
                                  /// </returns>
                                  public int M(int a, int b, int c) => 0;

                                  /**
                                   * <summary>
                                   * Runs
                                   * </summary>
                                   * {|#2:<param name="a">
                                   * </param>|}
                                   * <returns>Value</returns>
                                   */
                                  public int N(int a) => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8104ElementParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8104MessageFormat, 3));
    }

    /// <summary>
    /// Verifies an empty param element is reported whether or not it names a parameter, on a type without a primary constructor and on an explicit interface implementation as well
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    [Requirement("RH8104-002")]
    public async Task VerifyDiagnosticForParameterElementWithoutTextWhateverItNames()
    {
        const string source = """
                              /// <summary>Contract</summary>
                              internal interface I
                              {
                                  /// <summary>Runs</summary>
                                  /// <param name="value">Value</param>
                                  void Run(int value);
                              }

                              /// <summary>Service</summary>
                              /// {|#0:<param name="size"></param>|}
                              internal class C : I
                              {
                                  /// <summary>Runs</summary>
                                  /// {|#1:<param name="value"/>|}
                                  void I.Run(int value)
                                  {
                                  }

                                  /// <summary>Runs</summary>
                                  /// {|#2:<param name="other"></param>|}
                                  public void M(int value)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8104ElementParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8104MessageFormat, 3));
    }

    /// <summary>
    /// Verifies a param element whose content is only a tab or a no-break space counts as without text, while one with an asterisk in the middle of its line has text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8104-001")]
    [Requirement("SHARED-DOC-014")]
    public async Task VerifyParameterElementWithNoBreakSpaceOrTabIsWithoutText()
    {
        const string source = """
                              internal class C
                              {
                                  /// <summary>Runs</summary>
                                  /// {|#0:<param name="a">&#9;</param>|}
                                  /// {|#1:<param name="b">&#160;</param>|}
                                  /// <param name="c"> * </param>
                                  public void M(int a, int b, int c)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8104ElementParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8104MessageFormat, 2));
    }

    #endregion // Tests
}