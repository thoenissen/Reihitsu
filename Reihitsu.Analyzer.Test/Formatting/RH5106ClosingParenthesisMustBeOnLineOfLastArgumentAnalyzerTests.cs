using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer"/> and <see cref="RH5106ClosingParenthesisMustBeOnLineOfLastArgumentCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzerTests : AnalyzerTestsBase<RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer, RH5106ClosingParenthesisMustBeOnLineOfLastArgumentCodeFixProvider>
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
    /// Verifies that the violation is still reported without offering a fix when the token gap contains a preprocessor directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWithoutCodeFixWhenDirectivesArePresent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int first
                                #if FEATURE
                                                , int second
                                #endif
                                    {|#0:)|}
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

        await Verify(testData, Diagnostics(RH5106ClosingParenthesisMustBeOnLineOfLastArgumentAnalyzer.DiagnosticId, AnalyzerResources.RH5106MessageFormat));

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

    #endregion // Tests
}