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
    /// Verifies that the issue is fixed without deleting the header when the multi-line child starts on the parent's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5406-001")]
    [Requirement("RH5406-002")]
    public async Task VerifyIssueIsFixedWhenChildStartsOnParentLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true) {|#0:Other(1,
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
    /// Verifies that the three line breaks the fix creates are LF in a document whose line breaks are all a lone
    /// carriage return, when the body it wraps spans several lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a) M(a,
                                                 b);
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
                                             M(a,
                                               b);
                                         }
                                     }
                                 }
                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode).Replace("\r\n", "\r", StringComparison.Ordinal));

        // The fix creates the line breaks behind "if (a)", behind the opening brace and behind the body. Only those are
        // asserted to be LF; the line break inside the body comes from the input, and the layout is compared with every
        // line break read as LF.
        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode).Replace("\r\n", "\n", StringComparison.Ordinal),
                        actual.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\r", "\n", StringComparison.Ordinal));
        Assert.Contains("        if (a)\n        {\n            M(a,", actual);
        Assert.Contains("              b);\n        }", actual);
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