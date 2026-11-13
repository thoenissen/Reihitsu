using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer"/> and <see cref="RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzerTests : BatchCodeFixTestsBase<RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer, RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5406-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5406-001")]
    [Requirement("RH5406-002")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                            {|#0:Other(1,
                                                  2);|}
                                    }
                                
                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                             Other(1,
                                                   2);
                                         }
                                     }
                                 
                                     void Other(int value1, int value2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH5406MessageFormat));
    }

    /// <summary>
    /// Verifies the spec example: no fix is offered for a body on the line of its if that spans several lines, under LF and
    /// CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    [Requirement("RH5406-001")]
    [Requirement("RH5406-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task NoFixIsOfferedWhenAMultiLineChildStartsOnTheLineOfItsIf(string endOfLine)
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {|RH5406:Other(1,
                                                  2);|}
                                    }

                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testData, endOfLine));
    }

    /// <summary>
    /// Verifies that a single-line brace-less child statement is not flagged by RH5406 (it is reported by RH5405),
    /// so the two rules never report the same statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5406-001")]
    public async Task VerifySingleLineChildStatementIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a multi-line brace-less else child statement is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5406-001")]
    [Requirement("RH5406-002")]
    public async Task VerifyMultiLineElseChildStatementIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x)
                                    {
                                        if (x)
                                        {
                                        }
                                        else
                                            {|#0:Other(1,
                                                  2);|}
                                    }

                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(bool x)
                                     {
                                         if (x)
                                         {
                                         }
                                         else
                                         {
                                             Other(1,
                                                   2);
                                         }
                                     }

                                     void Other(int value1, int value2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH5406MessageFormat));
    }

    /// <summary>
    /// Verifies that when both the if-branch and the else-branch are multi-line brace-less child statements,
    /// the fix places the else keyword on its own line instead of appending it to the closing brace's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5406-001")]
    [Requirement("RH5406-002")]
    public async Task VerifyBothBranchesMultiLineUnbracedPlacesElseOnItsOwnLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x)
                                    {
                                        if (x)
                                            {|#0:Other(1,
                                                  2);|}
                                        else
                                            {|#1:Other(3,
                                                  4);|}
                                    }

                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(bool x)
                                     {
                                         if (x)
                                         {
                                             Other(1,
                                                   2);
                                         }
                                         else
                                         {
                                             Other(3,
                                                   4);
                                         }
                                     }

                                     void Other(int value1, int value2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH5406MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a body without braces that spans several lines is reported, whether it starts on the line of its
    /// else or on a later line, while a body on one line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5406-001")]
    [Requirement("RH5406-002")]
    [Requirement("SHARED-BRACE-001")]
    public async Task VerifyUnbracedBodyOnSeveralLinesIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5406:M(a,
                                              b);|}
                                        else {|RH5406:M(a,
                                                    b);|}
                                        if (a)
                                            M(a, b);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the body without braces of a while statement that spans several lines is not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    public async Task VerifyMultiLineLoopBodyIsNotChecked()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        while (a)
                                            M(a,
                                              b);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix wraps a body without braces that spans several lines in braces on their own lines in the
    /// column of the if
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWrapsMultiLineBodyInBraces()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5406:M(a,
                                              b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         if (a)
                                         {
                                             M(a,
                                               b);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix wraps a body without braces that spans several lines in braces on their own lines in the
    /// column of the if, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWrapsMultiLineBodyInBracesWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5406:M(a,
                                              b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         if (a)
                                         {
                                             M(a,
                                               b);
                                         }
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that no fix is offered for a body below its if that spans several lines in a document whose line breaks
    /// are all a lone carriage return, because the line breaks of the code the fix moves are not the detected line ending
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    [Requirement("SHARED-FIX-008")]
    public async Task NoFixIsOfferedInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                            {|RH5406:M(a,
                                              b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, "\r"));
    }

    /// <summary>
    /// Verifies that no fix is offered for a body on the line of its if that spans several lines in a document whose line
    /// breaks are all a lone carriage return
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForABodyOnTheLineOfItsIfInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a) {|RH5406:M(a,
                                                 b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, "\r"));
    }

    /// <summary>
    /// Verifies that the fix on the multi-line body of an <c>if</c> whose condition carries a trailing line comment keeps the
    /// comment behind the condition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public async Task VerifyFixKeepsTheTrailingCommentOfAnIfCondition()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) // c
                                            {|#0:Other(1,
                                                  2);|}
                                    }

                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         if (true) // c
                                         {
                                             Other(1,
                                                   2);
                                         }
                                     }

                                     void Other(int value1, int value2)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH5406MessageFormat));
    }

    /// <summary>
    /// Verifies a line comment behind a body that spans several lines is written once, behind the body inside the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWritesTrailingCommentOfMultiLineBodyOnce()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5406:M(a,
                                              b);|} // trail
                                        b++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         if (a)
                                         {
                                             M(a,
                                               b); // trail
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a line comment behind a body that spans several lines is written once, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWritesTrailingCommentOfMultiLineBodyOnceWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5406:M(a,
                                              b);|} // trail
                                        b++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         if (a)
                                         {
                                             M(a,
                                               b); // trail
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies the spec example: a body below its if that spans several lines keeps its continuation line in its column,
    /// under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsTheContinuationColumnOfABodyBelowItsIf(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5406:M(a,
                                                b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         if (a)
                                         {
                                             M(a,
                                                 b);
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a misindented body below its if that spans several lines keeps every line in its column,
    /// under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsEveryColumnOfAMisindentedMultiLineBodyBelowItsIf(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                          {|RH5406:M(a,
                                              b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         if (a)
                                         {
                                           M(a,
                                               b);
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that no fix is offered for an else body on the line of its else that spans several lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAMultiLineBodyOnTheLineOfItsElse()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                        {
                                            M(a, b);
                                        }
                                        else {|RH5406:M(a,
                                                    b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that a body on the line of its if that spans several lines because of a verbatim string gets no fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForABodyOnTheLineOfItsIfWithAVerbatimStringSpanningLines()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, string b)
                                    {
                                        if (a) {|RH5406:M(a, @"x
                                y");|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that a body below its if that holds a verbatim string spanning lines gets the fix and keeps the literal,
    /// under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsAVerbatimStringSpanningLinesInABodyBelowItsIf(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, string b)
                                    {
                                        if (a)
                                            {|RH5406:M(a, @"x
                                y");|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, string b)
                                     {
                                         if (a)
                                         {
                                             M(a, @"x
                                 y");
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that in a CRLF document an LF inside a verbatim string of the body below its if withholds the fix,
    /// because it lies in the code the fix moves
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    [Requirement("SHARED-FIX-008")]
    public async Task NoFixIsOfferedForALineFeedInsideAVerbatimStringOfTheBodyInACarriageReturnLineFeedDocument()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, string b)
                                    {
                                        if (a)
                                            {|RH5406:M(a, @"xLF
                                y");|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testCode).Replace("LF\r\n", "\n", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that a directive between two lines of a body below its if does not withhold the fix and keeps its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-BRACE-003")]
    public async Task FixIsOfferedForADirectiveBetweenTwoLinesOfABodyBelowItsIf()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5406:M(a,
                                #pragma warning disable CS0618
                                              b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         if (a)
                                         {
                                             M(a,
                                 #pragma warning disable CS0618
                                               b);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that no fix is offered when a pragma directive stands between the if and a body that spans several lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAPragmaDirectiveBetweenTheIfAndItsBody()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                #pragma warning disable CS0618
                                            {|RH5406:M(a,
                                              b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that a body below its if that holds braces of its own keeps all of its lines inside the new block, under
    /// LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsTheLinesOfABodyWithBracesOfItsOwn(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int[] xs)
                                    {
                                        if (a)
                                            {|RH5406:foreach (var x in xs)
                                            {
                                                M(a,xs);
                                            }|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int[] xs)
                                     {
                                         if (a)
                                         {
                                             foreach (var x in xs)
                                             {
                                                 M(a,xs);
                                             }
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a body below its if that holds a statement lambda keeps all of its lines inside the new block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task FixKeepsTheLinesOfABodyWithAStatementLambda()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, System.Action b)
                                    {
                                        if (a)
                                            {|RH5406:M(a, () =>
                                              {
                                                  M(a, null);
                                              });|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, System.Action b)
                                     {
                                         if (a)
                                         {
                                             M(a, () =>
                                               {
                                                   M(a, null);
                                               });
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that whitespace at the end of the last line of a body below its if, behind a delimited comment, stays there,
    /// and nothing is written behind the closing brace, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsTrailingWhitespaceOnTheLastBodyLineAndWritesNoneBehindTheClosingBrace(string endOfLine)
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   void M(bool a, int b)
                                                   {
                                                       if (a)
                                                           {|RH5406:M(a,
                                                             b);|} /* x */···
                                                   }
                                               }
                                               """);
        var fixedCode = ExpandWhitespaceMarkers("""
                                                class C
                                                {
                                                    void M(bool a, int b)
                                                    {
                                                        if (a)
                                                        {
                                                            M(a,
                                                              b); /* x */···
                                                        }
                                                    }
                                                }
                                                """);

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method1()
                                    {
                                        if (true)
                                            {|#0:Other(1,
                                                  2);|}
                                    }

                                    void Method2(bool y)
                                    {
                                        if (y)
                                            {|#1:Other(3,
                                                  4);|}
                                    }

                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method1()
                                     {
                                         if (true)
                                         {
                                             Other(1,
                                                   2);
                                         }
                                     }

                                     void Method2(bool y)
                                     {
                                         if (y)
                                         {
                                             Other(3,
                                                   4);
                                         }
                                     }

                                     void Other(int value1, int value2)
                                     {
                                     }
                                 }
                                 """;

        // RH5406 only reports multi-line embedded child statements, and both an if-branch and its own else-branch
        // resolve to the identical owning if-statement, which the batch fixer already collapses into a single
        // reformat instead of two interfering ones. Two independent if-statements in different methods are used
        // here because interference beyond that identical-owner collapse is unreachable for this rule
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH5406MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}