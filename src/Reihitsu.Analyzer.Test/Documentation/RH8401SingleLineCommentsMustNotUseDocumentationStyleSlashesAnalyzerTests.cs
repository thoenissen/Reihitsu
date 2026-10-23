using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
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
public class RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzerTests : BatchCodeFixTestsBase<RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer, RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic and code fix for a comment using documentation slashes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8401-001")]
    [Requirement("RH8401-003")]
    [Requirement("RH8401-004")]
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
    /// Verifies that a documentation-style comment is reported in a syntax tree whose documentation mode is parse, as in
    /// the default mode diagnose
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8401-001")]
    public async Task VerifyDiagnosticWhenDocumentationModeIsParse()
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

        await Verify(source,
                     test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject),
                     Diagnostics(RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId, AnalyzerResources.RH8401MessageFormat));
    }

    /// <summary>
    /// Characterizes a documentation-style comment in a syntax tree whose documentation mode is none: it is not reported.
    /// This test pins today's behavior; the decided behavior is stated by SHARED-DOC-008, under which a rule about an
    /// ordinary comment checks every documentation mode, and its change belongs to an existing follow-up
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
    /// Verifies that a comment reporting several diagnostics converges: applying the fix clears every diagnostic
    /// in the comment and the re-analyzed result reports nothing further
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8401-001")]
    [Requirement("RH8401-003")]
    [Requirement("RH8401-004")]
    public async Task VerifyDiagnosticsAndCodeFixForMultiLineDocumentationStyleComment()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  {|#0:///|} just a comment
                                  {|#1:///|} second line
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
                                       // second line
                                       internal void TestMethod()
                                       {
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource, Diagnostics(RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId, AnalyzerResources.RH8401MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a single application converts every line of a line-feed separated comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8401-004")]
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
    [Requirement("RH8401-004")]
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
    [Requirement("RH8401-004")]
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
    [Requirement("RH8401-004")]
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
    [Requirement("RH8401-004")]
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
    [Requirement("RH8401-004")]
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

    /// <summary>
    /// Verifies that <c>///</c> comments without XML are reported on each line's slashes wherever they stand: in
    /// front of a declaration, in a method body, behind code, with a character reference, and without any text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8401-001")]
    [Requirement("RH8401-003")]
    public async Task VerifyCommentsWithoutXmlAreReportedWhereverTheyStand()
    {
        const string testData = """
                                class C
                                {
                                    {|#0:///|} Temporary workaround
                                    {|#1:///|} for the old service
                                    int A;

                                    void M()
                                    {
                                        {|#2:///|} Call it again
                                        M();
                                        int b = 0; {|#3:///|} Behind code
                                    }

                                    {|#4:///|} Tom &amp; Jerry
                                    int D;

                                    {|#5:///|}
                                    int E;
                                }
                                """;

        await Verify(testData, Diagnostics(RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId, AnalyzerResources.RH8401MessageFormat, 6));
    }

    /// <summary>
    /// Verifies that documentation comments holding an element, an inline element, or an XML comment, a delimited
    /// comment, and a four-slash comment are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8401-002")]
    public async Task VerifyCommentsWithXmlAndOtherCommentFormsDoNotProduceDiagnostics()
    {
        const string testData = """
                                class C
                                {
                                    /// <summary>Runs</summary>
                                    void M()
                                    {
                                    }

                                    /// Stops <c>now</c>
                                    void N()
                                    {
                                    }

                                    /// <!-- Note -->
                                    int A;

                                    /** Plain text */
                                    int B;

                                    //// Four slashes
                                    int D;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix replaces the slashes of every line and keeps a <c>///</c> further along a line as text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8401-003")]
    [Requirement("RH8401-004")]
    public async Task VerifyFixTurnsTheWholeCommentIntoAnOrdinaryComment()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        {|#0:///|} Temporary workaround
                                        {|#1:///|} see http://example.com///path
                                        M();
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         // Temporary workaround
                                         // see http://example.com///path
                                         M();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId, AnalyzerResources.RH8401MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix applied to the diagnostic of the comment's second line converts the first line as well
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH8401-004")]
    public async Task VerifyFixAppliedToTheSecondLineConvertsEveryLine()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        /// Temporary workaround
                                        /// for the old service
                                        M();
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         // Temporary workaround
                                         // for the old service
                                         M();
                                     }
                                 }
                                 """;
        var secondLineSlashes = testData.IndexOf("/// for", StringComparison.Ordinal);

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId,
                                                   root => Location.Create(root.SyntaxTree, new TextSpan(secondLineSlashes, 3)));

        Assert.HasCount(1, actions);

        var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        var changedSolution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        var changedText = await changedSolution.Projects.Single().Documents.Single().GetTextAsync(CancellationToken.None);

        Assert.AreEqual(fixedData, changedText.ToString());
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

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string source = """
                              namespace TestNamespace;

                              internal class TestClass
                              {
                                  {|#0:///|} just a comment
                                  {|#1:///|} second line
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
                                       // second line
                                       internal void TestMethod()
                                       {
                                       }
                                   }
                                   """;

        // Both diagnostics belong to the same documentation comment and both fixes replace the identical
        // ParentTrivia.FullSpan, so the batch merge must apply that replacement once rather than twice
        return new FixAllScenario(source, fixedSource, Diagnostics(RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer.DiagnosticId, AnalyzerResources.RH8401MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}