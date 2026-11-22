using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests that the whole pipeline indents and cleans every line of a large input, under LF and CRLF line endings: a whole file,
/// as the CLI formats it, a detached node, and a node formatted in its document
/// </summary>
[TestClass]
public class LargeFileFullPipelineTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The number of members of the generated input, large enough that the input holds a few thousand tokens
    /// </summary>
    private const int GeneratedMemberCount = 300;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Verifies that every member of a large file and every line of its documentation comment is moved to the member's
    /// column, that every line loses the whitespace in front of its line break, and that the whitespace-only lines and line
    /// breaks that end the file are removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    [Requirement("FMT-INDENT-008")]
    [Requirement("FMT-CLEAN-001")]
    [Requirement("FMT-CLEAN-005")]
    public void IndentsAndCleansEveryLineOfALargeFile()
    {
        var input = $"namespace Bench;\n\npublic class Generated\n{{\n{GenerateMembers(string.Empty, "   ")}}}\n    \n\n";
        var expected = $"namespace Bench;\n\npublic class Generated\n{{\n{GenerateMembers("    ", string.Empty)}}}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that formatting a detached class declaration with many members indents and cleans every line of it, and that
    /// its last token loses the line breaks behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    [Requirement("FMT-CLEAN-001")]
    [Requirement("FMT-CLEAN-008")]
    public void IndentsAndCleansEveryLineOfADetachedNode()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var node = SyntaxFactory.ParseMemberDeclaration(NormalizeLineEndings($"public class Generated\n{{\n{GenerateMembers(string.Empty, "   ")}}}\n", endOfLine));

            Assert.IsNotNull(node);

            // Act
            var actual = ReihitsuFormatter.FormatNode(node, cancellationToken: TestContext.CancellationToken).Output.ToFullString();

            // Assert
            Assert.AreEqual(NormalizeLineEndings($"public class Generated\n{{\n{GenerateMembers("    ", string.Empty)}}}", endOfLine), actual, $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Verifies that formatting a class declaration with many members in its document indents and cleans every line of it,
    /// and leaves the text outside it unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test</returns>
    [TestMethod]
    [Requirement("FMT-INDENT-002")]
    [Requirement("FMT-CLEAN-001")]
    public async Task IndentsAndCleansEveryLineOfANodeInItsDocument()
    {
        foreach (var endOfLine in _lineEndings)
        {
            // Arrange
            var input = NormalizeLineEndings($"namespace Bench\n{{\n    public class Generated\n    {{\n{GenerateMembers(string.Empty, "   ")}    }}\n}}\n", endOfLine);
            var expected = NormalizeLineEndings($"namespace Bench\n{{\n    public class Generated\n    {{\n{GenerateMembers("        ", string.Empty)}    }}\n}}\n", endOfLine);

            // Act
            var actual = await FormatClassInDocument(input);

            // Assert
            Assert.AreEqual(expected, actual, $"Unexpected output under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    /// <summary>
    /// Generates members that each carry a three-line documentation comment, separated by blank lines, with LF line endings
    /// </summary>
    /// <param name="memberIndentation">The whitespace in front of every member line and documentation comment line</param>
    /// <param name="lineEndWhitespace">The whitespace in front of the line break of every member line and blank line</param>
    /// <returns>The member lines, each ending with a line break</returns>
    private static string GenerateMembers(string memberIndentation, string lineEndWhitespace)
    {
        var builder = new StringBuilder();

        for (var memberIndex = 1; memberIndex <= GeneratedMemberCount; memberIndex++)
        {
            if (memberIndex > 1)
            {
                builder.Append(lineEndWhitespace).Append('\n');
            }

            builder.Append(memberIndentation)
                   .Append("/// <summary>\n")
                   .Append(memberIndentation)
                   .Append("/// Gets value ")
                   .Append(memberIndex)
                   .Append('\n')
                   .Append(memberIndentation)
                   .Append("/// </summary>\n")
                   .Append(memberIndentation)
                   .Append("public int Value")
                   .Append(memberIndex)
                   .Append(" => ")
                   .Append(memberIndex)
                   .Append(';')
                   .Append(lineEndWhitespace)
                   .Append('\n');
        }

        return builder.ToString();
    }

    /// <summary>
    /// Formats the class <c>Generated</c> in a document with the given text through <see cref="ReihitsuFormatter.FormatNodeInDocumentAsync"/>
    /// </summary>
    /// <param name="text">The document text</param>
    /// <returns>The document's text after formatting</returns>
    private async Task<string> FormatClassInDocument(string text)
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

            var target = root.DescendantNodes().OfType<ClassDeclarationSyntax>().Single();
            var result = await ReihitsuFormatter.FormatNodeInDocumentAsync(document, target, TestContext.CancellationToken);

            return (await result.Output.GetTextAsync(TestContext.CancellationToken)).ToString();
        }
    }

    #endregion // Methods
}