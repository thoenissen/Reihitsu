using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer"/> and <see cref="RH5024ClosingBraceMustNotBePrecededByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer, RH5024ClosingBraceMustNotBePrecededByBlankLineCodeFixProvider>
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
    [Requirement("RH5024-001")]
    [Requirement("RH5024-004")]
    [Requirement("RH5024-005")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 0;
                                {|#0:
                                |}    }
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

        await Verify(testData, fixedData, Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that raw strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-003")]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       value

                                                       }
                                                       """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a closing brace with a trailing semicolon is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-001")]
    [Requirement("RH5024-005")]
    public async Task VerifyClosingBraceWithTrailingSemicolonIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int[] Create()
                                    {
                                        return new[]
                                        {
                                            1,
                                {|#0:
                                |}        };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int[] Create()
                                     {
                                         return new[]
                                         {
                                             1,
                                         };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that a closing brace preceded by a blank line inside a multi-line comment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-003")]
    public async Task VerifyMultiLineCommentClosingBraceDoesNotProduceDiagnostics()
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
    /// Verifies that a closing brace preceded by a blank line inside disabled code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-003")]
    public async Task VerifyDisabledCodeClosingBraceDoesNotProduceDiagnostics()
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
    /// Verifies that a blank line between an <c>#endregion</c> directive and the closing brace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-001")]
    [Requirement("RH5024-005")]
    public async Task VerifyBlankLineBetweenEndRegionAndClosingBraceIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Properties

                                    public int Value { get; set; }

                                    #endregion
                                {|#0:
                                |}}
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     #region Properties

                                     public int Value { get; set; }

                                     #endregion
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat));
    }

    /// <summary>
    /// Verifies that a blank line preceding a trailing <c>#endregion</c> directive that hugs the closing brace does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-002")]
    public async Task VerifyBlankLineBeforeTrailingEndRegionDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    #region Properties
                                    public int Value { get; set; }

                                    #endregion
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the blank line above a closing brace is reported below code and below an endregion directive, whatever
    /// follows the brace on its line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-001")]
    [Requirement("RH5024-004")]
    public async Task VerifyBlankLineAboveClosingBraceIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    int[] _a = new[]
                                    {
                                        1,
                                {|RH5024:
                                |}    };

                                    void M()
                                    {
                                        _a[0]++;
                                {|RH5024:
                                |}    } // note

                                    #region R

                                    void N()
                                    {
                                    }
                                    #endregion // R
                                {|RH5024:
                                |}}
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a blank line is not reported when a comment line or a directive line stands between it and the closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-002")]
    public async Task VerifyBlankLineSeparatedFromClosingBraceByCommentOrDirectiveIsNotReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        _ = 1;

                                        #region R

                                        _ = 2;
                                        #endregion // R
                                    }

                                    void N()
                                    {
                                        _ = 1;

                                        // trailing
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a closing brace in a raw string, in a multi-line comment, or in disabled code is not considered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-003")]
    public async Task VerifyBraceInStringCommentOrDisabledCodeIsNotConsidered()
    {
        const string testCode = """"
                                class C
                                {
                                    string _s = """

                                        }
                                        """;

                                    /*

                                    } */
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
    /// Verifies the fix deletes each reported blank line, also above a closing brace followed by while
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-005")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesBlankLinesAboveClosingBraces()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        do
                                        {
                                            a++;
                                {|RH5024:
                                |}        } while (a < 3);
                                {|RH5024:
                                |}    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M(int a)
                                     {
                                         do
                                         {
                                             a++;
                                         } while (a < 3);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a whitespace-only line above a closing brace is reported and deleted together with its CRLF line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5024-001")]
    [Requirement("RH5024-005")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceOnlyLineAboveClosingBraceWithCarriageReturnLineFeed()
    {
        const string testCode = "class C\n{\n    void M()\n    {\n        M();\n        \n    }\n}\n";
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
                                            int first = 0;
                                {|#0:
                                |}        }
                                        if (true)
                                        {
                                            int second = 0;
                                {|#1:
                                |}        }
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

        // Two adjacent if blocks, each with a blank line directly before its closing brace, so the second fix's
        // removal span sits close behind the first fix's own removal
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5024ClosingBraceMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5024MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}