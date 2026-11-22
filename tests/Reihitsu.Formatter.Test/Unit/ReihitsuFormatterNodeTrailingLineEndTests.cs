using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests that <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/> and
/// <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> end the target's last line as document formatting does:
/// the line break that ends the target's trailing trivia in the detected line ending, without whitespace in front of it
/// </summary>
[TestClass]
public class ReihitsuFormatterNodeTrailingLineEndTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: in a <c>\r\n</c> document, the <c>\n</c> behind the property <c>P</c> is written back as
    /// <c>\r\n</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task LineFeedBehindTheTargetInACarriageReturnLineFeedDocumentIsWrittenAsCarriageReturnLineFeed()
    {
        const string input = "class C\r\n{\r\n    int P { get { return 1; } }\n    int Q { get { return 2; } }\r\n}\r\n";
        const string expected = "class C\r\n{\r\n    int P\r\n    {\r\n        get\r\n        {\r\n            return 1;\r\n        }\r\n    }\r\n    int Q { get { return 2; } }\r\n}\r\n";

        await AssertFormatsExactly(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies the spec example's prose: in a document whose line breaks are all a lone <c>\r</c>, every line break of the
    /// formatted target, the one behind its closing brace included, is <c>\n</c>, and the lines outside it keep their <c>\r</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task LoneCarriageReturnBehindTheTargetIsWrittenAsLineFeed()
    {
        const string input = "class C\r{\r    int P { get { return 1; } }\r    int Q { get { return 2; } }\r}\r";
        const string expected = "class C\r{\r    int P\n    {\n        get\n        {\n            return 1;\n        }\n    }\n    int Q { get { return 2; } }\r}\r";

        await AssertFormatsExactly(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies that the line break behind a comment in the target's trailing trivia is written in the detected line ending,
    /// and the comment stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task LineFeedBehindACommentBehindTheTargetIsWrittenAsCarriageReturnLineFeed()
    {
        const string input = "class C\r\n{\r\n    int P { get { return 1; } } // c\n    int Q { get { return 2; } }\r\n}\r\n";
        const string expected = "class C\r\n{\r\n    int P\r\n    {\r\n        get\r\n        {\r\n            return 1;\r\n        }\r\n    } // c\r\n    int Q { get { return 2; } }\r\n}\r\n";

        await AssertFormatsExactly(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies that in a <c>\n</c> document, the <c>\r\n</c> behind the target is written back as <c>\n</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task CarriageReturnLineFeedBehindTheTargetInALineFeedDocumentIsWrittenAsLineFeed()
    {
        const string input = "class C\n{\n    int P { get { return 1; } }\r\n    int Q { get { return 2; } }\n}\n";
        const string expected = "class C\n{\n    int P\n    {\n        get\n        {\n            return 1;\n        }\n    }\n    int Q { get { return 2; } }\n}\n";

        await AssertFormatsExactly(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies that a line break inside the text of a delimited comment behind the target keeps its characters, while the line
    /// break that ends the trailing trivia is written in the detected line ending
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task LineBreakInsideACommentBehindTheTargetKeepsItsCharacters()
    {
        const string input = "class C\r\n{\r\n    int P { get { return 1; } } /* a\n       b */\n    int Q { get { return 2; } }\r\n}\r\n";
        const string expected = "class C\r\n{\r\n    int P\r\n    {\r\n        get\r\n        {\r\n            return 1;\r\n        }\r\n    } /* a\n       b */\r\n    int Q { get { return 2; } }\r\n}\r\n";

        await AssertFormatsExactly(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies that a trailing trivia that does not end in a line break, because code follows the target on its line, is kept as
    /// it stands, its whitespace included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task TrailingTriviaWithoutALineBreakIsKeptAsItStands()
    {
        const string input = "class C\r\n{\r\n    int P { get { return 1; } }   int Q { get { return 2; } }\r\n}\r\n";
        const string expected = "class C\r\n{\r\n    int P\r\n    {\r\n        get\r\n        {\r\n            return 1;\r\n        }\r\n    }   int Q { get { return 2; } }\r\n}\r\n";

        await AssertFormatsExactly(input, "P", null, expected, assertsSecondPass: false);
    }

    /// <summary>
    /// Verifies the spec example: the whitespace behind the comment behind a method whose expression body is converted to a
    /// block is removed, and the space in front of the comment stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WhitespaceBehindADelimitedCommentBehindAConvertedMethodIsRemoved()
    {
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int GetValue() => _value; /* c */···
                             }

                             """;

        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int GetValue()
                                    {
                                        return _value;
                                    } /* c */
                                }

                                """;

        await AssertFormatsUnderEveryLineEnding(input, "GetValue", null, expected);
    }

    /// <summary>
    /// Verifies the spec example's prose: without the comment, the whitespace that stood behind <c>;</c> is not written behind
    /// the closing brace of the converted method
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WhitespaceBehindAConvertedMethodIsRemoved()
    {
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int GetValue() => _value;···
                             }

                             """;

        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int GetValue()
                                    {
                                        return _value;
                                    }
                                }

                                """;

        await AssertFormatsUnderEveryLineEnding(input, "GetValue", null, expected);
    }

    /// <summary>
    /// Verifies the spec example's prose: the whitespace behind a target whose last token formatting keeps as the result's last
    /// token is removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WhitespaceBehindAKeptLastTokenIsRemoved()
    {
        const string input = """
                             class C
                             {
                                 int P { get { return 1; } }···
                                 int Q { get { return 2; } }
                             }

                             """;

        const string expected = """
                                class C
                                {
                                    int P
                                    {
                                        get
                                        {
                                            return 1;
                                        }
                                    }
                                    int Q { get { return 2; } }
                                }

                                """;

        await AssertFormatsUnderEveryLineEnding(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies that the whitespace at the end of the target's last line is removed also when formatting leaves that line as it
    /// was written, whatever whitespace characters it holds
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WhitespaceBehindALastLineFormattingLeavesUnchangedIsRemoved()
    {
        const string input = "class C\n{\n    int P\n    {\n        get\n        {\n            return 1;\n        }\n    }\t··\n    int Q { get { return 2; } }\n}\n";
        const string expected = "class C\n{\n    int P\n    {\n        get\n        {\n            return 1;\n        }\n    }\n    int Q { get { return 2; } }\n}\n";

        await AssertFormatsUnderEveryLineEnding(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies that the whitespace in front of a single-line comment behind the target stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WhitespaceInFrontOfACommentBehindTheTargetStays()
    {
        const string input = """
                             class C
                             {
                                 int P { get { return 1; } }···// c
                                 int Q { get { return 2; } }
                             }

                             """;

        const string expected = """
                                class C
                                {
                                    int P
                                    {
                                        get
                                        {
                                            return 1;
                                        }
                                    }···// c
                                    int Q { get { return 2; } }
                                }

                                """;

        await AssertFormatsUnderEveryLineEnding(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies the spec example's prose: whitespace behind a single-line comment behind the target is the comment's text and
    /// stays
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WhitespaceInsideASingleLineCommentBehindTheTargetStays()
    {
        const string input = """
                             class C
                             {
                                 int P { get { return 1; } } // c···
                                 int Q { get { return 2; } }
                             }

                             """;

        const string expected = """
                                class C
                                {
                                    int P
                                    {
                                        get
                                        {
                                            return 1;
                                        }
                                    } // c···
                                    int Q { get { return 2; } }
                                }

                                """;

        await AssertFormatsUnderEveryLineEnding(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies that whitespace and a foreign line break behind the target are both brought to what document formatting writes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WhitespaceInFrontOfAForeignLineBreakIsRemovedAndTheLineBreakIsWrittenAsDetected()
    {
        const string input = "class C\r\n{\r\n    int P { get { return 1; } }   \n    int Q { get { return 2; } }\r\n}\r\n";
        const string expected = "class C\r\n{\r\n    int P\r\n    {\r\n        get\r\n        {\r\n            return 1;\r\n        }\r\n    }\r\n    int Q { get { return 2; } }\r\n}\r\n";

        await AssertFormatsExactly(input, "P", null, expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> writes the <c>\n</c> behind its target
    /// in a <c>\r\n</c> document as <c>\r\n</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WithContextLineFeedBehindTheTargetInACarriageReturnLineFeedDocumentIsWrittenAsCarriageReturnLineFeed()
    {
        const string input = "class C\r\n{\r\n    void M(){int x=1;}\n    int y;\r\n}\r\n";
        const string expected = "class C\r\n{\r\n    void M()\r\n    {\r\n        int x = 1;\r\n    }\r\n    int y;\r\n}\r\n";

        await AssertFormatsExactly(input, "M", "C", expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> writes the lone <c>\r</c> behind its
    /// target in a document whose line breaks are all a lone <c>\r</c> as <c>\n</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WithContextLoneCarriageReturnBehindTheTargetIsWrittenAsLineFeed()
    {
        const string input = "class C\r{\r    void M(){int x=1;}\r    int y;\r}\r";
        const string expected = "class C\r{\r    void M()\n    {\n        int x = 1;\n    }\n    int y;\r}\r";

        await AssertFormatsExactly(input, "M", "C", expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> removes the whitespace in front of the
    /// line break behind its target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task WithContextWhitespaceBehindTheTargetIsRemoved()
    {
        const string input = """
                             class C
                             {
                                 void M(){int x=1;}···
                                 int y;
                             }

                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        int x = 1;
                                    }
                                    int y;
                                }

                                """;

        await AssertFormatsUnderEveryLineEnding(input, "M", "C", expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> writes the <c>\n</c> behind a combined
    /// field declaration that the split makes several fields of, in a <c>\r\n</c> document, as <c>\r\n</c> behind the last field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task SplitFieldLineFeedBehindTheTargetInACarriageReturnLineFeedDocumentIsWrittenAsCarriageReturnLineFeed()
    {
        const string input = "class C\r\n{\r\n    private int _x, _y;\n    private int _z;\r\n}\r\n";
        const string expected = "class C\r\n{\r\n    private int _x;\r\n    private int _y;\r\n    private int _z;\r\n}\r\n";

        await AssertFormatsExactly(input, "_x", "C", expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> writes the lone <c>\r</c> behind a
    /// combined field declaration that the split makes several fields of as <c>\n</c>, in a document whose line breaks are all a
    /// lone <c>\r</c>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task SplitFieldLoneCarriageReturnBehindTheTargetIsWrittenAsLineFeed()
    {
        const string input = "class C\r{\r    private int _x, _y;\r    private int _z;\r}\r";
        const string expected = "class C\r{\r    private int _x;\n    private int _y;\n    private int _z;\r}\r";

        await AssertFormatsExactly(input, "_x", "C", expected);
    }

    /// <summary>
    /// Verifies that <see cref="ReihitsuFormatter.FormatNodeInDocumentWithContextAsync"/> removes the whitespace in front of the
    /// line break behind a combined field declaration that the split makes several fields of
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    public async Task SplitFieldWhitespaceBehindTheTargetIsRemoved()
    {
        const string input = """
                             class C
                             {
                                 private int _x, _y;···
                                 private int _z;
                             }

                             """;

        const string expected = """
                                class C
                                {
                                    private int _x;
                                    private int _y;
                                    private int _z;
                                }

                                """;

        await AssertFormatsUnderEveryLineEnding(input, "_x", "C", expected);
    }

    /// <summary>
    /// Finds the member declaration that declares the given name: a method, property, type, enum member, or a field that
    /// declares a variable of that name
    /// </summary>
    /// <param name="root">The root to search</param>
    /// <param name="name">The declared name</param>
    /// <returns>The declaration</returns>
    private static SyntaxNode FindDeclaration(SyntaxNode root, string name)
    {
        return root.DescendantNodes()
                   .OfType<MemberDeclarationSyntax>()
                   .First(node => node switch
                                  {
                                      MethodDeclarationSyntax method => method.Identifier.ValueText == name,
                                      PropertyDeclarationSyntax property => property.Identifier.ValueText == name,
                                      BaseTypeDeclarationSyntax type => type.Identifier.ValueText == name,
                                      EnumMemberDeclarationSyntax enumMember => enumMember.Identifier.ValueText == name,
                                      FieldDeclarationSyntax field => field.Declaration.Variables.Any(variable => variable.Identifier.ValueText == name),
                                      _ => false
                                  });
    }

    /// <summary>
    /// Writes every <c>·</c> of the text as a space, so trailing whitespace is visible in the test source
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The text with spaces</returns>
    private static string ShowSpaces(string text)
    {
        return text.Replace('·', ' ');
    }

    /// <summary>
    /// Makes line breaks and trailing whitespace visible in an assertion message
    /// </summary>
    /// <param name="text">The text</param>
    /// <returns>The text with <c>\r</c> and <c>\n</c> written as escapes</returns>
    private static string Escape(string text)
    {
        return text.Replace("\r", "\\r").Replace("\n", "\\n");
    }

    /// <summary>
    /// Formats the named target, with the named context when one is given, in the document under LF and CRLF, and asserts the
    /// document's exact text
    /// </summary>
    /// <param name="inputWithLf">The document text with LF line endings, <c>·</c> marking a space</param>
    /// <param name="targetName">The name the target declares</param>
    /// <param name="contextName">The name of the context type, or <see langword="null"/> for <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/></param>
    /// <param name="expectedWithLf">The expected text with LF line endings, <c>·</c> marking a space</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsUnderEveryLineEnding(string inputWithLf, string targetName, string contextName, string expectedWithLf)
    {
        foreach (var endOfLine in _lineEndings)
        {
            await AssertFormatsExactly(NormalizeLineEndings(ShowSpaces(inputWithLf), endOfLine),
                                       targetName,
                                       contextName,
                                       NormalizeLineEndings(ShowSpaces(expectedWithLf), endOfLine));
        }
    }

    /// <summary>
    /// Formats the named target, with the named context when one is given, in the document and asserts the document's exact
    /// text, line endings included, and that a second pass with the same target changes nothing
    /// </summary>
    /// <param name="input">The exact document text</param>
    /// <param name="targetName">The name the target declares</param>
    /// <param name="contextName">The name of the context type, or <see langword="null"/> for <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/></param>
    /// <param name="expected">The exact expected text</param>
    /// <param name="assertsSecondPass">Whether a second pass is asserted to change nothing</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task AssertFormatsExactly(string input, string targetName, string contextName, string expected, bool assertsSecondPass = true)
    {
        var firstPass = await Format(input, targetName, contextName);

        Assert.AreEqual(Escape(expected), Escape(firstPass), $"Unexpected first pass.{Environment.NewLine}Expected: {Escape(expected)}{Environment.NewLine}Actual:   {Escape(firstPass)}");

        if (assertsSecondPass)
        {
            var secondPass = await Format(firstPass, targetName, contextName);

            Assert.AreEqual(Escape(expected), Escape(secondPass), $"The second pass must not change the output.{Environment.NewLine}Expected: {Escape(expected)}{Environment.NewLine}Actual:   {Escape(secondPass)}");
        }
    }

    /// <summary>
    /// Formats the named target, with the named context when one is given, in a document with the given text
    /// </summary>
    /// <param name="text">The exact document text</param>
    /// <param name="targetName">The name the target declares</param>
    /// <param name="contextName">The name of the context type, or <see langword="null"/> for <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/></param>
    /// <returns>The document's text after formatting</returns>
    private async Task<string> Format(string text, string targetName, string contextName)
    {
        using (var workspace = new AdhocWorkspace())
        {
            var project = workspace.AddProject("TestProject", LanguageNames.CSharp);
            var document = project.AddDocument("Test.cs", SourceText.From(text));
            var root = await document.GetSyntaxRootAsync(TestContext.CancellationToken);

            if (root == null)
            {
                throw new InvalidOperationException("Expected a syntax root.");
            }

            var target = FindDeclaration(root, targetName);
            var result = contextName == null
                             ? (await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken)).Output
                             : (await ReihitsuFormatter.FormatNodeInDocumentWithContextAsync(document, target, FindDeclaration(root, contextName), TestContext.CancellationToken)).Output;

            return (await result.GetTextAsync(TestContext.CancellationToken)).ToString();
        }
    }

    #endregion // Methods
}