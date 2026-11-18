using System;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Cleanup;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.Cleanup;

/// <summary>
/// Tests that <see cref="CleanupPhase"/> cleans every token of a whole file, under LF and CRLF line endings: many lines at
/// once, the first and the last token of the file, code around documentation comments, directives, disabled text, and
/// literals, and a detached root
/// </summary>
[TestClass]
public class CleanupPhaseWholeFileTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The number of members of the generated file, large enough that every line of a file of a few thousand tokens is cleaned
    /// </summary>
    private const int GeneratedMemberCount = 300;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that the whitespace in front of every line break of a large file is removed, on code lines and on
    /// whitespace-only lines, and that the whitespace-only lines and line breaks that end the file are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    [Requirement("FMT-CLEAN-005")]
    public void RemovesWhitespaceInFrontOfEveryLineBreakOfALargeFile()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var input = $"{GenerateMembers("   ", endOfLine)}{endOfLine}    {endOfLine}{endOfLine}";
            var expected = GenerateMembers(string.Empty, endOfLine);

            // Act
            var actual = Clean(input);

            // Assert
            Assert.AreEqual(expected, actual, $"Every line of the file must be cleaned under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the whitespace that ends a large file without a final line break is removed, as well as the whitespace in
    /// front of every line break before it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    [Requirement("FMT-CLEAN-005")]
    public void RemovesWhitespaceThatEndsALargeFileWithoutAFinalLineBreak()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var input = GenerateMembers("   ", endOfLine) + "   ";
            var expected = GenerateMembers(string.Empty, endOfLine);

            // Act
            var actual = Clean(input);

            // Assert
            Assert.AreEqual(expected, actual, $"The end of the file must be cleaned under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the first line of the file, whose first token starts the text, loses the whitespace in front of its line
    /// break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    public void RemovesWhitespaceInFrontOfTheLineBreakOfTheFirstLine()
    {
        AssertCleansUnderEveryLineEnding("using System;   \n\nclass C\n{\n}", "using System;\n\nclass C\n{\n}");
    }

    /// <summary>
    /// Verifies that disabled text and the contents of a literal keep their whitespace, and that documentation comments and
    /// directives stay as written, while the code lines around them lose the whitespace in front of their line breaks
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    [Requirement("FMT-CLEAN-004")]
    public void KeepsDisabledTextAndLiteralsWhileCleaningTheCodeAroundThem()
    {
        const string input = "class C   \n"
                             + "{\n"
                             + "    /// <summary>\n"
                             + "    /// Doc\n"
                             + "    /// </summary>\n"
                             + "    int _x;   \n"
                             + "#pragma warning disable CS0169\n"
                             + "    int _y;   \n"
                             + "#if NEVER\n"
                             + "    int _z;   \n"
                             + "#endif\n"
                             + "    #region Members\n"
                             + "    string _s = @\"a   \n"
                             + "b\";   \n"
                             + "    #endregion\n"
                             + "}\n";
        const string expected = "class C\n"
                                + "{\n"
                                + "    /// <summary>\n"
                                + "    /// Doc\n"
                                + "    /// </summary>\n"
                                + "    int _x;\n"
                                + "#pragma warning disable CS0169\n"
                                + "    int _y;\n"
                                + "#if NEVER\n"
                                + "    int _z;   \n"
                                + "#endif\n"
                                + "    #region Members\n"
                                + "    string _s = @\"a   \n"
                                + "b\";\n"
                                + "    #endregion\n"
                                + "}";

        AssertCleansUnderEveryLineEnding(input, expected);
    }

    /// <summary>
    /// Verifies that the last token of a detached root loses only the line breaks behind it, and every line of the root loses
    /// the whitespace in front of its line break
    /// </summary>
    [TestMethod]
    [Requirement("FMT-CLEAN-001")]
    [Requirement("FMT-CLEAN-008")]
    public void CleansADetachedRoot()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var node = SyntaxFactory.ParseMemberDeclaration(NormalizeLineEndings("class C   \n{\n    int _x;   \n\n    int _y;   \n}\n", endOfLine));

            Assert.IsNotNull(node);
            Assert.IsNull(node.Parent);

            // Act
            var actual = CleanupPhase.Execute(node, TestContext.CancellationToken).ToFullString();

            // Assert
            Assert.AreEqual(NormalizeLineEndings("class C\n{\n    int _x;\n\n    int _y;\n}", endOfLine), actual, $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that the phase throws <see cref="OperationCanceledException"/> when the cancellation token is already cancelled,
    /// instead of cleaning every token of the file
    /// </summary>
    [TestMethod]
    public void ThrowsWhenCancellationIsRequested()
    {
        // Arrange
        var root = CSharpSyntaxTree.ParseText(GenerateMembers("   ", "\n"), cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        using (var cancellationTokenSource = new CancellationTokenSource())
        {
            cancellationTokenSource.Cancel();

            // Act & Assert
            Assert.ThrowsExactly<OperationCanceledException>(() => CleanupPhase.Execute(root, cancellationTokenSource.Token));
        }
    }

    /// <summary>
    /// Generates a class whose members each carry a three-line documentation comment, followed by a blank line
    /// </summary>
    /// <param name="lineEndWhitespace">The whitespace in front of the line break of every code line and blank line</param>
    /// <param name="endOfLine">The line ending</param>
    /// <returns>The source text, ending with the closing brace of the class</returns>
    private static string GenerateMembers(string lineEndWhitespace, string endOfLine)
    {
        var builder = new StringBuilder();

        builder.Append("namespace Bench;")
               .Append(lineEndWhitespace)
               .Append(endOfLine)
               .Append(lineEndWhitespace)
               .Append(endOfLine)
               .Append("public class Generated")
               .Append(lineEndWhitespace)
               .Append(endOfLine)
               .Append('{')
               .Append(lineEndWhitespace)
               .Append(endOfLine);

        for (var memberIndex = 1; memberIndex <= GeneratedMemberCount; memberIndex++)
        {
            builder.Append("    /// <summary>")
                   .Append(endOfLine)
                   .Append("    /// Gets value ")
                   .Append(memberIndex)
                   .Append(endOfLine)
                   .Append("    /// </summary>")
                   .Append(endOfLine)
                   .Append("    public int Value")
                   .Append(memberIndex)
                   .Append(" => ")
                   .Append(memberIndex)
                   .Append(';')
                   .Append(lineEndWhitespace)
                   .Append(endOfLine)
                   .Append(lineEndWhitespace)
                   .Append(endOfLine);
        }

        builder.Append('}');

        return builder.ToString();
    }

    /// <summary>
    /// Cleans the input under LF and CRLF line endings and asserts the expected output for each
    /// </summary>
    /// <param name="input">The input, with LF line endings</param>
    /// <param name="expected">The expected output, with LF line endings</param>
    private void AssertCleansUnderEveryLineEnding(string input, string expected)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var actual = Clean(NormalizeLineEndings(input, endOfLine));

            Assert.AreEqual(NormalizeLineEndings(expected, endOfLine), actual, $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Runs the cleanup phase on the given source text
    /// </summary>
    /// <param name="input">The source text</param>
    /// <returns>The cleaned source text</returns>
    private string Clean(string input)
    {
        var root = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        return CleanupPhase.Execute(root, TestContext.CancellationToken).ToFullString();
    }

    #endregion // Methods
}