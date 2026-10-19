using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8108GenericTypeParametersMustBeDocumentedAnalyzer"/>
/// </summary>
[TestClass]
public class RH8108GenericTypeParametersMustBeDocumentedAnalyzerTests : AnalyzerTestsBase<RH8108GenericTypeParametersMustBeDocumentedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing type parameter comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8108-001")]
    [Requirement("RH8108-002")]
    public async Task VerifyDiagnosticForMissingTypeParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  /// <summary>Creates a value.</summary>
                                  internal T Create<{|#0:T|}>()
                                  {
                                      return default;
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8108GenericTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8108MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a missing type parameter comment on a partial type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8108-001")]
    public async Task VerifyDiagnosticForMissingTypeParameterDocumentationOnPartialType()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a generic value.</summary>
                              internal partial class TestClass<{|#0:T|}>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8108GenericTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8108MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8108-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class TestClass
                              {
                                  /// <summary>Creates a value.</summary>
                                  internal T Create<{|#0:T|}>()
                                  {
                                      return default;
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: each undocumented type parameter of a documented type, delegate, or method is reported on its name, while undocumented declarations, inheritdoc, explicit implementations, and local functions are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8108-001")]
    [Requirement("RH8108-002")]
    [Requirement("SHARED-DOC-009")]
    [Requirement("SHARED-DOC-011")]
    public async Task VerifySpecificationExampleReportsUndocumentedTypeParameters()
    {
        const string source = """
                              /// <summary>
                              /// Contract
                              /// </summary>
                              /// <typeparam name="T">Element</typeparam>
                              public interface I<T>
                              {
                                  /// <summary>
                                  /// Converts
                                  /// </summary>
                                  /// <typeparam name="U">Target</typeparam>
                                  void Convert<U>();
                              }

                              /// <summary>
                              /// Factory
                              /// </summary>
                              public delegate T Factory<{|#0:T|}>();

                              /// <summary>
                              /// Pair
                              /// </summary>
                              /// <typeparam name="TFirst">First</typeparam>
                              public record Pair<TFirst, {|#1:TSecond|}>;

                              /// <summary>
                              /// Service
                              /// </summary>
                              public class C : I<int>
                              {
                                  /// <summary>
                                  /// Creates
                                  /// </summary>
                                  private T Create<{|#2:T|}>() => default;

                                  public void Undocumented<T>()
                                  {
                                  }

                                  /// <inheritdoc/>
                                  public void Convert<U>()
                                  {
                                  }

                                  /// <summary>
                                  /// Converts
                                  /// </summary>
                                  void I<int>.Convert<U>()
                                  {
                                  }

                                  private void Run()
                                  {
                                      void Local<T>()
                                      {
                                      }
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8108GenericTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8108MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the type parameters of a documented explicit interface implementation are not reported, while the same type parameter of an implicit implementation is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8108-001")]
    [Requirement("SHARED-DOC-009")]
    public async Task VerifyNoDiagnosticForTypeParametersOfExplicitInterfaceImplementation()
    {
        const string source = """
                              /// <summary>Contract</summary>
                              internal interface I
                              {
                                  /// <summary>Converts</summary>
                                  /// <typeparam name="T">Target</typeparam>
                                  void Convert<T>();
                              }

                              /// <summary>Service</summary>
                              internal class C : I
                              {
                                  /// <summary>Converts</summary>
                                  void I.Convert<T>()
                                  {
                                  }

                                  /// <summary>Converts</summary>
                                  public void Convert<{|#0:T|}>()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8108GenericTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8108MessageFormat));
    }

    /// <summary>
    /// Verifies the undocumented type parameters of documented struct, interface, and record struct declarations are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8108-001")]
    [Requirement("RH8108-002")]
    public async Task VerifyDiagnosticForStructAndInterfaceTypeParameters()
    {
        const string source = """
                              /// <summary>Value</summary>
                              internal struct S<{|#0:T|}>
                              {
                              }

                              /// <summary>Contract</summary>
                              internal interface I<{|#1:T|}>
                              {
                              }

                              /// <summary>Pair</summary>
                              internal record struct P<{|#2:T|}>;
                              """;

        await Verify(source, Diagnostics(RH8108GenericTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8108MessageFormat, 3));
    }

    /// <summary>
    /// Verifies a later documentation comment documents a type parameter with its typeparam element and exempts the type parameters with its inheritdoc element, while a typeparam element nested in remarks does not count
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8108-001")]
    [Requirement("SHARED-DOC-011")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyLaterDocumentationCommentIsReadForTypeParameters()
    {
        const string source = """
                              internal class C
                              {
                                  /// <summary>Creates</summary>

                                  /// <typeparam name="T">Type</typeparam>
                                  public T A<T>() => default;

                                  /// <summary>Creates</summary>

                                  /// <inheritdoc/>
                                  public T B<T>() => default;

                                  /// <summary>Creates</summary>
                                  /// <remarks><typeparam name="T">Type</typeparam></remarks>
                                  public T D<{|#0:T|}>() => default;
                              }
                              """;

        await Verify(source, Diagnostics(RH8108GenericTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8108MessageFormat));
    }

    /// <summary>
    /// Verifies an included file documents a type parameter with its typeparam element and exempts the type parameters with its inheritdoc element, while included content with only a summary does not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8108-001")]
    [Requirement("SHARED-DOC-011")]
    [Requirement("SHARED-DOC-012")]
    public async Task VerifyIncludedDocumentationIsReadForTypeParameters()
    {
        const string source = """
                              internal class C
                              {
                                  /// <include file='docs.xml' path='doc/members/member[@name="A"]/*'/>
                                  public T A<T>() => default;

                                  /// <include file='docs.xml' path='doc/members/member[@name="B"]/*'/>
                                  public T B<{|#0:T|}>() => default;

                                  /// <include file='docs.xml' path='doc/members/member[@name="I"]/*'/>
                                  public T I<T>() => default;
                              }
                              """;

        const string documentation = """
                                     <doc>
                                         <members>
                                             <member name="A">
                                                 <summary>Value</summary>
                                                 <typeparam name="T">Type</typeparam>
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

        await Verify(source, test => test.SolutionTransforms.Add(CreateIncludedDocumentationTransform("docs.xml", documentation)), Diagnostics(RH8108GenericTypeParametersMustBeDocumentedAnalyzer.DiagnosticId, AnalyzerResources.RH8108MessageFormat));
    }

    /// <summary>
    /// Verifies the type parameters of a documented local function are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8108-001")]
    public async Task VerifyNoDiagnosticForDocumentedLocalFunction()
    {
        const string source = """
                              internal class C
                              {
                                  /// <summary>Runs</summary>
                                  public void Run()
                                  {
                                      /// <summary>Local</summary>
                                      void Local<T>()
                                      {
                                      }

                                      Local<int>();
                                  }
                              }
                              """;

        await Verify(source);
    }

    #endregion // Tests
}