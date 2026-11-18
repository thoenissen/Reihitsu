using System;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.TabReplacement;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.TabReplacement;

/// <summary>
/// Tests that <see cref="TabReplacementPhase"/> replaces the tabs around every token of a whole file, under LF and CRLF line
/// endings: many lines at once, tokens with and without a tab next to each other, the first and the last token of the file,
/// tokens whose leading trivia also holds a directive with a tab, the text whose tabs are kept, and a detached root
/// </summary>
[TestClass]
public class TabReplacementPhaseWholeFileTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The number of members of the generated file, large enough that the file holds a few thousand tokens
    /// </summary>
    private const int GeneratedMemberCount = 300;

    /// <summary>
    /// Four spaces, the replacement of one tab
    /// </summary>
    private const string FourSpaces = "    ";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that every tab in the whitespace of the code of a large file becomes four spaces — in front of a line's first
    /// token and in front of a documentation comment, between two tokens, in the code of an interpolation hole, in front of a
    /// trailing comment, at the end of a line, and on a whitespace-only line — while the tabs inside the documentation
    /// comments, in front of their continuation lines, in the text of the literals, and inside the comments are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    [Requirement("FMT-TAB-003")]
    public void ReplacesEveryTabInTheWhitespaceOfTheCodeOfALargeFile()
    {
        AssertReplacesUnderEveryLineEnding(GenerateFile(_ => "\t", "\t"), GenerateFile(_ => FourSpaces, "\t"));
    }

    /// <summary>
    /// Verifies that, when the documentation comments are not parsed, the <c>///</c> lines are ordinary comments, so the tabs
    /// in front of every one of them are whitespace of the code and become four spaces, while the tabs inside them are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    [Requirement("FMT-TAB-003")]
    public void ReplacesTheTabsInFrontOfEveryCommentLineOfALargeFileParsedWithoutDocumentationComments()
    {
        var parseOptions = CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None);

        AssertReplacesUnderEveryLineEnding(GenerateFile(_ => "\t", "\t"), GenerateFile(_ => FourSpaces, FourSpaces), parseOptions);
    }

    /// <summary>
    /// Verifies that in a large file whose members alternate between tabs and whitespace without a tab, only the tabs are
    /// replaced, and the whitespace of the members without a tab stays exactly as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void ReplacesTheTabsOfEveryOtherMemberAndKeepsTheWhitespaceOfTheMembersBetweenThem()
    {
        AssertReplacesUnderEveryLineEnding(GenerateFile(memberIndex => memberIndex % 2 == 0 ? "\t" : "  ", "\t"),
                                           GenerateFile(memberIndex => memberIndex % 2 == 0 ? FourSpaces : "  ", "\t"));
    }

    /// <summary>
    /// Verifies that in a large file every tab between the tokens of a directive becomes four spaces, also where the token
    /// that carries the directive in its leading trivia has a tab in front of it and behind it, while the tabs inside a region
    /// description, an <c>#endregion</c> comment, and disabled text are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    [Requirement("FMT-TAB-002")]
    [Requirement("FMT-TAB-003")]
    public void ReplacesTheTabsAroundAndInsideTheDirectivesOfALargeFile()
    {
        AssertReplacesUnderEveryLineEnding(GenerateFileWithDirectives("\t", "\t"), GenerateFileWithDirectives(FourSpaces, FourSpaces));
    }

    /// <summary>
    /// Verifies that the tabs between the tokens of directives are replaced in a large file whose code holds no tab in its
    /// whitespace at all
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-002")]
    public void ReplacesTheTabsInsideTheDirectivesOfALargeFileWithoutTabsInTheCode()
    {
        AssertReplacesUnderEveryLineEnding(GenerateFileWithDirectives(FourSpaces, "\t"), GenerateFileWithDirectives(FourSpaces, FourSpaces));
    }

    /// <summary>
    /// Verifies that a large file whose tabs all stand inside documentation comments, in front of their continuation lines,
    /// inside comments, in the text of literals, and in disabled text is returned unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-003")]
    public void KeepsALargeFileWhoseTabsAllStandInTextThatKeepsThem()
    {
        var input = GenerateFile(_ => FourSpaces, "\t") + "\n#if NEVER\n\tdisabled\ttext\n#endif\n";

        AssertReplacesUnderEveryLineEnding(input, input);
    }

    /// <summary>
    /// Verifies that a tab in front of the first token of the file and a tab on the last line, which the end of the file
    /// carries, become four spaces each, as well as the tabs on the lines between them
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void ReplacesTheTabsInFrontOfTheFirstTokenAndOnTheLastLineOfALargeFile()
    {
        AssertReplacesUnderEveryLineEnding($"\tusing System;\n{GenerateFile(_ => "\t", "\t")}\t",
                                           $"{FourSpaces}using System;\n{GenerateFile(_ => FourSpaces, "\t")}{FourSpaces}");
    }

    /// <summary>
    /// Verifies that every tab in the whitespace of a detached class declaration with many members becomes four spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    public void ReplacesEveryTabInTheWhitespaceOfADetachedRoot()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var node = SyntaxFactory.ParseMemberDeclaration(NormalizeLineEndings(GenerateClass(_ => "\t", "\t"), endOfLine));

            Assert.IsNotNull(node);
            Assert.IsNull(node.Parent);

            // Act
            var actual = TabReplacementPhase.Execute(node, TestContext.CancellationToken).ToFullString();

            // Assert
            Assert.AreEqual(NormalizeLineEndings(GenerateClass(_ => FourSpaces, "\t"), endOfLine), actual, $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the phase throws <see cref="OperationCanceledException"/> when the cancellation token is already cancelled,
    /// instead of replacing the tabs of every token of the file
    /// </summary>
    [TestMethod]
    public void ThrowsWhenCancellationIsRequested()
    {
        // Arrange
        var root = CSharpSyntaxTree.ParseText(GenerateFile(_ => "\t", "\t"), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            cancellationTokenSource.Cancel();

            // Act & Assert
            Assert.ThrowsExactly<OperationCanceledException>(() => TabReplacementPhase.Execute(root, cancellationTokenSource.Token));
        }
    }

    /// <summary>
    /// Generates a file with one class whose members are generated by <see cref="GenerateClass"/>
    /// </summary>
    /// <param name="codeWhitespace">The whitespace of the code of a member, by member index</param>
    /// <param name="continuationIndentation">The whitespace in front of the continuation lines of every documentation comment</param>
    /// <returns>The source text with LF line endings, ending with a line break</returns>
    private static string GenerateFile(Func<int, string> codeWhitespace, string continuationIndentation)
    {
        return $"namespace Bench;\n\n{GenerateClass(codeWhitespace, continuationIndentation)}";
    }

    /// <summary>
    /// Generates a class whose members each carry a three-line documentation comment with a tab in its text, an expression
    /// body with an interpolated string that holds a tab in its text and the given whitespace in its hole, and a trailing
    /// comment with a tab in it, followed by a field and a whitespace-only line
    /// </summary>
    /// <param name="codeWhitespace">
    /// The whitespace of the code of a member, by member index: in front of the first line of the
    /// documentation comment, the member, and the field, between the tokens, in front of the trailing comment, at the end of
    /// the field line, and on the whitespace-only line behind it
    /// </param>
    /// <param name="continuationIndentation">The whitespace in front of the continuation lines of every documentation comment</param>
    /// <returns>The source text with LF line endings, ending with a line break</returns>
    private static string GenerateClass(Func<int, string> codeWhitespace, string continuationIndentation)
    {
        var builder = new StringBuilder("public class Generated\n{\n");

        for (var memberIndex = 1; memberIndex <= GeneratedMemberCount; memberIndex++)
        {
            var whitespace = codeWhitespace(memberIndex);

            builder.Append(whitespace)
                   .Append("/// <summary>\n")
                   .Append(continuationIndentation)
                   .Append("/// Gets\tvalue ")
                   .Append(memberIndex)
                   .Append('\n')
                   .Append(continuationIndentation)
                   .Append("/// </summary>\n")
                   .Append(whitespace)
                   .Append("public string Value")
                   .Append(memberIndex)
                   .Append(whitespace)
                   .Append("=>")
                   .Append(whitespace)
                   .Append("$\"v\t{")
                   .Append(whitespace)
                   .Append(memberIndex)
                   .Append(whitespace)
                   .Append("}\";")
                   .Append(whitespace)
                   .Append("// c\t")
                   .Append(memberIndex)
                   .Append('\n')
                   .Append(whitespace)
                   .Append("private int _field")
                   .Append(memberIndex)
                   .Append(';')
                   .Append(whitespace)
                   .Append('\n')
                   .Append(whitespace)
                   .Append('\n');
        }

        builder.Append("}\n");

        return builder.ToString();
    }

    /// <summary>
    /// Generates a file with one class whose members each stand in a region and behind a <c>#pragma</c> directive with the given
    /// whitespace around and inside the directives, and whose members alternate with disabled text that holds a tab
    /// </summary>
    /// <param name="indentation">The whitespace in front of every line of the code and every directive line</param>
    /// <param name="whitespace">
    /// The whitespace between the tokens of every directive and at the end of every <c>#pragma</c>
    /// line
    /// </param>
    /// <returns>The source text with LF line endings, ending with a line break</returns>
    private static string GenerateFileWithDirectives(string indentation, string whitespace)
    {
        var builder = new StringBuilder("namespace Bench;\n\npublic class Generated\n{\n");

        for (var memberIndex = 1; memberIndex <= GeneratedMemberCount; memberIndex++)
        {
            builder.Append(indentation)
                   .Append("#region")
                   .Append(whitespace)
                   .Append("Part")
                   .Append(memberIndex)
                   .Append("\tof the file\n")
                   .Append(indentation)
                   .Append("#pragma")
                   .Append(whitespace)
                   .Append("warning")
                   .Append(whitespace)
                   .Append("disable")
                   .Append(whitespace)
                   .Append("CS0169")
                   .Append(whitespace)
                   .Append('\n')
                   .Append(indentation)
                   .Append("private int _value")
                   .Append(memberIndex)
                   .Append(";\n")
                   .Append("#if NEVER\n")
                   .Append("\tdisabled\ttext ")
                   .Append(memberIndex)
                   .Append('\n')
                   .Append("#endif\n")
                   .Append(indentation)
                   .Append("#endregion // Part")
                   .Append(memberIndex)
                   .Append("\tof the file\n");
        }

        builder.Append("}\n");

        return builder.ToString();
    }

    /// <summary>
    /// Runs the phase on the input under LF and CRLF line endings and asserts the expected output for each
    /// </summary>
    /// <param name="input">The input, with LF line endings</param>
    /// <param name="expected">The expected output, with LF line endings</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the default options</param>
    private void AssertReplacesUnderEveryLineEnding(string input, string expected, CSharpParseOptions parseOptions = null)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var root = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), parseOptions, cancellationToken: TestContext.CancellationToken)
                                       .GetRoot(TestContext.CancellationToken);

            var actual = TabReplacementPhase.Execute(root, TestContext.CancellationToken).ToFullString();

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), actual, $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    #endregion // Methods
}