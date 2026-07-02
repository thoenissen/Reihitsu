using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer"/> and <see cref="RH8306XmlDocumentationElementTextMustNotEndWithPeriodCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzerTests : AnalyzerTestsBase<RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer, RH8306XmlDocumentationElementTextMustNotEndWithPeriodCodeFixProvider>
{
    #region Tests

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

        await Verify(testData, fixedData, Diagnostics(RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a multi-sentence summary does not produce a diagnostic on its trailing period
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenSummaryContainsMultipleSentences()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Validates the input. Throws when the value is empty.
                                    /// </summary>
                                    public void Method(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multi-sentence parameter description does not produce a diagnostic on its trailing period
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenParamContainsMultipleSentences()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <param name="value">The value to validate. It must not be empty.</param>
                                    public void Method(string value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multi-sentence summary using a question mark as an internal terminator does not produce a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenSummaryContainsQuestionTerminatedSentence()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Is the value valid? Returns the validation result.
                                    /// </summary>
                                    public bool Method()
                                    {
                                        return true;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single sentence containing a decimal number is still detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleSentenceWithDecimalNumberIsStillDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Returns the value 1.0 by default{|#0:.|}
                                    /// </summary>
                                    public double Method()
                                    {
                                        return 1.0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Returns the value 1.0 by default
                                     /// </summary>
                                     public double Method()
                                     {
                                         return 1.0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
    }

    /// <summary>
    /// Verifies that a single sentence containing an abbreviation is still detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySingleSentenceWithAbbreviationIsStillDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <param name="value">The value to validate, e.g. a name{|#0:.|}</param>
                                    public void Method(string value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <param name="value">The value to validate, e.g. a name</param>
                                     public void Method(string value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8306XmlDocumentationElementTextMustNotEndWithPeriodAnalyzer.DiagnosticId, AnalyzerResources.RH8306MessageFormat));
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

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// <summary>
                                  /// This method validates the input.
                                  /// </summary>
                                  internal void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    #endregion // Tests
}