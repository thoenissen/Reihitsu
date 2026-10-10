using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer"/>
/// </summary>
[TestClass]
public class RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzerTests : AnalyzerTestsBase<RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for parameter documentation in the wrong order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForParameterDocumentationInWrongOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <param name="firstValue">First value.</param>
                                  /// {|#0:<param name="missingValue">Missing value.</param>|}
                                  internal void TestMethod(int firstValue, int secondValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8102MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for every tag when correctly named parameters are documented out of order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForParameterDocumentationInPermutedOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Saves the customer.</summary>
                                  /// {|#0:<param name="overwrite">Whether an existing customer may be replaced.</param>|}
                                  /// {|#1:<param name="customer">The customer to save.</param>|}
                                  internal void Save(int customer, bool overwrite)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8102MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when parameter documentation matches the declared order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForParameterDocumentationInDeclaredOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Saves the customer.</summary>
                                  /// <param name="customer">The customer to save.</param>
                                  /// <param name="overwrite">Whether an existing customer may be replaced.</param>
                                  internal void Save(int customer, bool overwrite)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a diagnostic is reported for conversion operator parameter documentation in the wrong order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForConversionOperatorParameterDocumentationInWrongOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;
                                  public int Fraction => 0;

                                  /// <summary>Converts to a plain amount with a fraction.</summary>
                                  /// <param name="firstValue">First value.</param>
                                  /// {|#0:<param name="thirdValue">Third value.</param>|}
                                  /// <returns>The plain amount.</returns>
                                  public static implicit operator int(Money firstValue) => firstValue.Amount;
                              }
                              """;

        await Verify(source, Diagnostics(RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8102MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for primary-constructor parameter documentation in permuted order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrimaryConstructorParameterDocumentationInPermutedOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a pair of coordinates.</summary>
                              /// {|#0:<param name="Y">The y coordinate.</param>|}
                              /// {|#1:<param name="X">The x coordinate.</param>|}
                              internal readonly record struct Point(int X, int Y);
                              """;

        await Verify(source, Diagnostics(RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8102MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that parameter tags nested in remarks are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForParameterDocumentationNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <remarks><param name="missingValue">Nested parameter.</param></remarks>
                                  internal void TestMethod(int firstValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <param name="firstValue">First value.</param>
                                  /// {|#0:<param name="missingValue">Missing value.</param>|}
                                  internal void TestMethod(int firstValue, int secondValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}