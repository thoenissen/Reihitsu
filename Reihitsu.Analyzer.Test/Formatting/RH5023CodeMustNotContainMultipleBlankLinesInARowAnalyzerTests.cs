using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

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