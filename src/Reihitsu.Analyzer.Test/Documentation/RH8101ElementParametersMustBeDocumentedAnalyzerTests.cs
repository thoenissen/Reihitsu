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
    [Requirement("RH8101-001")]
    [Requirement("RH8101-002")]
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
    [Requirement("RH8101-001")]
    [Requirement("SHARED-DOC-008")]
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
    [Requirement("RH8101-001")]
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
    [Requirement("RH8101-001")]
    [Requirement("RH8101-002")]
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
    [Requirement("RH8101-001")]
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
    [Requirement("RH8101-001")]
    [Requirement("RH8101-002")]
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
    [Requirement("RH8101-001")]
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
    [Requirement("RH8101-001")]
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
    [Requirement("RH8101-001")]
    [Requirement("RH8101-002")]
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
    [Requirement("RH8101-001")]
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
    [Requirement("RH8101-001")]
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

    /// <summary>
    /// Verifies a parameter declared with the verbatim prefix is matched to its documentation by its value, so
    /// <c>@class</c> documented by <c>name="class"</c> is not reported while the undocumented <c>@event</c> is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-002")]
    [Requirement("RH8101-001")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyVerbatimParameterIsMatchedToItsDocumentationByItsValue()
    {
        const string source = """
                              class C
                              {
                                  /// <summary>Summary</summary>
                                  /// <param name="class">Class</param>
                                  /// <param name="value">Value</param>
                                  void M(int @class, int value, int {|#0:@event|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: each undocumented parameter of a documented method, constructor, delegate, indexer, operator, conversion operator, or primary constructor is reported on its name, while undocumented declarations, inheritdoc, explicit implementations, and local functions are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8101-001")]
    [Requirement("RH8101-002")]
    [Requirement("SHARED-DOC-009")]
    [Requirement("SHARED-DOC-011")]
    public async Task VerifySpecificationExampleReportsUndocumentedParameters()
    {
        const string source = """
                              /// <summary>
                              /// Point
                              /// </summary>
                              /// <param name="X">X</param>
                              public record Point(int X, int {|#0:Y|});

                              /// <summary>
                              /// Handler
                              /// </summary>
                              public delegate void Handler(int {|#1:value|});

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
                                  /// Creates
                                  /// </summary>
                                  public C(int {|#2:size|})
                                  {
                                  }

                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param name="a">A</param>
                                  private void M(int a, int {|#3:b|})
                                  {
                                  }

                                  public void N(int a)
                                  {
                                  }

                                  /// <inheritdoc/>
                                  public void O(int a)
                                  {
                                  }

                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  void I.Run(int value)
                                  {
                                  }

                                  /// <summary>
                                  /// Item
                                  /// </summary>
                                  public int this[int {|#4:index|}] => 0;

                                  /// <summary>
                                  /// Adds
                                  /// </summary>
                                  /// <param name="left">Left</param>
                                  /// <returns>Sum</returns>
                                  public static C operator +(C left, C {|#5:right|}) => left;

                                  /// <summary>
                                  /// Converts
                                  /// </summary>
                                  /// <returns>Value</returns>
                                  public static implicit operator int(C {|#6:value|}) => 0;

                                  private void P()
                                  {
                                      void Local(int value)
                                      {
                                      }
                                  }
                              }

                              /// <summary>
                              /// Extensions
                              /// </summary>
                              public static class Extensions
                              {
                                  /// <summary>
                                  /// Text values
                                  /// </summary>
                                  /// <param name="value">Value</param>
                                  extension(string value)
                                  {
                                      /// <summary>
                                      /// Counts
                                      /// </summary>
                                      public int Count(char {|#7:c|}) => 0;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat, 8));
    }

    /// <summary>
    /// Verifies the parameters of a documented explicit interface implementation are not reported, while the same parameters of a public method with the same signature are
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8101-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForParametersOfExplicitInterfaceImplementation()
    {
        const string source = """
                              using System;

                              /// <summary>Formatter</summary>
                              internal class Formatter : IFormattable
                              {
                                  /// <summary>Formats</summary>
                                  string IFormattable.ToString(string format, IFormatProvider formatProvider) => "";

                                  /// <summary>Formats</summary>
                                  public string ToString(string {|#0:format|}, IFormatProvider {|#1:formatProvider|}) => "";
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no parameter is reported when a later documentation comment of the declaration has a top-level inheritdoc element, while one nested in remarks does not exempt the parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8101-001")]
    [Requirement("SHARED-DOC-011")]
    public async Task VerifyNoDiagnosticWhenLaterDocumentationCommentHasInheritdoc()
    {
        const string source = """
                              internal class C
                              {
                                  /// <summary>Runs</summary>

                                  /// <inheritdoc/>
                                  public void M(int value)
                                  {
                                  }

                                  /// <summary>Runs</summary>
                                  /// <remarks><inheritdoc/></remarks>
                                  public void N(int {|#0:value|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies a top-level param element in a later documentation comment documents the parameter, while a param element nested in remarks does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8101-001")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyParameterIsDocumentedByLaterDocumentationComment()
    {
        const string source = """
                              internal class C
                              {
                                  /// <summary>Runs</summary>

                                  /// <param name="value"></param>
                                  public void M(int value)
                                  {
                                  }

                                  /// <summary>Runs</summary>
                                  /// <remarks><param name="value">Value</param></remarks>
                                  public void N(int {|#0:value|})
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies an included file documents a parameter with its param element and exempts the parameters with its inheritdoc element, while included content with only a summary does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8101-001")]
    [Requirement("SHARED-DOC-011")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyIncludedDocumentationIsReadForParameters()
    {
        const string source = """
                              internal class C
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="A"]/*'/>
                                  public void A(int value)
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="B"]/*'/>
                                  public void B(int {|#0:value|})
                                  {
                                  }

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public void I(int value)
                                  {
                                  }
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>Value</summary>
                                                 <param name="value">Value</param>
                                             </member>
                                             <member name="B">
                                                 <summary>Value</summary>
                                             </member>
                                             <member name="I">
                                                 <inheritdoc/>
                                             </member>
                                         </members>
                                     </doc>
                                     """;

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies an undocumented parameter of a documented interface method is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8101-001")]
    [Requirement("RH8101-002")]
    public async Task VerifyDiagnosticForInterfaceMethodParameter()
    {
        const string source = """
                              internal interface I
                              {
                                  /// <summary>Runs</summary>
                                  void Run(int {|#0:value|});
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies the parameters of a documented local function and of a lambda are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8101-001")]
    public async Task VerifyNoDiagnosticForDocumentedLocalFunctionAndLambda()
    {
        const string source = """
                              using System;

                              internal class C
                              {
                                  /// <summary>Runs</summary>
                                  public void Run()
                                  {
                                      /// <summary>Local</summary>
                                      void Local(int value)
                                      {
                                      }

                                      Func<int, int> lambda = value => value;

                                      Local(lambda(0));
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example of a parameter in the middle of the list: the undocumented parameter
    /// <c>b</c> between two documented parameters is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-006")]
    [Requirement("RH8101-001")]
    public async Task VerifyUndocumentedParameterInTheMiddleOfTheListIsReported()
    {
        const string source = """
                              class C
                              {
                                  /// <summary>Summary</summary>
                                  /// <param name="a">A</param>
                                  /// <param name="c">C</param>
                                  void M(int a, int {|#0:b|}, int c)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8101ElementParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8101MessageFormat));
    }

    /// <summary>
    /// Verifies every parameter counts as documented whatever the position of its <c>param</c> element among the others,
    /// so parameters documented in the reverse order of the parameter list are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-006")]
    [Requirement("RH8101-001")]
    public async Task VerifyParametersDocumentedInAnotherOrderAreNotReported()
    {
        const string source = """
                              class C
                              {
                                  /// <summary>Summary</summary>
                                  /// <param name="c">C</param>
                                  /// <param name="b">B</param>
                                  /// <param name="a">A</param>
                                  void M(int a, int b, int c)
                                  {
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}