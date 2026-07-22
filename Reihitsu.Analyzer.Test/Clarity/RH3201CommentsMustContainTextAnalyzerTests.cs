using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3201CommentsMustContainTextAnalyzer"/> and <see cref="RH3201CommentsMustContainTextCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3201CommentsMustContainTextAnalyzerTests : AnalyzerTestsBase<RH3201CommentsMustContainTextAnalyzer, RH3201CommentsMustContainTextCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying empty single-line comment is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptySingleLineCommentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0://|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying single-line comment with only whitespace is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SingleLineCommentWithWhitespaceIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0://   |}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying empty multi-line comment is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyMultiLineCommentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:/**/|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying multi-line comment with only whitespace is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MultiLineCommentWithWhitespaceIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:/*   */|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying valid single-line comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidSingleLineCommentIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        // This is a valid comment
                                        var value = 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying valid multi-line comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ValidMultiLineCommentIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        /* This is a valid comment */
                                        var value = 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying multiple empty comments are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task MultipleEmptyCommentsAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0://|}
                                        {|#1:/**/|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text.", 2));
    }

    /// <summary>
    /// Verifying empty comment after statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyCommentAfterStatementIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        var value = 1; {|#0://|}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying empty multi-line comment with newlines is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyMultiLineCommentWithNewlinesIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        {|#0:/*
                                        */|}
                                        var value = 1;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {

                                         var value = 1;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH3201CommentsMustContainTextAnalyzer.DiagnosticId, "Comments must contain text."));
    }

    /// <summary>
    /// Verifying empty separator lines inside single-line comment blocks are allowed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptySingleLineCommentInsideCommentBlockIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        // Comment 1
                                        //
                                        // Comment 2
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying an empty separator before an ordinary four-slash comment is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptySingleLineCommentBeforeFourSlashCommentIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        // First
                                        //
                                        //// Second
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}