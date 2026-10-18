using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer"/> and <see cref="RH5025OpeningBraceMustNotBePrecededByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer, RH5025OpeningBraceMustNotBePrecededByBlankLineCodeFixProvider>
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
    [Requirement("RH5025-001")]
    [Requirement("RH5025-003")]
    [Requirement("RH5025-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                {|#0:
                                |}    {
                                        int value = 0;
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

        await Verify(testData, fixedData, Diagnostics(RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5025MessageFormat));
    }

    /// <summary>
    /// Verifies that raw-string content does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawStringOpeningBraceDoesNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    private const string Value = """
                                                                 
                                                                 {
                                                                 """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an opening brace preceded by a blank line inside a multi-line comment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineCommentOpeningBraceDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /*
                                    void Disabled()

                                    {
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
    /// Verifies that an opening brace preceded by a blank line inside disabled code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDisabledCodeOpeningBraceDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                #if false
                                    void Disabled()

                                    {
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
    /// Verifies the blank line above an opening brace that continues the code above it is reported for namespaces, types,
    /// accessor lists, members, initializers, statements, lambdas, switch expressions, property patterns, and anonymous objects
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-001")]
    [Requirement("RH5025-003")]
    public async Task VerifyBlankLineAboveOpeningBraceThatContinuesCodeIsReported()
    {
        const string testCode = """
                                namespace N
                                {|RH5025:
                                |}{
                                    class C
                                {|RH5025:
                                |}    {
                                        int P
                                {|RH5025:
                                |}        {
                                            get;
                                        }

                                        void M(int a, string s)
                                {|RH5025:
                                |}        {
                                            int[] i = new[]
                                {|RH5025:
                                |}            {
                                                1,
                                            };
                                            if (a > 0)
                                {|RH5025:
                                |}            {
                                            }

                                            System.Action f = () =>
                                {|RH5025:
                                |}            {
                                            };
                                            var b = a switch
                                {|RH5025:
                                |}            {
                                                _ => 0,
                                            };
                                            var c = s is
                                {|RH5025:
                                |}                { Length: 0 };
                                            var d = new
                                {|RH5025:
                                |}            {
                                                X = 1,
                                            };
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a blank line above a comment line that precedes the brace, and a brace in a multi-line comment or in
    /// disabled code, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-002")]
    public async Task VerifyBlankLineSeparatedByCommentOrBraceOutsideCodeIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()

                                    // note
                                    {
                                    }

                                    /*

                                    { */
                                #if NEVER
                                    void N()

                                    {
                                    }
                                #endif
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix deletes each reported blank line and nothing else
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesBlankLinesAboveOpeningBraces()
    {
        const string testCode = """
                                class C
                                {|RH5025:
                                |}{
                                    void M()
                                {|RH5025:
                                |}    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a whitespace-only line above an opening brace is reported and deleted together with its CRLF line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5025-001")]
    [Requirement("RH5025-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceOnlyLineAboveOpeningBraceWithCarriageReturnLineFeed()
    {
        const string testCode = "class C\n{\n    void M()\n    \n    {\n    }\n}\n";
        const string fixedCode = "class C\n{\n    void M()\n    {\n    }\n}\n";

        var actual = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testCode));

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedCode), actual);
    }

    /// <summary>
    /// Verifies the blank line between an extension block header and its opening brace is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlankLineAboveExtensionBlockBraceIsReported()
    {
        const string testCode = """
                                static class E
                                {
                                    extension(int i)
                                {|RH5025:
                                |}    {
                                        public int Twice => i * 2;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void First()
                                {|#0:
                                |}    {
                                        int value = 0;
                                    }

                                    void Second()
                                {|#1:
                                |}    {
                                        int value = 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void First()
                                     {
                                         int value = 0;
                                     }

                                     void Second()
                                     {
                                         int value = 0;
                                     }
                                 }
                                 """;

        // Two adjacent method declarations, each with a blank line directly before its opening brace, so the
        // second fix's removal span sits close behind the first fix's own removal
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5025OpeningBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5025MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}