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
public class RH5601UseTabsCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH5601UseTabsCorrectlyAnalyzer, RH5601UseTabsCorrectlyCodeFixProvider>
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
    [Requirement("RH5601-001")]
    [Requirement("RH5601-003")]
    [Requirement("RH5601-004")]
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
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
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
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
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
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
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
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
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
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
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
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
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
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
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
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
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
    [Requirement("RH5601-002")]
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
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyIssueIsDetectedAndFixedForTabInPragmaDirective()
    {
        const string testData = "internal class TestClass\r\n{\r\n#pragma{|#0:\t|}warning disable CS0168\r\n    void Method()\r\n    {\r\n    }\r\n#pragma warning restore CS0168\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n#pragma    warning disable CS0168\r\n    void Method()\r\n    {\r\n    }\r\n#pragma warning restore CS0168\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat));
    }

    /// <summary>
    /// Verifies that tabs between two tokens and in front of a trailing comment are reported and each replaced with
    /// four spaces, whatever column the tab starts in
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabsBetweenTokensAndBeforeTrailingCommentAreDetectedAndFixed()
    {
        const string testData = "internal class TestClass\n{\n    int{|#0:\t|}x;{|#1:\t|}// note\n}";
        const string fixedData = "internal class TestClass\n{\n    int    x;    // note\n}";

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a tab between the tokens of a <c>#define</c> directive is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabInDefineDirectiveIsDetectedAndFixed()
    {
        const string testData = "#define{|#0:\t|}FEATURE\ninternal class TestClass\n{\n}";
        const string fixedData = "#define    FEATURE\ninternal class TestClass\n{\n}";

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat));
    }

    /// <summary>
    /// Verifies that consecutive tabs between two tokens are reported one by one, each on its own character
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-003")]
    [Requirement("RH5601-004")]
    public async Task VerifyConsecutiveTabsAreReportedOneByOne()
    {
        const string testData = "internal class TestClass\n{\n    int{|#0:\t|}{|#1:\t|}x;\n}";
        const string fixedData = "internal class TestClass\n{\n    int        x;\n}";

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that tabs inside regular, verbatim, and interpolated string literals and inside a character literal
    /// are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNoDiagnosticsForTabInRegularVerbatimInterpolatedAndCharacterLiterals()
    {
        const string testData = "internal class TestClass\n{\n    string s = \"a\tb\";\n    char t = '\t';\n    string u = $\"{1}\t\";\n    string v = @\"a\tb\";\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies a tab in the text of each kind of literal and comment, the format clause of a hole included, is content
    /// and is not reported, while a tab in a hole's expression or alignment clause is code and is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyTabsInLiteralAndCommentTextAreNotReportedButTabsInHoleCodeAre()
    {
        const string source = """"
                              class C
                              {
                                  /// <summary>a→b</summary>
                                  void M()
                                  {
                                      var a = "a→b";
                                      var b = @"a→b";
                                      var c = "a→b"u8;
                                      var d = """a→b""";
                                      var e = '→';
                                      var f = $"a→{1}→b{2:→D3}";
                                      var g = $"{1{|#0:→|}+ 2}{1,{|#1:→|}5}";
                                      // a→b
                                      /* a→b */
                                  }

                                  /** a→b */
                                  int x;
                              }
                              """";

        var testData = source.Replace("→", "\t");

        await Verify(testData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat, 2));
    }

    /// <summary>
    /// Verifies a tab in the text of each kind of literal and comment is not reported while a tab in a hole's code is,
    /// when the document uses CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyTabsInLiteralAndCommentTextAreNotReportedButTabsInHoleCodeAreWithCarriageReturnLineFeed()
    {
        const string source = """"
                              class C
                              {
                                  /// <summary>a→b</summary>
                                  void M()
                                  {
                                      var a = "a→b";
                                      var b = @"a→b";
                                      var c = "a→b"u8;
                                      var d = """a→b""";
                                      var e = '→';
                                      var f = $"a→{1}→b{2:→D3}";
                                      var g = $"{1{|#0:→|}+ 2}{1,{|#1:→|}5}";
                                      // a→b
                                      /* a→b */
                                  }

                                  /** a→b */
                                  int x;
                              }
                              """";

        var testData = source.Replace("→", "\t");

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat, 2));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = "internal class TestClass\r\n{\r\n{|#0:\t|}{|#1:\t|}void Method()\r\n    {\r\n    }\r\n}";
        const string fixedCode = "internal class TestClass\r\n{\r\n        void Method()\r\n    {\r\n    }\r\n}";

        // The two tabs are consecutive, so their single-character replacement spans directly abut one another,
        // exercising the batch fixer's handling of adjacent same-line replacements
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH5601MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}