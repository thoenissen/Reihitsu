using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer"/> and <see cref="RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzerTests : BatchCodeFixTestsBase<RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer, RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that single-line XML documentation elements do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
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
    [Requirement("RH8307-002")]
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
    [Requirement("RH8307-001")]
    [Requirement("RH8307-003")]
    [Requirement("RH8307-004")]
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

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned parameter element is detected and fixed by collapsing it to a single line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-001")]
    [Requirement("RH8307-003")]
    [Requirement("RH8307-004")]
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

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that a misaligned summary element is detected and fixed by moving its content to the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-001")]
    [Requirement("RH8307-003")]
    [Requirement("RH8307-004")]
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

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that a multiline remarks element is detected and fixed by moving the first content line below the opening tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-001")]
    [Requirement("RH8307-003")]
    [Requirement("RH8307-004")]
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

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// <summary><see cref="string"/>
                                  /// values</summary>
                                  internal void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that top-level and nested elements whose content starts beside the start tag while the end tag is on
    /// a later line are reported, with text or a nested element as the first content
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-001")]
    [Requirement("RH8307-003")]
    public async Task VerifyContentStartingBesideTheStartTagIsReported()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    /// {|#0:<param name="value">The value
                                    /// </param>|}
                                    void M(string value)
                                    {
                                    }

                                    /// {|#1:<summary>Stops
                                    /// now
                                    /// </summary>|}
                                    void N()
                                    {
                                    }

                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    /// <remarks>
                                    /// {|#2:<para>First
                                    /// second</para>|}
                                    /// </remarks>
                                    int A;

                                    /// <summary>
                                    /// Count
                                    /// </summary>
                                    /// {|#3:<remarks><see cref="C"/>
                                    /// tail</remarks>|}
                                    int B;
                                }
                                """;

        await Verify(testData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that elements on one line, elements whose content starts on a later line, and elements whose content
    /// is only white space are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyCompleteLayoutsDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Runs</summary>
                                    /// <param name="value">The value</param>
                                    void M(string value)
                                    {
                                    }

                                    /// <summary>
                                    /// Stops
                                    /// </summary>
                                    /// <remarks>
                                    /// Remarks</remarks>
                                    void N()
                                    {
                                    }

                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    /// <remarks>
                                    /// </remarks>
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix puts an element with one content line on one line and puts the content of a summary on
    /// lines of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-004")]
    public async Task VerifyFixLaysTheElementOutAsTheFormatterDoes()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    /// {|#0:<param name="value">The value
                                    /// </param>|}
                                    void M(string value)
                                    {
                                    }

                                    /// {|#1:<summary>Stops
                                    /// now
                                    /// </summary>|}
                                    void N()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     /// <param name="value">The value</param>
                                     void M(string value)
                                     {
                                     }

                                     /// <summary>
                                     /// Stops
                                     /// now
                                     /// </summary>
                                     void N()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat, 2));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// {|#0:<summary>First line
                                  /// Second line
                                  /// </summary>|}
                                  /// {|#1:<remarks>First remark
                                  /// Second remark
                                  /// </remarks>|}
                                  void Method()
                                  {
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       /// <summary>
                                       /// First line
                                       /// Second line
                                       /// </summary>
                                       /// <remarks>
                                       /// First remark
                                       /// Second remark
                                       /// </remarks>
                                       void Method()
                                       {
                                       }
                                   }
                                   """;

        // Both diagnostics resolve to the same member, so the batch fixer must apply the formatted rewrite of
        // that declaration once rather than duplicating its content across two identical changes
        return new FixAllScenario(source, fixedSource, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}