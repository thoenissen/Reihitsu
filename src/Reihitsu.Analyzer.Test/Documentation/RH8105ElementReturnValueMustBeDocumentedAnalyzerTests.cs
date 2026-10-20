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
    [Requirement("RH8105-001")]
    [Requirement("RH8105-002")]
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
    [Requirement("RH8105-001")]
    [Requirement("RH8105-002")]
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
    [Requirement("RH8105-001")]
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
    [Requirement("RH8105-001")]
    [Requirement("RH8105-002")]
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
    [Requirement("RH8105-001")]
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
    [Requirement("RH8105-001")]
    [Requirement("SHARED-DOC-008")]
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

    /// <summary>
    /// Verifies the specification example: a documented member returning a value without a top-level returns or inheritdoc element is reported on its return type, while a later comment, inheritdoc, explicit implementations, undocumented members, and void are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8105-001")]
    [Requirement("RH8105-002")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifySpecificationExampleReportsMissingReturnValueDescriptions()
    {
        const string source = """
                              using System.Threading.Tasks;

                              /// <summary>
                              /// Factory
                              /// </summary>
                              public delegate {|#0:int|} Factory();

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
                                  private int _field;

                                  /// <summary>
                                  /// Counts
                                  /// </summary>
                                  private {|#1:int|} Count() => 0;

                                  /// <summary>
                                  /// Loads
                                  /// </summary>
                                  public async {|#2:Task|} LoadAsync() => await Task.CompletedTask;

                                  /// <summary>
                                  /// Refers
                                  /// </summary>
                                  public {|#3:ref int|} Refer() => ref _field;

                                  /// <summary>
                                  /// Finds
                                  /// </summary>
                                  /// <remarks>
                                  /// <returns>Nested</returns>
                                  /// </remarks>
                                  public {|#4:int|} Find() => 0;

                                  /// <summary>
                                  /// Measures
                                  /// </summary>
                                  /// <returns/>
                                  public int Measure() => 0;

                                  /// <summary>
                                  /// Sizes
                                  /// </summary>

                                  /// <returns>Size</returns>
                                  public int Size() => 0;

                                  /// <inheritdoc/>
                                  public int Get() => 0;

                                  /// <summary>
                                  /// Gets
                                  /// </summary>
                                  int I.Get() => 0;

                                  public int Undocumented() => 0;

                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  public void Run()
                                  {
                                  }

                                  /// <summary>
                                  /// Adds
                                  /// </summary>
                                  /// <param name="left">Left</param>
                                  /// <param name="right">Right</param>
                                  public static {|#5:C|} operator +(C left, C right) => left;

                                  /// <summary>
                                  /// Converts
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  public static implicit operator {|#6:int|}(C value) => 0;
                              }
                              """;

        await Verify(source, Diagnostics(RH8105ElementReturnValueMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8105MessageFormat, 7));
    }

    /// <summary>
    /// Verifies the specification example: a returns element in included content counts, while included content with only a summary does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8105-001")]
    [Requirement("RH8105-002")]
    public async Task VerifySpecificationExampleReadsIncludedReturnValueDescription()
    {
        const string source = """
                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="A"]/*'/>
                                  public int A() => 0;

                                  /// <include file='docs.xml' path='doc/members/member[@name="B"]/*'/>
                                  public {|#0:int|} B() => 0;
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>Value</summary>
                                                 <returns>Value</returns>
                                             </member>
                                             <member name="B">
                                                 <summary>Value</summary>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8105ElementReturnValueMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8105MessageFormat));
    }

    /// <summary>
    /// Verifies an inheritdoc element in a later documentation comment or in included content counts as the return value description, while one nested in remarks does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8105-001")]
    public async Task VerifyInheritdocInLaterCommentOrIncludedContentCounts()
    {
        const string source = """
                              internal class C
                              {
                                  /// <summary>Gets</summary>

                                  /// <inheritdoc/>
                                  public int A() => 0;

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public int B() => 0;

                                  /// <summary>Gets</summary>
                                  /// <remarks><inheritdoc/></remarks>
                                  public {|#0:int|} D() => 0;
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="I">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8105ElementReturnValueMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8105MessageFormat));
    }

    /// <summary>
    /// Verifies the diagnostic of a member returning by readonly reference covers ref readonly and the type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8105-001")]
    [Requirement("RH8105-002")]
    public async Task VerifyDiagnosticIsOnRefReadonlyReturnType()
    {
        const string source = """
                              internal class C
                              {
                                  private int _field;

                                  /// <summary>Refers</summary>
                                  public {|#0:ref readonly int|} Refer() => ref _field;
                              }
                              """;

        await Verify(source, Diagnostics(RH8105ElementReturnValueMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8105MessageFormat));
    }

    /// <summary>
    /// Verifies a ValueTask, a type parameter, and a generic Task are values, on interface methods and extension block methods as well, while local functions are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8105-001")]
    [Requirement("RH8105-002")]
    public async Task VerifyDiagnosticForEveryReturnTypeOtherThanVoid()
    {
        const string source = """
                              using System.Threading.Tasks;

                              /// <summary>Contract</summary>
                              internal interface I
                              {
                                  /// <summary>Loads</summary>
                                  {|#0:ValueTask|} LoadAsync();

                                  /// <summary>Gets</summary>
                                  /// <typeparam name="T">Type</typeparam>
                                  {|#1:T|} Get<T>();
                              }

                              /// <summary>Extensions</summary>
                              internal static class Extensions
                              {
                                  /// <summary>Text values</summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                      /// <summary>Counts</summary>
                                      public {|#2:Task<int>|} CountAsync() => Task.FromResult(0);
                                  }

                                  /// <summary>Runs</summary>
                                  public static void Run()
                                  {
                                      /// <summary>Local</summary>
                                      int Local() => 0;

                                      Local();
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8105ElementReturnValueMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8105MessageFormat, 3));
    }

    #endregion // Tests
}