using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer"/> and <see cref="RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzerTests : AnalyzerTestsBase<RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer, RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies that single-line XML documentation elements do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForSingleLineXmlDocumentationElement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <param name="value">The value</param>
                                    void Method(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiline XML documentation elements with content starting on the next line do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenTextStartsOnNewLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <remarks>
                                    /// First line
                                    /// Second line
                                    /// </remarks>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that elements with inline XML content on the opening-tag line are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedInlineXmlContentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary><see cref="string"/>
                                    /// values</summary>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// <see cref="string"/>
                                     /// values
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH0450MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned parameter element is detected and fixed by collapsing it to a single line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedParamElementIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<param name="value">The value
                                    /// </param>|}
                                    void Method(string value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <param name="value">The value</param>
                                     void Method(string value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH0450MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned summary element is detected and fixed by moving its content to the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedSummaryElementIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>First line
                                    /// Second line
                                    /// </summary>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// First line
                                     /// Second line
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH0450MessageFormat));
    }

    /// <summary>
    /// Verifies that a multiline remarks element is detected and fixed by moving the first content line below the opening tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedMultilineRemarksElementIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<remarks>First line
                                    /// Second line
                                    /// </remarks>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <remarks>
                                     /// First line
                                     /// Second line
                                     /// </remarks>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH0450MessageFormat));
    }

    #endregion // Members
}