using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8105ElementReturnValueMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8105ElementReturnValueMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8105ElementReturnValueMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing returns tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForMissingReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Gets the value.</summary>
                                  internal {|#0:int|} GetValue()
                                  {
                                      return 1;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8105ElementReturnValueMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8105MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for an operator missing returns documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForOperatorMissingReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  /// <summary>Adds two amounts.</summary>
                                  /// <param name="left">Left operand.</param>
                                  /// <param name="right">Right operand.</param>
                                  public static {|#0:Money|} operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8105ElementReturnValueMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8105MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an operator whose return value is documented
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForOperatorWithReturnsDocumented()
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
    /// Verifies a diagnostic is reported for a conversion operator missing returns documentation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForConversionOperatorMissingReturnsDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;

                                  /// <summary>Converts to a plain amount.</summary>
                                  /// <param name="value">The amount.</param>
                                  public static implicit operator {|#0:int|}(Money value) => value.Amount;
                              }
                              """;

        await Verify(source, Diagnostics(RH8105ElementReturnValueMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8105MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for a conversion operator whose return value is documented
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForConversionOperatorWithReturnsDocumented()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;

                                  /// <summary>Converts to a plain amount.</summary>
                                  /// <param name="value">The amount.</param>
                                  /// <returns>The plain amount.</returns>
                                  public static implicit operator int(Money value) => value.Amount;
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
                                  /// <summary>Gets the value.</summary>
                                  internal {|#0:int|} GetValue()
                                  {
                                      return 1;
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}