using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8106ElementReturnValueDocumentationMustHaveTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH8106ElementReturnValueDocumentationMustHaveTextAnalyzerTests : AnalyzerTestsBase<RH8106ElementReturnValueDocumentationMustHaveTextAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for an empty returns tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8106-001")]
    [Requirement("RH8106-002")]
    [Requirement("SHARED-DOC-014")]
    public async Task VerifyDiagnosticForEmptyReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Gets the value.</summary>
                                  /// {|#0:<returns></returns>|}
                                  internal int GetValue()
                                  {
                                      return 1;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8106ElementReturnValueDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8106MessageFormat));
    }

    /// <summary>
    /// Verifies that an empty returns tag nested in remarks is ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8106-001")]
    public async Task VerifyNoDiagnosticForEmptyReturnsNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Gets the value.</summary>
                                  /// <remarks><returns></returns></remarks>
                                  internal int GetValue()
                                  {
                                      return 1;
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an empty returns tag on an operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8106-001")]
    [Requirement("RH8106-002")]
    public async Task VerifyDiagnosticForEmptyOperatorReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  /// <summary>Adds two amounts.</summary>
                                  /// <param name="left">Left operand.</param>
                                  /// <param name="right">Right operand.</param>
                                  /// {|#0:<returns></returns>|}
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8106ElementReturnValueDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8106MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an empty returns tag on a conversion operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8106-001")]
    [Requirement("RH8106-002")]
    public async Task VerifyDiagnosticForEmptyConversionOperatorReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;

                                  /// <summary>Converts to a plain amount.</summary>
                                  /// <param name="value">The amount.</param>
                                  /// {|#0:<returns></returns>|}
                                  public static implicit operator int(Money value) => value.Amount;
                              }
                              """;

        await Verify(source, Diagnostics(RH8106ElementReturnValueDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8106MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8106-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class TestClass
                              {
                                  /// <summary>Gets the value.</summary>
                                  /// {|#0:<returns></returns>|}
                                  internal int GetValue()
                                  {
                                      return 1;
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: a top-level returns element without text is reported on the whole element, on a delegate, methods, an explicit implementation, and an operator, while a nested one is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8106-001")]
    [Requirement("RH8106-002")]
    public async Task VerifySpecificationExampleReportsReturnsElementsWithoutText()
    {
        const string source = """
                              using System.Threading.Tasks;

                              /// <summary>
                              /// Factory
                              /// </summary>
                              /// {|#0:<returns></returns>|}
                              public delegate int Factory();

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Gets
                                  /// </summary>
                                  /// <returns>Value</returns>
                                  int Get();
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C : I
                              {
                                  /// <summary>
                                  /// Counts
                                  /// </summary>
                                  /// {|#1:<returns/>|}
                                  public int Count() => 0;

                                  /// <summary>
                                  /// Loads
                                  /// </summary>
                                  /// {|#2:<returns>
                                  /// </returns>|}
                                  public Task LoadAsync() => Task.CompletedTask;

                                  /// <summary>
                                  /// Gets
                                  /// </summary>
                                  /// {|#3:<returns>&#32;</returns>|}
                                  int I.Get() => 0;

                                  /// <summary>
                                  /// Finds
                                  /// </summary>
                                  /// <returns>Value</returns>
                                  /// <remarks>
                                  /// <returns></returns>
                                  /// </remarks>
                                  public int Find() => 0;

                                  /// <summary>
                                  /// Adds
                                  /// </summary>
                                  /// <param name="left">Left</param>
                                  /// <param name="right">Right</param>
                                  /// {|#4:<returns></returns>|}
                                  public static C operator +(C left, C right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8106ElementReturnValueDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8106MessageFormat, 5));
    }

    /// <summary>
    /// Verifies the shared specification example: a returns element with only a line break counts as without text, while one with text in a delimited comment does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8106-001")]
    [Requirement("RH8106-002")]
    [Requirement("SHARED-DOC-014")]
    public async Task VerifySpecificationExampleJudgesReturnsTextLikeSummaryText()
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
                                  /// <param name="a"/>
                                  /// <param name="b">&#32;</param>
                                  /// <param name="c">Value</param>
                                  /// {|#0:<returns>
                                  /// </returns>|}
                                  public int M(int a, int b, int c) => 0;

                                  /**
                                   * <summary>
                                   * Runs
                                   * </summary>
                                   * <param name="a">
                                   * </param>
                                   * <returns>Value</returns>
                                   */
                                  public int N(int a) => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8106ElementReturnValueDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8106MessageFormat));
    }

    /// <summary>
    /// Verifies a returns element of a delimited comment whose content is only the asterisks that start its lines counts as without text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8106-001")]
    [Requirement("RH8106-002")]
    [Requirement("SHARED-DOC-014")]
    public async Task VerifyDiagnosticForReturnsElementWithoutTextInDelimitedComment()
    {
        const string source = """
                              internal class C
                              {
                                  /**
                                   * <summary>Counts</summary>
                                   * {|#0:<returns>
                                   * </returns>|}
                                   */
                                  public int Count() => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8106ElementReturnValueDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8106MessageFormat));
    }

    #endregion // Tests
}