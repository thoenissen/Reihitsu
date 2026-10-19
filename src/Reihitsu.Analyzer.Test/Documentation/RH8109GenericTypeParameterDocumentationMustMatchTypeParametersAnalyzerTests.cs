using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer"/>
/// </summary>
[TestClass]
public class RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzerTests : AnalyzerTestsBase<RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for type parameter documentation in the wrong order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8109-001")]
    [Requirement("RH8109-002")]
    [Requirement("SHARED-DOC-015")]
    public async Task VerifyDiagnosticForTypeParameterDocumentationInWrongOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a pair.</summary>
                              /// <typeparam name="TFirst">First value.</typeparam>
                              /// {|#0:<typeparam name="TMissing">Missing value.</typeparam>|}
                              internal class Pair<TFirst, TSecond>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8109MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for every tag when correctly named type parameters are documented out of order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8109-001")]
    [Requirement("RH8109-002")]
    [Requirement("SHARED-DOC-015")]
    public async Task VerifyDiagnosticForTypeParameterDocumentationInPermutedOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a pair.</summary>
                              /// {|#0:<typeparam name="TSecond">Second value.</typeparam>|}
                              /// {|#1:<typeparam name="TFirst">First value.</typeparam>|}
                              internal class Pair<TFirst, TSecond>
                              {
                              }
                              """;

        await Verify(source, Diagnostics(RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8109MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when type parameter documentation matches the declared order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8109-001")]
    [Requirement("SHARED-DOC-015")]
    public async Task VerifyNoDiagnosticForTypeParameterDocumentationInDeclaredOrder()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a pair.</summary>
                              /// <typeparam name="TFirst">First value.</typeparam>
                              /// <typeparam name="TSecond">Second value.</typeparam>
                              internal class Pair<TFirst, TSecond>
                              {
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that type parameter tags nested in remarks are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8109-001")]
    [Requirement("SHARED-DOC-015")]
    public async Task VerifyNoDiagnosticForTypeParameterDocumentationNestedInRemarks()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>Represents a value.</summary>
                              /// <remarks><typeparam name="TMissing">Nested parameter.</typeparam></remarks>
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
    [Requirement("RH8109-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              /// <summary>Represents a pair.</summary>
                              /// <typeparam name="TFirst">First value.</typeparam>
                              /// {|#0:<typeparam name="TMissing">Missing value.</typeparam>|}
                              internal class Pair<TFirst, TSecond>
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: typeparam elements in another order than the type parameters, or after the last one, are reported on the whole element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8109-001")]
    [Requirement("RH8109-002")]
    public async Task VerifySpecificationExampleReportsTypeParameterElementsThatDoNotFollowTheTypeParameters()
    {
        const string source = """
                              /// <summary>
                              /// Pair
                              /// </summary>
                              /// {|#0:<typeparam name="TSecond">Second</typeparam>|}
                              /// {|#1:<typeparam name="TFirst">First</typeparam>|}
                              public class Pair<TFirst, TSecond>
                              {
                              }

                              /// <summary>
                              /// Factory
                              /// </summary>
                              /// <typeparam name="T">Result</typeparam>
                              /// {|#2:<typeparam name="U">Extra</typeparam>|}
                              public delegate T Factory<T>();

                              /// <summary>
                              /// Contract
                              /// </summary>
                              public interface I
                              {
                                  /// <summary>
                                  /// Maps
                                  /// </summary>
                                  /// <typeparam name="TSource">Source</typeparam>
                                  /// <typeparam name="TTarget">Target</typeparam>
                                  void Map<TSource, TTarget>();
                              }
                              """;

        await Verify(source, Diagnostics(RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8109MessageFormat, 3));
    }

    /// <summary>
    /// Verifies the shared specification example: typeparam elements are paired with the type parameters by position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8109-001")]
    [Requirement("RH8109-002")]
    [Requirement("SHARED-DOC-015")]
    public async Task VerifySpecificationExamplePairsTypeParameterElementsByPosition()
    {
        const string source = """
                              /// <summary>
                              /// Pair
                              /// </summary>
                              /// {|#0:<typeparam name="TSecond">Second</typeparam>|}
                              /// {|#1:<typeparam name="TFirst">First</typeparam>|}
                              public class Pair<TFirst, TSecond>
                              {
                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param name="a">A</param>
                                  /// <param name="c">C</param>
                                  public void M(int a, int b)
                                  {
                                  }

                                  /// <summary>
                                  /// Runs
                                  /// </summary>
                                  /// <param>Unnamed</param>
                                  /// <param name="b">B</param>
                                  /// <param name="c">Extra</param>
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
                                  /// <param name="@event">Event</param>
                                  public void P(int @class, int @event)
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8109MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the typeparam elements of a method are checked, an explicit interface implementation included, while an unnamed element keeps its position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8109-001")]
    [Requirement("RH8109-002")]
    [Requirement("SHARED-DOC-015")]
    public async Task VerifyTypeParameterElementsOfMethodsAreCheckedIncludingExplicitImplementations()
    {
        const string source = """
                              /// <summary>Contract</summary>
                              internal interface I
                              {
                                  /// <summary>Maps</summary>
                                  /// <typeparam name="TSource">Source</typeparam>
                                  /// <typeparam name="TTarget">Target</typeparam>
                                  void Map<TSource, TTarget>();
                              }

                              /// <summary>Service</summary>
                              internal class C : I
                              {
                                  /// <summary>Maps</summary>
                                  /// {|#0:<typeparam name="TTarget">Target</typeparam>|}
                                  /// {|#1:<typeparam name="TSource">Source</typeparam>|}
                                  void I.Map<TSource, TTarget>()
                                  {
                                  }

                                  /// <summary>Maps</summary>
                                  /// <typeparam>Source</typeparam>
                                  /// <typeparam name="TTarget">Target</typeparam>
                                  /// {|#2:<typeparam name="TExtra">Extra</typeparam>|}
                                  public void Map<TSource, TTarget>()
                                  {
                                  }
                              }
                              """;

        await Verify(source, Diagnostics(RH8109GenericTypeParameterDocumentationMustMatchTypeParametersAnalyzer.DiagnosticId, AnalyzerResources.RH8109MessageFormat, 3));
    }

    #endregion // Tests
}