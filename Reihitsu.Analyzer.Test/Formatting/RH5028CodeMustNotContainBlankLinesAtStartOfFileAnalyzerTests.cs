using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer"/> and <see cref="RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzerTests : BatchCodeFixTestsBase<RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer, RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that files starting with content do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenFileStartsWithContent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that leading blank lines are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLeadingBlankLinesAreDetectedAndFixed()
    {
        const string testData = """
                                
                                
                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostic(RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.DiagnosticId).WithSpan(1, 1, 3, 1).WithMessage(AnalyzerResources.RH5028MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """


                                internal class TestClass
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                 }
                                 """;

        const string secondSource = """

                                    namespace TestNamespace
                                    {
                                    }
                                    """;
        const string secondFixedSource = """
                                         namespace TestNamespace
                                         {
                                         }
                                         """;

        // RH5028's analyzer (Reihitsu.Analyzer/Rules/Layout/RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.cs)
        // reports at most one diagnostic per syntax tree, so the Fix All scenario needs two documents to report
        // two diagnostics at all
        return new FixAllScenario(testData,
                                  fixedData,
                                  [
                                      Diagnostic(RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.DiagnosticId).WithSpan("/0/Test0.cs", 1, 1, 3, 1).WithMessage(AnalyzerResources.RH5028MessageFormat),
                                      Diagnostic(RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.DiagnosticId).WithSpan("/0/Test1.cs", 1, 1, 2, 1).WithMessage(AnalyzerResources.RH5028MessageFormat)
                                  ],
                                  config =>
                                  {
                                      config.TestState.Sources.Add(("/0/Test1.cs", secondSource));
                                      config.FixedState.Sources.Add(("/0/Test1.cs", secondFixedSource));

                                      // Fix All in document scope corrects one document per iteration, and this rule reports at most one
                                      // diagnostic per file, so two documents need at most two iterations
                                      config.NumberOfFixAllIterations = -2;
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}