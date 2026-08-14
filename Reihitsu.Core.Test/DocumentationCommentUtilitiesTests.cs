using System;
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
    /// Verifies that a missing separator is replaced by a single space
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesInsertsMissingSeparator()
    {
        Assert.AreEqual("/// Text", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("///Text", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that a canonical single space is preserved
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesKeepsCanonicalSeparator()
    {
        Assert.AreEqual("/// Text", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("/// Text", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that content indentation beyond the separator is preserved
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesKeepsContentIndentation()
    {
        Assert.AreEqual("///   Text", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("///   Text", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that a tab separator is replaced by a single space
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesReplacesTabSeparator()
    {
        Assert.AreEqual("/// Text", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("///\tText", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that an empty suffix is left alone
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesKeepsEmptySuffix()
    {
        Assert.AreEqual("///", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("///", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that a whitespace-only suffix is reduced to nothing
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesReducesWhitespaceOnlySuffix()
    {
        Assert.AreEqual("///", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("///   ", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that only the first exterior on a line is recognized and later slashes stay content
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesTreatsLaterSlashesAsContent()
    {
        Assert.AreEqual("/// see /// here", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("/// see /// here", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that continuation indentation before the exterior is preserved verbatim
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesPreservesContinuationIndentation()
    {
        Assert.AreEqual("/// A\n\t /// B", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("///A\n\t ///B", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that non-breaking space, form feed and vertical tab count as line-leading indentation
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesAcceptsExoticIndentation()
    {
        Assert.AreEqual("/// A\n\u00A0\u000B\u000C/// B", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("///A\n\u00A0\u000B\u000C///B", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that every line terminator C# recognizes starts a new documentation line
    /// </summary>
    /// <param name="lineTerminator">Terminator separating the two documentation lines</param>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void NormalizeDocumentationLinePrefixesNormalizesEveryLineTerminator(string lineTerminator)
    {
        var input = string.Concat("///A", lineTerminator, "///B");
        var expected = string.Concat("/// A", lineTerminator, "/// B");

        Assert.AreEqual(expected, DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes(input, TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that a trailing terminator does not produce a match past the end of the text
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesLeavesTrailingTerminatorAlone()
    {
        Assert.AreEqual("/// A\n", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("///A\n", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that a line carrying no exterior is copied verbatim
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesCopiesNonDocumentationLinesVerbatim()
    {
        Assert.AreEqual("  plain\n/// A", DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("  plain\n///A", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that normalizing already normalized text is a no-op
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesIsIdempotent()
    {
        var once = DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes("///A\r\n  ///\tB\r\n", TestContext.CancellationToken);

        Assert.AreEqual(once, DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes(once, TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that a null comment text is rejected
    /// </summary>
    [TestMethod]
    public void NormalizeDocumentationLinePrefixesRejectsNullText()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => DocumentationCommentUtilities.NormalizeDocumentationLinePrefixes(null, TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that every exterior becomes an ordinary comment marker on every line terminator
    /// </summary>
    /// <param name="lineTerminator">Terminator separating the two documentation lines</param>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void ConvertDocumentationExteriorsToCommentsConvertsEveryLineTerminator(string lineTerminator)
    {
        var input = string.Concat("///A", lineTerminator, "  ///B");
        var expected = string.Concat("//A", lineTerminator, "  //B");

        Assert.AreEqual(expected, DocumentationCommentUtilities.ConvertDocumentationExteriorsToComments(input, TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that content after the exterior is preserved verbatim, including a later exterior-looking run
    /// </summary>
    [TestMethod]
    public void ConvertDocumentationExteriorsToCommentsPreservesContent()
    {
        Assert.AreEqual("//  spaced /// content", DocumentationCommentUtilities.ConvertDocumentationExteriorsToComments("///  spaced /// content", TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that converting already converted text is a no-op
    /// </summary>
    [TestMethod]
    public void ConvertDocumentationExteriorsToCommentsIsIdempotent()
    {
        var once = DocumentationCommentUtilities.ConvertDocumentationExteriorsToComments("///A\n  ///B", TestContext.CancellationToken);

        Assert.AreEqual(once, DocumentationCommentUtilities.ConvertDocumentationExteriorsToComments(once, TestContext.CancellationToken));
    }

    /// <summary>
    /// Verifies that a null comment text is rejected
    /// </summary>
    [TestMethod]
    public void ConvertDocumentationExteriorsToCommentsRejectsNullText()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => DocumentationCommentUtilities.ConvertDocumentationExteriorsToComments(null, TestContext.CancellationToken));
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