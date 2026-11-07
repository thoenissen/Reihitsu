using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Test methods for <see cref="RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer"/> and <see cref="RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzerTests : BatchCodeFixTestsBase<RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer, RH8305SummaryElementMustSpanAtLeastThreeLinesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that a correctly formatted three-line summary does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-002")]
    public async Task VerifyNoDiagnosticsForCorrectThreeLineSummary()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Summary text
                                    /// </summary>
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single-line summary is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-001")]
    [Requirement("RH8305-003")]
    [Requirement("RH8305-004")]
    public async Task VerifySingleLineSummaryIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>Summary text</summary>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Summary text
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line summary with inline XML content is detected and fixed without breaking the content
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-001")]
    [Requirement("RH8305-003")]
    [Requirement("RH8305-004")]
    public async Task VerifySingleLineSummaryWithInlineXmlIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>Uses <see cref="string"/> values</summary>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Uses <see cref="string"/> values
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that a multiline summary with multiple content lines does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-002")]
    public async Task VerifyNoDiagnosticsForMultilineSummaryWithMultipleContentLines()
    {
        const string testData = """
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

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a two-line summary with content on start-tag line produces a diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-001")]
    [Requirement("RH8305-003")]
    [Requirement("RH8305-004")]
    public async Task VerifyDiagnosticForTwoLineSummaryWithContentOnStartTagLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary>Summary text
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
                                     /// Summary text
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that an empty single-line summary is detected and fixed to the three-line form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-001")]
    [Requirement("RH8305-003")]
    public async Task VerifyEmptySingleLineSummaryIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// {|#0:<summary></summary>|}
                                    void Method()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// 
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
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
                                  /// <summary>Summary text</summary>
                                  internal void Execute()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that summaries whose end tag starts on the line of the start tag or on the next line are reported,
    /// also in a later documentation comment of the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-001")]
    [Requirement("RH8305-003")]
    public async Task VerifySummariesOnFewerThanThreeLinesAreReported()
    {
        const string testData = """
                                class C
                                {
                                    /// {|#0:<summary>Runs</summary>|}
                                    void M()
                                    {
                                    }

                                    /// {|#1:<summary>Stops
                                    /// </summary>|}
                                    void N()
                                    {
                                    }

                                    /// {|#2:<summary>
                                    /// Value</summary>|}
                                    int A;

                                    /// <remarks>Remarks</remarks>

                                    /// {|#3:<summary>Count</summary>|}
                                    int B;
                                }
                                """;

        await Verify(testData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that summaries on three lines, an empty summary element, a nested summary, and a summary in a
    /// delimited documentation comment are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-002")]
    public async Task VerifyOtherSummariesDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    void M()
                                    {
                                    }

                                    /// <summary/>
                                    void N()
                                    {
                                    }

                                    /// <summary>Stops
                                    /// now
                                    /// </summary>
                                    void O()
                                    {
                                    }

                                    /// <remarks><summary>Nested</summary></remarks>
                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    int A;

                                    /** <summary>Count</summary> */
                                    int B;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix puts the start tag, each content line, and the end tag on documentation lines of their own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifyFixLaysTheSummaryOutOnThreeLines()
    {
        const string testData = """
                                class C
                                {
                                    /// {|#0:<summary>Runs <see cref="C"/></summary>|}
                                    void M()
                                    {
                                    }

                                    /// {|#1:<summary>Stops
                                    /// now</summary>|}
                                    void N()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Runs <see cref="C"/>
                                     /// </summary>
                                     void M()
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

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix writes the new documentation lines with CRLF line breaks in a CRLF document
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifyFixLaysTheSummaryOutWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Runs</summary>
                                    void M()
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
                                     void M()
                                     {
                                     }
                                 }
                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), actual);
    }

    /// <summary>
    /// Verifies that a one-line summary is not reported when documentation comments are not parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-001")]
    [Requirement("SHARED-DOC-008")]
    public async Task VerifyOneLineSummaryDoesNotProduceDiagnosticsWhenDocumentationModeIsNone()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Runs</summary>
                                    void M()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: summaries in comments that the formatter leaves as written are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-002")]
    public async Task VerifySpecificationExampleSummariesInCommentsLeftAsWrittenDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>S
                                    int A;

                                    /// <summary>S</returns>
                                    int B;

                                    /// <summary>Gets the value.</summary>
                                    /// <remarks>Text
                                    /// more
                                    int D;

                                    int E; /// <summary>end</summary>
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a summary without an end tag is not reported, so no fix can turn the field into comment text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-002")]
    public async Task VerifySummaryWithoutEndTagDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>S
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a summary closed by an end tag of another name is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-002")]
    public async Task VerifySummaryClosedByEndTagOfAnotherNameDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>S</returns>
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a complete one-line summary is not reported when another element of its comment has no end tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-002")]
    public async Task VerifyCompleteSummaryBesideElementWithoutEndTagDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Gets the value.</summary>
                                    /// <remarks>Text
                                    /// more
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a one-line summary kept behind the last field of a type is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-002")]
    public async Task VerifySummaryKeptBehindCodeDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    int E; /// <summary>end</summary>
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// {|#0:<summary>First summary</summary>|}
                                  void First()
                                  {
                                  }

                                  /// {|#1:<summary>Second summary</summary>|}
                                  void Second()
                                  {
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       /// <summary>
                                       /// First summary
                                       /// </summary>
                                       void First()
                                       {
                                       }

                                       /// <summary>
                                       /// Second summary
                                       /// </summary>
                                       void Second()
                                       {
                                       }
                                   }
                                   """;

        // The two members are directly adjacent, so the fix formats each declaration next to the one blank
        // line separating them; this is the shape most likely to have either rewrite reach into the other's
        // leading trivia
        return new FixAllScenario(source, fixedSource, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}