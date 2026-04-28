using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzer"/> and <see cref="RH0451NoContentShouldAppearAfterClosingXmlTagsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzerTests : AnalyzerTestsBase<RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzer, RH0451NoContentShouldAppearAfterClosingXmlTagsCodeFixProvider>
{
    /// <summary>
    /// Verifies that text on the next documentation line does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenTextStartsOnNextLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Summary.</summary>
                                    /// Additional text
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that another XML element after a closing tag is not reported by this analyzer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenAnotherElementFollowsClosingTag()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Summary.</summary><remarks>More details.</remarks>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that inline nested XML content remains allowed inside a larger element
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForNestedInlineXmlContent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Uses <see cref="string"/> values.</summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that text after a closing tag is detected and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTextAfterClosingTagIsDetectedAndRemoved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>Summary.</summary> {|#0:Additional text|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>Summary.</summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId, AnalyzerResources.RH0451MessageFormat));
    }

    /// <summary>
    /// Verifies that text after a self-closing tag is detected and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTextAfterSelfClosingTagIsDetectedAndRemoved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <inheritdoc/> {|#0:Additional text|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <inheritdoc/>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0451NoContentShouldAppearAfterClosingXmlTagsAnalyzer.DiagnosticId, AnalyzerResources.RH0451MessageFormat));
    }
}