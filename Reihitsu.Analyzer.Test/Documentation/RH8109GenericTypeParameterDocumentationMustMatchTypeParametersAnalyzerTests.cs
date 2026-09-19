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

    #endregion // Tests
}