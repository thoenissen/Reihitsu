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
    /// Verifies that a missing exterior separator is inserted without changing the content
    /// </summary>
    [TestMethod]
    public void NormalizeExteriorSuffixAddsMissingSeparator()
    {
        // Act
        var result = DocumentationCommentUtilities.NormalizeExteriorSuffix("Summary.");

        // Assert
        Assert.AreEqual(" Summary.", result);
    }

    /// <summary>
    /// Verifies that an invalid separator is replaced without removing the indentation which follows it
    /// </summary>
    [TestMethod]
    public void NormalizeExteriorSuffixPreservesContentIndentation()
    {
        // Act
        var result = DocumentationCommentUtilities.NormalizeExteriorSuffix("\t  Second");

        // Assert
        Assert.AreEqual("   Second", result);
    }

    /// <summary>
    /// Verifies that content indentation after an already canonical separator is left unchanged
    /// </summary>
    [TestMethod]
    public void NormalizeExteriorSuffixLeavesCanonicalContentIndentationUnchanged()
    {
        // Act
        var result = DocumentationCommentUtilities.NormalizeExteriorSuffix("   Second");

        // Assert
        Assert.AreEqual("   Second", result);
    }

    /// <summary>
    /// Verifies that extra whitespace without documentation content is removed
    /// </summary>
    [TestMethod]
    public void NormalizeExteriorSuffixRemovesWhitespaceOnlyExcess()
    {
        // Act
        var result = DocumentationCommentUtilities.NormalizeExteriorSuffix("  ");

        // Assert
        Assert.AreEqual(string.Empty, result);
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