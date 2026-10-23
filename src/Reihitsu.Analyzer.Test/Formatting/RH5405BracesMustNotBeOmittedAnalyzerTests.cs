using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5405BracesMustNotBeOmittedAnalyzer"/> and <see cref="RH5405BracesMustNotBeOmittedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5405BracesMustNotBeOmittedAnalyzerTests : BatchCodeFixTestsBase<RH5405BracesMustNotBeOmittedAnalyzer, RH5405BracesMustNotBeOmittedCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Input of the SHARED-FIX-008 example: a body on the line of its <c>if</c>, without markup
    /// </summary>
    private const string LineOfIfTestCode = """
                                            class C
                                            {
                                                void M(bool a, bool b)
                                                {
                                                    if (a) M(a, b);
                                                }
                                            }
                                            """;

    /// <summary>
    /// Result of the SHARED-FIX-008 example, in which the fix creates three line breaks
    /// </summary>
    private const string LineOfIfFixedCode = """
                                             class C
                                             {
                                                 void M(bool a, bool b)
                                                 {
                                                     if (a)
                                                     {
                                                         M(a, b);
                                                     }
                                                 }
                                             }
                                             """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5405-001")]
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
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
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
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is fixed without deleting the header when the child shares the parent's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyIssueIsFixedWhenChildSharesParentLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x)
                                    {
                                        if (x) {|#0:Foo();|}
                                    }

                                    void Foo()
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
                                             Foo();
                                         }
                                     }

                                     void Foo()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line brace-less else child statement is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyElseChildStatementIsDetectedAndFixed()
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
                                            {|#0:return;|}
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
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that when both the if-branch and the else-branch are unbraced, the fix places the else keyword on
    /// its own line at the statement's indentation instead of appending it to the closing brace's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyBothBranchesUnbracedPlacesElseOnItsOwnLine()
    {
        const string testData = """
                                internal class Sample
                                {
                                    void Method(bool x)
                                    {
                                        if (x)
                                            {|#0:Foo();|}
                                        else
                                            {|#1:Bar();|}
                                    }

                                    void Foo()
                                    {
                                    }

                                    void Bar()
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Sample
                                 {
                                     void Method(bool x)
                                     {
                                         if (x)
                                         {
                                             Foo();
                                         }
                                         else
                                         {
                                             Bar();
                                         }
                                     }

                                     void Foo()
                                     {
                                     }

                                     void Bar()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a multi-line brace-less child statement is not flagged by RH5405 (it is reported by RH5406),
    /// so the two rules never report the same statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5405-001")]
    public async Task VerifyMultiLineChildStatementIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                            Other(1,
                                                  2);
                                    }

                                    void Other(int value1, int value2)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an "else if" does not produce a diagnostic for the nested if-statement header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    public async Task VerifyElseIfDoesNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(bool x, bool y)
                                    {
                                        if (x)
                                        {
                                        }
                                        else if (y)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix keeps the blank line that separates the braced statement from the previous statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyBlankLineAboveFixedStatementIsKept()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int value)
                                    {
                                        var copy = value;

                                        if (copy > 0)
                                            {|#0:copy++;|}

                                        return copy;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int value)
                                     {
                                         var copy = value;

                                         if (copy > 0)
                                         {
                                             copy++;
                                         }

                                         return copy;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix inserts the blank line that separates the braced statement from a preceding declaration statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlankLineIsInsertedAboveFixedStatementAfterDeclaration()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int value)
                                    {
                                        var copy = value;
                                        if (copy > 0)
                                            {|#0:copy++;|}

                                        return copy;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int value)
                                     {
                                         var copy = value;

                                         if (copy > 0)
                                         {
                                             copy++;
                                         }

                                         return copy;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix inserts the blank line that separates the braced statement from a preceding closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlankLineIsInsertedAboveFixedStatementAfterClosingBrace()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int value)
                                    {
                                        if (value > 1)
                                        {
                                            value--;
                                        }
                                        if (value > 0)
                                            {|#0:value++;|}

                                        return value;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int value)
                                     {
                                         if (value > 1)
                                         {
                                             value--;
                                         }

                                         if (value > 0)
                                         {
                                             value++;
                                         }

                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat));
    }

    /// <summary>
    /// Verifies that fixing two adjacent statements inserts the blank line above each of them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlankLinesAreInsertedAboveAdjacentFixedStatements()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int value)
                                    {
                                        var copy = value;
                                        if (copy > 0)
                                            {|#0:copy++;|}
                                        if (copy > 1)
                                            {|#1:copy--;|}

                                        return copy;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int value)
                                     {
                                         var copy = value;

                                         if (copy > 0)
                                         {
                                             copy++;
                                         }

                                         if (copy > 1)
                                         {
                                             copy--;
                                         }

                                         return copy;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the bodies of an if, of an else if, and of an else without braces are each reported, while the
    /// body of a while statement is not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyBodiesOfIfElseIfAndElseAreCheckedButNotLoopBodies()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|}
                                        else if (b)
                                            {|RH5405:M(a, b);|}
                                        else
                                            {|RH5405:M(a, b);|}
                                        while (a)
                                            M(a, b);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the unbraced bodies of do, for, foreach, using, lock, and fixed statements are not checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-001")]
    public async Task VerifyBodiesOfLoopAndResourceStatementsAreNotChecked()
    {
        const string testCode = """
                                class C
                                {
                                    unsafe void M(bool a, int[] v, System.IDisposable d)
                                    {
                                        do
                                            M(a, v, d);
                                        while (a);
                                        for (var i = 0; i < 1; i++)
                                            M(a, v, d);
                                        foreach (var x in v)
                                            M(a, v, d);
                                        using (d)
                                            M(a, v, d);
                                        lock (v)
                                            M(a, v, d);
                                        fixed (int* p = v)
                                            M(a, v, d);
                                    }
                                }
                                """;

        await Verify(testCode, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies that a body without braces on one line is reported, whether it is on the line of its if or on a later
    /// line, while a body that spans several lines and a block are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5405-001")]
    [Requirement("RH5405-002")]
    public async Task VerifyUnbracedBodyOnOneLineIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|}
                                        if (a) {|RH5405:M(a, b);|}
                                        if (a)
                                            M(a,
                                              b);
                                        if (a)
                                        {
                                            M(a, b);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix wraps a body on the line of its if in braces on their own lines in the column of the if,
    /// with the body indented one level inside them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWrapsBodyOnLineOfIfInBraces()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a) {|RH5405:M(a, b);|}
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
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix wraps a body on the line of its if in braces on their own lines in the column of the if,
    /// with the body indented one level inside them, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-FIX-008")]
    public async Task VerifyFixWrapsBodyOnLineOfIfInBracesWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a) {|RH5405:M(a, b);|}
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
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies that every line break of the fix result is CRLF in a document whose line breaks are all CRLF,
    /// including the line breaks the fix creates
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesCarriageReturnLineFeedInADocumentThatUsesIt()
    {
        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(LineOfIfTestCode));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(LineOfIfFixedCode), actual);
    }

    /// <summary>
    /// Verifies that every line break of the fix result is LF in a document whose line breaks are all LF, including
    /// the line breaks the fix creates
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesLineFeedInADocumentThatUsesIt()
    {
        var actual = await ApplyCodeFixAsync(NormalizeToLineFeed(LineOfIfTestCode));

        Assert.AreEqual(NormalizeToLineFeed(LineOfIfFixedCode), actual);
    }

    /// <summary>
    /// Verifies that the line breaks the fix creates are LF in a document whose line breaks are all a lone carriage
    /// return, and that the line break the fix keeps from the input stays a lone carriage return
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesLineFeedInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        var source = NormalizeToLineFeed(LineOfIfTestCode).Replace("\n", "\r", StringComparison.Ordinal);

        var actual = await ApplyCodeFixAsync(source);

        // The three line breaks inside the new block are created by the fix; the one behind its closing brace is the
        // line break that ended the statement's line in the input
        const string expected = "class C\r"
                                + "{\r"
                                + "    void M(bool a, bool b)\r"
                                + "    {\r"
                                + "        if (a)\n"
                                + "        {\n"
                                + "            M(a, b);\n"
                                + "        }\r"
                                + "    }\r"
                                + "}";

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the LF line breaks inside a verbatim string literal stay LF when the fix is applied in a document
    /// whose other line breaks are CRLF, and that they do not decide the line ending of the line breaks the fix
    /// creates, although they outnumber the document's other line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task FixKeepsTheLineFeedsInsideALiteralAndWritesTheDocumentsCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    string _s = LITERAL;

                                    void M(bool a, bool b)
                                    {
                                        if (a) M(a, b);
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     string _s = LITERAL;

                                     void M(bool a, bool b)
                                     {
                                         if (a)
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;
        const string literal = "@\"1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11\n12\n13\n14\"";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode).Replace("LITERAL", literal, StringComparison.Ordinal));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode).Replace("LITERAL", literal, StringComparison.Ordinal), actual);
    }

    /// <summary>
    /// Verifies that the LF line breaks inside a block comment stay LF when the fix is applied in a document whose
    /// other line breaks are CRLF, and that they do not decide the line ending of the line breaks the fix creates,
    /// although they outnumber the document's other line breaks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-008")]
    public async Task FixKeepsTheLineFeedsInsideACommentAndWritesTheDocumentsCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    COMMENT
                                    void M(bool a, bool b)
                                    {
                                        if (a) M(a, b);
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     COMMENT
                                     void M(bool a, bool b)
                                     {
                                         if (a)
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;
        const string comment = "/* 1\n2\n3\n4\n5\n6\n7\n8\n9\n10\n11\n12\n13\n14 */";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode).Replace("COMMENT", comment, StringComparison.Ordinal));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode).Replace("COMMENT", comment, StringComparison.Ordinal), actual);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Normalizes the line endings of the provided text to LF, irrespective of how the source file is stored
    /// </summary>
    /// <param name="text">Text to normalize</param>
    /// <returns>The text with LF line endings</returns>
    private static string NormalizeToLineFeed(string text)
    {
        return text.Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method1(bool x)
                                    {
                                        if (x)
                                            {|#0:Foo();|}
                                    }

                                    void Method2(bool y)
                                    {
                                        if (y)
                                            {|#1:Bar();|}
                                    }

                                    void Foo()
                                    {
                                    }

                                    void Bar()
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method1(bool x)
                                     {
                                         if (x)
                                         {
                                             Foo();
                                         }
                                     }

                                     void Method2(bool y)
                                     {
                                         if (y)
                                         {
                                             Bar();
                                         }
                                     }

                                     void Foo()
                                     {
                                     }

                                     void Bar()
                                     {
                                     }
                                 }
                                 """;

        // RH5405 only reports single-line embedded child statements; nesting a second violation inside the first
        // would make the outer child multi-line, which RH5406 owns instead, so the only way two diagnostics could
        // share an owning statement is an if/else pair - and there both branches resolve to the identical
        // if-statement node, collapsing to what the batch fixer treats as a single reformat rather than two
        // interfering ones. Two independent if-statements in different methods are used instead, because
        // interference beyond that identical-owner collapse is unreachable for this rule
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, AnalyzerResources.RH5405MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}