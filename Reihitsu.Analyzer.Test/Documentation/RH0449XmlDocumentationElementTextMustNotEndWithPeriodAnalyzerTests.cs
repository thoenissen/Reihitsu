using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH0449XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer"/> and <see cref="RH0449XmlDocumentationElementTextMustNotEndWithPeriodCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0449XmlDocumentationElementTextMustNotEndWithPeriodAnalyzerTests : AnalyzerTestsBase<RH0449XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer, RH0449XmlDocumentationElementTextMustNotEndWithPeriodCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies that supported XML documentation elements without trailing periods do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenSupportedElementsDoNotEndWithPeriod()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Validates the input
                                    /// </summary>
                                    /// <param name="value">The value to validate</param>
                                    /// <returns>True if the input is valid</returns>
                                    bool Method(string value)
                                    {
                                        return value.Length > 0;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multiline summary ending with a period is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultilineSummaryEndingWithPeriodIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// This method validates the input{|#0:.|}
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// This method validates the input
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0449XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.DiagnosticId, AnalyzerResources.RH0449MessageFormat));
    }

    /// <summary>
    /// Verifies that a parameter element ending with a period is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyParamEndingWithPeriodIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <param name="value">The <see cref="string"/> value{|#0:.|}</param>
                                    void Method(string value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <param name="value">The <see cref="string"/> value</param>
                                     void Method(string value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0449XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.DiagnosticId, AnalyzerResources.RH0449MessageFormat));
    }

    /// <summary>
    /// Verifies that a returns element ending with a period is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyReturnsEndingWithPeriodIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <returns>True if the input is valid{|#0:.|}</returns>
                                    bool Method()
                                    {
                                        return true;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <returns>True if the input is valid</returns>
                                     bool Method()
                                     {
                                         return true;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0449XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.DiagnosticId, AnalyzerResources.RH0449MessageFormat));
    }

    /// <summary>
    /// Verifies that unsupported inline code elements do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForUnsupportedCodeElement()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// <code>return value.</code>
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Members
}