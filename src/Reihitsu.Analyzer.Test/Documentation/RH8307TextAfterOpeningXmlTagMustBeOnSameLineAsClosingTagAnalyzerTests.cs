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
    /// Verifies that top-level elements whose content starts beside the start tag while the end tag is on a later line
    /// are reported, with text or a nested element as the first content
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
                                    /// Count
                                    /// </summary>
                                    /// {|#2:<remarks><see cref="C"/>
                                    /// tail</remarks>|}
                                    int B;
                                }
                                """;

        await Verify(testData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat, 3));
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

    /// <summary>
    /// Verifies the conforming part of the specification example of a layout rule: the half-wrapped element of a
    /// <c>///</c> comment, which the formatter lays out again, is reported, and one application of the fix produces what
    /// the formatter produces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyHalfWrappedElementThatTheFormatterChangesIsReportedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    /// {|#0:<summary>Total
                                    /// sum</summary>|}
                                    int A;
                                }
                                """;
        const string plainData = """
                                 class C
                                 {
                                     /// <summary>Total
                                     /// sum</summary>
                                     int A;
                                 }
                                 """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Total
                                     /// sum
                                     /// </summary>
                                     int A;
                                 }
                                 """;

        Assert.AreEqual(fixedData, Format(plainData), "The formatter must change the reported shape into the fixed one.");

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: elements of comments that the formatter leaves as written are not reported,
    /// also when the element itself is complete
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifySpecificationExampleElementsInCommentsLeftAsWrittenDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Gets A
                                    /// </summary>
                                    /// <remarks>Text
                                    /// more
                                    int A;

                                    /// <summary>
                                    /// Gets B
                                    /// </summary>
                                    /// <remarks>Text
                                    /// more</returns>
                                    int B;

                                    /// <summary>Gets D
                                    /// now</summary>
                                    /// <remarks>Text
                                    int D;

                                    int E; /// <remarks>a
                                    /// b</remarks>
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a remarks element without an end tag whose content starts beside its start tag is not reported, so
    /// no fix can turn the field into comment text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyElementWithoutEndTagDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Gets A
                                    /// </summary>
                                    /// <remarks>Text
                                    /// more
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an element closed by an end tag of another name is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyElementClosedByEndTagOfAnotherNameDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Gets B
                                    /// </summary>
                                    /// <remarks>Text
                                    /// more</returns>
                                    int B;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a complete half-wrapped element is not reported when another element of its comment has no end tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyCompleteElementBesideElementWithoutEndTagDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Gets D
                                    /// now</summary>
                                    /// <remarks>Text
                                    int D;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a half-wrapped element kept behind the last field of a type is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyElementKeptBehindCodeDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    int E; /// <remarks>a
                                    /// b</remarks>
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example for a type directly in the compilation unit: the fix lays out the element and
    /// keeps the type's code as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-004")]
    public async Task VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnit()
    {
        const string testData = """
                                /// <summary>
                                /// Total
                                /// </summary>
                                /// {|#0:<remarks>Sum
                                /// more</remarks>|}
                                public class C { int A(int a) => a; }
                                """;
        const string fixedData = """
                                 /// <summary>
                                 /// Total
                                 /// </summary>
                                 /// <remarks>
                                 /// Sum
                                 /// more
                                 /// </remarks>
                                 public class C { int A(int a) => a; }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example for a type directly in the compilation unit in a document whose line breaks
    /// are CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-004")]
    public async Task VerifySpecificationExampleKeepsTheRestOfATypeDirectlyInTheCompilationUnitWithCarriageReturnLineFeed()
    {
        var testData = NormalizeToCarriageReturnLineFeed("""
                                                         /// <summary>
                                                         /// Total
                                                         /// </summary>
                                                         /// {|#0:<remarks>Sum
                                                         /// more</remarks>|}
                                                         public class C { int A(int a) => a; }
                                                         """);
        var fixedData = NormalizeToCarriageReturnLineFeed("""
                                                          /// <summary>
                                                          /// Total
                                                          /// </summary>
                                                          /// <remarks>
                                                          /// Sum
                                                          /// more
                                                          /// </remarks>
                                                          public class C { int A(int a) => a; }
                                                          """);

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: an element in a comment that starts behind code is reported, and no fix is
    /// offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-005")]
    public async Task VerifyNoCodeFixForElementInCommentWrittenBehindCode()
    {
        const string source = """
                              class C
                              {
                                  int A; /// {|RH8307:<remarks>R
                                  /// more</remarks>|}
                                  int B;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a summary in a comment that starts behind code is reported and gets no fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-005")]
    public async Task VerifyNoCodeFixForSummaryInCommentWrittenBehindCode()
    {
        const string source = """
                              class C
                              {
                                  int A; /// {|RH8307:<summary>Total
                                  /// sum</summary>|}
                                  int B;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the fix keeps a method body that the formatter would lay out differently as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-004")]
    public async Task VerifyFixKeepsMethodBodyAsWritten()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Runs
                                    /// </summary>
                                    /// {|#0:<param name="v">The value
                                    /// </param>|}
                                    void M(int v){int a=1;}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <summary>
                                     /// Runs
                                     /// </summary>
                                     /// <param name="v">The value</param>
                                     void M(int v){int a=1;}
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps the nested members of a documented type as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-004")]
    public async Task VerifyFixKeepsNestedMembersOfDocumentedTypeAsWritten()
    {
        const string testData = """
                                namespace N
                                {
                                    /// {|#0:<summary>Total
                                    /// sum</summary>|}
                                    public class C
                                    {
                                        int A(int a) => a;

                                        void B(){ }

                                        ///Text
                                        int D;
                                    }
                                }
                                """;
        const string fixedData = """
                                 namespace N
                                 {
                                     /// <summary>
                                     /// Total
                                     /// sum
                                     /// </summary>
                                     public class C
                                     {
                                         int A(int a) => a;

                                         void B(){ }

                                         ///Text
                                         int D;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps the other documentation comment of the declaration and the blank line between the two
    /// comments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-004")]
    public async Task VerifyFixKeepsOtherDocumentationCommentAndBlankLine()
    {
        const string testData = """
                                class C
                                {
                                    /// <remarks>Remarks</remarks>

                                    /// {|#0:<summary>Total
                                    /// sum</summary>|}
                                    int B;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     /// <remarks>Remarks</remarks>

                                     /// <summary>
                                     /// Total
                                     /// sum
                                     /// </summary>
                                     int B;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out the element of a declaration directly in the compilation unit in one application
    /// and keeps the declaration's code, for several kinds of type declaration and wherever in the file it stands
    /// </summary>
    /// <param name="prefix">Code in front of the documented declaration</param>
    /// <param name="declaration">Documented declaration</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-004")]
    [DataRow("", "public class C\n{\n}")]
    [DataRow("", "public struct S\n{\n}")]
    [DataRow("", "public enum E\n{\n    A,\n}")]
    [DataRow("", "public delegate void D();")]
    [DataRow("", "public record R(int A);")]
    [DataRow("public class A\n{\n}\n\n", "public class C\n{\n}")]
    [DataRow("using System;\n\n", "public class C\n{\n}")]
    public async Task VerifyFixLaysOutElementOfDeclarationDirectlyInTheCompilationUnit(string prefix, string declaration)
    {
        var testData = $"{prefix}/// {{|#0:<summary>Total\n/// sum</summary>|}}\n{declaration}";
        var fixedData = $"{prefix}/// <summary>\n/// Total\n/// sum\n/// </summary>\n{declaration}";

        await Verify(testData, fixedData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment the formatter refuses in the body of the documented method does not withhold the fix:
    /// the fix changes only the reported element, so the body, refused comment included, stays as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-004")]
    public async Task VerifyFixLaysOutElementOfMemberThatHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              internal class TestClass
                              {
                                  /// {|#0:<summary><see cref="string"/>
                                  /// values</summary>|}
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
                                       /// <see cref="string"/>
                                       /// values
                                       /// </summary>
                                       private void Method()
                                       {
                                           int /* c */ y = 1;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that a half-wrapped top-level element of a delimited documentation comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyElementOfDelimitedDocumentationCommentDoesNotProduceDiagnostics()
    {
        const string source = """
                              class C
                              {
                                  /** <summary>
                                  Total
                                  </summary>
                                  <remarks>Sum
                                  more</remarks> */
                                  int A;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that a half-wrapped element nested in another element of a delimited documentation comment is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyNestedElementOfDelimitedDocumentationCommentDoesNotProduceDiagnostics()
    {
        const string source = """
                              class C
                              {
                                  /** <summary>
                                  Total
                                  </summary>
                                  <remarks>
                                  <para>Sum
                                  more</para>
                                  </remarks> */
                                  int A;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies that the same half-wrapped nested element in a single-line documentation comment is not reported either
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyNestedElementOfSingleLineDocumentationCommentDoesNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Total
                                    /// </summary>
                                    /// <remarks>
                                    /// <para>Sum
                                    /// more</para>
                                    /// </remarks>
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example: an element in the documentation comment of a positional record is reported
    /// once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-003")]
    public async Task VerifySpecificationExampleElementOfPositionalRecordIsReportedOnce()
    {
        const string testData = """
                                /// <summary>
                                /// Point
                                /// </summary>
                                /// {|#0:<remarks>Sum
                                /// more</remarks>|}
                                public record R(int A);
                                """;

        await Verify(testData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that an element in the documentation comment of a type with a primary constructor is reported once,
    /// wherever the type stands
    /// </summary>
    /// <param name="prefix">Code in front of the documented declaration</param>
    /// <param name="declaration">Documented declaration</param>
    /// <param name="suffix">Code behind the documented declaration</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-003")]
    [DataRow("", "public class C(int a)\n{\n    public int A => a;\n}", "")]
    [DataRow("", "public struct S(int a)\n{\n    public int A => a;\n}", "")]
    [DataRow("", "public record struct P(int A);", "")]
    [DataRow("", "public record class Q(int A)\n{\n}", "")]
    [DataRow("namespace N\n{\n", "public record R(int A);", "\n}")]
    [DataRow("public class O\n{\n", "public record R(int A);", "\n}")]
    public async Task VerifyElementOfTypeWithPrimaryConstructorIsReportedOnce(string prefix, string declaration, string suffix)
    {
        var testData = $"{prefix}/// <summary>\n/// Point\n/// </summary>\n/// {{|#0:<remarks>Sum\n/// more</remarks>|}}\n{declaration}{suffix}";

        await Verify(testData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies that an element in the documentation comment of an explicit constructor is still reported, next to the
    /// primary constructor whose second run is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-003")]
    public async Task VerifyElementOfExplicitConstructorIsReportedOnce()
    {
        const string testData = """
                                public class C
                                {
                                    /// <summary>
                                    /// Creates
                                    /// </summary>
                                    /// {|#0:<remarks>Sum
                                    /// more</remarks>|}
                                    public C()
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH8307TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, AnalyzerResources.RH8307MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: half-wrapped top-level elements of delimited documentation comments are not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifySpecificationExampleElementsOfDelimitedDocumentationCommentsDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /** <summary>Total
                                     * sum</summary> */
                                    int A;

                                    /** <summary>
                                    Count
                                    </summary>
                                    <remarks>Sum
                                    more</remarks> */
                                    int B;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example: half-wrapped elements nested inside another element, a paragraph and an inline
    /// formatting element, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifySpecificationExampleNestedElementsDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    /// <remarks>
                                    /// <para>First
                                    /// second</para>
                                    /// Text <b>bold
                                    /// still bold</b>
                                    /// </remarks>
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example of the qualifier "at the top level": the half-wrapped remarks element is
    /// reported, and the half-wrapped paragraph nested inside it is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-001")]
    [Requirement("RH8307-002")]
    public async Task VerifySpecificationExampleOnlyTheTopLevelElementIsReported()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    /// {|RH8307:<remarks>Text
                                    /// <para>First
                                    /// second</para>
                                    /// </remarks>|}
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a top-level element whose first content is a half-wrapped nested element is reported, and the nested
    /// element is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-001")]
    [Requirement("RH8307-002")]
    public async Task VerifyTopLevelElementStartingWithHalfWrappedNestedElementIsReportedAlone()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    /// {|RH8307:<remarks><para>First
                                    /// second</para>
                                    /// </remarks>|}
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that half-wrapped elements two levels deep, a list item and its description inside remarks, are not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyElementsNestedTwoLevelsDeepDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Value
                                    /// </summary>
                                    /// <remarks>
                                    /// <list type="bullet">
                                    /// <item><description>First
                                    /// second</description>
                                    /// </item>
                                    /// </list>
                                    /// </remarks>
                                    int A;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that half-wrapped inline elements nested in a summary, a code span and a reference with content, are not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8307-002")]
    public async Task VerifyInlineElementsNestedInSummaryDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>
                                    /// Uses <c>first
                                    /// second</c> and <see cref="C">the
                                    /// type</see>
                                    /// </summary>
                                    int A;
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