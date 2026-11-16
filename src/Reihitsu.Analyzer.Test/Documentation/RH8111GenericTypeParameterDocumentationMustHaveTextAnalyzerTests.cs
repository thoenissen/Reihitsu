using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer"/>
/// </summary>
[TestClass]
public class RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzerTests : AnalyzerTestsBase<RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for an empty type parameter tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8111-001")]
    [Requirement("RH8111-002")]
    [Requirement("SHARED-DOC-014")]
    public async Task VerifyDiagnosticForEmptyTypeParameterDocumentation()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a generic type.</summary>
                              /// {|#0:<typeparam name="T"></typeparam>|}
                              internal class Repository<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8111MessageFormat));
    }

    /// <summary>
    /// Verifies that empty type parameter tags nested in remarks are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8111-001")]
    public async Task VerifyNoDiagnosticForEmptyTypeParameterNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a generic type.</summary>
                              /// <remarks><typeparam name="T"></typeparam></remarks>
                              internal class Repository<T>
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8111-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Represents a generic type.</summary>
                              /// {|#0:<typeparam name="T"></typeparam>|}
                              internal class Repository<T>
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: an empty or line-break-only typeparam element at the top level is reported on the whole element, on a class, a method, and an interface, while a nested one and one with text are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8111-001")]
    [Requirement("RH8111-002")]
    public async Task VerifySpecificationExampleReportsTypeParameterElementsWithoutText()
    {
        const string source = """
                              /// <summary>
                              /// Container
                              /// </summary>
                              /// {|#0:<typeparam name="T"></typeparam>|}
                              /// <typeparam name="U">Key</typeparam>
                              /// <remarks>
                              /// <typeparam name="V"></typeparam>
                              /// </remarks>
                              public class C<T, U>
                              {
                                  /// <summary>
                                  /// Creates
                                  /// </summary>
                                  /// {|#1:<typeparam name="TResult">
                                  /// </typeparam>|}
                                  /// <returns>Result</returns>
                                  public TResult Create<TResult>() => default;
                              }

                              /// <summary>
                              /// Contract
                              /// </summary>
                              /// {|#2:<typeparam name="T"/>|}
                              public interface I<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8111MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the shared specification example: an empty typeparam element counts as without text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8111-001")]
    [Requirement("RH8111-002")]
    [Requirement("SHARED-DOC-014")]
    public async Task VerifySpecificationExampleJudgesTypeParameterTextLikeSummaryText()
    {
        const string source = """
                              /// <summary>
                              /// Container
                              /// </summary>
                              /// {|#0:<typeparam name="T"></typeparam>|}
                              public class C<T>
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param name="a"/>
                                  /// <param name="b">&#32;</param>
                                  /// <param name="c">Value</param>
                                  /// <returns>
                                  /// </returns>
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

        await Verify(source, Diagnostics(RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8111MessageFormat));
    }

    /// <summary>
    /// Verifies an empty typeparam element is reported on a delegate, on a non-generic method, on an explicit interface implementation, and in a delimited comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8111-001")]
    [Requirement("RH8111-002")]
    [Requirement("SHARED-DOC-014")]
    public async Task VerifyDiagnosticForTypeParameterElementWithoutTextOnOtherDeclarations()
    {
        const string source = """
                              /// <summary>Factory</summary>
                              /// {|#0:<typeparam name="T">&#32;</typeparam>|}
                              internal delegate T Factory<T>();

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
                                  /// {|#1:<typeparam name="T"/>|}
                                  void I.Convert<T>()
                                  {
                                  }

                                  /// <summary>Runs</summary>
                                  /// {|#2:<typeparam name="T"></typeparam>|}
                                  public void Run()
                                  {
                                  }

                                  /**
                                   * <summary>Creates</summary>
                                   * {|#3:<typeparam name="T">
                                   * </typeparam>|}
                                   */
                                  public void Create<T>()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8111MessageFormat, 4));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a <c>&lt;typeparam&gt;</c> element without text on a union, and none for a
    /// union whose element has text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8111-001")]
    [Requirement("RH8111-002")]
    public async Task VerifyDiagnosticForUnionTypeParamElementWithoutText()
    {
        const string source = """
                              /// <summary>
                              /// Result
                              /// </summary>
                              /// {|#0:<typeparam name="TValue"></typeparam>|}
                              public union Result<TValue>(TValue, string);

                              /// <summary>
                              /// Option
                              /// </summary>
                              /// <typeparam name="T">Value</typeparam>
                              public union Option<T>(T, string);
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport), Diagnostics(RH8111GenericTypeParameterDocumentationMustHaveTextAnalyzer.DiagnosticId, AnalyzerResources.RH8111MessageFormat));
    }

    /// <summary>
    /// Verifies a diagnostic is reported for a <c>&lt;typeparam&gt;</c> element without text on a union, whether or not
    /// it names a type parameter of the union
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8111-001")]
    public async Task VerifyDiagnosticForUnionTypeParamElementWithoutTextNamingAnotherTypeParameter()
    {
        const string source = """
                              /// <summary>
                              /// Result
                              /// </summary>
                              /// <typeparam name="TValue">Value</typeparam>
                              /// {|RH8111:<typeparam name="TOther"></typeparam>|}
                              public union Result<TValue, TError>(TValue, TError);
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source, test => test.TestState.AdditionalReferences.Add(unionSupport));
    }

    /// <summary>
    /// Verifies a type parameter element without text is still reported under the default documentation mode on a union whose
    /// documentation comment holds a <c>&lt;param&gt;</c> element, which this rule judges from the syntax
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8111-001")]
    public async Task VerifyDiagnosticForUnionTypeParamElementWithoutTextWhoseDocumentationHoldsParamElement()
    {
        const string source = """
                              /// <summary>
                              /// Result
                              /// </summary>
                              /// {|RH8111:<typeparam name="TValue"></typeparam>|}
                              /// <param name="x">X</param>
                              public union Result<TValue>(TValue, string);
                              """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await Verify(source,
                     test =>
                     {
                         test.TestState.AdditionalReferences.Add(unionSupport);
                         test.CompilerDiagnostics = CompilerDiagnostics.None;
                     });
    }

    #endregion // Tests
}