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
public class RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzerTests : AnalyzerTestsBase<RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer, RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider>
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
}