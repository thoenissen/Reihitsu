using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5407UseBracesConsistentlyAnalyzer"/> and <see cref="RH5407UseBracesConsistentlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5407UseBracesConsistentlyAnalyzerTests : BatchCodeFixTestsBase<RH5407UseBracesConsistentlyAnalyzer, RH5407UseBracesConsistentlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5407-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                            return;
                                        }
                                        else
                                        {
                                            return;
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
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5407-001")]
    [Requirement("RH5407-002")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                            return;
                                        }
                                        else
                                            {|#0:return;|}
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
                                             return;
                                         }
                                         else
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH5407MessageFormat));
    }

    /// <summary>
    /// Verifies that when the else-branch is already braced and the if-branch is not, the fix braces the
    /// if-branch and places the else keyword on its own line instead of appending it to the closing brace's
    /// line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5407-001")]
    [Requirement("RH5407-002")]
    public async Task VerifyIfBranchUnbracedIsFixedWithElseOnItsOwnLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                            {|#0:return;|}
                                        else
                                        {
                                            return;
                                        }
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
                                             return;
                                         }
                                         else
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH5407MessageFormat));
    }

    /// <summary>
    /// Verifies that else-if chains do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("RH5407-001")]
    public async Task VerifyElseIfChainsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool first, bool second)
                                    {
                                        if (first)
                                        {
                                            return;
                                        }
                                        else if (second)
                                            return;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is fixed without deleting the else keyword when the child shares the else line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5407-001")]
    [Requirement("RH5407-002")]
    public async Task VerifyIssueIsFixedWhenChildSharesElseLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                            return;
                                        }
                                        else {|#0:return;|}
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
                                             return;
                                         }
                                         else
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH5407MessageFormat));
    }

    /// <summary>
    /// Verifies that the unbraced body of an if statement whose other branch has braces is reported, also for an else
    /// if checked against its own else, while an if statement whose two bodies both lack braces is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5407-001")]
    [Requirement("RH5407-002")]
    [Requirement("SHARED-BRACE-001")]
    public async Task VerifyBranchWithoutBracesNextToBracedBranchIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                            {|RH5407:M(a, b);|}
                                        else
                                        {
                                            M(a, b);
                                        }

                                        if (a)
                                        {
                                            M(a, b);
                                        }
                                        else if (b)
                                            {|RH5407:M(a, b);|}
                                        else
                                        {
                                            M(a, b);
                                        }

                                        if (a)
                                            M(a, b);
                                        else
                                            M(a, b);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an if statement without else is not reported, whatever its body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5407-001")]
    public async Task VerifyIfStatementWithoutElseIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                            M(a, b);
                                        if (b)
                                        {
                                            M(a, b);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix wraps the unbraced branch in braces on their own lines in the column of the if
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWrapsUnbracedBranchInBraces()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                            {|RH5407:M(a, b);|}
                                        else
                                        {
                                            M(a, b);
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b)
                                     {
                                         if (a)
                                         {
                                             M(a, b);
                                         }
                                         else
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix wraps the unbraced branch in braces on their own lines in the column of the if, with CRLF
    /// line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWrapsUnbracedBranchInBracesWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                            {|RH5407:M(a, b);|}
                                        else
                                        {
                                            M(a, b);
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b)
                                     {
                                         if (a)
                                         {
                                             M(a, b);
                                         }
                                         else
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that the fix on the unbraced branch of an <c>if</c> whose condition carries a trailing line comment keeps the
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
                                            {|#0:return;|}
                                        else
                                        {
                                            return;
                                        }
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
                                             return;
                                         }
                                         else
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH5407MessageFormat));
    }

    /// <summary>
    /// Verifies a line comment behind an else body whose if body is braced is written once, behind the body inside the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWritesTrailingCommentOfElseBodyOnce()
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
                                        else
                                            {|RH5407:M(a, b);|} // trail
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
                                             M(a, b);
                                         }
                                         else
                                         {
                                             M(a, b); // trail
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a line comment behind an else body whose if body is braced is written once, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWritesTrailingCommentOfElseBodyOnceWithCarriageReturnLineFeed()
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
                                        else
                                            {|RH5407:M(a, b);|} // trail
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
                                             M(a, b);
                                         }
                                         else
                                         {
                                             M(a, b); // trail
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies the spec example: an else behind a closing brace stays there, and the braces of its body stand in the
    /// column of that brace, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsAnElseBehindAClosingBraceAndPlacesTheBracesInTheColumnOfThatBrace(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                        {
                                            M(a, b);
                                        } else {|RH5407:M(a, b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b)
                                     {
                                         if (a)
                                         {
                                             M(a, b);
                                         } else
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the body of an else if behind a closing brace gets braces in the column of that brace, and the braced
    /// else that follows keeps its lines, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixPlacesTheBracesOfAnElseIfBodyBehindAClosingBraceInTheColumnOfThatBrace(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                        {
                                            M(a, b);
                                        } else if (b) {|RH5407:M(a, b);|}
                                        else
                                        {
                                            M(a, b);
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b)
                                     {
                                         if (a)
                                         {
                                             M(a, b);
                                         } else if (b)
                                         {
                                             M(a, b);
                                         }
                                         else
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that no fix is offered for the body of an else when a comment stands between it and the closing brace in
    /// front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAnElseBehindACommentBehindAClosingBrace()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                        {
                                            M(a, b);
                                        } /* c */ else {|RH5407:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that no fix is offered when a pragma directive stands between the else and its body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAPragmaDirectiveBetweenTheElseAndItsBody()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                        {
                                            M(a, b);
                                        }
                                        else
                                #pragma warning disable CS0618
                                            {|RH5407:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that whitespace at the end of the line of an else body stays there, and nothing is written behind the
    /// closing brace, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsTrailingWhitespaceOnTheElseBodyLineAndWritesNoneBehindTheClosingBrace(string endOfLine)
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   void M(bool a, bool b)
                                                   {
                                                       if (a)
                                                       {
                                                           M(a, b);
                                                       }
                                                       else
                                                           {|RH5407:M(a, b);|} /* x */···
                                                   }
                                               }
                                               """);
        var fixedCode = ExpandWhitespaceMarkers("""
                                                class C
                                                {
                                                    void M(bool a, bool b)
                                                    {
                                                        if (a)
                                                        {
                                                            M(a, b);
                                                        }
                                                        else
                                                        {
                                                            M(a, b); /* x */···
                                                        }
                                                    }
                                                }
                                                """);

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that no fix is offered for an else body in a document whose line breaks are all a lone carriage return
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
                                        {
                                            M(a, b);
                                        }
                                        else
                                            {|RH5407:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, "\r"));
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
                                        {
                                            return;
                                        }
                                        else
                                            {|#0:return;|}
                                    }

                                    void Method2()
                                    {
                                        if (true)
                                        {
                                            return;
                                        }
                                        else
                                            {|#1:return;|}
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
                                             return;
                                         }
                                         else
                                         {
                                             return;
                                         }
                                     }

                                     void Method2()
                                     {
                                         if (true)
                                         {
                                             return;
                                         }
                                         else
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        // The rule reports at most one diagnostic per if-statement, so a shared owner between two occurrences is
        // unreachable; two independent if/else statements in different methods exercise the batch fixer applying
        // two genuinely separate reformats
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5407UseBracesConsistentlyAnalyzer.DiagnosticId, AnalyzerResources.RH5407MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}