using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH2109RazorCodeBlocksShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Test code
    /// </summary>
    private const string TestCode = """
                                    namespace Example;

                                    internal static class Placeholder
                                    {
                                    }
                                    """;

    /// <summary>
    /// Razor source of the shared example for text that looks like a code block
    /// </summary>
    private const string SharedExampleSource = """
                                               <!-- @code { -->
                                               @* @code { *@
                                               <p>@@code { }</p>
                                               <p>user@code{</p>
                                               @code {
                                               }
                                               """;

    /// <summary>
    /// Razor source with a code block commented out by a multi-line Razor comment, followed by a real code block
    /// </summary>
    private const string CommentedOutCodeBlockSource = """
                                                       @*
                                                       @code {
                                                           private int Old;
                                                       }
                                                       *@
                                                       @code {
                                                       }
                                                       """;

    /// <summary>
    /// Razor source of the specification example of code-block lines inside a C# block comment and a C# raw string literal
    /// </summary>
    private const string CodeBlockLinesInCSharpCommentAndRawStringSource = """"
                                                                           @code {
                                                                               /*
                                                                               @code {
                                                                               }
                                                                               */
                                                                               string s = """
                                                                           @code {
                                                                           """;
                                                                           }
                                                                           """";

    /// <summary>
    /// Razor source of the specification example of a Razor comment opener that starts a line inside a C# block comment
    /// </summary>
    private const string RazorCommentOpenerLineInCSharpCommentSource = """
                                                                       @code {
                                                                           /*
                                                                           @* not a Razor comment
                                                                           */
                                                                       }
                                                                       @code {
                                                                       }
                                                                       """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifying Razor code blocks trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("RH2109-003")]
    public async Task VerifyRazorCodeBlocksTriggerDiagnostics()
    {
        const string razorSource = """
                                   @page "/counter"

                                   <h1>@CurrentCount</h1>

                                   @code {
                                       private int CurrentCount;
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlock(5, 1));
    }

    /// <summary>
    /// Verifying files without Razor code blocks do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-002")]
    public async Task VerifyRazorFileWithoutCodeBlocksDoesNotTriggerDiagnostics()
    {
        const string razorSource = """
                                   @page "/counter"

                                   <h1>Counter</h1>
                                   """;

        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)));
    }

    /// <summary>
    /// Verifying .razor.cs files do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-002")]
    public async Task VerifyRazorCodeBehindFilesDoNotTriggerDiagnostics()
    {
        const string codeBehindSource = """
                                        namespace Example;

                                        public partial class Counter
                                        {
                                            private int CurrentCount;
                                        }
                                        """;

        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("Counter.razor.cs", codeBehindSource)));
    }

    /// <summary>
    /// Verifying multiple Razor code blocks trigger multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("RH2109-003")]
    public async Task VerifyMultipleRazorCodeBlocksTriggerMultipleDiagnostics()
    {
        const string razorSource = """
                                   @code {
                                       private int First;
                                   }

                                   @code {
                                       private int Second;
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlock(1, 1),
                     RazorCodeBlock(5, 1));
    }

    /// <summary>
    /// Verifying that a code block with its brace on the same line and one with its brace on the next line are both
    /// reported, each on the five characters of the directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("RH2109-003")]
    public async Task VerifyCodeBlocksWithBraceOnSameAndNextLineAreReportedOnDirective()
    {
        const string razorSource = """
                                   @page "/counter"
                                   <h1>@CurrentCount</h1>
                                   @code {
                                       private int CurrentCount;
                                   }
                                   @code
                                   {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 3, 1),
                     RazorCodeBlockSpan("Counter.razor", 6, 1));
    }

    /// <summary>
    /// Verifying that a code block without white space between the directive and its brace is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyCodeBlockWithoutWhiteSpaceBeforeBraceIsReported()
    {
        const string razorSource = """
                                   @code{
                                       private int CurrentCount;
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying that a code block indented with white space is reported on the directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("RH2109-003")]
    public async Task VerifyIndentedCodeBlockIsReportedOnDirective()
    {
        const string razorSource = """
                                   <div>
                                   </div>
                                       @code {
                                       }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 3, 5));
    }

    /// <summary>
    /// Verifying that the file extension is compared case-insensitively
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyUpperCaseRazorExtensionIsReported()
    {
        const string razorSource = """
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.RAZOR", razorSource)),
                     RazorCodeBlockSpan("Counter.RAZOR", 1, 1));
    }

    /// <summary>
    /// Verifying that a code block in a <c>.cshtml</c> file is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-002")]
    public async Task VerifyCodeBlockInCshtmlFileIsNotReported()
    {
        const string razorSource = """
                                   @code {
                                       private int CurrentCount;
                                   }
                                   """;

        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("Index.cshtml", razorSource)));
    }

    /// <summary>
    /// Verifying code-block text in comments and strings: the text inside an HTML comment, a Razor comment, and
    /// behind other characters in a C# string is not reported, while a line-starting <c>@code {</c> inside a C# block
    /// comment of a code block is reported like the real directive, because C# comments and literals of a code block
    /// are read like the rest of the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideCSharpBlockCommentIsReportedWhileTextInMarkupCommentsIsNot()
    {
        const string razorSource = """
                                   <!-- @code { -->
                                   @* @code { *@
                                   @code {
                                       /*
                                       @code {
                                       */
                                       private string s = "@code{";
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 3, 1),
                     RazorCodeBlockSpan("Counter.razor", 5, 5));
    }

    /// <summary>
    /// Verifying code-block text in C# comments and literals of a code block: a line-starting <c>@code {</c> inside
    /// a C# block comment and inside a C# raw string literal is reported, while the text behind other characters in a
    /// C# string literal is not, because C# comments and literals of a code block are read like the rest of the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLinesInCSharpCommentAndRawStringAreReported()
    {
        const string razorSource = """"
                                   @code {
                                       /*
                                       @code {
                                       */
                                       private string s = "@code{";
                                       private string t = """
                                   @code {
                                   """;
                                   }
                                   """";

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 3, 5),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that an escaped <c>@@code {</c> and email-like text <c>user@code{</c> are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyEscapedAndEmailLikeCodeBlockTextIsNotReported()
    {
        const string razorSource = """
                                   <p>@@code { }</p>
                                   <p>user@code{</p>
                                   """;

        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)));
    }

    /// <summary>
    /// Verifying the shared example: the code-block text in an HTML comment, in a Razor comment, behind the escape
    /// <c>@@</c>, and inside <c>user@code</c> is not reported, only the directive on the last two lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifySharedExampleReportsOnlyTheDirective()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", SharedExampleSource)),
                     RazorCodeBlockSpan("Counter.razor", 5, 1));
    }

    /// <summary>
    /// Verifying the shared example with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifySharedExampleWithCarriageReturnLineFeedReportsOnlyTheDirective()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", SharedExampleSource.Replace("\n", "\r\n"))),
                     RazorCodeBlockSpan("Counter.razor", 5, 1));
    }

    /// <summary>
    /// Verifying the specification example: a <c>@code</c> behind other characters on its line is not reported, and an
    /// indented <c>@code</c> that starts its own line inside an element is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("RH2109-003")]
    public async Task VerifyCodeBlockInTheMiddleOfALineIsNotReported()
    {
        const string razorSource = """
                                   <div>@code { int X; }</div>
                                   <div>
                                       @code {
                                       }
                                   </div>
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 3, 5));
    }

    /// <summary>
    /// Verifying that a code block indented with a tab is reported on the directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("RH2109-003")]
    public async Task VerifyCodeBlockIndentedWithTabIsReported()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", "<div>\n\t@code {\n\t}\n</div>")),
                     RazorCodeBlockSpan("Counter.razor", 2, 2));
    }

    /// <summary>
    /// Verifying that a <c>@code</c> that is not followed by a brace is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyCodeWithoutBraceIsNotReported()
    {
        const string razorSource = """
                                   @code
                                   <p>Text</p>
                                   """;

        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)));
    }

    /// <summary>
    /// Verifying that an escaped <c>@@code {</c> at the start of a line is not reported, while the directive on the next
    /// line is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyEscapedCodeBlockTextAtStartOfLineIsNotReported()
    {
        const string razorSource = """
                                   @@code {
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 2, 1));
    }

    /// <summary>
    /// Verifying that a multi-line Razor comment around a whole code block is not reported, while the directive behind
    /// the comment is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockInsideMultiLineRazorCommentIsNotReported()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", CommentedOutCodeBlockSource)),
                     RazorCodeBlockSpan("Counter.razor", 6, 1));
    }

    /// <summary>
    /// Verifying a multi-line Razor comment around a whole code block with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockInsideMultiLineRazorCommentWithCarriageReturnLineFeedIsNotReported()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", CommentedOutCodeBlockSource.Replace("\n", "\r\n"))),
                     RazorCodeBlockSpan("Counter.razor", 6, 1));
    }

    /// <summary>
    /// Verifying that a code-block line inside a multi-line HTML comment is not reported, while the directive behind the
    /// comment is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockInsideMultiLineHtmlCommentIsNotReported()
    {
        const string razorSource = """
                                   <!--
                                   @code {
                                   }
                                   -->
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 5, 1));
    }

    /// <summary>
    /// Verifying that a code-block line inside a multi-line Razor comment within a code block is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockLineInsideRazorCommentWithinCodeBlockIsNotReported()
    {
        const string razorSource = """
                                   @code {
                                       @*
                                   @code {
                                       *@
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying a code-block line inside a multi-line C# documentation block comment within a code block: it is
    /// reported, because C# comments and literals of a code block are read like the rest of the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideDocumentationBlockCommentIsReported()
    {
        const string razorSource = """
                                   @code {
                                       /**
                                   @code {
                                       */
                                       private int CurrentCount;
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 3, 1));
    }

    /// <summary>
    /// Verifying a code-block line inside a multi-line C# verbatim string with escaped quotes within a code block:
    /// it is reported, because C# comments and literals of a code block are read like the rest of the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideVerbatimStringIsReported()
    {
        const string razorSource = """
                                   @code {
                                       private string s = @"a ""b""
                                   @code {
                                   ";
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 3, 1));
    }

    /// <summary>
    /// Verifying a code-block line inside a multi-line C# interpolated verbatim string within a code block: it is
    /// reported, because C# comments and literals of a code block are read like the rest of the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideInterpolatedVerbatimStringIsReported()
    {
        const string razorSource = """
                                   @code {
                                       private string s = $@"a {CurrentCount}
                                   @code {{
                                   ";
                                       private int CurrentCount;
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 3, 1));
    }

    /// <summary>
    /// Verifying a code-block line inside a multi-line C# interpolated raw string, whose hole holds braces, within
    /// a code block: it is reported like the directive after the block, because C# comments and literals of a code
    /// block are read like the rest of the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideInterpolatedRawStringIsReported()
    {
        const string razorSource = """"
                                   @code {
                                       private string s = $$"""
                                   @code {
                                   {{new int[] { 1 }.Length}}
                                   """;
                                   }
                                   @code {
                                   }
                                   """";

        await Verify(TestCode,
                     test =>
                     {
                         // The source holds "$$", which markup parsing would remove as a position marker
                         test.TestState.MarkupHandling = MarkupMode.None;
                         test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource));
                     },
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 3, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying a code-block line inside a C# block comment of a <c>@functions</c> block: it is reported like the
    /// directive after the block, because C# comments and literals of a <c>@functions</c> block are read like the rest
    /// of the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideBlockCommentOfFunctionsBlockIsReported()
    {
        const string razorSource = """
                                   @functions {
                                       /*
                                   @code {
                                       */
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 3, 1),
                     RazorCodeBlockSpan("Counter.razor", 6, 1));
    }

    /// <summary>
    /// Verifying a code-block line inside a C# block comment that follows closing braces in a C# string, a
    /// character literal, and a line comment of the same code block: it is reported, because C# comments and literals
    /// of a code block are read like the rest of the file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideBlockCommentAfterClosingBracesInLiteralsIsReported()
    {
        const string razorSource = """
                                   @code {
                                       private string s = "}";
                                       private char c = '}';
                                       // }
                                       /*
                                   @code {
                                       */
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 6, 1));
    }

    /// <summary>
    /// Verifying that opening braces inside a C# string, a character literal, and a line comment of a code block change
    /// nothing, so the directive behind the block is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyOpeningBracesInsideLiteralsAndCommentsDoNotKeepCodeBlockOpen()
    {
        const string razorSource = """
                                   @code {
                                       private string s = "{";
                                       private char c = '{';
                                       // {
                                   }
                                   <p>/*</p>
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying a code-block line inside a C# block comment that follows nested braces of a code block: it is
    /// reported like the directives, because C# comments and literals of a code block are read like the rest of the
    /// file
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideBlockCommentAfterNestedBracesIsReported()
    {
        const string razorSource = """
                                   @code {
                                       private void M()
                                       {
                                           if (CurrentCount > 0) { }
                                       }
                                       /*
                                   @code {
                                       */
                                       private int CurrentCount;
                                   }
                                   <p>/*</p>
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1),
                     RazorCodeBlockSpan("Counter.razor", 12, 1));
    }

    /// <summary>
    /// Verifying that comment and string starts inside C# literals and comments start nothing: <c>/*</c> and an escaped
    /// quote inside a string, a quote inside a character literal and a line comment, and a verbatim string that ends in a
    /// backslash
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyCommentAndStringStartsInsideLiteralsStartNothing()
    {
        const string razorSource = """
                                   @code {
                                       private string s = "/* \" ";
                                       private char c = '"';
                                       // " /*
                                       private string v = @"\";
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that a C# comment start in markup outside a code block starts no comment, so the directive behind it is
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyCSharpCommentStartInMarkupDoesNotHideDirective()
    {
        const string razorSource = """
                                   <p>/*</p>
                                   @code {
                                   }
                                   <p>*/</p>
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 2, 1));
    }

    /// <summary>
    /// Verifying that an HTML comment opener inside a C# string of a <c>@{ }</c> block starts no comment, so the
    /// directive behind it is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyHtmlCommentOpenerInStringOfCodeStatementBlockDoesNotHideDirective()
    {
        const string razorSource = """
                                   @{
                                       var s = "<!--";
                                   }
                                   <p>x</p>
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 5, 1));
    }

    /// <summary>
    /// Verifying that an HTML comment opener inside a C# string of a <c>@{ }</c> block starts no comment, so the
    /// directive behind it is reported even when a real HTML comment follows the directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyHtmlCommentOpenerInStringOfCodeStatementBlockDoesNotHideDirectiveBeforeLaterHtmlComment()
    {
        const string razorSource = """
                                   @{
                                       var s = "<!--";
                                   }
                                   @code {
                                   }
                                   <!-- real -->
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 4, 1));
    }

    /// <summary>
    /// Verifying that a Razor comment opener inside a C# string of a <c>@{ }</c> block starts no comment, so the
    /// directive behind it is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyRazorCommentOpenerInStringOfCodeStatementBlockDoesNotHideDirective()
    {
        const string razorSource = """
                                   @{
                                       var s = "@*";
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 4, 1));
    }

    /// <summary>
    /// Verifying that an HTML comment opener inside a C# string of an implicit expression starts no comment, so the
    /// directive behind it is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyHtmlCommentOpenerInImplicitExpressionDoesNotHideDirective()
    {
        const string razorSource = """
                                   <p>@M("<!--")</p>
                                   @code {
                                       string M(string s) => s;
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 2, 1));
    }

    /// <summary>
    /// Verifying that a Razor comment opener inside a C# string of an explicit expression starts no comment, so the
    /// directive behind it is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyRazorCommentOpenerInExplicitExpressionDoesNotHideDirective()
    {
        const string razorSource = """
                                   <p>@("@*")</p>
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 2, 1));
    }

    /// <summary>
    /// Verifying that an HTML comment opener inside a C# line comment of an <c>@if</c> block starts no comment, so the
    /// directive behind it is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyHtmlCommentOpenerInCSharpCommentOfControlFlowBlockDoesNotHideDirective()
    {
        const string razorSource = """
                                   @if (true)
                                   {
                                       // <!-- not a comment
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 5, 1));
    }

    /// <summary>
    /// Verifying that a Razor comment opener behind a letter is text and starts no comment, so the directive behind it
    /// is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyRazorCommentOpenerAfterLetterDoesNotHideDirective()
    {
        const string razorSource = """
                                   <p>user@*x</p>
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 2, 1));
    }

    /// <summary>
    /// Verifying that an HTML comment opener inside an attribute value starts no comment, so the directive behind it is
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyHtmlCommentOpenerInAttributeValueDoesNotHideDirective()
    {
        const string razorSource = """
                                   <input value="<!--" />
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 2, 1));
    }

    /// <summary>
    /// Verifying that an HTML comment opener inside a string of a <c>script</c> element starts no comment, so the
    /// directive behind it is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyHtmlCommentOpenerInScriptStringDoesNotHideDirective()
    {
        const string razorSource = """
                                   <script>
                                       var s = "<!--";
                                   </script>
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 4, 1));
    }

    /// <summary>
    /// Verifying that an opening brace in the markup text of a templated delegate inside a code block changes nothing, so
    /// the directive after the block is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyOpeningBraceInTemplateMarkupOfCodeBlockDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @code {
                                       RenderFragment F => @<p>{</p>;
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 4, 1));
    }

    /// <summary>
    /// Verifying that a block comment opener in the markup text of a templated delegate inside a code block changes
    /// nothing, so the directive after the block is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyBlockCommentOpenerInTemplateMarkupOfCodeBlockDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @code {
                                       RenderFragment F => @<p>/* note</p>;
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 4, 1));
    }

    /// <summary>
    /// Verifying that an opening brace in the markup text of a templated delegate inside a <c>@functions</c> block
    /// changes nothing, so the directive after the block is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyOpeningBraceInTemplateMarkupOfFunctionsBlockDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @functions {
                                       RenderFragment F => @<p>{</p>;
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 4, 1));
    }

    /// <summary>
    /// Verifying that a block comment opener in the markup text of a templated delegate inside a <c>@functions</c>
    /// block changes nothing, so the directive after the block is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyBlockCommentOpenerInTemplateMarkupOfFunctionsBlockDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @functions {
                                       RenderFragment F => @<p>/* note</p>;
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 4, 1));
    }

    /// <summary>
    /// Verifying that an opening brace in markup written directly in a method with a <c>__builder</c> parameter inside
    /// a code block does not hide the directive after the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyOpeningBraceInMarkupOfBuilderMethodDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @code {
                                       void Render(RenderTreeBuilder __builder)
                                       {
                                           <p>{</p>
                                       }
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that a block comment opener in markup written directly in a method with a <c>__builder</c> parameter
    /// inside a code block does not hide the directive after the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyBlockCommentOpenerInMarkupOfBuilderMethodDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @code {
                                       void Render(RenderTreeBuilder __builder)
                                       {
                                           <p>/* note</p>
                                       }
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that an opening brace in markup written directly in a <c>__builder</c> lambda inside a code block does
    /// not hide the directive after the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyOpeningBraceInMarkupOfBuilderLambdaDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @code {
                                       RenderFragment F => __builder =>
                                       {
                                           <p>{</p>
                                       };
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that an opening brace in markup written directly in a method with a <c>__builder</c> parameter inside
    /// a <c>@functions</c> block does not hide the directive after the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyOpeningBraceInMarkupOfBuilderMethodInFunctionsBlockDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @functions {
                                       void Render(RenderTreeBuilder __builder)
                                       {
                                           <p>{</p>
                                       }
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that a block comment opener in markup written directly in a method with a <c>__builder</c> parameter
    /// inside a <c>@functions</c> block does not hide the directive after the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyBlockCommentOpenerInMarkupOfBuilderMethodInFunctionsBlockDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @functions {
                                       void Render(RenderTreeBuilder __builder)
                                       {
                                           <p>/* note</p>
                                       }
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that an opening brace in a <c>text</c> element inside a code block does not hide the directive after
    /// the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyOpeningBraceInTextElementOfCodeBlockDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @code {
                                       void Render(RenderTreeBuilder __builder)
                                       {
                                           <text>{</text>
                                       }
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that an opening brace in markup written inline after a statement brace inside a code block does not
    /// hide the directive after the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    public async Task VerifyOpeningBraceInInlineMarkupOfCodeBlockDoesNotHideLaterDirective()
    {
        const string razorSource = """
                                   @code {
                                       void Render(RenderTreeBuilder __builder)
                                       {
                                           if (true) { <p>{</p> }
                                       }
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying the specification example of a line inside a multi-line C# comment or literal: a line-starting
    /// <c>@code {</c> inside a C# block comment and inside a C# raw string literal of a code block is read like markup
    /// and reported like the directive of the code block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifySpecExampleCodeBlockLinesInsideCSharpCommentAndRawStringAreReported()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", CodeBlockLinesInCSharpCommentAndRawStringSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 3, 5),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying the specification example of a line inside a multi-line C# comment or literal with CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifySpecExampleCodeBlockLinesInsideCSharpCommentAndRawStringWithCarriageReturnLineFeedAreReported()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", CodeBlockLinesInCSharpCommentAndRawStringSource.Replace("\n", "\r\n"))),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 3, 5),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying the specification example of a Razor comment opener that starts a line inside a C# block comment: it
    /// opens a Razor comment for the rule, and without a closing <c>*@</c> the comment runs to the end of the file, so the
    /// real directive behind the code block is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifySpecExampleRazorCommentOpenerLineInsideCSharpCommentHidesTheRestOfTheFile()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", RazorCommentOpenerLineInCSharpCommentSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying the specification example of a Razor comment opener that starts a line inside a C# block comment with
    /// CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifySpecExampleRazorCommentOpenerLineInsideCSharpCommentWithCarriageReturnLineFeedHidesTheRestOfTheFile()
    {
        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", RazorCommentOpenerLineInCSharpCommentSource.Replace("\n", "\r\n"))),
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying the specification example of the closer pairing: a Razor comment opened by a line-starting <c>@*</c>
    /// inside a C# block comment is not closed by a later HTML comment closer <c>--&gt;</c>, so the real directive behind it
    /// is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifySpecExampleRazorCommentOpenerLineInsideCSharpCommentIsNotClosedByHtmlCommentCloser()
    {
        const string razorSource = """
                                   @code {
                                       /*
                                       @* x
                                       */
                                   }
                                   <!-- -->
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying that a Razor comment opened by a line-starting <c>@*</c> inside a C# block comment ends at the next
    /// <c>*@</c>, so the real directive behind that closer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyRazorCommentOpenerLineInsideCSharpCommentEndsAtRazorCommentCloser()
    {
        const string razorSource = """
                                   @code {
                                       /*
                                       @* x
                                       */
                                   }
                                   @* *@
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that an HTML comment opener that starts a line inside a C# block comment opens an HTML comment for the
    /// rule, and without a closing <c>--&gt;</c> the comment runs to the end of the file, so the real directive behind the
    /// code block is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyHtmlCommentOpenerLineInsideCSharpCommentHidesTheRestOfTheFile()
    {
        const string razorSource = """
                                   @code {
                                       /*
                                       <!-- not an HTML comment
                                       */
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying that an HTML comment opened by a line-starting <c>&lt;!--</c> inside a C# block comment is not closed by a
    /// later Razor comment closer <c>*@</c>, so the real directive behind it is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyHtmlCommentOpenerLineInsideCSharpCommentIsNotClosedByRazorCommentCloser()
    {
        const string razorSource = """
                                   @code {
                                       /*
                                       <!-- x
                                       */
                                   }
                                   @* *@
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying that an HTML comment opened by a line-starting <c>&lt;!--</c> inside a C# block comment ends at the next
    /// <c>--&gt;</c>, so the real directive behind that closer is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyHtmlCommentOpenerLineInsideCSharpCommentEndsAtHtmlCommentCloser()
    {
        const string razorSource = """
                                   @code {
                                       /*
                                       <!-- x
                                       */
                                   }
                                   <!-- -->
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that a Razor comment opener that starts a line inside a C# verbatim string literal opens a Razor comment
    /// for the rule, so the real directive behind the code block is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyRazorCommentOpenerLineInsideVerbatimStringHidesTheRestOfTheFile()
    {
        const string razorSource = """
                                   @code {
                                       private string s = @"
                                   @* not a Razor comment
                                   ";
                                   }
                                   @code {
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying that a line-starting <c>@code {</c> inside a C# block comment of a <c>@{ }</c> statement block is read like
    /// markup and reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideBlockCommentOfStatementBlockIsReported()
    {
        const string razorSource = """
                                   @{
                                       /*
                                       @code {
                                       */
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 3, 5));
    }

    /// <summary>
    /// Verifying that a line-starting <c>@code {</c> inside a C# block comment of an <c>@if</c> block is read like markup and
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineInsideBlockCommentOfControlFlowBlockIsReported()
    {
        const string razorSource = """
                                   @if (true)
                                   {
                                       /*
                                       @code {
                                       */
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 4, 5));
    }

    /// <summary>
    /// Verifying that a line-starting <c>@code</c> inside a C# block comment whose opening brace stands on the next line is
    /// read like markup and reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockLineWithBraceOnNextLineInsideBlockCommentIsReported()
    {
        const string razorSource = """
                                   @code {
                                       /*
                                       @code
                                       {
                                       }
                                       */
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1),
                     RazorCodeBlockSpan("Counter.razor", 3, 5));
    }

    /// <summary>
    /// Verifying that a <c>@code {</c> inside a single-line C# comment of a code block is not reported, because the
    /// comment's opener stands before it on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-004")]
    public async Task VerifyCodeBlockTextInsideSingleLineCommentIsNotReported()
    {
        const string razorSource = """
                                   @code {
                                       // @code {
                                       /* @code { */
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Characterizes a <c>@functions</c> block: it is not reported. This test pins today's behavior without
    /// stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFunctionsBlockIsNotReported()
    {
        const string razorSource = """
                                   @functions {
                                       private int CurrentCount;
                                   }
                                   """;

        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Razor code block diagnostic
    /// </summary>
    /// <param name="line">Line</param>
    /// <param name="column">Column</param>
    /// <returns>Diagnostic</returns>
    private static DiagnosticResult RazorCodeBlock(int line, int column)
    {
        return Diagnostic(RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.DiagnosticId).WithLocation("Counter.razor", line, column)
                                                                                    .WithMessage(AnalyzerResources.RH2109MessageFormat);
    }

    /// <summary>
    /// Razor code block diagnostic covering the five characters of the directive
    /// </summary>
    /// <param name="path">Path of the additional file</param>
    /// <param name="line">Line</param>
    /// <param name="column">Column</param>
    /// <returns>Diagnostic</returns>
    private static DiagnosticResult RazorCodeBlockSpan(string path, int line, int column)
    {
        return Diagnostic(RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer.DiagnosticId).WithSpan(path, line, column, line, column + 5)
                                                                                    .WithMessage(AnalyzerResources.RH2109MessageFormat);
    }

    #endregion // Methods
}