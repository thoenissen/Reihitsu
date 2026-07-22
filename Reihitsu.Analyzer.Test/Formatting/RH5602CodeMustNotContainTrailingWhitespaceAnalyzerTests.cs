using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5602CodeMustNotContainTrailingWhitespaceAnalyzer"/> and <see cref="RH5602CodeMustNotContainTrailingWhitespaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5602CodeMustNotContainTrailingWhitespaceAnalyzerTests : AnalyzerTestsBase<RH5602CodeMustNotContainTrailingWhitespaceAnalyzer, RH5602CodeMustNotContainTrailingWhitespaceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that lines without trailing whitespace do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenTrailingWhitespaceIsAbsent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingWhitespaceIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var value = 0;{|#0:    |}
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var value = 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, AnalyzerResources.RH5602MessageFormat));
    }

    /// <summary>
    /// Verifies that trailing whitespace inside raw strings does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawStringsDoNotProduceDiagnostics()
    {
        const string testData = """"
                                internal class TestClass
                                {
                                    string Property => """
                                                       value    
                                                       """;
                                }
                                """";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace inside verbatim strings does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyVerbatimStringsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Property => @"Value    
                                Another";
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace inside a single-line comment does not produce diagnostics, since the
    /// whitespace is part of the comment trivia's own text and no formatter phase can remove it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTrailingWhitespaceInSingleLineComment()
    {
        const string testData = "internal class TestClass\r\n{\r\n    // note \r\n    void Method()\r\n    {\r\n    }\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace on an interior line of a multi-line comment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTrailingWhitespaceInMultiLineComment()
    {
        const string testData = "internal class TestClass\r\n{\r\n    void Method()\r\n    {\r\n        /* first line   \r\n           second line */\r\n    }\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace inside a single-line documentation comment does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTrailingWhitespaceInSingleLineDocumentationComment()
    {
        const string testData = "internal class TestClass\r\n{\r\n    /// <summary>\r\n    /// note \r\n    /// </summary>\r\n    void Method()\r\n    {\r\n    }\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace on an interior line of a multi-line documentation comment does not
    /// produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTrailingWhitespaceInMultiLineDocumentationComment()
    {
        const string testData = "internal class TestClass\r\n{\r\n    /**\r\n     * note   \r\n     */\r\n    void Method()\r\n    {\r\n    }\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace inside preprocessor-disabled text does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForTrailingWhitespaceInDisabledText()
    {
        const string testData = "internal class TestClass\r\n{\r\n#if false\r\n    void Disabled()   \r\n    {\r\n    }\r\n#endif\r\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace immediately after a multi-line comment's closing delimiter, on the same
    /// line, is still detected and fixed, since that whitespace sits outside the comment's own trivia and the
    /// formatter can remove it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingWhitespaceAfterMultiLineCommentIsStillFlagged()
    {
        const string testData = "internal class TestClass\r\n{\r\n    void Method()\r\n    {\r\n        var value = 0; /* note */{|#0:   |}\r\n    }\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n    void Method()\r\n    {\r\n        var value = 0; /* note */\r\n    }\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, AnalyzerResources.RH5602MessageFormat));
    }

    /// <summary>
    /// Verifies that trailing whitespace on a preprocessor directive line itself is still detected and fixed,
    /// since that whitespace is ordinary formatting rather than comment or disabled-text content
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixedForTrailingWhitespaceInPragmaDirective()
    {
        const string testData = "internal class TestClass\r\n{\r\n#pragma warning disable CS0168{|#0:   |}\r\n    void Method()\r\n    {\r\n    }\r\n#pragma warning restore CS0168\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n#pragma warning disable CS0168\r\n    void Method()\r\n    {\r\n    }\r\n#pragma warning restore CS0168\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, AnalyzerResources.RH5602MessageFormat));
    }

    #endregion // Tests
}