using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer"/> and <see cref="RH5109ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzerTests : AnalyzerTestsBase<RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer, RH5109ParametersMustBeOnSameLineOrSeparateLinesCodeFixProvider>
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
                                    void Method(
                                        int first,
                                        int second,
                                        int third)
                                    {
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
                                    void Method{|#0:(|}int first, int second,
                                                int third)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that the continuation lines are aligned under the relocated first parameter when the original first
    /// parameter started on a line below the opening parenthesis rather than next to it (issue #456)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLinesAlignUnderRelocatedFirstParameter()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}
                                        int first, int second,
                                        int third)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that a multi-line parameter list whose parameters all start on the same line is detected, because
    /// the formatter splits exactly that shape onto separate lines (issue #247)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineParameterListWithSharedStartLineIsDetected()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}int first, System.Func<int,
                                                                              int> second)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that a single parameter whose type spans multiple lines is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultiLineSingleParameterIsNotFlagged()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(System.Func<int,
                                                            int> only)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix formats only the rebuilt parameter list and leaves the surrounding member body
    /// untouched, so the fix diff does not inherit unrelated whole-member reformatting (issue #456)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixDoesNotReformatMemberBody()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0:(|}int first, int second,
                                                int third)
                                    {
                                System.Console.WriteLine();
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second,
                                                 int third)
                                     {
                                 System.Console.WriteLine();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId, AnalyzerResources.RH5109MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is not offered when the parameter list contains a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenParameterListContainsComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int a, int b, /* note */
                                                int c)
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5109ParametersMustBeOnSameLineOrSeparateLinesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single()
                                                               .ParameterList
                                                               .OpenParenToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}