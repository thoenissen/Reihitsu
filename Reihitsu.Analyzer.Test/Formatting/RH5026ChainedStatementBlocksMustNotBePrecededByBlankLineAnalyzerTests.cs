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
public class RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzerTests : AnalyzerTestsBase<RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer, RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineCodeFixProvider>
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

    #endregion // Tests
}