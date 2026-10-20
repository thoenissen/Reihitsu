using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer"/>
/// </summary>
[TestClass]
public class RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzerTests : AnalyzerTestsBase<RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a missing type parameter name attribute
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8110-001")]
    [Requirement("RH8110-002")]
    [Requirement("SHARED-DOC-016")]
    public async Task VerifyDiagnosticForMissingTypeParameterName()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a generic type.</summary>
                              /// {|#0:<typeparam>Value.</typeparam>|}
                              internal class Repository<T>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8110MessageFormat));
    }

    /// <summary>
    /// Verifies that unnamed type parameter tags nested in remarks are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8110-001")]
    [Requirement("SHARED-DOC-016")]
    public async Task VerifyNoDiagnosticForUnnamedTypeParameterNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a generic type.</summary>
                              /// <remarks><typeparam>Nested parameter.</typeparam></remarks>
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
    [Requirement("RH8110-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Represents a generic type.</summary>
                              /// {|#0:<typeparam>Value.</typeparam>|}
                              internal class Repository<T>
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: a typeparam element without a name or with an empty name is reported on the whole element, whether or not the declaration is generic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8110-001")]
    [Requirement("RH8110-002")]
    public async Task VerifySpecificationExampleReportsTypeParameterElementsWithoutName()
    {
        const string source = """
                              /// <summary>
                              /// Container
                              /// </summary>
                              /// {|#0:<typeparam>Element</typeparam>|}
                              public class C<T>
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// {|#1:<typeparam/>|}
                                  public void Run()
                                  {
                                  }
                              }

                              /// <summary>
                              /// Handler
                              /// </summary>
                              /// {|#2:<typeparam name="">Argument</typeparam>|}
                              public delegate void Handler<T>(T value);
                              """;

        await Verify(source, Diagnostics(RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8110MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the shared specification example: a top-level typeparam element without a name attribute is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8110-001")]
    [Requirement("RH8110-002")]
    [Requirement("SHARED-DOC-016")]
    public async Task VerifySpecificationExampleReportsTypeParameterElementWithoutNameAttribute()
    {
        const string source = """
                              /// <summary>
                              /// Container
                              /// </summary>
                              /// {|#0:<typeparam>Element</typeparam>|}
                              public class C<T>
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param>First</param>
                                  /// <param name="">Second</param>
                                  /// <param name=" ">Third</param>
                                  /// <param Name="d">Fourth</param>
                                  /// <param name="e">Fifth</param>
                                  /// <remarks>
                                  /// <param>Nested</param>
                                  /// </remarks>
                                  public void M(int a, int b, int c, int d, int e)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8110MessageFormat));
    }

    /// <summary>
    /// Verifies a typeparam element whose name is only white space, or which has only a differently cased Name attribute, is reported, while a named one is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8110-001")]
    [Requirement("RH8110-002")]
    [Requirement("SHARED-DOC-016")]
    public async Task VerifyDiagnosticForWhiteSpaceAndDifferentlyCasedTypeParameterNames()
    {
        const string source = """
                              /// <summary>Map</summary>
                              /// {|#0:<typeparam name=" ">Key</typeparam>|}
                              /// {|#1:<typeparam Name="TValue">Value</typeparam>|}
                              /// <typeparam name="TOther">Other</typeparam>
                              internal class Map<TKey, TValue, TOther>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8110MessageFormat, 2));
    }

    /// <summary>
    /// Verifies unnamed typeparam elements are reported on struct, interface, record, record struct, and explicit interface implementation declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8110-001")]
    [Requirement("RH8110-002")]
    public async Task VerifyDiagnosticForUnnamedTypeParameterElementsOfOtherTypeParameterOwners()
    {
        const string source = """
                              /// <summary>Value</summary>
                              /// {|#0:<typeparam>T</typeparam>|}
                              internal struct S<T>
                              {
                              }

                              /// <summary>Pair</summary>
                              /// {|#1:<typeparam>T</typeparam>|}
                              internal record P<T>;

                              /// <summary>Pair</summary>
                              /// {|#2:<typeparam>T</typeparam>|}
                              internal record struct Q<T>;

                              /// <summary>Contract</summary>
                              /// {|#3:<typeparam>T</typeparam>|}
                              internal interface I<T>
                              {
                                  /// <summary>Converts</summary>
                                  /// <typeparam name="U">Target</typeparam>
                                  void Convert<U>();
                              }

                              /// <summary>Service</summary>
                              internal class C : I<int>
                              {
                                  /// <summary>Converts</summary>
                                  /// {|#4:<typeparam>Target</typeparam>|}
                                  void I<int>.Convert<U>()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8110GenericTypeParameterDocumentationMustDeclareParameterNameAnalyzer.DiagnosticId, AnalyzerResources.RH8110MessageFormat, 5));
    }

    #endregion // Tests
}