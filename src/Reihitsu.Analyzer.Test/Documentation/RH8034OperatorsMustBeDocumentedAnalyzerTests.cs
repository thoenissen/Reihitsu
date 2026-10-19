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
    [Requirement("RH8034-001")]
    [Requirement("RH8034-002")]
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
    [Requirement("RH8034-001")]
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
    [Requirement("RH8034-001")]
    [Requirement("SHARED-DOC-005")]
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
    [Requirement("RH8034-001")]
    [Requirement("RH8034-002")]
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
    [Requirement("RH8034-001")]
    [Requirement("SHARED-DOC-009")]
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
    [Requirement("RH8034-001")]
    [Requirement("SHARED-DOC-008")]
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

    /// <summary>
    /// Verifies the specification example: undocumented binary, checked, conversion, compound assignment, and static abstract operators are reported, while documented and explicitly implemented ones are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8034-001")]
    [Requirement("RH8034-002")]
    public async Task VerifySpecificationExampleReportsUndocumentedOperators()
    {
        const string source = """
                              /// <summary>
                              /// Money
                              /// </summary>
                              public readonly struct Money
                              {
                                  public static Money operator {|#0:+|}(Money left, Money right) => left;

                                  public static Money operator checked {|#1:-|}(Money left, Money right) => left;

                                  public static Money operator {|#2:-|}(Money left, Money right) => left;

                                  /// <remarks>
                                  /// No summary
                                  /// </remarks>
                                  public static implicit {|#3:operator|} int(Money value) => 0;

                                  /// <summary>
                                  /// Negates an amount
                                  /// </summary>
                                  /// <param name="value">Amount</param>
                                  /// <returns>The negated amount</returns>
                                  public static Money operator -(Money value) => value;
                              }

                              /// <summary>
                              /// Counter
                              /// </summary>
                              public class Counter
                              {
                                  public void operator {|#4:+=|}(int value)
                                  {
                                  }
                              }

                              /// <summary>
                              /// Addable values
                              /// </summary>
                              /// <typeparam name="T">Value</typeparam>
                              public interface IAddable<T> where T : IAddable<T>
                              {
                                  static abstract T operator {|#5:+|}(T left, T right);
                              }

                              /// <summary>
                              /// Number
                              /// </summary>
                              public readonly struct Number : IAddable<Number>
                              {
                                  static Number IAddable<Number>.operator +(Number left, Number right) => left;
                              }
                              """;

        await Verify(source, Diagnostics(RH8034OperatorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8034MessageFormat, 6));
    }

    /// <summary>
    /// Verifies undocumented unary, true, false, increment, and unsigned shift operators are reported on their operator token
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8034-001")]
    [Requirement("RH8034-002")]
    public async Task VerifyDiagnosticForUnaryAndTrueFalseOperatorsIsLocatedOnOperatorToken()
    {
        const string source = """
                              /// <summary>Flag</summary>
                              public readonly struct Flag
                              {
                                  public static Flag operator {|#0:!|}(Flag value) => value;

                                  public static bool operator {|#1:true|}(Flag value) => true;

                                  public static bool operator {|#2:false|}(Flag value) => false;

                                  public static Flag operator {|#3:++|}(Flag value) => value;

                                  public static Flag operator {|#4:>>>|}(Flag value, int count) => value;
                              }
                              """;

        await Verify(source, Diagnostics(RH8034OperatorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8034MessageFormat, 5));
    }

    /// <summary>
    /// Verifies undocumented explicit, checked, and implicit conversion operators are reported on their operator keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8034-001")]
    [Requirement("RH8034-002")]
    public async Task VerifyDiagnosticForConversionOperatorsIsLocatedOnOperatorKeyword()
    {
        const string source = """
                              /// <summary>Money</summary>
                              public readonly struct Money
                              {
                                  public static explicit {|#0:operator|} int(Money value) => 0;

                                  public static explicit {|#1:operator|} checked int(Money value) => 0;

                                  public static implicit {|#2:operator|} Money(long value) => default;
                              }
                              """;

        await Verify(source, Diagnostics(RH8034OperatorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8034MessageFormat, 3));
    }

    /// <summary>
    /// Verifies undocumented operators declared in a class, a record, a record struct, and an extension block are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8034-001")]
    [Requirement("SHARED-DOC-001")]
    public async Task VerifyDiagnosticForOperatorsOfClassRecordRecordStructAndExtensionBlock()
    {
        const string source = """
                              /// <summary>Vector</summary>
                              public class Vector
                              {
                                  public static Vector operator {|#0:*|}(Vector left, int factor) => left;
                              }

                              /// <summary>Person</summary>
                              public record Person
                              {
                                  public static Person operator {|#1:+|}(Person left, Person right) => left;
                              }

                              /// <summary>Pair</summary>
                              public record struct Pair
                              {
                                  public static Pair operator {|#2:-|}(Pair value) => value;
                              }

                              /// <summary>Extensions</summary>
                              public static class Extensions
                              {
                                  /// <summary>Arrays</summary>
                                  /// <param name="values">Values</param>
                                  extension(int[] values)
                                  {
                                      public static int[] operator {|#3:+|}(int[] left, int[] right) => left;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8034OperatorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8034MessageFormat, 4));
    }

    /// <summary>
    /// Verifies undocumented interface operators and conversion operators are reported whether or not they carry an access modifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8034-001")]
    public async Task VerifyDiagnosticForInterfaceOperatorsWhateverTheirModifiers()
    {
        const string source = """
                              /// <summary>Addable values</summary>
                              /// <typeparam name="T">Value</typeparam>
                              public interface IAddable<T> where T : IAddable<T>
                              {
                                  static abstract T operator {|#0:+|}(T left, T right);

                                  public static virtual T operator {|#1:-|}(T left, T right) => left;

                                  static abstract explicit {|#2:operator|} int(T value);
                              }
                              """;

        await Verify(source, Diagnostics(RH8034OperatorsMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8034MessageFormat, 3));
    }

    /// <summary>
    /// Verifies an explicit implementation of an interface conversion operator is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8034-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForExplicitlyImplementedConversionOperator()
    {
        const string source = """
                              /// <summary>Values that convert to integers</summary>
                              /// <typeparam name="T">Value</typeparam>
                              public interface IConvertsToInt<T> where T : IConvertsToInt<T>
                              {
                                  /// <summary>Converts a value</summary>
                                  /// <param name="value">Value</param>
                                  /// <returns>The integer</returns>
                                  static abstract explicit operator int(T value);
                              }

                              /// <summary>Number</summary>
                              public readonly struct Number : IConvertsToInt<Number>
                              {
                                  static explicit IConvertsToInt<Number>.operator int(Number value) => 0;
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}