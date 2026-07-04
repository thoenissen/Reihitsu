using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6023CodeMustNotContainAlignmentPaddingAnalyzer"/> and <see cref="RH6023CodeMustNotContainAlignmentPaddingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6023CodeMustNotContainAlignmentPaddingAnalyzerTests : AnalyzerTestsBase<RH6023CodeMustNotContainAlignmentPaddingAnalyzer, RH6023CodeMustNotContainAlignmentPaddingCodeFixProvider>
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
                                        var a = 2;
                                        var abc = 3;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that padding before an assignment operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPaddingBeforeAssignmentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var a{|#0:   |}= 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var a = 2;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023CodeMustNotContainAlignmentPaddingAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that padding between a type and an identifier is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPaddingBetweenTypeAndIdentifierIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int{|#0:  |}value = 2;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int value = 2;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023CodeMustNotContainAlignmentPaddingAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that padding around a binary operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPaddingAroundBinaryOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return a{|#0:  |}+{|#1:  |}b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int a, int b)
                                     {
                                         return a + b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023CodeMustNotContainAlignmentPaddingAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that padding after an opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPaddingAfterOpeningParenthesisIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        Method({|#0:  |}x);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x)
                                     {
                                         Method(x);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023CodeMustNotContainAlignmentPaddingAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that padding after a comma is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPaddingAfterCommaIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x, int y)
                                    {
                                        Method(x,{|#0:  |}y);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                         Method(x, y);
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6023CodeMustNotContainAlignmentPaddingAnalyzer.DiagnosticId, AnalyzerResources.RH6023MessageFormat));
    }

    /// <summary>
    /// Verifies that multiple spaces inside a string literal do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleSpacesInStringLiteralAreIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var text = "a    b";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiple spaces inside a comment do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleSpacesInCommentAreIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        // a    b
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiple spaces inside interpolated string text do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleSpacesInInterpolatedStringTextAreIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x)
                                    {
                                        var text = $"a    {x}";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}