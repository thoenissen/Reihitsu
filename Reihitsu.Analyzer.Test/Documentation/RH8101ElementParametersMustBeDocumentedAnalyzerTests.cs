using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8101ElementParametersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8101ElementParametersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8101ElementParametersMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing parameter comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Runs the method.</summary>
                                  /// <param name="secondValue">Second value.</param>
                                  internal void TestMethod(int {|#0:firstValue|}, int secondValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
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
                                  /// <param name="secondValue">Second value.</param>
                                  internal void TestMethod(int {|#0:firstValue|}, int secondValue)
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies extension member method parameters are still validated by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForExtensionMemberParameterWithoutDocumentation()
    {
        const string source = """
                              public static class Extensions
                              {
                                  /// <summary>Provides text helpers.</summary>
                                  /// <param name="value">The source text.</param>
                                  extension(string value)
                                  {
                                      /// <summary>Creates a token.</summary>
                                      /// <param name="offset">The start offset.</param>
                                      public int Parse(int {|#0:length|}, int offset) => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an operator parameter missing documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForOperatorParameterWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  /// <summary>Adds two amounts.</summary>
                                  /// <param name="left">Left operand.</param>
                                  /// <returns>The sum.</returns>
                                  public static Money operator +(Money left, Money {|#0:right|}) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an operator whose parameters are all documented
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForOperatorWithAllParametersDocumented()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  /// <summary>Adds two amounts.</summary>
                                  /// <param name="left">Left operand.</param>
                                  /// <param name="right">Right operand.</param>
                                  /// <returns>The sum.</returns>
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a conversion operator parameter missing documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForConversionOperatorParameterWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;

                                  /// <summary>Converts to a plain amount.</summary>
                                  /// <returns>The plain amount.</returns>
                                  public static implicit operator int(Money {|#0:value|}) => value.Amount;
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a class primary-constructor parameter missing documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForClassPrimaryConstructorParameterWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a named counter.</summary>
                              internal sealed class Counter(string {|#0:name|});
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a plain record primary-constructor parameter missing documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForRecordPrimaryConstructorParameterWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a pair of coordinates.</summary>
                              /// <param name="Y">The y coordinate.</param>
                              internal sealed record Point(int {|#0:X|}, int Y);
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for primary-constructor parameters missing documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForPrimaryConstructorParametersWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a pair of coordinates.</summary>
                              internal readonly record struct Point(int {|#0:X|}, int {|#1:Y|});
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for primary-constructor parameters that are all documented
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPrimaryConstructorWithAllParametersDocumented()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a pair of coordinates.</summary>
                              /// <param name="X">The x coordinate.</param>
                              /// <param name="Y">The y coordinate.</param>
                              internal readonly record struct Point(int X, int Y);
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a documented type without a primary constructor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTypeWithoutPrimaryConstructor()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>A helper.</summary>
                              internal sealed class Helper
                              {
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}