using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8034OperatorsMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8034OperatorsMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8034OperatorsMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for an operator without documentation, anchored on the operator token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForOperatorWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public static Money operator {|#0:+|}(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8034OperatorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8034MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an operator with a summary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForOperatorWithSummary()
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
    /// Verifies no diagnostic is reported for an operator using inheritdoc
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForOperatorWithInheritdoc()
    {
        const string source = """
                              namespace TestNamespace;

                              internal interface IAddable<T> where T : IAddable<T>
                              {
                                  /// <summary>Adds two values.</summary>
                                  static abstract T operator +(T left, T right);
                              }

                              internal readonly struct Money : IAddable<Money>
                              {
                                  /// <inheritdoc/>
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a conversion operator without documentation, anchored on the operator keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForConversionOperatorWithoutDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal readonly struct Money
                              {
                                  public int Amount => 0;

                                  public static implicit {|#0:operator|} int(Money value) => value.Amount;
                              }
                              """;

        await Verify(source, Diagnostics(RH8034OperatorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8034MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostic is reported for an explicitly implemented interface operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForExplicitlyImplementedInterfaceOperator()
    {
        const string source = """
                              namespace TestNamespace;

                              internal interface IAddable<T> where T : IAddable<T>
                              {
                                  /// <summary>Adds two values.</summary>
                                  static abstract T operator +(T left, T right);
                              }

                              internal readonly struct Money : IAddable<Money>
                              {
                                  static Money IAddable<Money>.operator +(Money left, Money right) => left;
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
                              internal readonly struct Money
                              {
                                  public static Money operator +(Money left, Money right) => left;
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}