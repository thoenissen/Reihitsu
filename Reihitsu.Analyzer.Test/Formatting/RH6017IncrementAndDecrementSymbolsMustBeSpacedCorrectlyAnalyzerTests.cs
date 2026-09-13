using System.Threading.Tasks;

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
    /// Verifies that a comment before the increment is preserved by the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCommentBeforeIncrementIsPreserved()
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
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 0;
                                         value /* keep me */++;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6017MessageFormat));
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