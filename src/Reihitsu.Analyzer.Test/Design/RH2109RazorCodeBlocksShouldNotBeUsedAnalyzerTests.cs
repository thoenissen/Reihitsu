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
    /// Verifying that code-block text inside an HTML comment, a Razor comment, a C# block comment, and a C# string of a
    /// <c>.razor</c> file is not reported, while the real directive is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockTextInCommentsAndStringsIsNotReported()
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
                     RazorCodeBlockSpan("Counter.razor", 3, 1));
    }

    /// <summary>
    /// Verifying that code-block text inside a C# block comment, a C# string literal, and a C# raw string literal of a
    /// <c>.razor</c> code block is not reported, while the directive of the block is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockTextInCSharpCommentAndStringLiteralsIsNotReported()
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
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
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
    /// Verifying that a code-block line inside a multi-line C# documentation block comment within a code block is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockLineInsideDocumentationBlockCommentIsNotReported()
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
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying that a code-block line inside a multi-line C# verbatim string with escaped quotes within a code block
    /// is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockLineInsideVerbatimStringIsNotReported()
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
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying that a code-block line inside a multi-line C# interpolated verbatim string within a code block is not
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockLineInsideInterpolatedVerbatimStringIsNotReported()
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
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying that a code-block line inside a multi-line C# interpolated raw string, whose hole holds braces, within a
    /// code block is not reported, and that the directive after the block is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockLineInsideInterpolatedRawStringIsNotReported()
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
                     RazorCodeBlockSpan("Counter.razor", 7, 1));
    }

    /// <summary>
    /// Verifying that a code-block line inside a C# block comment of a <c>@functions</c> block is not reported, while
    /// the directive after the block is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyCodeBlockLineInsideBlockCommentOfFunctionsBlockIsNotReported()
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
                     RazorCodeBlockSpan("Counter.razor", 6, 1));
    }

    /// <summary>
    /// Verifying that closing braces inside a C# string, a character literal, and a line comment do not end the code
    /// block, so a code-block line inside a later block comment of the same block is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyClosingBracesInsideLiteralsAndCommentsDoNotEndCodeBlock()
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
                     RazorCodeBlockSpan("Counter.razor", 1, 1));
    }

    /// <summary>
    /// Verifying that opening braces inside a C# string, a character literal, and a line comment do not keep the code
    /// block open, so markup text after the block is not read as C# and the directive behind it is reported
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
    /// Verifying that nested braces of C# code keep the code block open until its own closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH2109-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNestedBracesKeepCodeBlockOpenUntilItsClosingBrace()
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