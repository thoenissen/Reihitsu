using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0372FileMustNotEndWithANewlineAnalyzer"/> and <see cref="RH0372FileMustNotEndWithANewlineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0372FileMustNotEndWithANewlineAnalyzerTests : AnalyzerTestsBase<RH0372FileMustNotEndWithANewlineAnalyzer, RH0372FileMustNotEndWithANewlineCodeFixProvider>
{
    /// <summary>
    /// Verifies that files without a trailing newline do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenFileDoesNotEndWithNewline()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a trailing newline is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingNewlineIsDetectedAndFixed()
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
                     Diagnostic(RH0372FileMustNotEndWithANewlineAnalyzer.DiagnosticId).WithSpan(3, 2, 4, 1).WithMessage(AnalyzerResources.RH0372MessageFormat));
    }
}