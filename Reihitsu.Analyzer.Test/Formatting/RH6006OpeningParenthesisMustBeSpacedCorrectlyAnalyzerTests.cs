using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer, RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider>
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
                                    int Method()
                                    {
                                        return ({|#0: |}0);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method()
                                     {
                                         return (0);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6006MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is not offered when deleting the space would glue the parenthesis to a comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenSpaceSeparatesAComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method()
                                    {
                                        return ( /* keep */0);
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var openParen = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.OpenParenToken) && token.GetNextToken().IsKind(SyntaxKind.NumericLiteralToken));

                                                       return Location.Create(root.SyntaxTree, new TextSpan(openParen.Span.End, 1));
                                                   });

        Assert.IsEmpty(actions);
    }

    #endregion // Tests
}