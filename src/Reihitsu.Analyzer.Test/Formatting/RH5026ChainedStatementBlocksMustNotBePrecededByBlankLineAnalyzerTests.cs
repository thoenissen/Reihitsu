using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer"/> and <see cref="RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzerTests : BatchCodeFixTestsBase<RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer, RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineCodeFixProvider>
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
    [Requirement("RH5026-001")]
    [Requirement("RH5026-003")]
    [Requirement("RH5026-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                {|#0:
                                |}        else
                                        {
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
                                         }
                                         else
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5026MessageFormat));
    }

    /// <summary>
    /// Verifies that raw-string content does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawStringChainedBlocksDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    private const string Value = """
                                                                 
                                                                 else
                                                                 catch
                                                                 finally
                                                                 """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an identifier starting with a chained keyword does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIdentifierStartingWithChainedKeywordDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var elseBranch = 0;

                                        elseBranch = 1;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that chained keywords inside a multi-line comment do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineCommentChainedKeywordDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /*
                                    if (true)
                                    {
                                    }

                                    else
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
    /// Verifies that chained keywords inside disabled code do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDisabledCodeChainedKeywordDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                #if false
                                        if (true)
                                        {
                                        }

                                        else
                                        {
                                        }
                                #endif
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the blank line above else, else if, catch, and finally is reported once per keyword
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5026-001")]
    [Requirement("RH5026-003")]
    public async Task VerifyBlankLineAboveChainedKeywordIsReported()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool flag)
                                    {
                                        if (flag)
                                        {
                                        }
                                {|RH5026:
                                |}        else if (flag)
                                        {
                                        }
                                {|RH5026:
                                |}        else
                                        {
                                        }

                                        try
                                        {
                                        }
                                {|RH5026:
                                |}        catch
                                        {
                                        }
                                {|RH5026:
                                |}        finally
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a blank line is not reported above a keyword behind a closing brace, above an identifier that begins with
    /// a keyword, or above a keyword in a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5026-002")]
    public async Task VerifyKeywordThatDoesNotBeginItsLineIsNotConsidered()
    {
        const string testCode = """
                                class C
                                {
                                    void M(bool flag)
                                    {
                                        if (flag)
                                        {
                                            flag = false;

                                        } else
                                        {
                                        }

                                        var elseValue = 1;

                                        /*

                                        else */
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the fix deletes each reported blank line and nothing else
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5026-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesBlankLinesAboveChainedKeywords()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        try
                                        {
                                        }
                                {|RH5026:
                                |}        catch
                                        {
                                        }
                                {|RH5026:
                                |}        finally
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedCode = """
                                 class C
                                 {
                                     void M()
                                     {
                                         try
                                         {
                                         }
                                         catch
                                         {
                                         }
                                         finally
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies a whitespace-only line above a chained keyword is reported and deleted together with its CRLF line break
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5026-001")]
    [Requirement("RH5026-004")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceOnlyLineAboveChainedKeywordWithCarriageReturnLineFeed()
    {
        const string testCode = "class C\n{\n    void M(bool flag)\n    {\n        if (flag)\n        {\n        }\n        \n        else\n        {\n        }\n    }\n}\n";
        const string fixedCode = "class C\n{\n    void M(bool flag)\n    {\n        if (flag)\n        {\n        }\n        else\n        {\n        }\n    }\n}\n";

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
                                        }
                                {|#0:
                                |}        else
                                        {
                                        }
                                        try
                                        {
                                        }
                                {|#1:
                                |}        catch
                                        {
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
                                         }
                                         else
                                         {
                                         }
                                         try
                                         {
                                         }
                                         catch
                                         {
                                         }
                                     }
                                 }
                                 """;

        // Two adjacent chained blocks (an else clause and a catch clause), each preceded by a blank line, so the
        // second fix's removal span sits close behind the first fix's own removal
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5026MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}