using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0358CodeMustNotContainTrailingWhitespaceAnalyzer"/> and <see cref="RH0358CodeMustNotContainTrailingWhitespaceCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0358CodeMustNotContainTrailingWhitespaceAnalyzerTests : AnalyzerTestsBase<RH0358CodeMustNotContainTrailingWhitespaceAnalyzer, RH0358CodeMustNotContainTrailingWhitespaceCodeFixProvider>
{
    /// <summary>
    /// Verifies that lines without trailing whitespace do not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenTrailingWhitespaceIsAbsent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace is detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyTrailingWhitespaceIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0;{|#0:    |}
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0358CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, AnalyzerResources.RH0358MessageFormat));
    }

    /// <summary>
    /// Verifies that trailing whitespace inside raw strings does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       value    
                                                       """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace inside verbatim strings does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyVerbatimStringsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Property => @"Value    
                                Another";
                                }
                                """;

        await Verify(testData);
    }
}