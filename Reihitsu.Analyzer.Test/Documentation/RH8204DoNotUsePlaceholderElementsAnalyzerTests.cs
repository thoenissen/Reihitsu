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
public class RH8204DoNotUsePlaceholderElementsAnalyzerTests : BatchCodeFixTestsBase<RH8204DoNotUsePlaceholderElementsAnalyzer, RH8204DoNotUsePlaceholderElementsCodeFixProvider>
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

    /// <summary>
    /// Verifies the code fix strips a placeholder whose content is not well-formed XML instead of throwing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixForPlaceholderWithMalformedContent()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>This method {|#0:<placeholder>handles a & b</placeholder>|}.</summary>
                              internal class TestClass
                              {
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>This method handles a & b.</summary>
                                   internal class TestClass
                                   {
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              namespace TestNamespace;

                              /// <summary>This method {|#0:<placeholder>does work</placeholder>|} and {|#1:<placeholder>returns nothing</placeholder>|}.</summary>
                              internal class TestClass
                              {
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   /// <summary>This method does work and returns nothing.</summary>
                                   internal class TestClass
                                   {
                                   }
                                   """;

        // Two placeholders on the same summary line produce disjoint text edits, so the batch fixer can apply
        // both without either shifting the other's span
        return new FixAllScenario(source, fixedSource, Diagnostics(RH8204DoNotUsePlaceholderElementsAnalyzer.DiagnosticId, AnalyzerResources.RH8204MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}