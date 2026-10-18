using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer"/> and <see cref="RH5023CodeMustNotContainMultipleBlankLinesInARowCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzerTests : BatchCodeFixTestsBase<RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer, RH5023CodeMustNotContainMultipleBlankLinesInARowCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    [Requirement("RH5023-001")]
    [Requirement("RH5023-003")]
    [Requirement("RH5023-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 0;
                                
                                {|#0:
                                |}        int second = 1;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = 0;
                                 
                                         int second = 1;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer.DiagnosticId, AnalyzerResources.RH5023MessageFormat));
    }

    /// <summary>
    /// Verifies that multiple blank lines inside raw strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5023-002")]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       First


                                                       Second
                                                       """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiple blank lines inside verbatim strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5023-002")]
    public async Task VerifyVerbatimStringsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Property => @"First


                                Second";
                                }

                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiple blank lines inside a multi-line comment do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5023-002")]
    public async Task VerifyMultiLineCommentBlankLinesDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /*
                                    first


                                    second
                                    */
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiple blank lines inside disabled code do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5023-002")]
    public async Task VerifyDisabledCodeBlankLinesDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                #if false
                                    int first = 0;


                                    int second = 1;
                                #endif
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that <see cref="BlankLineSpanRemovalCodeFixProviderBase"/> withholds its fix when the
    /// reported span is not whitespace-only, so a future analyzer defect degrades to no fix being offered
    /// instead of deleting non-whitespace text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNonWhitespaceSpanWithholdsFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 0;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer.DiagnosticId,
                                                   root => root.DescendantTokens().First(token => token.Text == "first").GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that an empty reported span is treated as whitespace-only and still registers a fix, matching
    /// today's unconditional deletion of an empty span
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptySpanStillRegistersFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 0;
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var token = root.DescendantTokens().First(item => item.Text == "first");

                                                       return Location.Create(root.SyntaxTree, new TextSpan(token.SpanStart, 0));
                                                   });

        Assert.HasCount(1, actions);
    }

    /// <summary>
    /// Verifies every blank line after the first of a run is reported on its own and deleted, so the run shrinks to one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5023-001")]
    [Requirement("RH5023-003")]
    [Requirement("RH5023-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyEachBlankLineAfterFirstOfRunIsReportedAndDeleted()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = 1;

                                {|RH5023:
                                |}{|RH5023:
                                |}        a++;
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var a = 1;

                                         a++;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies lines of a multi-line comment, of disabled code, and of a verbatim string are neither reported nor
    /// counted as blank, so only the second blank line below the block comment is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5023-002")]
    public async Task VerifyLinesOfCommentsDisabledCodeAndStringsAreNotCounted()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = 1;
                                        /* one


                                           two */

                                {|RH5023:
                                |}        a++;
                                #if NEVER
                                        a++;


                                #endif
                                        var s = @"x


                                y";
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies blank lines inside an interpolated raw string and an interpolated verbatim string are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5023-002")]
    public async Task VerifyBlankLinesInsideInterpolatedStringsAreNotReported()
    {
        const string testCode = """"
                                class C
                                {
                                    string M(int x)
                                    {
                                        var a = $"""
                                            a


                                            {x}
                                            """;

                                        return a + $@"a


                                {x}";
                                    }
                                }
                                """";

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a whitespace-only line that follows a blank line is reported and deleted together with its CRLF line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5023-001")]
    [Requirement("RH5023-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceOnlySecondBlankLineWithCarriageReturnLineFeed()
    {
        const string testCode = "class C\n{\n    void M()\n    {\n        var a = 1;\n\n        \n        a++;\n    }\n}\n";
        const string fixedCode = "class C\n{\n    void M()\n    {\n        var a = 1;\n\n        a++;\n    }\n}\n";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode), actual);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int first = 0;

                                {|#0:
                                |}        int second = 1;

                                {|#1:
                                |}        int third = 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = 0;

                                         int second = 1;

                                         int third = 2;
                                     }
                                 }
                                 """;

        // Two adjacent excess-blank-line groups, each collapsing two blank lines down to one, so the second
        // fix's removal span sits close behind the first fix's own removal
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5023CodeMustNotContainMultipleBlankLinesInARowAnalyzer.DiagnosticId, AnalyzerResources.RH5023MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}