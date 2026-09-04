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
/// Test methods for <see cref="RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer"/> and <see cref="RH5106ClosingParenthesisMustBeOnLineOfLastArgumentCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzerTests : BatchCodeFixTestsBase<RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer, RH5106ClosingParenthesisMustBeOnLineOfLastArgumentCodeFixProvider>
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
                                    void Method(int first,
                                                int second
                                    {|#0:)|}
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int first,
                                                 int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId, AnalyzerResources.RH5106MessageFormat));
    }

    /// <summary>
    /// Verifies that methods are valid when the closing parenthesis is on the line of the last argument
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenClosingParenthesisIsOnLastArgumentLine()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first,
                                                int second)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
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
                                    TestClass(int first,
                                              int second
                                    {|#0:)|}
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     TestClass(int first,
                                               int second)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId, AnalyzerResources.RH5106MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported and no fix is offered when the token gap contains a preprocessor
    /// directive, because the formatter refuses to collapse the closing parenthesis across that directive (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticAndNoCodeFixWhenDirectivesArePresent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                #if FEATURE
                                                , int second
                                #endif
                                    )
                                    {
                                    }
                                }
                                """;
        const string codeFixData = """
                                   internal class TestClass
                                   {
                                       void Method(int first
                                   #if FEATURE
                                                   , int second
                                   #endif
                                       )
                                       {
                                       }
                                   }
                                   """;

        await Verify(testData);

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .First()
                                                               .ParameterList
                                                               .CloseParenToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported and no fix is offered when a comment sits in the gap before the
    /// closing parenthesis, because the formatter refuses to collapse across that comment (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticAndNoCodeFixWhenCommentIsInGap()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first,
                                                int second // note
                                    )
                                    {
                                    }
                                }
                                """;

        await Verify(testData);

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<MethodDeclarationSyntax>()
                                                               .First()
                                                               .ParameterList
                                                               .CloseParenToken
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that parameter-list closers are checked for the remaining direct parent shapes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRemainingDirectParameterListParentsAreDetected()
    {
        const string testData = """
                                using System;

                                internal class Primary(int value
                                {|#0:)|}
                                {
                                    ~Primary(
                                    {|#1:)|}
                                    {
                                    }

                                    void Method()
                                    {
                                        void Local(int item
                                        {|#7:)|}
                                        {
                                        }

                                        Func<int, int> parenthesized = (int item
                                        {|#2:)|} => item;
                                        Func<int, int> anonymous = delegate(int item
                                        {|#3:)|} { return item; };
                                    }

                                    public static Primary operator +(Primary left, Primary right
                                    {|#8:)|} => left;

                                    public static implicit operator int(Primary value
                                    {|#9:)|} => 0;
                                }

                                internal struct Value(int value
                                {|#4:)|}
                                {
                                }

                                internal interface Contract(int value
                                {|#5:)|}
                                {
                                }

                                extension(string value
                                {|#6:)|}
                                {
                                }

                                internal delegate void Handler(int value
                                {|#10:)|};

                                internal record Item(int value
                                {|#11:)|};

                                internal record struct Pair(int value
                                {|#12:)|};
                                """;

        await Verify(testData,
                     test => test.CompilerDiagnostics = CompilerDiagnostics.None,
                     Diagnostics(RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId, AnalyzerResources.RH5106MessageFormat, 13));
    }

    /// <summary>
    /// Verifies that malformed parameter lists do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMalformedSyntaxIsIgnored()
    {
        const string testData = """
                                internal class Example
                                {
                                    void Broken(
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a syntax error in an unrelated member suppresses the rule for the whole file, including a
    /// well-formed parameter list elsewhere in that file that would otherwise be reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUnrelatedSyntaxErrorSuppressesTheWholeFile()
    {
        const string testData = """
                                internal class Example
                                {
                                    private int _broken = ;

                                    private void Valid(int first,
                                                       int second
                                                       )
                                    {
                                    }
                                }
                                """;

        await Verify(testData, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the same parameter list is reported once the unrelated syntax error is removed, so the
    /// suppression above is caused by the error and not by the shape of the parameter list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySameParameterListIsReportedWithoutTheUnrelatedSyntaxError()
    {
        const string testData = """
                                internal class Example
                                {
                                    private void Valid(int first,
                                                       int second
                                                       {|#0:)|}
                                    {
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId, AnalyzerResources.RH5106MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void MethodA(int first,
                                                 int second
                                    {|#0:)|}
                                    {
                                    }

                                    void MethodB(int first,
                                                 int second
                                    {|#1:)|}
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void MethodA(int first,
                                                  int second)
                                     {
                                     }

                                     void MethodB(int first,
                                                  int second)
                                     {
                                     }
                                 }
                                 """;

        // Each fix only collapses the narrow whitespace gap between its own parameter list's last argument and its
        // own closing parenthesis, so the two gaps here, separated by a complete method body, can never share or
        // abut a text span; interference is structurally unreachable for this rule's gap-only fix, so this proves
        // both occurrences are corrected independently
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId, AnalyzerResources.RH5106MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}