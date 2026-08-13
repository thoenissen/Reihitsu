using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Documentation;
using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Documentation;

/// <summary>
/// Tests for <see cref="RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer"/> and
/// <see cref="RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzerTests : AnalyzerTestsBase<RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer, RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic and code fix for a comment using documentation slashes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForDocumentationStyleComment()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  {|#0:///|} just a comment
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        const string fixedSource = """
                                   namespace TestNamespace;

                                   internal class TestClass
                                   {
                                       // just a comment
                                       internal void TestMethod()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId, AnalyzerResources.RH8401MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported when documentation mode is none
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenDocumentationModeIsNone()
    {
        const string source = """
                              namespace TestNamespace;
                              
                              internal class TestClass
                              {
                                  {|#0:///|} just a comment
                                  internal void TestMethod()
                                  {
                                  }
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that a single application converts every line of a line-feed separated comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixConvertsEveryLineOfLineFeedSeparatedComment()
    {
        var source = BuildTwoLineCommentSource("\n");
        var expected = BuildTwoLineCommentSource("\n").Replace("///", "//");

        var actual = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a single application converts every line of a carriage-return line-feed separated comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixConvertsEveryLineOfCarriageReturnLineFeedSeparatedComment()
    {
        var source = BuildTwoLineCommentSource("\r\n");
        var expected = BuildTwoLineCommentSource("\r\n").Replace("///", "//");

        var actual = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a single application converts every line of a comment whose lines are separated by the
    /// Unicode line separator. C# recognizes U+2028 as a line terminator, so both lines are documentation lines
    /// and one application has to convert both — leaving the second line as <c>///</c> would emit a comment that
    /// is half <c>//</c> and half <c>///</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixConvertsEveryLineOfLineSeparatorSeparatedComment()
    {
        var source = BuildTwoLineCommentSource("\u2028");
        var expected = BuildTwoLineCommentSource("\u2028").Replace("///", "//");

        var actual = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a single application converts every line of a comment whose lines are separated by the
    /// next-line character. C# recognizes U+0085 as a line terminator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixConvertsEveryLineOfNextLineSeparatedComment()
    {
        var source = BuildTwoLineCommentSource("\u0085");
        var expected = BuildTwoLineCommentSource("\u0085").Replace("///", "//");

        var actual = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a single application converts every line of a carriage-return separated comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixConvertsEveryLineOfCarriageReturnSeparatedComment()
    {
        var source = BuildTwoLineCommentSource("\r");
        var expected = BuildTwoLineCommentSource("\r").Replace("///", "//");

        var actual = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the exterior is converted while the content after it is preserved verbatim, including a
    /// second <c>///</c> that is content rather than an exterior
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixPreservesContentAfterTheExterior()
    {
        var source = string.Concat("namespace TestNamespace;\n",
                                   "\n",
                                   "internal class TestClass\n",
                                   "{\n",
                                   "\t///  spaced /// content\n",
                                   "    internal void TestMethod()\n",
                                   "    {\n",
                                   "    }\n",
                                   "}\n");
        var expected = source.Replace("\t///  spaced", "\t//  spaced");

        var actual = await ApplyCodeFixAsync(source);

        Assert.AreEqual(expected, actual);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Builds a source document whose documentation comment spans two lines separated by the given terminator
    /// </summary>
    /// <param name="lineTerminator">Terminator separating the two documentation lines</param>
    /// <returns>The source document</returns>
    private static string BuildTwoLineCommentSource(string lineTerminator)
    {
        return string.Concat("namespace TestNamespace;\n",
                             "\n",
                             "internal class TestClass\n",
                             "{\n",
                             "    ///just a comment",
                             lineTerminator,
                             "    ///second line\n",
                             "    internal void TestMethod()\n",
                             "    {\n",
                             "    }\n",
                             "}\n");
    }

    #endregion // Methods
}