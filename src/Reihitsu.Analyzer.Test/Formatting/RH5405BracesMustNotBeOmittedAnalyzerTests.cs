using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    /// An unbraced body of an <c>else if</c> whose condition carries a trailing line comment
    /// </summary>
    private const string ElseIfWithTrailingCommentTestCode = """
                                                             class C
                                                             {
                                                                 void M(bool x, bool y)
                                                                 {
                                                                     if (x)
                                                                     {
                                                                         Foo();
                                                                     }
                                                                     else if (y) // note
                                                                         {|RH5405:Foo();|}
                                                                 }

                                                                 void Foo()
                                                                 {
                                                                 }
                                                             }
                                                             """;

    /// <summary>
    /// Result of the fix on <see cref="ElseIfWithTrailingCommentTestCode"/>: the comment stays behind the condition; the braces
    /// stand in the column of the <c>else</c>, which starts the line of the <c>else if</c>
    /// </summary>
    private const string ElseIfWithTrailingCommentFixedCode = """
                                                              class C
                                                              {
                                                                  void M(bool x, bool y)
                                                                  {
                                                                      if (x)
                                                                      {
                                                                          Foo();
                                                                      }
                                                                      else if (y) // note
                                                                      {
                                                                          Foo();
                                                                      }
                                                                  }

                                                                  void Foo()
                                                                  {
                                                                  }
                                                              }
                                                              """;

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

    /// <summary>
    /// An unbraced body of an <c>if</c> whose condition carries a trailing line comment, behind another statement
    /// </summary>
    private const string IfWithTrailingCommentTestCode = """
                                                         class C
                                                         {
                                                             void M(bool a)
                                                             {
                                                                 Foo();

                                                                 if (a) // c
                                                                     {|RH5405:Foo();|}
                                                             }

                                                             void Foo()
                                                             {
                                                             }
                                                         }
                                                         """;

    /// <summary>
    /// Result of the fix on <see cref="IfWithTrailingCommentTestCode"/>: the comment stays behind the condition
    /// </summary>
    private const string IfWithTrailingCommentFixedCode = """
                                                          class C
                                                          {
                                                              void M(bool a)
                                                              {
                                                                  Foo();

                                                                  if (a) // c
                                                                  {
                                                                      Foo();
                                                                  }
                                                              }

                                                              void Foo()
                                                              {
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
    /// Verifies that the fix inserts no blank line between the braced statement and a preceding declaration statement
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyNoBlankLineIsInsertedAboveFixedStatementAfterDeclaration()
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
    /// Verifies that the fix inserts no blank line between the braced statement and a preceding closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyNoBlankLineIsInsertedAboveFixedStatementAfterClosingBrace()
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
    /// Verifies that fixing two adjacent statements inserts no blank line above either of them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyNoBlankLinesAreInsertedAboveAdjacentFixedStatements()
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
    /// Verifies that the fix on the body of an <c>else if</c> whose condition carries a trailing line comment keeps the
    /// comment behind the condition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyFixKeepsTheTrailingCommentOfAnElseIfCondition()
    {
        await Verify(ElseIfWithTrailingCommentTestCode, ElseIfWithTrailingCommentFixedCode);
    }

    /// <summary>
    /// Verifies that the fix on the body of an <c>else if</c> whose condition carries a trailing line comment keeps the
    /// comment behind the condition, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyFixKeepsTheTrailingCommentOfAnElseIfConditionWithCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(ElseIfWithTrailingCommentTestCode), NormalizeToCarriageReturnLineFeed(ElseIfWithTrailingCommentFixedCode));
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
    /// Verifies that no fix is offered for a body on the line of its if in a document whose line breaks are all a lone
    /// carriage return, because the line break that ends the body is not the detected line ending
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
                                        if (a) {|RH5405:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, "\r"));
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

    /// <summary>
    /// Verifies that the fix on the body of an <c>if</c> whose condition carries a trailing line comment keeps the comment
    /// behind the condition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public async Task VerifyFixKeepsTheTrailingCommentOfAnIfCondition()
    {
        await Verify(IfWithTrailingCommentTestCode, IfWithTrailingCommentFixedCode);
    }

    /// <summary>
    /// Verifies that the fix on the body of an <c>if</c> whose condition carries a trailing line comment keeps the comment
    /// behind the condition, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public async Task VerifyFixKeepsTheTrailingCommentOfAnIfConditionWithCarriageReturnLineFeed()
    {
        await Verify(NormalizeToCarriageReturnLineFeed(IfWithTrailingCommentTestCode), NormalizeToCarriageReturnLineFeed(IfWithTrailingCommentFixedCode));
    }

    /// <summary>
    /// Verifies that the fix on the body of an <c>if</c> that is the body of a loop, whose condition carries a trailing
    /// line comment, keeps the comment behind the condition
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BREAK-036")]
    public async Task VerifyFixKeepsTheTrailingCommentOfAnIfConditionInALoopBody()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        while (b)
                                            if (a) // c
                                                {|RH5405:Foo();|}
                                    }

                                    void Foo()
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b)
                                     {
                                         while (b)
                                             if (a) // c
                                             {
                                                 Foo();
                                             }
                                     }

                                     void Foo()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the spec example: a line comment behind the body stays behind the body, inside the block, and is not
    /// repeated behind the closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWritesTrailingCommentOfBodyOnce()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|} // trail
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
                                             M(a, b); // trail
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a line comment behind the body is written once, with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWritesTrailingCommentOfBodyOnceWithCarriageReturnLineFeed()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|} // trail
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
                                             M(a, b); // trail
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testCode), NormalizeToCarriageReturnLineFeed(fixedCode));
    }

    /// <summary>
    /// Verifies a line comment behind a body on the line of its if is written once, behind the body inside the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWritesTrailingCommentOfBodyOnLineOfIfOnce()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a) {|RH5405:M(a, b);|} // trail
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
                                             M(a, b); // trail
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a multi-line comment behind the body is written once, behind the body inside the block, with its text
    /// unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWritesMultiLineTrailingCommentOfBodyOnce()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|} /* a
                                            b */
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
                                             M(a, b); /* a
                                             b */
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a line comment behind the body of an if that is itself the body of another if is written once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixWritesTrailingCommentOfNestedIfBodyOnce()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool c, int b)
                                    {
                                        if (a)
                                            if (c)
                                                {|RH5405:M(a, c, b);|} // trail
                                        b++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool c, int b)
                                     {
                                         if (a)
                                             if (c)
                                             {
                                                 M(a, c, b); // trail
                                             }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a line comment behind the body of an if followed by else stays behind that body and is written once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixKeepsTrailingCommentOfIfBodyFollowedByElseOnce()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|} // one
                                        else
                                        {
                                            M(a, b);
                                        }
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
                                             M(a, b); // one
                                         }
                                         else
                                         {
                                             M(a, b);
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies fixing the bodies of an if and its else, each with a line comment behind it, writes each comment once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task VerifyFixAllWritesTrailingCommentsOfBothBranchesOnce()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|} // one
                                        else
                                            {|RH5405:M(a, b);|} // two
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
                                             M(a, b); // one
                                         }
                                         else
                                         {
                                             M(a, b); // two
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a comment the formatter refuses in the else block does not withhold the fix for the unbraced body
    /// of the if: the fix wraps only that body and leaves the else block as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task FixIsOfferedWhenTheElseBlockHoldsACommentTheFormatterRefuses()
    {
        const string source = """
                              internal class TestClass
                              {
                                  private void Method(bool x)
                                  {
                                      if (x) {|RH5405:Foo();|}
                                      else
                                      {
                                          int /* c */ y = 1;
                                      }
                                  }

                                  private void Foo()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       private void Method(bool x)
                                       {
                                           if (x)
                                           {
                                               Foo();
                                           }
                                           else
                                           {
                                               int /* c */ y = 1;
                                           }
                                       }

                                       private void Foo()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the spec example: the header with its spacing, the body with its spacing, and the absence of a blank
    /// line above the statement stay as written, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsTheHeaderTheBodyAndTheBlankLinesAsWritten(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        b++;
                                        if ( a )
                                            {|RH5405:M(a,b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         b++;
                                         if ( a )
                                         {
                                             M(a,b);
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: a body below its if that is not indented one level deeper keeps its column, under LF
    /// and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsTheColumnOfAMisindentedBodyBelowItsIf(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                          {|RH5405:M(a, b);|}
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
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a body below its if that is indented further than one level keeps its column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task FixKeepsTheColumnOfABodyIndentedFurtherThanOneLevel()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                                {|RH5405:M(a, b);|}
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
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the spec example: the other body of the same if, which RH5406 reports, keeps its shape and gets no braces,
    /// under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixLeavesTheOtherBodyAsWritten(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a,b);|}
                                        else
                                            M(a,
                                              b);
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
                                             M(a,b);
                                         }
                                         else
                                             M(a,
                                               b);
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the braces of the body of an else if stand in the column of the else, under LF and CRLF
    /// line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixPlacesTheBracesOfAnElseIfBodyInTheColumnOfTheElse(string endOfLine)
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
                                        else if (b)
                                            {|RH5405:M(a, b);|}
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
                                         else if (b)
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the body of an else if on the line of the else if moves below braces in the column of the else
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task FixPlacesTheBracesOfAnElseIfBodyOnItsLineInTheColumnOfTheElse()
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
                                        else if (b) {|RH5405:M(a, b);|}
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
                                         else if (b)
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the spec example: the fix applied to the if body of an if and an else written on one line keeps the else,
    /// with its unchanged body, on the line of the new closing brace, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsAnElseBehindTheBodyOnTheLineOfTheClosingBrace(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a) M(a, b); else M(a, b);
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
                                         } else M(a, b);
                                     }
                                 }
                                 """;

        var actual = await TryApplyCodeFixAtAsync(ConvertEndOfLine(testCode, endOfLine), RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, GetIfBodyLocation);

        Assert.AreEqual(ConvertEndOfLine(fixedCode, endOfLine), actual);
    }

    /// <summary>
    /// Verifies that the fix applied to the if body below its if keeps an else behind that body on the line of the new
    /// closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task FixKeepsAnElseBehindABodyBelowItsIfOnTheLineOfTheClosingBrace()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                            M(a, b); else M(a, b);
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
                                         } else M(a, b);
                                     }
                                 }
                                 """;

        var actual = await TryApplyCodeFixAtAsync(testCode, RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, GetIfBodyLocation);

        Assert.AreEqual(fixedCode, actual);
    }

    /// <summary>
    /// Verifies that the else body of an if and an else on one line gets no fix while the if body in front of it is
    /// unbraced, because the body stands in front of the else on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAnElseBodyBehindAnUnbracedBodyOnItsLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a) M(a, b); else M(a, b);
                                    }
                                }
                                """;

        Assert.IsNull(await TryApplyCodeFixAtAsync(testCode, RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, GetElseBodyLocation));
    }

    /// <summary>
    /// Verifies the fix on both bodies of an if and an else written on one line: the if body is wrapped first, after which
    /// the else stands behind its closing brace and its body is wrapped in braces in the column of that brace. Fix All
    /// needs two iterations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-BRACE-003")]
    public async Task FixAllWrapsBothBodiesOfAnIfAndAnElseOnOneLineInTwoIterations()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a) {|RH5405:M(a, b);|} else {|RH5405:M(a, b);|}
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

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 2;
                         test.NumberOfFixAllIterations = 2;
                     });
    }

    /// <summary>
    /// Verifies the fix on an if that is the body of another if on the same line: the outer body is wrapped first, after
    /// which the inner if starts its line and its body is wrapped too. Fix All needs two iterations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-BRACE-003")]
    public async Task FixAllWrapsTheOuterAndTheInnerBodyOfNestedIfsOnOneLineInTwoIterations()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (b) {|RH5405:if (a) {|RH5405:M(a, b);|}|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b)
                                     {
                                         if (b)
                                         {
                                             if (a)
                                             {
                                                 M(a, b);
                                             }
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test =>
                     {
                         test.NumberOfIncrementalIterations = 2;
                         test.NumberOfFixAllIterations = 2;
                     });
    }

    /// <summary>
    /// Verifies the spec example: the body of an if that is the body of another if on the same line gets no fix, while
    /// the outer body gets the fix that wraps the inner if as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForTheBodyOfAnIfBehindTheHeaderOfAnotherIf()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b, int i)
                                    {
                                        if (b) if (a) M(a, b, i);
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, bool b, int i)
                                     {
                                         if (b)
                                         {
                                             if (a) M(a, b, i);
                                         }
                                     }
                                 }
                                 """;

        Assert.IsNull(await TryApplyCodeFixAtAsync(testCode, RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, root => GetIfStatements(root)[1].Statement.GetLocation()));
        Assert.AreEqual(fixedCode, await TryApplyCodeFixAtAsync(testCode, RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, GetIfBodyLocation));
    }

    /// <summary>
    /// Verifies the spec example: a statement behind a body on the line of its if starts its own line below the closing
    /// brace, while the comment behind the body stays inside the block, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixMovesAStatementBehindABodyOnTheLineOfItsIfBelowTheClosingBrace(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a) {|RH5405:M(a, b);|} /* c */ b++;
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
                                             M(a, b); /* c */
                                         }
                                         b++;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a statement behind a body below its if starts its own line below the closing brace, in the column of
    /// the brace, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixMovesAStatementBehindABodyBelowItsIfBelowTheClosingBrace(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|} b++;
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
                                         b++;
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: the blank line directly below the if is removed, while a comment above the body and the
    /// blank line between that comment and the body stay inside the block, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixRemovesTheBlankLineBelowTheIfAndKeepsTheCommentAboveTheBody(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)

                                            // c

                                            {|RH5405:M(a, b);|}
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
                                             // c

                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that the blank lines directly below the if are removed when no comment stands between the if and its body,
    /// under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixRemovesTheBlankLinesBetweenTheIfAndItsBody(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)


                                            {|RH5405:M(a, b);|}
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
                                     }
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a delimited comment between the if and a body on its line stays behind the header
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    public async Task FixKeepsADelimitedCommentBetweenTheHeaderAndTheBodyBehindTheHeader()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a) /* h */ {|RH5405:M(a, b);|}
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         if (a) /* h */
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the spec example: whitespace at the end of the line of a body below its if, behind a delimited comment,
    /// stays there, and nothing is written behind the closing brace, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsTrailingWhitespaceBehindACommentOnTheBodyLineAndWritesNoneBehindTheClosingBrace(string endOfLine)
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   void M(bool a, int b)
                                                   {
                                                       if (a)
                                                           {|RH5405:M(a, b);|} /* x */···
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
                                                            M(a, b); /* x */···
                                                        }
                                                    }
                                                }
                                                """);

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that whitespace at the end of the line of a body below its if, without a comment, stays there, and
    /// nothing is written behind the closing brace, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsTrailingWhitespaceOnTheBodyLineAndWritesNoneBehindTheClosingBrace(string endOfLine)
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   void M(bool a, int b)
                                                   {
                                                       if (a)
                                                           {|RH5405:M(a, b);|}···
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
                                                            M(a, b);···
                                                        }
                                                    }
                                                }
                                                """);

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that whitespace at the end of the line of a body on the line of its if stays at the end of the body's
    /// line, with and without a delimited comment, and nothing is written behind the closing brace, under LF and CRLF
    /// line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <param name="behindBody">What stands behind the body on its line, in front of the whitespace</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n", "")]
    [DataRow("\r\n", "")]
    [DataRow("\n", " /* x */")]
    [DataRow("\r\n", " /* x */")]
    public async Task FixKeepsTrailingWhitespaceOfABodyOnTheLineOfItsIfOnTheBodyLine(string endOfLine, string behindBody)
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   void M(bool a, int b)
                                                   {
                                                       if (a) {|RH5405:M(a, b);|}BEHIND···
                                                   }
                                               }
                                               """).Replace("BEHIND", behindBody, StringComparison.Ordinal);
        var fixedCode = ExpandWhitespaceMarkers("""
                                                class C
                                                {
                                                    void M(bool a, int b)
                                                    {
                                                        if (a)
                                                        {
                                                            M(a, b);BEHIND···
                                                        }
                                                    }
                                                }
                                                """).Replace("BEHIND", behindBody, StringComparison.Ordinal);

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that spaces at the end of a single-line comment behind the body are comment text and stay behind the
    /// comment, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixKeepsTheSpacesAtTheEndOfASingleLineCommentBehindTheBody(string endOfLine)
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   void M(bool a, int b)
                                                   {
                                                       if (a)
                                                           {|RH5405:M(a, b);|} // x···
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
                                                            M(a, b); // x···
                                                        }
                                                    }
                                                }
                                                """);

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies the spec example: no fix is offered for a body on the line of its if that a delimited comment behind it
    /// continues onto a later line, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task NoFixIsOfferedForABodyOnTheLineOfItsIfWhoseCommentContinuesOntoALaterLine(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a) {|RH5405:M(a, b);|} /* one
                                        two */
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies the spec example: a body on the line of its if with a delimited comment closed on that line behind it
    /// gets the fix, and the comment stays behind the body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-BRACE-003")]
    public async Task FixIsOfferedForABodyOnTheLineOfItsIfWithAClosedDelimitedCommentBehindIt()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a) {|RH5405:M(a, b);|} /* one */
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
                                             M(a, b); /* one */
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the spec example: no fix is offered for the body of an if behind a statement on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAnIfBehindAStatementOnItsLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b, int i)
                                    {
                                        b = a; if (a) {|RH5405:M(a, b, i);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the spec example: no fix is offered for the body of an if behind a case label on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAnIfBehindACaseLabelOnItsLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b, int i)
                                    {
                                        switch (i)
                                        {
                                            case 1: if (a) {|RH5405:M(a, b, i);|}
                                                break;
                                        }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that no fix is offered for the body of an if behind the opening brace of a block on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAnIfBehindAnOpeningBraceOnItsLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        { if (a) {|RH5405:M(a, b);|} }
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that no fix is offered for the body of an if behind a loop header on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAnIfBehindALoopHeaderOnItsLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        while (a) if (a) {|RH5405:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that no fix is offered for the body of an if behind a comment on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForAnIfBehindACommentOnItsLine()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        /* x */ if (a) {|RH5405:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the spec example: no fix is offered when a pragma directive stands between the if and its body, under LF
    /// and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task NoFixIsOfferedForAPragmaDirectiveBetweenTheIfAndItsBody(string endOfLine)
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                #pragma warning disable CS0618
                                            {|RH5405:M(a, b);|}
                                #pragma warning restore CS0618
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies the spec example: no fix is offered when a region and its end stand around the body below its if
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForARegionAroundTheBody()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            #region R
                                            {|RH5405:M(a, b);|}
                                            #endregion
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that no fix is offered when a nullable directive stands between the if and its body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForANullableDirectiveBetweenTheIfAndItsBody()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                #nullable enable
                                            {|RH5405:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that no fix is offered when disabled text stands between the if and its body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForDisabledTextBetweenTheIfAndItsBody()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                #if NEVER
                                            M(b, a);
                                #endif
                                            {|RH5405:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies the spec example: a pragma directive behind the body, in front of the next statement, does not withhold
    /// the fix and stays behind the closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-BRACE-003")]
    public async Task FixIsOfferedForAPragmaDirectiveBehindTheBody()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|}
                                #pragma warning restore CS0618
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
                                 #pragma warning restore CS0618
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the spec example: no fix is offered when a tab stands in front of the if
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    public async Task NoFixIsOfferedForATabInFrontOfTheIf()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                               →void M(bool a, int b)
                                               →{
                                               →→if (a)
                                               →→→{|RH5405:M(a, b);|}
                                               →}
                                               }
                                               """);

        await VerifyNoCodeFixIsOffered(testCode);
    }

    /// <summary>
    /// Verifies that a tab in front of a body below its if, with spaces in front of the if, does not withhold the fix, and
    /// the body line keeps its tabs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-BRACE-003")]
    public async Task FixIsOfferedForATabInFrontOfABodyBelowItsIf()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   void M(bool a, int b)
                                                   {
                                                       if (a)
                                               →→→{|RH5405:M(a, b);|}
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
                                                →→→M(a, b);
                                                        }
                                                    }
                                                }
                                                """);

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a tab inside a string literal of the body does not withhold the fix and stays in the literal
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-BRACE-003")]
    public async Task FixIsOfferedForATabInsideAStringLiteralOfTheBody()
    {
        var testCode = ExpandWhitespaceMarkers("""
                                               class C
                                               {
                                                   void M(bool a, string b)
                                                   {
                                                       if (a) {|RH5405:M(a, "x→y");|}
                                                   }
                                               }
                                               """);
        var fixedCode = ExpandWhitespaceMarkers("""
                                                class C
                                                {
                                                    void M(bool a, string b)
                                                    {
                                                        if (a)
                                                        {
                                                            M(a, "x→y");
                                                        }
                                                    }
                                                }
                                                """);

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies the spec example: in a CRLF document whose line break behind the if is LF, no fix is offered, because that
    /// LF lies in the code the fix moves
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    [Requirement("SHARED-FIX-008")]
    public async Task NoFixIsOfferedForALineFeedBehindTheIfInACarriageReturnLineFeedDocument()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        b++;
                                        if (a)LF
                                            {|RH5405:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testCode).Replace("LF\r\n", "\n", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies that in a CRLF document an LF inside a comment behind the body withholds the fix, because it lies in the
    /// code the fix moves
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    [Requirement("SHARED-FIX-008")]
    public async Task NoFixIsOfferedForALineFeedInsideACommentBehindTheBodyInACarriageReturnLineFeedDocument()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|} /* oneLF
                                            two */
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(testCode).Replace("LF\r\n", "\n", StringComparison.Ordinal));
    }

    /// <summary>
    /// Verifies the spec example: in a CRLF document whose line break behind a statement above the if is LF, the fix is
    /// offered, every line break it creates is CRLF, and that LF stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-002")]
    [Requirement("SHARED-BRACE-003")]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesCarriageReturnLineFeedWhenALineFeedStandsAboveTheIf()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        b++;LF
                                        if (a)
                                            M(a, b);
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(bool a, int b)
                                     {
                                         b++;LF
                                         if (a)
                                         {
                                             M(a, b);
                                         }
                                     }
                                 }
                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode).Replace("LF\r\n", "\n", StringComparison.Ordinal));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode).Replace("LF\r\n", "\n", StringComparison.Ordinal), actual);
    }

    /// <summary>
    /// Verifies that in a CRLF document an LF behind the other body of the if does not withhold the fix and stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    [Requirement("SHARED-FIX-008")]
    public async Task FixIsOfferedWhenALineFeedStandsBehindTheOtherBody()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            M(a, b);
                                        else
                                        {
                                            M(a, b);
                                        }LF
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
                                             M(a, b);
                                         }LF
                                         b++;
                                     }
                                 }
                                 """;

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode).Replace("LF\r\n", "\n", StringComparison.Ordinal));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode).Replace("LF\r\n", "\n", StringComparison.Ordinal), actual);
    }

    /// <summary>
    /// Verifies the spec example: no fix is offered for a body below its if in a document whose line breaks are all a lone
    /// carriage return
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-BRACE-003")]
    [Requirement("SHARED-FIX-008")]
    public async Task NoFixIsOfferedForABodyBelowItsIfInADocumentWhoseLineBreaksAreLoneCarriageReturns()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool a, int b)
                                    {
                                        if (a)
                                            {|RH5405:M(a, b);|}
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, "\r"));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the if statements of the syntax tree in document order
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The if statements</returns>
    private static List<IfStatementSyntax> GetIfStatements(SyntaxNode root)
    {
        return root.DescendantNodes()
                   .OfType<IfStatementSyntax>()
                   .ToList();
    }

    /// <summary>
    /// Gets the location of the body of the first if statement
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>Location of the body</returns>
    private static Location GetIfBodyLocation(SyntaxNode root)
    {
        return GetIfStatements(root)[0].Statement.GetLocation();
    }

    /// <summary>
    /// Gets the location of the body of the first else clause
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>Location of the body</returns>
    private static Location GetElseBodyLocation(SyntaxNode root)
    {
        return root.DescendantNodes()
                   .OfType<ElseClauseSyntax>()
                   .First()
                   .Statement
                   .GetLocation();
    }

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