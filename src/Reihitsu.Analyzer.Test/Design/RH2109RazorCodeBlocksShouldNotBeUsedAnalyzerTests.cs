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
    /// Characterizes code-block text inside an HTML comment, a Razor comment, a C# block comment, and a C# string of a
    /// <c>.razor</c> file: each occurrence is reported like the real directive. These cases are an open question about
    /// intended behavior, so this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeBlockTextInCommentsAndStringsIsReported()
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
                     RazorCodeBlockSpan("Counter.razor", 1, 6),
                     RazorCodeBlockSpan("Counter.razor", 2, 4),
                     RazorCodeBlockSpan("Counter.razor", 3, 1),
                     RazorCodeBlockSpan("Counter.razor", 5, 5),
                     RazorCodeBlockSpan("Counter.razor", 7, 25));
    }

    /// <summary>
    /// Characterizes an escaped <c>@@code {</c> and email-like text <c>user@code{</c>: both are reported. These cases
    /// are an open question about intended behavior, so this test pins today's behavior without stating it as intended
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEscapedAndEmailLikeCodeBlockTextIsReported()
    {
        const string razorSource = """
                                   <p>@@code { }</p>
                                   <p>user@code{</p>
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlockSpan("Counter.razor", 1, 5),
                     RazorCodeBlockSpan("Counter.razor", 2, 8));
    }

    /// <summary>
    /// Characterizes a <c>@functions</c> block: it is not reported. The case is an open question about intended
    /// behavior, so this test pins today's behavior without stating it as intended
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