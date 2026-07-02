using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6008ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer, RH6008ClosingSquareBracketsMustBeSpacedCorrectlyCodeFixProvider>
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
                                        int[] values = [0];
                                        _ = values[0];
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
                                        int[] values = [0];
                                        _ = values[0{|#0: |}];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] values = [0];
                                         _ = values[0];
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6008MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before the closing bracket is preserved by the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeClosingBracketIsPreserved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = [0];
                                        _ = values[0 /* keep me */{|#0: |}];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] values = [0];
                                         _ = values[0 /* keep me */];
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6008MessageFormat));
    }

    /// <summary>
    /// Verifies that multi-line collection expressions do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCollectionExpressionsDoNotProduceDiagnostics()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        List<int> values =
                                        [
                                            1,
                                            2,
                                            3,
                                        ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}