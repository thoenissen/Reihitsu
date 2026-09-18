using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer, RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyCodeFixProvider>
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
                                        int value = 0;
                                        value++;
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
                                        int value = 0;
                                        value{|#0: |}++;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 0;
                                         value++;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6017MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment before a same-line increment still reports the diagnostic but withholds the
    /// fix, because deleting the whitespace run would glue the comment to the operator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineIncrementWithPrecedingComment()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 0;
                                        value /* keep me */{|#0: |}++;
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6017MessageFormat));

        var actions = await GetCodeFixActionsAsync(testData.Replace("{|#0: |}", " "),
                                                   RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var operatorToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.PlusPlusToken));
                                                       var lastTrailingTrivia = operatorToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the operator.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedForSameLineIncrementWithPrecedingComment"/> with the source
    /// normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedForSameLineIncrementWithPrecedingCommentCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 0;
                                        value /* keep me */{|#0: |}++;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6017MessageFormat));

        var actions = await GetCodeFixActionsAsync(NormalizeToCarriageReturnLineFeed(testData.Replace("{|#0: |}", " ")),
                                                   RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var operatorToken = root.DescendantTokens().First(token => token.IsKind(SyntaxKind.PlusPlusToken));
                                                       var lastTrailingTrivia = operatorToken.GetPreviousToken().TrailingTrivia.Last();

                                                       Assert.IsTrue(lastTrailingTrivia.IsKind(SyntaxKind.WhitespaceTrivia),
                                                                     "The preceding token's trailing trivia must end with the whitespace run immediately before the operator.");

                                                       return Location.Create(root.SyntaxTree, lastTrailingTrivia.Span);
                                                   });

        Assert.IsEmpty(actions,
                       "The comment sits in the same token gap as the reported whitespace run; the fix must be withheld.");
    }

    /// <summary>
    /// Verifies that a continuation-line increment preceded by a block comment does not produce a diagnostic.
    /// RH6017's analyzer requires the immediately preceding token, not merely a preceding comment, to share
    /// the operator's line, so a comment on a continuation line never enters the reported span here, unlike
    /// the shared <c>SameLinePrecedingWhitespaceAnalysis</c> a sibling rule such as RH6003 uses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineIncrementWithPrecedingCommentDoesNotProduceDiagnostic()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 0;
                                        value
                                            /* keep me */++;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Repeats <see cref="VerifyContinuationLineIncrementWithPrecedingCommentDoesNotProduceDiagnostic"/> with
    /// the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyContinuationLineIncrementWithPrecedingCommentDoesNotProduceDiagnosticCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int value = 0;
                                        value
                                            /* keep me */++;
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData));
    }

    /// <summary>
    /// Verifies that indentation before a line-leading increment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLineLeadingIncrementDoesNotProduceDiagnostics()
    {
        const string testData = """
                                using System.Linq;

                                internal class TestClass
                                {
                                    bool Method(System.Collections.Generic.IEnumerable<int> values)
                                    {
                                        int endOfLineCount = 0;
                                        var leadingTrivia = values;

                                        return leadingTrivia.Any(value => value > 0
                                                                       && ++endOfLineCount >= 2);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Characterizes a malformed standalone prefix increment with a missing operand: Roslyn's error recovery
    /// still attaches the <c>++</c> token to a <c>PreIncrementExpression</c> node rather than leaving it an
    /// orphan token, so this rule's exclusion of prefix expressions still applies and no diagnostic is produced
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMalformedStandalonePrefixIncrementDoesNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        ++
                                    }
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that increment and decrement operator overload declarations do not produce a diagnostic. The
    /// pre-conversion tree walk excluded only tokens whose parent was a prefix unary expression, so it also
    /// inspected an operator declaration's <c>OperatorToken</c> — a real, non-missing token that is neither a
    /// prefix nor a postfix unary expression's operand — and incorrectly flagged the space RH6005 requires
    /// between the <c>operator</c> keyword and the symbol as a violation to remove. Node-kind dispatch on
    /// exactly <see cref="Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostIncrementExpression"/> and
    /// <see cref="Microsoft.CodeAnalysis.CSharp.SyntaxKind.PostDecrementExpression"/> never reaches an operator
    /// declaration at all, which corrects this rule's owner set rather than narrowing coverage of its own
    /// concern: an operator declaration was never a postfix or prefix increment/decrement expression
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOperatorOverloadDeclarationsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass operator ++(TestClass value) => value;

                                    public static TestClass operator --(TestClass value) => value;

                                    public static TestClass operator checked ++(TestClass value) => value;
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
                                        int second = 0;
                                        first{|#0: |}++;
                                        second{|#1: |}--;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int first = 0;
                                         int second = 0;
                                         first++;
                                         second--;
                                     }
                                 }
                                 """;

        // Two statements on adjacent lines each carry their own unwanted whitespace run before the increment or
        // decrement operator; the fixes only remove their own run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6017MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}