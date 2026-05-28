using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8204DoNotUsePlaceholderElementsAnalyzer"/> and
/// <see cref="RH8204DoNotUsePlaceholderElementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8204DoNotUsePlaceholderElementsAnalyzerTests : AnalyzerTestsBase<RH8204DoNotUsePlaceholderElementsAnalyzer, RH8204DoNotUsePlaceholderElementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic and code fix for a placeholder tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForPlaceholderElement()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>This method {|#0:<placeholder>does work</placeholder>|}.</summary>
                              internal class TestClass
                              {
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>This method does work.</summary>
                                   internal class TestClass
                                   {
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat));
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
                              
                              /// <summary>This method {|#0:<placeholder>does work</placeholder>|}.</summary>
                              internal class TestClass
                              {
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}