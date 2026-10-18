using System;
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
public class RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer, RH6006OpeningParenthesisMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6006-001")]
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
    [Requirement("RH6006-001")]
    [Requirement("RH6006-002")]
    [Requirement("RH6006-003")]
    [Requirement("SHARED-WS-001")]
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

    /// <summary>
    /// Verifies that whitespace after an opening parenthesis is reported and deleted when the closing parenthesis or
    /// the end of the line follows it, and that a parenthesis directly followed by a line break is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6006-001")]
    [Requirement("RH6006-002")]
    [Requirement("RH6006-003")]
    public async Task VerifyWhitespaceBeforeClosingParenthesisAndAtTheEndOfTheLineIsDetectedAndFixed()
    {
        const string testData = "internal class TestClass\n"
                                + "{\n"
                                + "    void Method(int a)\n"
                                + "    {\n"
                                + "        Method({|#0:\t|}a);\n"
                                + "        Other({|#1: |});\n"
                                + "        Method({|#2:   |}\n"
                                + "            a);\n"
                                + "        Method(\n"
                                + "            a);\n"
                                + "    }\n"
                                + "\n"
                                + "    void Other()\n"
                                + "    {\n"
                                + "    }\n"
                                + "}";
        const string fixedData = "internal class TestClass\n"
                                 + "{\n"
                                 + "    void Method(int a)\n"
                                 + "    {\n"
                                 + "        Method(a);\n"
                                 + "        Other();\n"
                                 + "        Method(\n"
                                 + "            a);\n"
                                 + "        Method(\n"
                                 + "            a);\n"
                                 + "    }\n"
                                 + "\n"
                                 + "    void Other()\n"
                                 + "    {\n"
                                 + "    }\n"
                                 + "}";

        await Verify(testData, fixedData, Diagnostics(RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6006MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that a comment on a later line of the token gap withholds the fix while the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6006-001")]
    [Requirement("RH6006-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenACommentIsOnALaterLineOfTheGap()
    {
        const string testData = "internal class TestClass\n"
                                + "{\n"
                                + "    void Method(int a)\n"
                                + "    {\n"
                                + "        Method({|#0:  |}\n"
                                + "            // first argument\n"
                                + "            a);\n"
                                + "    }\n"
                                + "}";

        await Verify(testData, Diagnostics(RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6006MessageFormat));

        var spanStart = testData.IndexOf("{|#0:", StringComparison.Ordinal);
        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:  |}", "  "),
                                                   RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root => Location.Create(root.SyntaxTree, new TextSpan(spanStart, 2)));

        Assert.IsEmpty(actions, "The comment sits in the token gap of the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a directive in the token gap withholds the fix while the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6006-001")]
    [Requirement("RH6006-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenADirectiveIsInTheGap()
    {
        const string testData = "internal class TestClass\n"
                                + "{\n"
                                + "    void Method(int a)\n"
                                + "    {\n"
                                + "        Method({|#0:  |}\n"
                                + "#if FEATURE\n"
                                + "#endif\n"
                                + "            a);\n"
                                + "    }\n"
                                + "}";

        await Verify(testData, Diagnostics(RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6006MessageFormat));

        var spanStart = testData.IndexOf("{|#0:", StringComparison.Ordinal);
        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0:  |}", "  "),
                                                   RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root => Location.Create(root.SyntaxTree, new TextSpan(spanStart, 2)));

        Assert.IsEmpty(actions, "The directive sits in the token gap of the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a comment after the next token, outside the token gap, does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6006-003")]
    [Requirement("SHARED-WS-001")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsOfferedWhenACommentFollowsTheNextToken()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int a)
                                    {
                                        Method({|#0: |}a /* note */);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int a)
                                     {
                                         Method(a /* note */);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6006MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return ({|#0: |}a + ({|#1: |}b));
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int a, int b)
                                     {
                                         return (a + (b));
                                     }
                                 }
                                 """;

        // The outer and inner opening parentheses each carry their own trailing whitespace run a few characters
        // apart in the same expression; the fixes only remove their own run, so the batch fixer converges in
        // one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6006MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}