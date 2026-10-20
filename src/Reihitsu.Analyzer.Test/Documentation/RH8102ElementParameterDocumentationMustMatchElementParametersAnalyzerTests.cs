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
    [Requirement("RH8102-001")]
    [Requirement("RH8102-002")]
    [Requirement("SHARED-DOC-015")]
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
    [Requirement("RH8102-001")]
    [Requirement("RH8102-002")]
    [Requirement("SHARED-DOC-015")]
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
    [Requirement("RH8102-001")]
    [Requirement("SHARED-DOC-015")]
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
    [Requirement("RH8102-001")]
    [Requirement("SHARED-DOC-015")]
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
    [Requirement("RH8102-001")]
    [Requirement("SHARED-DOC-015")]
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
    [Requirement("RH8102-001")]
    [Requirement("SHARED-DOC-015")]
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
    [Requirement("RH8102-001")]
    [Requirement("SHARED-DOC-008")]
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

    /// <summary>
    /// Verifies the specification example: a param element naming another parameter than the one in its position, on a primary constructor, an explicit implementation, or a method, is reported on the whole element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8102-001")]
    [Requirement("RH8102-002")]
    public async Task VerifySpecificationExampleReportsParameterElementsThatDoNotFollowTheParameters()
    {
        const string source = """
                              /// <summary>
                              /// Point
                              /// </summary>
                              /// <param name="X">X</param>
                              /// {|#0:<param name="Z">Z</param>|}
                              public record Point(int X, int Y);

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  void Run(int value);
                              }

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C : I
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|#1:<param name="other">Other</param>|}
                                  void I.Run(int value)
                                  {
                                  }

                                  /// <summary>
                                  /// Saves
                                  /// </summary>
                                  /// {|#2:<param name="overwrite">Overwrite</param>|}
                                  /// {|#3:<param name="customer">Customer</param>|}
                                  public void Save(int customer, bool overwrite)
                                  {
                                  }

                                  /// <summary>
                                  /// Saves
                                  /// </summary>
                                  /// <param name="customer">Customer</param>
                                  /// <param name="overwrite">Overwrite</param>
                                  public void Store(int customer, bool overwrite)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8102MessageFormat, 4));
    }

    /// <summary>
    /// Verifies the shared specification example: param elements are paired with the parameters by position, an unnamed element keeps its position, nested elements and parameters without an element are not reported, and names are compared by the value of the identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8102-001")]
    [Requirement("RH8102-002")]
    [Requirement("SHARED-DOC-015")]
    public async Task VerifySpecificationExamplePairsParameterElementsByPosition()
    {
        const string source = """
                              /// <summary>
                              /// Pair
                              /// </summary>
                              /// <typeparam name="TSecond">Second</typeparam>
                              /// <typeparam name="TFirst">First</typeparam>
                              public class Pair<TFirst, TSecond>
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param name="a">A</param>
                                  /// {|#0:<param name="c">C</param>|}
                                  public void M(int a, int b)
                                  {
                                  }

                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param>Unnamed</param>
                                  /// <param name="b">B</param>
                                  /// {|#1:<param name="c">Extra</param>|}
                                  public void N(int a, int b)
                                  {
                                  }

                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param name="a">A</param>
                                  /// <remarks>
                                  /// <param name="z">Nested</param>
                                  /// </remarks>
                                  public void O(int a, int b)
                                  {
                                  }

                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param name="class">Class</param>
                                  /// {|#2:<param name="@event">Event</param>|}
                                  public void P(int @class, int @event)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8102MessageFormat, 3));
    }

    /// <summary>
    /// Verifies a param element with an empty or white-space name is not reported and still takes the position of its parameter, so the following element is paired with the next parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8102-001")]
    [Requirement("SHARED-DOC-015")]
    public async Task VerifyNoDiagnosticForEmptyOrWhiteSpaceNameKeepingItsPosition()
    {
        const string source = """
                              internal class C
                              {
                                  /// <summary>Runs</summary>
                                  /// <param name="">A</param>
                                  /// <param name=" ">B</param>
                                  /// <param name="c">C</param>
                                  /// {|#0:<param name="c">Again</param>|}
                                  public void M(int a, int b, int c)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8102MessageFormat));
    }

    /// <summary>
    /// Verifies a parameter without a param element is not reported when the elements written so far follow the parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8102-001")]
    [Requirement("SHARED-DOC-015")]
    public async Task VerifyNoDiagnosticForParameterWithoutElement()
    {
        const string source = """
                              internal class C
                              {
                                  /// <summary>Runs</summary>
                                  /// <param name="a">A</param>
                                  public void M(int a, int b, int c)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the param elements of constructor, delegate, and indexer declarations are checked against their parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8102-001")]
    [Requirement("RH8102-002")]
    public async Task VerifyDiagnosticForConstructorDelegateAndIndexerParameterElements()
    {
        const string source = """
                              /// <summary>Handler</summary>
                              /// {|#0:<param name="other">Other</param>|}
                              internal delegate void Handler(int value);

                              internal class C
                              {
                                  /// <summary>Creates</summary>
                                  /// {|#1:<param name="other">Other</param>|}
                                  public C(int size)
                                  {
                                  }

                                  /// <summary>Item</summary>
                                  /// {|#2:<param name="other">Other</param>|}
                                  public int this[int index] => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8102ElementParameterDocumentationMustMatchElementParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8102MessageFormat, 3));
    }

    #endregion // Tests
}