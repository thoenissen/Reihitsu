using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer"/> and <see cref="RH5105OpeningParenthesisMustBeOnDeclarationLineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzerTests : AnalyzerTestsBase<RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer, RH5105OpeningParenthesisMustBeOnDeclarationLineCodeFixProvider>
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

    #endregion // Tests
}