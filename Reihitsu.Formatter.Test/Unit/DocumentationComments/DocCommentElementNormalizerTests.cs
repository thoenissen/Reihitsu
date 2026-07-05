using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.DocumentationComments;

namespace Reihitsu.Formatter.Test.Unit.DocumentationComments;

/// <summary>
/// Tests for <see cref="DocCommentElementNormalizer"/>, the element-building half of the
/// documentation-comment phase. These pin the collapse/expand decision and the summary handling
/// independently of the candidate location and line-prefix normalization done by the phase
/// </summary>
[TestClass]
public class DocCommentElementNormalizerTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a single-line summary element is reported as requiring normalization
    /// </summary>
    [TestMethod]
    public void RequiresNormalizationReturnsTrueForSingleLineSummary()
    {
        // Arrange
        var (element, sourceText) = ParseElement("""
                                                 /// <summary> Does a thing. </summary>
                                                 public class C
                                                 {
                                                 }
                                                 """,
                                                 "summary");

        // Act
        var result = DocCommentElementNormalizer.RequiresNormalization(element, sourceText);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a summary already spanning at least three lines is left untouched
    /// </summary>
    [TestMethod]
    public void RequiresNormalizationReturnsFalseForSummarySpanningThreeLines()
    {
        // Arrange
        var (element, sourceText) = ParseElement("""
                                                 /// <summary>
                                                 /// Does a thing.
                                                 /// </summary>
                                                 public class C
                                                 {
                                                 }
                                                 """,
                                                 "summary");

        // Act
        var result = DocCommentElementNormalizer.RequiresNormalization(element, sourceText);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that an element whose content starts on the start-tag line but spans multiple lines requires normalization
    /// </summary>
    [TestMethod]
    public void RequiresNormalizationReturnsTrueForMisalignedElement()
    {
        // Arrange
        var (element, sourceText) = ParseElement("""
                                                 public class C
                                                 {
                                                     /// <returns> The value
                                                     /// </returns>
                                                     public int M() => 0;
                                                 }
                                                 """,
                                                 "returns");

        // Act
        var result = DocCommentElementNormalizer.RequiresNormalization(element, sourceText);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Verifies that a well-aligned multi-line element does not require normalization
    /// </summary>
    [TestMethod]
    public void RequiresNormalizationReturnsFalseForWellAlignedElement()
    {
        // Arrange
        var (element, sourceText) = ParseElement("""
                                                 public class C
                                                 {
                                                     /// <returns>
                                                     /// The value
                                                     /// </returns>
                                                     public int M() => 0;
                                                 }
                                                 """,
                                                 "returns");

        // Act
        var result = DocCommentElementNormalizer.RequiresNormalization(element, sourceText);

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Verifies that a single-line summary is expanded across three lines
    /// </summary>
    [TestMethod]
    public void BuildReplacementExpandsSingleLineSummary()
    {
        // Arrange
        var (element, sourceText) = ParseElement("""
                                                 /// <summary> Does a thing. </summary>
                                                 public class C
                                                 {
                                                 }
                                                 """,
                                                 "summary");

        const string expected = """
                                <summary>
                                /// Does a thing.
                                /// </summary>
                                """;

        // Act
        var result = DocCommentElementNormalizer.BuildReplacement(element, sourceText);

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Verifies that a non-summary element with single-line content is collapsed to one line
    /// </summary>
    [TestMethod]
    public void BuildReplacementCollapsesNonSummaryElement()
    {
        // Arrange
        var (element, sourceText) = ParseElement("""
                                                 public class C
                                                 {
                                                     /// <returns> The value
                                                     /// </returns>
                                                     public int M() => 0;
                                                 }
                                                 """,
                                                 "returns");

        // Act
        var result = DocCommentElementNormalizer.BuildReplacement(element, sourceText);

        // Assert
        Assert.AreEqual("<returns>The value</returns>", result);
    }

    /// <summary>
    /// Verifies that sentence text before a mid-line element is not duplicated on rebuilt continuation lines
    /// </summary>
    [TestMethod]
    public void BuildReplacementDoesNotDuplicateLeadingSentenceText()
    {
        // Arrange
        var (element, sourceText) = ParseElement("""
                                                 public class C
                                                 {
                                                     /// <summary>
                                                     /// Use <c>first
                                                     /// second</c> carefully.
                                                     /// </summary>
                                                     public void M()
                                                     {
                                                     }
                                                 }
                                                 """,
                                                 "c");

        const string expected = """
                                <c>
                                    /// first
                                    /// second
                                    /// </c>
                                """;

        // Act
        var result = DocCommentElementNormalizer.BuildReplacement(element, sourceText);

        // Assert
        Assert.AreEqual(expected, result);
        Assert.DoesNotContain("Use", result);
    }

    /// <summary>
    /// Verifies that significant indentation inside a code element is preserved when the element is rebuilt
    /// </summary>
    [TestMethod]
    public void BuildReplacementPreservesCodeIndentation()
    {
        // Arrange
        var (element, sourceText) = ParseElement("""
                                                 public class C
                                                 {
                                                     /// <code>var x = 1;
                                                     ///     if (x == 1)
                                                     /// </code>
                                                     public void M()
                                                     {
                                                     }
                                                 }
                                                 """,
                                                 "code");

        const string expected = """
                                <code>
                                    /// var x = 1;
                                    ///     if (x == 1)
                                    /// </code>
                                """;

        // Act
        var result = DocCommentElementNormalizer.BuildReplacement(element, sourceText);

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Parses the given source and returns the first XML documentation element with the requested name
    /// </summary>
    /// <param name="code">Source code to parse</param>
    /// <param name="localName">Local name of the XML element to locate</param>
    /// <returns>The located element together with the parsed source text</returns>
    private (XmlElementSyntax Element, SourceText SourceText) ParseElement(string code, string localName)
    {
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.CancellationToken);
        var sourceText = tree.GetText(TestContext.CancellationToken);
        var element = tree.GetRoot(TestContext.CancellationToken)
                          .DescendantNodes(descendIntoTrivia: true)
                          .OfType<XmlElementSyntax>()
                          .First(obj => obj.StartTag.Name.LocalName.ValueText == localName);

        return (element, sourceText);
    }

    #endregion // Methods
}