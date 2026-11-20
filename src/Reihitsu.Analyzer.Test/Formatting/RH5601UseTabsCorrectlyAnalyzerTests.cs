using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
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

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
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

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
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

        await Verify(testData, Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
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

        await Verify(NormalizeToCarriageReturnLineFeed(testData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the fix replaces the tabs its rule reports in front of a line and in front of a trailing comment,
    /// and keeps a tab inside the comment's text
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyCodeFixKeepsTabInsideTheTextOfATrailingComment()
    {
        const string testData = "class C\n{\n{|RH5601:\t|}int x;{|RH5601:\t|}// a\tb\n}";
        const string fixedData = "class C\n{\n    int x;    // a\tb\n}";

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the fix keeps a tab inside the text of a trailing comment under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyCodeFixKeepsTabInsideTheTextOfATrailingCommentUnderCarriageReturnLineFeed()
    {
        const string testData = "class C\r\n{\r\n{|RH5601:\t|}int x;{|RH5601:\t|}// a\tb\r\n}";
        const string fixedData = "class C\r\n{\r\n    int x;    // a\tb\r\n}";

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the tabs indenting the content line and the closing line of an interpolated multi-line raw string are part of the literal and are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNoDiagnosticsForTabsInTheClosingLineOfAnInterpolatedRawString()
    {
        const string testData = """"
                                class C
                                {
                                    string s = $"""
                                →→a
                                →→""";
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that the tabs indenting the lines of an interpolated multi-line raw string are not reported when the document uses CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNoDiagnosticsForTabsInTheClosingLineOfAnInterpolatedRawStringWithCarriageReturnLineFeed()
    {
        const string testData = """"
                                class C
                                {
                                    string s = $"""
                                →→a
                                →→""";
                                }
                                """";

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that the tabs indenting the content line and the closing line of a plain multi-line raw string are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNoDiagnosticsForTabsInTheClosingLineOfARawString()
    {
        const string testData = """"
                                class C
                                {
                                    string w = """
                                →→a
                                →→""";
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that a tab after the opening quotes of an interpolated multi-line raw string is part of the literal and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNoDiagnosticsForTabAfterTheOpeningQuotesOfAnInterpolatedRawString()
    {
        const string testData = """"
                                class C
                                {
                                    string z = $"""→
                                        a
                                        """;
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that a tab after the opening quotes of a plain multi-line raw string is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNoDiagnosticsForTabAfterTheOpeningQuotesOfARawString()
    {
        const string testData = """"
                                class C
                                {
                                    string y = """→
                                        a
                                        """;
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that the tabs indenting a content line that starts with a hole, and the closing line after it, are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNoDiagnosticsForTabsInTheClosingLineAfterAHoleLine()
    {
        const string testData = """"
                                class C
                                {
                                    static int x = 1;

                                    string s = $"""
                                →→{x}
                                →→""";
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that the tabs indenting the lines of a double-dollar interpolated multi-line raw string are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNoDiagnosticsForTabsInTheClosingLineOfADoubleDollarRawString()
    {
        const string testData = """"
                                class C
                                {
                                    static int x = 1;

                                    string s = $$"""
                                →→{{x}}
                                →→""";
                                }
                                """";

        // The source holds no markup, because the markup parser reads "$$" as a position marker
        await Verify(ExpandWhitespaceMarkers(testData), static test => test.TestState.MarkupHandling = MarkupMode.None);
    }

    /// <summary>
    /// Verifies that indentation mixing a tab and spaces in the lines of an interpolated multi-line raw string is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyNoDiagnosticsForMixedIndentationOfAnInterpolatedRawString()
    {
        const string testData = """"
                                class C
                                {
                                    string s = $"""
                                →  a
                                →  """;
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that a tab in front of a line inside a hole of an interpolated multi-line raw string is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    public async Task VerifyTabInAHoleLineOfAnInterpolatedRawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string u = $"""
                                        {1
                                {|#0:→|}+ 2}
                                        """;
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string u = $"""
                                         {1
                                     + 2}
                                         """;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab behind the closing quotes of an interpolated multi-line raw string is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    public async Task VerifyTabBehindTheClosingQuotesOfAnInterpolatedRawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string s = $"""
                                        a
                                        """{|#0:→|};
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string s = $"""
                                         a
                                         """    ;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab behind a regular interpolated string is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    public async Task VerifyTabBehindAnInterpolatedStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string s = $"a"{|#0:→|};
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string s = $"a"    ;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab in front of the opening quotes of an interpolated multi-line raw string on a continuation line is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    public async Task VerifyTabInFrontOfTheOpeningQuotesOfAnInterpolatedRawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string s =
                                {|#0:→|}$"""
                                        a
                                        """;
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string s =
                                     $"""
                                         a
                                         """;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix of a reported tab next to an interpolated multi-line raw string leaves the tabs indenting the literal's lines untouched, so the result compiles
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-002")]
    [Requirement("SHARED-FIX-003")]
    public async Task VerifyCodeFixLeavesTheTabsOfAnInterpolatedRawStringUntouched()
    {
        const string testData = """"
                                class C
                                {
                                {|#0:→|}string s = $"""
                                →→a
                                →→""";
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string s = $"""
                                 →→a
                                 →→""";
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that every tab of the requirement's example is reported, the tabs in front of and behind literals,
    /// in front of a literal behind a <c>/** */</c> comment, and inside an interpolation hole included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyEveryTabOfTheRequirementExampleIsReported()
    {
        const string testData = """"
                                #define{|RH5601:→|}FEATURE
                                class C
                                {
                                {|RH5601:→|}int{|RH5601:→|}x;{|RH5601:→|}// note
                                    string u = $"""
                                        {1
                                {|RH5601:→|}+ 2}
                                        """;
                                #pragma{|RH5601:→|}warning disable CS0168
                                #pragma warning restore CS0168
                                    string a ={|RH5601:→|}"abc";{|RH5601:→|}// note
                                    string b =
                                {|RH5601:→|}"abc";
                                    string c = /** c */{|RH5601:→|}"abc";
                                    char d ={|RH5601:→|}'d';
                                    string e = $"{{|RH5601:→|}1{|RH5601:→|}}";
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that a tab that starts a line in front of a string literal is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabStartingALineInFrontOfAStringLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string b =
                                {|#0:→|}"abc";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string b =
                                     "abc";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab that starts a line in front of a string literal is reported and fixed under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabStartingALineInFrontOfAStringLiteralIsDetectedAndFixedWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    string b =
                                {|#0:→|}"abc";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string b =
                                     "abc";
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(fixedData)), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab that starts a line in front of the opening quotes of a multi-line raw string literal is
    /// code, and is reported and fixed without touching the literal's lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabStartingALineInFrontOfARawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string m =
                                {|#0:→|}"""
                                        a
                                        """;
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string m =
                                     """
                                         a
                                         """;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab between a <c>/** */</c> comment and a string literal is code, and is reported and fixed, with
    /// the documentation mode the verifier parses with by default
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindADelimitedDocumentationCommentInFrontOfAStringLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string c = /** c */{|#0:→|}"abc";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string c = /** c */    "abc";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab between a <c>/** */</c> comment and a string literal is reported and fixed when the document is
    /// parsed with <see cref="DocumentationMode.Parse"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindADelimitedDocumentationCommentInFrontOfAStringLiteralIsDetectedAndFixedInDocumentationModeParse()
    {
        const string testData = """
                                class C
                                {
                                    string c = /** c */{|#0:→|}"abc";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string c = /** c */    "abc";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData),
                     ExpandWhitespaceMarkers(fixedData),
                     static test => test.SolutionTransforms.Add(ApplyDocumentationModeParseToTestProject),
                     Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab between a <c>/** */</c> comment and a string literal is reported and fixed when the document is
    /// parsed with <see cref="DocumentationMode.None"/>, where the comment is an ordinary comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindADelimitedDocumentationCommentInFrontOfAStringLiteralIsDetectedAndFixedInDocumentationModeNone()
    {
        const string testData = """
                                class C
                                {
                                    string c = /** c */{|#0:→|}"abc";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string c = /** c */    "abc";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData),
                     ExpandWhitespaceMarkers(fixedData),
                     static test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject),
                     Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab behind a regular string literal, in front of an operator, is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindAStringLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string f = "a"{|#0:→|}+ "b";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string f = "a"    + "b";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab behind a regular string literal is reported and fixed under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindAStringLiteralIsDetectedAndFixedWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    string f = "a"{|#0:→|}+ "b";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string f = "a"    + "b";
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(fixedData)), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab between a string literal and the semicolon behind it is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBetweenAStringLiteralAndItsSemicolonIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string i = "a"{|#0:→|};
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string i = "a"    ;
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab behind a character literal is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindACharacterLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    int g = 'a'{|#0:→|}+ 1;
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int g = 'a'    + 1;
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab in front of a character literal is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabInFrontOfACharacterLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    char d ={|#0:→|}'d';
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     char d =    'd';
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab behind a verbatim string literal is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindAVerbatimStringLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string h = @"a"{|#0:→|}+ "b";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string h = @"a"    + "b";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab behind a UTF-8 string literal is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindAUtf8StringLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var j = "a"u8{|#0:→|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var j = "a"u8    ;
                                     }
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab behind a single-line raw string literal is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindASingleLineRawStringLiteralIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string k = """a"""{|#0:→|};
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string k = """a"""    ;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a tab behind the closing quotes of a multi-line raw string literal is code, and is reported and
    /// fixed without touching the literal's lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    public async Task VerifyTabBehindTheClosingQuotesOfARawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string l = """
                                        a
                                        """{|#0:→|};
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string l = """
                                         a
                                         """    ;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the tabs on both sides of a string literal inside an interpolation hole are code, and are reported
    /// and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-004")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyTabsAroundAStringLiteralInAHoleAreDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string n = $"{{|#0:→|}"x"{|#1:→|}}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string n = $"{    "x"    }";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a tab inside the text of a literal next to a reported tab stays content: the fix replaces only the
    /// tab behind the literal
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5601-001")]
    [Requirement("RH5601-002")]
    public async Task VerifyCodeFixReplacesTheTabBehindAStringLiteralAndKeepsTheTabInsideIt()
    {
        const string testData = """
                                class C
                                {
                                    string o = "a→b"{|#0:→|};
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string o = "a→b"    ;
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat));
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
                                  Diagnostics(RH5601UseTabsCorrectlyAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}