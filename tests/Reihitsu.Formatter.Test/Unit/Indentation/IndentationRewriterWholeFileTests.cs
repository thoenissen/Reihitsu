using System;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Indentation.Utilities;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Indentation;

/// <summary>
/// Tests that <see cref="IndentationRewriter.Apply"/> places every line of a whole file, under LF and CRLF line endings: many
/// members at once, lines that start with a documentation comment, a directive, a region, disabled text, or a delimited
/// comment, multi-line raw strings behind a documentation comment, a missing token, and the first and last token of the file
/// </summary>
[TestClass]
public class IndentationRewriterWholeFileTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The number of members of the generated file, large enough that every line of a file of a few thousand tokens is placed
    /// </summary>
    private const int GeneratedMemberCount = 300;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that every member of a large file, and every line of the documentation comment in front of it, is moved from
    /// column 0 to the member's column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    [Requirement("FMT-INDENT-008")]
    public void IndentsEveryMemberAndDocumentationCommentLineOfALargeFile()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var input = GenerateDocumentedMembers(string.Empty, endOfLine);
            var expected = GenerateDocumentedMembers("    ", endOfLine);

            // Act
            var actual = RunIndent(input, endOfLine, TestContext.CancellationToken);

            // Assert
            Assert.AreEqual(expected, actual, $"Every line of the file must be placed under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that every member of a large file, and every line of the comment in front of it, is moved from column 0 to the
    /// member's column when the file is parsed without documentation comments, so that every <c>///</c> line is an ordinary
    /// single-line comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    [Requirement("FMT-INDENT-006")]
    public void IndentsEveryMemberAndCommentLineOfALargeFileParsedWithoutDocumentationComments()
    {
        var parseOptions = CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None);

        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var input = GenerateDocumentedMembers(string.Empty, endOfLine);
            var expected = GenerateDocumentedMembers("    ", endOfLine);

            // Act
            var actual = RunIndent(input, endOfLine, TestContext.CancellationToken, parseOptions);

            // Assert
            Assert.AreEqual(expected, actual, $"Every line of the file must be placed under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that directives other than regions start at column 0, region directives and documentation comments take the
    /// column of the code around them, disabled text keeps its text, and a delimited comment moves only its first line, while
    /// the code between them is indented
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-007")]
    [Requirement("FMT-INDENT-009")]
    [Requirement("FMT-INDENT-010")]
    [Requirement("FMT-INDENT-011")]
    public void PlacesDirectivesRegionsDisabledTextAndCommentsWhileIndentingTheCodeAroundThem()
    {
        const string input = """
                             namespace N
                             {
                             class C
                             {
                             #region Members
                             /// <summary>
                             /// Doc
                             /// </summary>
                             int _x;
                                   #pragma warning disable CS0169
                             int _y;
                                 #if NEVER
                                   int   _z;
                                 #else
                             int _w;
                                 #endif
                             /* first
                                second */
                             int _v;
                             #endregion
                             }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    class C
                                    {
                                        #region Members
                                        /// <summary>
                                        /// Doc
                                        /// </summary>
                                        int _x;
                                #pragma warning disable CS0169
                                        int _y;
                                #if NEVER
                                      int   _z;
                                #else
                                        int _w;
                                #endif
                                        /* first
                                   second */
                                        int _v;
                                        #endregion
                                    }
                                }
                                """;

        AssertIndentsUnderEveryLineEnding(input, expected);
    }

    /// <summary>
    /// Verifies that the lines of a multi-line raw string and of an interpolated raw string with a hole move under the opening
    /// quotes when the field in front of them, which carries a documentation comment, moves to its column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-RAW-001")]
    [Requirement("FMT-RAW-002")]
    [Requirement("FMT-RAW-003")]
    public void MovesRawStringLinesUnderTheOpeningQuotesBehindADocumentationComment()
    {
        const string input = """"
                             class C
                             {
                             /// <summary>
                             /// Doc
                             /// </summary>
                             string _s = """
                                 text
                                   more
                                 """;
                             /// <summary>
                             /// Doc
                             /// </summary>
                             string _t = $"""
                                 a {1 + 2} b
                                 """;
                             }
                             """";
        const string expected = """"
                                class C
                                {
                                    /// <summary>
                                    /// Doc
                                    /// </summary>
                                    string _s = """
                                                text
                                                  more
                                                """;
                                    /// <summary>
                                    /// Doc
                                    /// </summary>
                                    string _t = $"""
                                                 a {1 + 2} b
                                                 """;
                                }
                                """";

        AssertIndentsUnderEveryLineEnding(input, expected);
    }

    /// <summary>
    /// Verifies that a missing token is left as it is and the lines around it are still placed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void PlacesTheLinesAroundAMissingToken()
    {
        const string input = """
                             class C
                             {
                             void M()
                             {
                             var x = 1
                             var y = 2;
                             }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1
                                        var y = 2;
                                    }
                                }
                                """;

        AssertIndentsUnderEveryLineEnding(input, expected);
    }

    /// <summary>
    /// Verifies that the first token of the file, whose leading trivia starts the text, moves to column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    public void PlacesTheFirstTokenOfTheFile()
    {
        const string input = """
                                 class C
                             {
                             int _x;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int _x;
                                }
                                """;

        AssertIndentsUnderEveryLineEnding(input, expected);
    }

    /// <summary>
    /// Characterizes the end of the file: a whitespace-only line in the leading trivia of the end-of-file token keeps its text,
    /// the whitespace on the last line, in front of the end of the file, takes that line's column 0, and the lines in front of
    /// them are placed
    /// </summary>
    [TestMethod]
    public void PlacesTheLastLineInFrontOfTheEndOfTheFile()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var input = NormalizeLineEndings("class C\n{\nint _x;\n}\n      \n   ", endOfLine);
            var expected = NormalizeLineEndings("class C\n{\n    int _x;\n}\n      \n", endOfLine);

            // Act
            var actual = RunIndent(input, endOfLine, TestContext.CancellationToken);

            // Assert
            Assert.AreEqual(expected, actual, $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Generates a class whose members each carry a three-line documentation comment
    /// </summary>
    /// <param name="memberIndentation">The whitespace in front of every member line</param>
    /// <param name="endOfLine">The line ending</param>
    /// <returns>The source text</returns>
    private static string GenerateDocumentedMembers(string memberIndentation, string endOfLine)
    {
        var builder = new StringBuilder();

        builder.Append("namespace Bench;")
               .Append(endOfLine)
               .Append(endOfLine)
               .Append("public class Generated")
               .Append(endOfLine)
               .Append('{')
               .Append(endOfLine);

        for (var memberIndex = 1; memberIndex <= GeneratedMemberCount; memberIndex++)
        {
            builder.Append(memberIndentation)
                   .Append("/// <summary>")
                   .Append(endOfLine)
                   .Append(memberIndentation)
                   .Append("/// Gets value ")
                   .Append(memberIndex)
                   .Append(endOfLine)
                   .Append(memberIndentation)
                   .Append("/// </summary>")
                   .Append(endOfLine)
                   .Append(memberIndentation)
                   .Append("public int Value")
                   .Append(memberIndex)
                   .Append(" => ")
                   .Append(memberIndex)
                   .Append(';')
                   .Append(endOfLine)
                   .Append(endOfLine);
        }

        builder.Append('}');

        return builder.ToString();
    }

    /// <summary>
    /// Computes the layout model and applies indentation to the given input
    /// </summary>
    /// <param name="input">The source text to indent</param>
    /// <param name="endOfLine">The line ending of the formatting context</param>
    /// <param name="token">The cancellation token</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <returns>The indented source text</returns>
    private static string RunIndent(string input, string endOfLine, CancellationToken token, CSharpParseOptions parseOptions = null)
    {
        var tree = CSharpSyntaxTree.ParseText(input, parseOptions, cancellationToken: token);
        var context = new FormattingContext(endOfLine);
        var root = tree.GetRoot(token);
        var model = LayoutComputer.Compute(root, context, token);
        var result = IndentationRewriter.Apply(root, model, token);

        return result.ToFullString();
    }

    /// <summary>
    /// Indents the input under LF and CRLF line endings and asserts the expected output for each
    /// </summary>
    /// <param name="input">The input, with LF line endings</param>
    /// <param name="expected">The expected output, with LF line endings</param>
    private void AssertIndentsUnderEveryLineEnding(string input, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var actual = RunIndent(NormalizeLineEndings(input, endOfLine), endOfLine, TestContext.CancellationToken);

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), actual, $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    #endregion // Methods
}