using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5601UseTabsCorrectlyAnalyzer"/> and <see cref="RH5601UseTabsCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5601UseTabsCorrectlyAnalyzerTests : AnalyzerTestsBase<RH5601UseTabsCorrectlyAnalyzer, RH5601UseTabsCorrectlyCodeFixProvider>
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
        const string testData = "internal class TestClass\r\n{\r\n{|#0:\t|}void Method()\r\n    {\r\n    }\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n    void Method()\r\n    {\r\n    }\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab inside a multi-line raw string literal is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTabInMultiLineRawStringLiteral()
    {
        const string testData = "internal class TestClass\r\n{\r\n    private const string Value = \"\"\"\r\n        col1\tcol2\r\n        \"\"\";\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a tab inside a single-line raw string literal is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTabInSingleLineRawStringLiteral()
    {
        const string testData = "internal class TestClass\r\n{\r\n    private const string Value = \"\"\"col1\tcol2\"\"\";\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a tab inside a UTF-8 string literal is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTabInUtf8StringLiteral()
    {
        const string testData = "internal class TestClass\r\n{\r\n    private static System.ReadOnlySpan<byte> Value => \"col1\tcol2\"u8;\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a tab inside a UTF-8 multi-line raw string literal is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTabInUtf8RawStringLiteral()
    {
        const string testData = "internal class TestClass\r\n{\r\n    private static System.ReadOnlySpan<byte> Value => \"\"\"\r\n        col1\tcol2\r\n        \"\"\"u8;\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix does not modify a tab inside a raw string literal
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotModifyRawStringContent()
    {
        const string testData = "internal class TestClass\r\n{\r\n    private const string Value = \"\"\"\r\n        col1\tcol2\r\n        \"\"\";\r\n}";

        var tabIndex = testData.IndexOf('\t');

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5601UseTabsCorrectlyAnalyzer.DiagnosticId,
                                                   root => Location.Create(root.SyntaxTree, new TextSpan(tabIndex, 1)));

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a tab inside a single-line comment is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTabInSingleLineComment()
    {
        const string testData = "internal class TestClass\r\n{\r\n    // col\tumn\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a tab inside a multi-line comment is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTabInMultiLineComment()
    {
        const string testData = "internal class TestClass\r\n{\r\n    void Method()\r\n    {\r\n        /* col\tumn */\r\n    }\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a tab inside a single-line documentation comment is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTabInSingleLineDocumentationComment()
    {
        const string testData = "internal class TestClass\r\n{\r\n    /// <summary>\r\n    /// col\tumn\r\n    /// </summary>\r\n    void Method()\r\n    {\r\n    }\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a tab inside a multi-line documentation comment is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTabInMultiLineDocumentationComment()
    {
        const string testData = "internal class TestClass\r\n{\r\n    /** col\tumn */\r\n    void Method()\r\n    {\r\n    }\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a tab inside preprocessor-disabled text is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTabInDisabledText()
    {
        const string testData = "internal class TestClass\r\n{\r\n#if false\r\n\tvoid Disabled()\r\n\t{\r\n\t}\r\n#endif\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix does not offer an action for a tab inside preprocessor-disabled text,
    /// since applying it would edit inactive code
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixDoesNotOfferActionForTabInDisabledText()
    {
        const string testData = "internal class TestClass\r\n{\r\n#if false\r\n\tvoid Disabled()\r\n\t{\r\n\t}\r\n#endif\r\n}";

        var tabIndex = testData.IndexOf('\t');

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5601UseTabsCorrectlyAnalyzer.DiagnosticId,
                                                   root => Location.Create(root.SyntaxTree, new TextSpan(tabIndex, 1)));

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a tab inside a non-region preprocessor directive's interior is still flagged and fixed,
    /// since that whitespace is ordinary formatting rather than comment or disabled-text content
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixedForTabInPragmaDirective()
    {
        const string testData = "internal class TestClass\r\n{\r\n#pragma{|#0:\t|}warning disable CS0168\r\n    void Method()\r\n    {\r\n    }\r\n#pragma warning restore CS0168\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n#pragma    warning disable CS0168\r\n    void Method()\r\n    {\r\n    }\r\n#pragma warning restore CS0168\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat));
    }

    #endregion // Tests
}