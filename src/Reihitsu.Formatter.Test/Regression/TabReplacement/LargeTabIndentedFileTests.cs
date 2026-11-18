using System.Text;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.TabReplacement;

/// <summary>
/// Tests that the whole pipeline formats a large input whose whitespace is written with tabs exactly as the same input written
/// with four spaces per tab, under LF and CRLF line endings: a whole file, as the CLI formats it, and a detached node
/// </summary>
[TestClass]
public class LargeTabIndentedFileTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The number of members of the generated input, large enough that the input holds a few thousand tokens
    /// </summary>
    private const int GeneratedMemberCount = 300;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that a large file indented with tabs, with tabs between its tokens, in front of its trailing comments, and at
    /// the end of its lines, is formatted like the same file written with four spaces per tab, while the tabs inside the
    /// documentation comments and the comments are kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-TAB-001")]
    [Requirement("FMT-TAB-003")]
    public void FormatsALargeTabIndentedFileLikeTheSameFileWithSpaces()
    {
        var input = $"namespace Bench;\n\npublic class Generated\n{{\n{GenerateMembers("\t", "\t", "\t")}}}\n";
        var expected = $"namespace Bench;\n\npublic class Generated\n{{\n{GenerateMembers("    ", " ", string.Empty)}}}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that formatting a detached class declaration with many members whose whitespace holds tabs gives the output the
    /// same class gets with four spaces per tab
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-023")]
    public void FormatsALargeTabIndentedDetachedNodeLikeTheSameNodeWithSpaces()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var withTabs = SyntaxFactory.ParseMemberDeclaration(NormalizeLineEndings($"public class Generated\n{{\n{GenerateMembers("\t", "\t", "\t")}}}\n", endOfLine));
            var withSpaces = SyntaxFactory.ParseMemberDeclaration(NormalizeLineEndings($"public class Generated\n{{\n{GenerateMembers("    ", "    ", "    ")}}}\n", endOfLine));

            Assert.IsNotNull(withTabs);
            Assert.IsNotNull(withSpaces);

            // Act
            var actualWithTabs = ReihitsuFormatter.FormatNode(withTabs, cancellationToken: TestContext.CancellationToken).Output.ToFullString();
            var actualWithSpaces = ReihitsuFormatter.FormatNode(withSpaces, cancellationToken: TestContext.CancellationToken).Output.ToFullString();

            // Assert
            Assert.AreEqual(NormalizeLineEndings($"public class Generated\n{{\n{GenerateMembers("    ", " ", string.Empty)}}}", endOfLine), actualWithTabs, $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
            Assert.AreEqual(actualWithSpaces, actualWithTabs, $"The node with tabs must get the output of the node with spaces under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Generates members that each carry a three-line documentation comment with a tab in its text, separated by blank lines,
    /// with LF line endings; every other member line ends with a trailing comment with a tab in it, the others with the given
    /// whitespace
    /// </summary>
    /// <param name="whitespace">
    /// The whitespace in front of every member line and documentation comment line, and in front of
    /// the trailing comment
    /// </param>
    /// <param name="gap">The whitespace around the <c>=&gt;</c> of every member</param>
    /// <param name="lineEndWhitespace">The whitespace in front of the line break of every member line without a trailing comment and every blank line</param>
    /// <returns>The member lines, each ending with a line break</returns>
    private static string GenerateMembers(string whitespace, string gap, string lineEndWhitespace)
    {
        var builder = new StringBuilder();

        for (var memberIndex = 1; memberIndex <= GeneratedMemberCount; memberIndex++)
        {
            if (memberIndex > 1)
            {
                builder.Append(lineEndWhitespace).Append('\n');
            }

            builder.Append(whitespace)
                   .Append("/// <summary>\n")
                   .Append(whitespace)
                   .Append("/// Gets\tvalue ")
                   .Append(memberIndex)
                   .Append('\n')
                   .Append(whitespace)
                   .Append("/// </summary>\n")
                   .Append(whitespace)
                   .Append("public int Value")
                   .Append(memberIndex)
                   .Append(gap)
                   .Append("=>")
                   .Append(gap)
                   .Append(memberIndex)
                   .Append(';');

            if (memberIndex % 2 == 1)
            {
                builder.Append(whitespace)
                       .Append("// c\t")
                       .Append(memberIndex);
            }
            else
            {
                builder.Append(lineEndWhitespace);
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    #endregion // Methods
}