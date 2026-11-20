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
public class RH5602CodeMustNotContainTrailingWhitespaceAnalyzerTests : BatchCodeFixTestsBase<RH5602CodeMustNotContainTrailingWhitespaceAnalyzer, RH5602CodeMustNotContainTrailingWhitespaceCodeFixProvider>
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
    [Requirement("RH5602-001")]
    [Requirement("RH5602-003")]
    [Requirement("RH5602-004")]
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

        await Verify(testData, fixedData, Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that trailing whitespace inside raw strings does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-002")]
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
    [Requirement("RH5602-002")]
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
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    public async Task VerifyTrailingWhitespaceAfterMultiLineCommentIsStillFlagged()
    {
        const string testData = "internal class TestClass\r\n{\r\n    void Method()\r\n    {\r\n        var value = 0; /* note */{|#0:   |}\r\n    }\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n    void Method()\r\n    {\r\n        var value = 0; /* note */\r\n    }\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that trailing whitespace after an opening brace, after a statement, and after a character literal
    /// is reported once per line and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-003")]
    [Requirement("RH5602-004")]
    public async Task VerifyTrailingWhitespaceAfterBraceStatementAndCharacterLiteralIsDetectedAndFixed()
    {
        const string testData = "internal class TestClass\n{{|#0:   |}\n    int x;{|#1:\t|}\n    char c = 'a';{|#2:  |}\n}";
        const string fixedData = "internal class TestClass\n{\n    int x;\n    char c = 'a';\n}";

        await Verify(testData, fixedData, Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that a line of whitespace only is reported as a whole and becomes an empty line that stays in place
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-003")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceOnlyLineIsReportedAsAWholeAndBecomesEmpty()
    {
        const string testData = "internal class TestClass\n{\n    int x;\n{|#0:        |}\n    int y;\n}";
        const string fixedData = "internal class TestClass\n{\n    int x;\n\n    int y;\n}";

        await Verify(testData, fixedData, Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace inside a line, followed by more code on the same line, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    public async Task VerifyWhitespaceInsideALineIsNotReported()
    {
        const string testData = "internal class TestClass\n{\n    int z;  int w;\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that whitespace at the end of a line inside an interpolated verbatim string literal is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-002")]
    public async Task VerifyInterpolatedVerbatimStringsDoNotProduceDiagnostics()
    {
        const string testData = "internal class TestClass\n{\n    string Property => $@\"{1}   \nAnother\";\n}";

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that trailing whitespace in the text behind a hole of an interpolated multi-line raw string is part of the literal and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-002")]
    public async Task VerifyNoDiagnosticsForTrailingWhitespaceBehindAHoleInAnInterpolatedRawString()
    {
        const string testData = """"
                                class C
                                {
                                    string w = $"""
                                        {1}··
                                        """;
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that every run of the requirement's example is reported, the whitespace behind the end of each kind of
    /// literal and inside an interpolation hole included, while the whitespace inside a verbatim literal's text is not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-002")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyEveryRunOfTheRequirementExampleIsReported()
    {
        const string testData = """"
                                class C
                                {{|RH5602:···|}
                                    int x;{|RH5602:→|}
                                {|RH5602:········|}
                                    char c = 'a';{|RH5602:··|}
                                    int y; /* note */{|RH5602:···|}
                                    int z;  int w;
                                    string s = "a";{|RH5602:···|}
                                    string t = $"a{1}b";{|RH5602:··|}
                                    string u = @"a";{|RH5602:·|}
                                    string v = $@"{1 +{|RH5602:··|}
                                        2}";
                                    string p = @"a··
                                b";{|RH5602:··|}
                                    string q = """
                                        a
                                        """;{|RH5602:·|}
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that whitespace behind a regular string literal at the end of its line is code, and is reported and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceBehindAStringLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string s = "a";{|#0:··|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = "a";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace behind a regular string literal at the end of its line is reported and deleted under
    /// CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceBehindAStringLiteralIsDetectedAndFixedWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    string s = "a";{|#0:··|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = "a";
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(fixedData)), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace behind an interpolated string at the end of its line is code, and is reported and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceBehindAnInterpolatedStringIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string t = $"a{1}b";{|#0:··|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string t = $"a{1}b";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace behind a verbatim string literal at the end of its line is code, and is reported and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceBehindAVerbatimStringLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string u = @"a";{|#0:·|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string u = @"a";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace behind the <c>u8</c> suffix of a UTF-8 string literal is code, and is reported and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceBehindAUtf8StringLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var v = "a"u8;{|#0:··|}
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var v = "a"u8;
                                     }
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace behind the closing quotes of a multi-line raw string literal is code, and is reported and
    /// deleted, while its content lines stay as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceBehindTheClosingQuotesOfARawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string x = """
                                        a
                                        """;{|#0:·|}
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string x = """
                                         a
                                         """;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that on the last line of a multi-line verbatim string literal the whitespace behind its end is reported and
    /// deleted, while the whitespace at the end of its first line is part of its text and stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-002")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceBehindAMultiLineVerbatimStringIsDetectedAndFixedAndItsTextIsKept()
    {
        const string testData = """
                                class C
                                {
                                    string w = @"a··
                                b";{|#0:··|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string w = @"a··
                                 b";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace at the end of a line inside an interpolation hole of an interpolated verbatim string is
    /// code, and is reported and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyWhitespaceAtTheEndOfAHoleLineOfAnInterpolatedVerbatimStringIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string v = $@"{1 +{|#0:··|}
                                        2}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string v = $@"{1 +
                                         2}";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace at the end of a line inside an interpolation hole of a regular interpolated string that
    /// spans lines is code, and is reported and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyWhitespaceAtTheEndOfAHoleLineOfARegularInterpolatedStringIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string v = $"{1 +{|#0:··|}
                                        2}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string v = $"{1 +
                                         2}";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace at the end of a line inside an interpolation hole of an interpolated raw string is code,
    /// and is reported and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyWhitespaceAtTheEndOfAHoleLineOfAnInterpolatedRawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string r = $"""
                                        {1 +{|#0:··|}
                                        2}
                                        """;
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string r = $"""
                                         {1 +
                                         2}
                                         """;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that a line of whitespace only inside an interpolation hole of an interpolated raw string is code, and is
    /// reported as a whole and becomes an empty line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyWhitespaceOnlyLineInAHoleOfAnInterpolatedRawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string r = $"""
                                        {1 +
                                {|#0:············|}
                                        2}
                                        """;
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string r = $"""
                                         {1 +

                                         2}
                                         """;
                                 }
                                 """";

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that whitespace behind the opening quotes of a multi-line raw string literal, interpolated or not, is
    /// part of the literal and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-002")]
    public async Task VerifyNoDiagnosticsForWhitespaceBehindTheOpeningQuotesOfARawString()
    {
        const string testData = """"
                                class C
                                {
                                    string x = """··
                                        a
                                        """;
                                    string y = $"""··
                                        {1}
                                        """;
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that a content line of whitespace only in a multi-line raw string literal, interpolated or not, and in a
    /// verbatim string literal is part of the literal and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-002")]
    public async Task VerifyNoDiagnosticsForWhitespaceOnlyContentLinesOfMultiLineLiterals()
    {
        const string testData = """"
                                class C
                                {
                                    string x = """
                                        a
                                ··········
                                        b
                                        """;
                                    string y = $"""
                                        {1}
                                ··········
                                        b
                                        """;
                                    string z = @"a
                                ····
                                b";
                                }
                                """";

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Verifies that whitespace at the end of a line that a string literal starts on the next line is code, and is
    /// reported and deleted
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceAtTheEndOfTheLineInFrontOfAStringLiteralIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string s ={|#0:··|}
                                        "a";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s =
                                         "a";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the whitespace behind several literals and in a hole of one document is reported and deleted
    /// together, as Fix All does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5602-001")]
    [Requirement("RH5602-004")]
    public async Task VerifyWhitespaceBehindSeveralLiteralsIsDetectedAndFixedTogether()
    {
        const string testData = """
                                class C
                                {
                                    string s = "a";{|#0:··|}
                                    string t = $"a{1}b";{|#1:··|}
                                    string v = $@"{1 +{|#2:··|}
                                        2}";{|#3:·|}
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = "a";
                                     string t = $"a{1}b";
                                     string v = $@"{1 +
                                         2}";
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), ExpandWhitespaceMarkers(fixedData), Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat, 4));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var first = 0;{|#0:    |}
                                        var second = 0;{|#1:    |}
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var first = 0;
                                         var second = 0;
                                     }
                                 }
                                 """;

        // The rule reports one diagnostic per trailing-whitespace run, and each statement's run is on its own
        // line; the two adjacent statements exercise the batch fixer applying two genuinely separate, surgical
        // removals
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5602CodeMustNotContainTrailingWhitespaceAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}