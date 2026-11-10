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

    /// <summary>
    /// Verifies that a comment the formatter refuses in the body of the documented method does not withhold the fix:
    /// the fix changes only the summary, so the body, refused comment included, stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifyFixLaysOutSummaryOfMemberThatHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// {|#0:<summary>Summary text</summary>|}
                                  private void Method()
                                  {
                                      int /* c */ y = 1;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       /// <summary>
                                       /// Summary text
                                       /// </summary>
                                       private void Method()
                                       {
                                           int /* c */ y = 1;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example for a type directly in the compilation unit: the fix lays out the summary and
    /// keeps the other line of the comment and the type's code as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnit()
    {
        const string testData = """
                                /// {|#0:<summary>Runs</summary>|}
                                ///Text
                                public class C { int A(int a) => a; }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Runs
                                 /// </summary>
                                 ///Text
                                 public class C { int A(int a) => a; }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example for a type directly in the compilation unit in a document whose line breaks
    /// are CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnitWithCarriageReturnLineFeed()
    {
        var testData = NormalizeToCarriageReturnLineFeed("""
                                                         /// {|#0:<summary>Runs</summary>|}
                                                         ///Text
                                                         public class C { int A(int a) => a; }
                                                         """);
        var fixedData = NormalizeToCarriageReturnLineFeed("""
                                                          /// <summary>
                                                          /// Runs
                                                          /// </summary>
                                                          ///Text
                                                          public class C { int A(int a) => a; }
                                                          """);

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: the other documentation comment of the declaration and the blank line
    /// between the two comments stay
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifySpecificationExampleKeepsOtherDocumentationCommentAndBlankLine()
    {
        const string testData = """
                                class C
                                {
                                    /// <remarks>Remarks</remarks>

                                    /// {|#0:<summary>Count</summary>|}
                                    int B;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <remarks>Remarks</remarks>

                                     /// <summary>
                                     /// Count
                                     /// </summary>
                                     int B;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: a summary in a comment that starts behind code is reported, and no fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-005")]
    public async Task VerifyNoCodeFixForSummaryInCommentWrittenBehindCode()
    {
        const string source = """
                              class C
                              {
                                  int A; /// {|RH8305:<summary>Value</summary>|}
                                  int B;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a summary in a comment that starts behind code is reported and gets no fix in a document whose
    /// line breaks are CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-005")]
    public async Task VerifyNoCodeFixForSummaryInCommentWrittenBehindCodeWithCarriageReturnLineFeed()
    {
        const string source = """
                              class C
                              {
                                  int A; /// {|RH8305:<summary>Value</summary>|}
                                  int B;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(source));
    }

    /// <summary>
    /// Verifies that the fix keeps a method body that the formatter would lay out differently as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifyFixKeepsMethodBodyAsWritten()
    {
        const string testData = """
                                class C
                                {
                                    /// {|#0:<summary>Runs</summary>|}
                                    void M(){int a=1;
                                a++;}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     void M(){int a=1;
                                 a++;}
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps the empty body of a documented type in a namespace as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifyFixKeepsEmptyTypeBodyInNamespace()
    {
        const string testData = """
                                namespace N
                                {
                                    /// {|#0:<summary>S</summary>|}
                                    public class C { }
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                     /// <summary>
                                     /// S
                                     /// </summary>
                                     public class C { }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps the nested members of a documented type as written: an expression body, a body on
    /// one line, and a nested member's documentation comment with a missing space and a blank line inside its leading
    /// trivia
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifyFixKeepsNestedMembersOfDocumentedTypeAsWritten()
    {
        const string testData = """
                                namespace N
                                {
                                    /// {|#0:<summary>S</summary>|}
                                    public class C
                                    {
                                        int A(int a) => a;

                                        void B(){ }

                                        /// <remarks>R</remarks>

                                        ///Text
                                        int D;
                                    }
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                     /// <summary>
                                     /// S
                                     /// </summary>
                                     public class C
                                     {
                                         int A(int a) => a;

                                         void B(){ }

                                         /// <remarks>R</remarks>

                                         ///Text
                                         int D;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps a line of the same comment that the formatter would rewrite as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifyFixKeepsOtherLineOfTheSameCommentAsWritten()
    {
        const string testData = """
                                class C
                                {
                                    /// {|#0:<summary>Runs</summary>|}
                                    ///Text
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
                                     ///Text
                                     void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out the summary of a declaration directly in the compilation unit in one application
    /// and keeps the declaration's code, for every kind of type declaration and wherever in the file it stands
    /// </summary>
    /// <param name="prefix">Code in front of the documented declaration</param>
    /// <param name="declaration">Documented declaration</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    [DataRow("", "public class C\n{\n}")]
    [DataRow("", "public class C { }")]
    [DataRow("", "public struct S\n{\n}")]
    [DataRow("", "public interface I\n{\n}")]
    [DataRow("", "public enum E\n{\n    A,\n}")]
    [DataRow("", "public delegate void D();")]
    [DataRow("", "public record R(int A);")]
    [DataRow("public class A\n{\n}\n\n", "public class C\n{\n}")]
    [DataRow("using System;\n\n", "public class C\n{\n}")]
    [DataRow("// File header\n\n", "public class C\n{\n}")]
    public async Task VerifyFixLaysOutSummaryOfDeclarationDirectlyInTheCompilationUnit(string prefix, string declaration)
    {
        var testData = $"{prefix}/// {{|#0:<summary>S</summary>|}}\n{declaration}";
        var fixedData = $"{prefix}/// <summary>\n/// S\n/// </summary>\n{declaration}";

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out the summary of a type directly in the compilation unit in a document whose line
    /// breaks are CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifyFixLaysOutSummaryOfTypeDirectlyInTheCompilationUnitWithCarriageReturnLineFeed()
    {
        await Verify("/// {|#0:<summary>S</summary>|}\r\npublic class C\r\n{\r\n}",
                     "/// <summary>\r\n/// S\r\n/// </summary>\r\npublic class C\r\n{\r\n}",
                     Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that Fix All lays out the summaries of two types directly in the compilation unit in one pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-004")]
    public async Task VerifyFixAllLaysOutSummariesOfTypesDirectlyInTheCompilationUnit()
    {
        const string testData = """
                                /// {|#0:<summary>A</summary>|}
                                public class A { }

                                /// {|#1:<summary>B</summary>|}
                                public class B { }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// A
                                 /// </summary>
                                 public class A { }

                                 /// <summary>
                                 /// B
                                 /// </summary>
                                 public class B { }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat, 2));
    }

    /// <summary>
    /// Verifies the specification example: a summary in the documentation comment of a positional record is reported once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-003")]
    public async Task VerifySpecificationExampleSummaryOfPositionalRecordIsReportedOnce()
    {
        const string testData = """
                                /// {|#0:<summary>Point</summary>|}
                                public record R(int A);
                                """;

        await Verify(testData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that a summary in the documentation comment of a type with a primary constructor is reported once, wherever
    /// the type stands
    /// </summary>
    /// <param name="prefix">Code in front of the documented declaration</param>
    /// <param name="declaration">Documented declaration</param>
    /// <param name="suffix">Code behind the documented declaration</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-003")]
    [DataRow("", "public class C(int a)\n{\n    public int A => a;\n}", "")]
    [DataRow("", "public struct S(int a)\n{\n    public int A => a;\n}", "")]
    [DataRow("", "public record struct P(int A);", "")]
    [DataRow("", "public record class Q(int A)\n{\n}", "")]
    [DataRow("namespace N\n{\n", "public record R(int A);", "\n}")]
    [DataRow("public class O\n{\n", "public record R(int A);", "\n}")]
    public async Task VerifySummaryOfTypeWithPrimaryConstructorIsReportedOnce(string prefix, string declaration, string suffix)
    {
        var testData = $"{prefix}/// {{|#0:<summary>Point</summary>|}}\n{declaration}{suffix}";

        await Verify(testData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
    }

    /// <summary>
    /// Verifies that a summary in the documentation comment of an explicit constructor is still reported once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8305-003")]
    public async Task VerifySummaryOfExplicitConstructorIsReportedOnce()
    {
        const string testData = """
                                public class C
                                {
                                    /// {|#0:<summary>Creates</summary>|}
                                    public C()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8305SummaryElementMustSpanAtLeastThreeLinesAnalyzer.DiagnosticId, AnalyzerResources.RH8305MessageFormat));
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