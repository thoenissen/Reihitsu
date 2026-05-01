using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzer"/> and <see cref="RH0371CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzerTests : AnalyzerTestsBase<RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzer, RH0371CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider>
{
    #region Members

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
                     Diagnostic(RH0371CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.DiagnosticId).WithSpan(1, 1, 3, 1).WithMessage(AnalyzerResources.RH0371MessageFormat));
    }

    #endregion // Members
}