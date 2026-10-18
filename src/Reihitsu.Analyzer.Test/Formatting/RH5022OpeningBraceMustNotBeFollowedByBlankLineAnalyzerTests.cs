using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer"/> and <see cref="RH5022OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer, RH5022OpeningBraceMustNotBeFollowedByBlankLineCodeFixProvider>
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
    [Requirement("RH5022-001")]
    [Requirement("RH5022-003")]
    [Requirement("RH5022-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                {|#0:
                                |}        int value = 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5022MessageFormat));
    }

    /// <summary>
    /// Verifies that blank lines inside raw strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5022-002")]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       {

                                                       }
                                                       """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an opening brace followed by a blank line inside a multi-line comment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5022-002")]
    public async Task VerifyMultiLineCommentOpeningBraceDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /*
                                    void Disabled()
                                    {

                                        int value = 0;
                                    }
                                    */
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an opening brace followed by a blank line inside disabled code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5022-002")]
    public async Task VerifyDisabledCodeOpeningBraceDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                #if false
                                    void Disabled()
                                    {

                                        int value = 0;
                                    }
                                #endif
                                    void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the blank line below the opening brace of a type, an initializer, and a member body is reported once
    /// per brace, from the line's start to the end of its line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5022-001")]
    [Requirement("RH5022-003")]
    public async Task VerifyBlankLineBelowOpeningBraceOfTypeInitializerAndMemberIsReported()
    {
        const string testCode = """
                                class C
                                {
                                {|RH5022:
                                |}    int[] _a = new[]
                                    {
                                {|RH5022:
                                |}        1,
                                    };

                                    void M()
                                    {
                                {|RH5022:
                                |}        _a[0]++;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a brace in a raw string, in a multi-line comment, or in disabled code is not considered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5022-002")]
    public async Task VerifyBraceInStringCommentOrDisabledCodeIsNotConsidered()
    {
        const string testCode = """"
                                class C
                                {
                                    string _s = """
                                        {

                                        """;

                                    /* {

                                    */
                                #if NEVER
                                    void M()
                                    {

                                    }
                                #endif
                                }
                                """";

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix deletes each reported blank line and nothing else
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5022-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesBlankLinesBelowOpeningBraces()
    {
        const string testCode = """
                                class C
                                {
                                {|RH5022:
                                |}    int[] _a = new[]
                                    {
                                {|RH5022:
                                |}        1,
                                    };
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     int[] _a = new[]
                                     {
                                         1,
                                     };
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a whitespace-only line below an opening brace is reported and deleted together with its CRLF line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5022-001")]
    [Requirement("RH5022-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceOnlyLineBelowOpeningBraceWithCarriageReturnLineFeed()
    {
        const string testCode = "class C\n{\n    void M()\n    {\n        \n        M();\n    }\n}\n";
        const string fixedCode = "class C\n{\n    void M()\n    {\n        M();\n    }\n}\n";

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
                                        if (true)
                                        {
                                {|#0:
                                |}            int first = 0;
                                        }
                                        if (true)
                                        {
                                {|#1:
                                |}            int second = 0;
                                        }
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
                                             int first = 0;
                                         }
                                         if (true)
                                         {
                                             int second = 0;
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent if blocks, each with a blank line directly after its opening brace, so the second fix's
        // removal span sits close behind the first fix's own removal
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5022OpeningBraceMustNotBeFollowedByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5022MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}