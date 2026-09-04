using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer"/> and <see cref="RH5105OpeningParenthesisMustBeOnDeclarationLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzerTests : BatchCodeFixTestsBase<RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer, RH5105OpeningParenthesisMustBeOnDeclarationLineCodeFixProvider>
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
                                    void Method
                                    {|#0:(|}int value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed for constructors
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConstructorIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass
                                    {|#0:(|}int value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     TestClass(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when a comment sits in the gap before the parenthesis, because the
    /// formatter refuses to collapse the opening parenthesis across that comment (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenCommentIsInGap()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method
                                    // why
                                    (int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when a preprocessor directive sits in the gap before the parenthesis,
    /// because the formatter refuses to collapse the opening parenthesis across that directive (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenDirectiveIsInGap()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method
                                #if FEATURE
                                #endif
                                    (int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the fix is not offered when a comment sits in the gap before the parenthesis
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenCommentIsInGap()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method
                                    // why
                                    (int value)
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single()
                                                               .ParameterList
                                                               .OpenParenToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the fix is not offered when a preprocessor directive sits in the gap before the parenthesis
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenDirectiveIsInGap()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method
                                #if FEATURE
                                #endif
                                    (int value)
                                    {
                                    }
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .Single()
                                                               .ParameterList
                                                               .OpenParenToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed for local functions
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLocalFunctionIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Outer()
                                    {
                                        void Local
                                        {|#0:(|}int value)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Outer()
                                     {
                                         void Local(int value)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed for operators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOperatorIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass operator +
                                    {|#0:(|}TestClass left, TestClass right)
                                    {
                                        return left;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public static TestClass operator +(TestClass left, TestClass right)
                                     {
                                         return left;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed for conversion operators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConversionOperatorIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static implicit operator int
                                    {|#0:(|}TestClass value)
                                    {
                                        return 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public static implicit operator int(TestClass value)
                                     {
                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed for delegate declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDelegateIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal delegate void TestDelegate
                                {|#0:(|}int value);
                                """;
        const string fixedData = """
                                 internal delegate void TestDelegate(int value);
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed for record declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRecordIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal record TestRecord
                                {|#0:(|}int Value);
                                """;
        const string fixedData = """
                                 internal record TestRecord(int Value);
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed for record struct declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRecordStructIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal record struct TestRecord
                                {|#0:(|}int Value);
                                """;
        const string fixedData = """
                                 internal record struct TestRecord(int Value);
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    /// <summary>
    /// Verifies that every remaining syntax-valid direct parameter-list parent with an intrinsic opening anchor is covered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRemainingDirectParameterListParentsAreDetected()
    {
        const string testData = """
                                using System;

                                internal class Primary
                                {|#0:(|}int value)
                                {
                                    ~Primary
                                    {|#1:(|})
                                    {
                                    }

                                    void Method()
                                    {
                                        Func<int, int> parenthesized =
                                        (int item) => item;
                                        Func<int, int> anonymous = delegate
                                        {|#2:(|}int item) { return item; };
                                    }
                                }

                                internal struct Value
                                {|#3:(|}int value)
                                {
                                }

                                internal interface Contract
                                {|#4:(|}int value)
                                {
                                }

                                extension
                                {|#5:(|}string value)
                                {
                                }
                                """;

        await Verify(testData,
                     test => test.CompilerDiagnostics = CompilerDiagnostics.None,
                     Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat, 6));
    }

    /// <summary>
    /// Verifies that parenthesized lambdas are ignored because their opening parenthesis belongs to the expression and
    /// has no declaration-internal anchor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyParenthesizedLambdasAreIgnored()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    void Method()
                                    {
                                        Func<int, int> inline = (int item) => item;

                                        Accept("callback",
                                               (int item) => item);
                                    }

                                    void Accept(string name, Func<int, int> callback)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that malformed syntax is ignored and simple lambdas remain outside the rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMalformedSyntaxAndSimpleLambdaAreIgnored()
    {
        const string testData = """
                                using System;

                                internal class Example
                                {
                                    Func<int, int> simple = value => value;

                                    void Broken
                                    (
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a syntax error in an unrelated member suppresses the rule for the whole file, including a
    /// well-formed declaration elsewhere in that file that would otherwise be reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUnrelatedSyntaxErrorSuppressesTheWholeFile()
    {
        const string testData = """
                                internal class Example
                                {
                                    private int _broken = ;

                                    private void Valid
                                    (int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the same declaration is reported once the unrelated syntax error is removed, so the
    /// suppression above is caused by the error and not by the shape of the declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameDeclarationIsReportedWithoutTheUnrelatedSyntaxError()
    {
        const string testData = """
                                internal class Example
                                {
                                    private void Valid
                                    {|#0:(|}int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void MethodA
                                    {|#0:(|}int value)
                                    {
                                    }

                                    void MethodB
                                    {|#1:(|}int value)
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void MethodA(int value)
                                     {
                                     }

                                     void MethodB(int value)
                                     {
                                     }
                                 }
                                 """;

        // Each fix only collapses the narrow whitespace gap between its own declaration's name and its own opening
        // parenthesis, so the two gaps here, separated by a complete method body, can never share or abut a text
        // span; interference is structurally unreachable for this rule's gap-only fix, so this proves both
        // occurrences are corrected independently
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}