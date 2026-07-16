using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for <see cref="LineBreakDetection"/>
/// </summary>
[TestClass]
public class LineBreakDetectionTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that an accessor list with only auto-accessors is recognized as an auto-property
    /// </summary>
    [TestMethod]
    public void IsAutoPropertyAccessorListReturnsTrueForAutoProperty()
    {
        // Arrange
        var accessorList = ParseAccessorList("class C { int P { get; set; } }");

        // Act
        var result = LineBreakDetection.IsAutoPropertyAccessorList(accessorList);

        // Assert
        Assert.IsTrue(result, "An accessor list with only auto-accessors should be detected as an auto-property.");
    }

    /// <summary>
    /// Verifies that an accessor list with a body is not recognized as an auto-property
    /// </summary>
    [TestMethod]
    public void IsAutoPropertyAccessorListReturnsFalseWhenAccessorHasBody()
    {
        // Arrange
        var accessorList = ParseAccessorList("class C { int P { get { return 0; } } }");

        // Act
        var result = LineBreakDetection.IsAutoPropertyAccessorList(accessorList);

        // Assert
        Assert.IsFalse(result, "An accessor list with a body accessor should not be detected as an auto-property.");
    }

    /// <summary>
    /// Verifies that a node spanning a single line is not reported as multi-line
    /// </summary>
    [TestMethod]
    public void IsMultiLineReturnsFalseForSingleLineNode()
    {
        // Arrange
        var statement = SyntaxFactory.ParseStatement("var x = 1;");

        // Act
        var result = LineBreakDetection.IsMultiLine(statement);

        // Assert
        Assert.IsFalse(result, "A single-line node should not be reported as multi-line.");
    }

    /// <summary>
    /// Verifies that a node spanning multiple lines is reported as multi-line
    /// </summary>
    [TestMethod]
    public void IsMultiLineReturnsTrueForMultiLineNode()
    {
        // Arrange
        var statement = SyntaxFactory.ParseStatement("var x =\n    1;");

        // Act
        var result = LineBreakDetection.IsMultiLine(statement);

        // Assert
        Assert.IsTrue(result, "A node spanning multiple lines should be reported as multi-line.");
    }

    /// <summary>
    /// Verifies that a node whose own text is single-line is not reported as multi-line when a
    /// blank line precedes it in leading trivia (issue #423)
    /// </summary>
    [TestMethod]
    public void IsMultiLineReturnsFalseForSingleLineNodeWithLeadingBlankLine()
    {
        // Arrange
        var statement = SyntaxFactory.ParseStatement("\n\nvar x = 1;");

        // Act
        var result = LineBreakDetection.IsMultiLine(statement);

        // Assert
        Assert.IsFalse(result, "A single-line node preceded by a blank line should not be reported as multi-line.");
    }

    /// <summary>
    /// Verifies that a node whose own text is single-line is not reported as multi-line when a
    /// comment line precedes it in leading trivia (issue #423)
    /// </summary>
    [TestMethod]
    public void IsMultiLineReturnsFalseForSingleLineNodeWithLeadingCommentLine()
    {
        // Arrange
        var statement = SyntaxFactory.ParseStatement("// note\nvar x = 1;");

        // Act
        var result = LineBreakDetection.IsMultiLine(statement);

        // Assert
        Assert.IsFalse(result, "A single-line node preceded by a comment line should not be reported as multi-line.");
    }

    /// <summary>
    /// Verifies that a node whose own text is single-line is not reported as multi-line when a
    /// trailing comment on its last token pushes trailing trivia onto a new line (issue #423)
    /// </summary>
    [TestMethod]
    public void IsMultiLineReturnsFalseForSingleLineNodeWithTrailingCommentLine()
    {
        // Arrange
        var statement = SyntaxFactory.ParseStatement("var x = 1; // note\n");

        // Act
        var result = LineBreakDetection.IsMultiLine(statement);

        // Assert
        Assert.IsFalse(result, "A single-line node followed by a trailing comment should not be reported as multi-line.");
    }

    /// <summary>
    /// Parses the first accessor list found in the given source text
    /// </summary>
    /// <param name="source">The C# source text</param>
    /// <returns>The first accessor list</returns>
    private AccessorListSyntax ParseAccessorList(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);

        return tree.GetRoot(TestContext.CancellationToken)
                   .DescendantNodes()
                   .OfType<AccessorListSyntax>()
                   .First();
    }

    #endregion // Methods
}