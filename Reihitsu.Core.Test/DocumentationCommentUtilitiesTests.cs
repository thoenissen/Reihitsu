using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Unit tests for <see cref="DocumentationCommentUtilities"/>
/// </summary>
[TestClass]
public class DocumentationCommentUtilitiesTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that the continuation prefix is the exterior plus a single space for a simple line
    /// </summary>
    [TestMethod]
    public void GetContinuationPrefixReturnsExteriorPlusSingleSpace()
    {
        // Arrange
        var sourceText = ParseLine("/// <summary>");

        // Act
        var result = DocumentationCommentUtilities.GetContinuationPrefix(sourceText, sourceText.Lines[0]);

        // Assert
        Assert.AreEqual("/// ", result);
    }

    /// <summary>
    /// Verifies that the continuation prefix preserves the leading indentation of the line
    /// </summary>
    [TestMethod]
    public void GetContinuationPrefixPreservesLeadingIndentation()
    {
        // Arrange
        var sourceText = ParseLine("        /// <returns>");

        // Act
        var result = DocumentationCommentUtilities.GetContinuationPrefix(sourceText, sourceText.Lines[0]);

        // Assert
        Assert.AreEqual("        /// ", result);
    }

    /// <summary>
    /// Verifies that sentence text before the first element is not part of the continuation prefix
    /// </summary>
    [TestMethod]
    public void GetContinuationPrefixIgnoresSentenceTextBeforeElement()
    {
        // Arrange
        var sourceText = ParseLine("    /// Stuff <c>value");

        // Act
        var result = DocumentationCommentUtilities.GetContinuationPrefix(sourceText, sourceText.Lines[0]);

        // Assert
        Assert.AreEqual("    /// ", result);
    }

    /// <summary>
    /// Verifies that a line without a documentation exterior yields an empty prefix
    /// </summary>
    [TestMethod]
    public void GetContinuationPrefixReturnsEmptyWhenNoExterior()
    {
        // Arrange
        var sourceText = ParseLine("public void Method()");

        // Act
        var result = DocumentationCommentUtilities.GetContinuationPrefix(sourceText, sourceText.Lines[0]);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Verifies that continuation exterior markers can be detected and aligned to an absolute column
    /// </summary>
    [TestMethod]
    public void AlignContinuationExteriorMarkersUsesRequestedColumn()
    {
        var documentationTrivia = ParseDocumentationTrivia("/// First.\n/// Second.\n");

        Assert.IsFalse(DocumentationCommentUtilities.AreContinuationExteriorMarkersAligned(documentationTrivia, 4));

        var alignedTrivia = DocumentationCommentUtilities.AlignContinuationExteriorMarkers(documentationTrivia, 4);

        Assert.AreEqual("/// First.\n    /// Second.\n", alignedTrivia.ToFullString());
        Assert.IsTrue(DocumentationCommentUtilities.AreContinuationExteriorMarkersAligned(alignedTrivia, 4));
    }

    /// <summary>
    /// Verifies that continuation exterior markers can be shifted with a containing formatted node
    /// </summary>
    [TestMethod]
    public void ShiftContinuationExteriorMarkersAppliesColumnOffset()
    {
        var documentationTrivia = ParseDocumentationTrivia("/// First.\n    /// Second.\n");

        var shiftedTrivia = DocumentationCommentUtilities.ShiftContinuationExteriorMarkers(documentationTrivia, 3);

        Assert.AreEqual("/// First.\n       /// Second.\n", shiftedTrivia.ToFullString());
    }

    /// <summary>
    /// Parses a single-line documentation comment trivia
    /// </summary>
    /// <param name="text">Documentation comment text</param>
    /// <returns>The parsed documentation comment trivia</returns>
    private SyntaxTrivia ParseDocumentationTrivia(string text)
    {
        var root = CSharpSyntaxTree.ParseText(text + "internal class Example;", cancellationToken: TestContext.CancellationToken)
                                   .GetRoot(TestContext.CancellationToken);

        return root.GetFirstToken()
                   .LeadingTrivia
                   .Single(trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
    }

    /// <summary>
    /// Parses a single line of source into a <see cref="SourceText"/>
    /// </summary>
    /// <param name="line">Line to parse</param>
    /// <returns>The parsed source text</returns>
    private SourceText ParseLine(string line)
    {
        return CSharpSyntaxTree.ParseText(line, cancellationToken: TestContext.CancellationToken).GetText(TestContext.CancellationToken);
    }

    #endregion // Tests
}