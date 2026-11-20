using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for the line-break carriers that <see cref="LineEndingUtilities.DetectEndOfLine"/> counts and
/// <see cref="LineEndingUtilities.NormalizeLineEndings"/> rewrites
/// </summary>
[TestClass]
public class LineEndingUtilitiesTests
{
    #region Constants

    /// <summary>
    /// Placeholder for the one line break of a fixture that a test replaces with the terminator under test
    /// </summary>
    private const string LineBreakPlaceholder = "¶";

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that a line break inside a CDATA section of a single-line documentation comment is counted and rewritten
    /// </summary>
    /// <param name="terminator">The line break inside the CDATA section</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u2028")]
    public void LineBreakInsideDocumentationCDataSectionIsACarrier(string terminator)
    {
        const string source = "class C\n{\n    /// <summary>\n    /// <![CDATA[a¶    /// b]]>\n    /// </summary>\n    int A;\n}";

        AssertCarrier<XmlCDataSectionSyntax>(source, terminator, static _ => true);
    }

    /// <summary>
    /// Verifies that a line break inside an XML comment of a single-line documentation comment is counted and rewritten
    /// </summary>
    /// <param name="terminator">The line break inside the XML comment</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u2028")]
    public void LineBreakInsideDocumentationXmlCommentIsACarrier(string terminator)
    {
        const string source = "class C\n{\n    /// <summary>\n    /// <!-- a¶    /// b -->\n    /// </summary>\n    int A;\n}";

        AssertCarrier<XmlCommentSyntax>(source, terminator, static _ => true);
    }

    /// <summary>
    /// Verifies that a line break inside an element tag of a single-line documentation comment, between the element name and
    /// an attribute, is counted and rewritten
    /// </summary>
    /// <param name="terminator">The line break inside the element tag</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u2028")]
    public void LineBreakInsideDocumentationElementTagIsACarrier(string terminator)
    {
        const string source = "class C\n{\n    /// <summary>\n    /// <para¶    /// foo=\"a\">x</para>\n    /// </summary>\n    int A;\n}";

        AssertCarrier<XmlElementStartTagSyntax>(source, terminator, static tag => tag.Name.LocalName.ValueText == "para");
    }

    /// <summary>
    /// Verifies that a line break inside an attribute value of a single-line documentation comment is counted and rewritten
    /// </summary>
    /// <param name="terminator">The line break inside the attribute value</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u2028")]
    public void LineBreakInsideDocumentationAttributeValueIsACarrier(string terminator)
    {
        const string source = "class C\n{\n    /// <summary>\n    /// <para foo=\"a¶    /// b\">x</para>\n    /// </summary>\n    int A;\n}";

        AssertCarrier<XmlTextAttributeSyntax>(source, terminator, static _ => true);
    }

    /// <summary>
    /// Verifies that a line break inside a CDATA section of a delimited documentation comment is counted and rewritten
    /// </summary>
    /// <param name="terminator">The line break inside the CDATA section</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [Requirement("FMT-EOL-003")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u2028")]
    public void LineBreakInsideDelimitedDocumentationCDataSectionIsACarrier(string terminator)
    {
        const string source = "class C\n{\n    /**\n     * <summary><![CDATA[a¶     * b]]></summary>\n     */\n    int A;\n}";

        AssertCarrier<XmlCDataSectionSyntax>(source, terminator, static _ => true);
    }

    /// <summary>
    /// Verifies that a line break inside an interpolation hole of a regular interpolated string is counted and rewritten
    /// </summary>
    /// <param name="terminator">The line break inside the interpolation hole</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [Requirement("FMT-EOL-001")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u2028")]
    public void LineBreakInsideInterpolationHoleIsACarrier(string terminator)
    {
        const string source = "class C\n{\n    string s = $\"{1¶                  + 2}\";\n}";

        AssertCarrier<InterpolationSyntax>(source, terminator, static _ => true);
    }

    /// <summary>
    /// Verifies that a line break inside an interpolation hole of a raw interpolated string is counted and rewritten
    /// </summary>
    /// <param name="terminator">The line break inside the interpolation hole</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [Requirement("FMT-EOL-001")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u2028")]
    public void LineBreakInsideRawInterpolationHoleIsACarrier(string terminator)
    {
        const string source = "class C\n{\n    string s = $\"\"\"\n                {1¶                 + 2}\n                \"\"\";\n}";

        AssertCarrier<InterpolationSyntax>(source, terminator, static _ => true);
    }

    /// <summary>
    /// Verifies that a line break in the literal text of a raw interpolated string is neither counted nor rewritten
    /// </summary>
    /// <param name="terminator">The line break in the literal text</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [Requirement("FMT-EOL-004")]
    [DataRow("\r\n")]
    [DataRow("\r")]
    [DataRow("\u2028")]
    public void LineBreakInTextOfRawInterpolatedStringIsNotACarrier(string terminator)
    {
        const string source = "class C\n{\n    string s = $\"\"\"\n                a¶                {1}\n                \"\"\";\n}";

        var root = CoreSyntaxTestHelper.ParseCompilationUnit(source.Replace(LineBreakPlaceholder, terminator));
        var text = root.DescendantNodes().OfType<InterpolatedStringTextSyntax>().First();

        Assert.Contains(terminator, text.ToFullString(), "The fixture must hold the line break in the literal text.");
        Assert.AreEqual(Environment.NewLine, LineEndingUtilities.DetectEndOfLine(text), "A node whose only line break is literal text holds no carrier, so the detection falls back.");
        Assert.AreSame(root, LineEndingUtilities.NormalizeLineEndings(root, "\n"), "The line break in the literal text must not be rewritten.");
    }

    /// <summary>
    /// Verifies that a file whose every line break between tokens, at the end of a directive, and inside a documentation
    /// comment is the same single-character terminator other than <c>\n</c> is detected as <c>\n</c>
    /// </summary>
    /// <param name="terminator">The terminator of every line break of the file</param>
    [TestMethod]
    [Requirement("FMT-PIPE-007")]
    [DataRow("\r")]
    [DataRow("\u0085")]
    [DataRow("\u2028")]
    [DataRow("\u2029")]
    public void DetectEndOfLineReturnsLineFeedWhenEveryLineBreakIsTheSameOtherTerminator(string terminator)
    {
        const string source = "class C\n{\n#pragma warning disable CS0169\n    /// <summary>\n    /// Gets the value.\n    /// </summary>\n    int x;\n#pragma warning restore CS0169\n}";

        var root = CoreSyntaxTestHelper.ParseCompilationUnit(source.Replace("\n", terminator));

        Assert.AreEqual("\n", LineEndingUtilities.DetectEndOfLine(root));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Verifies that the one line break of a fixture is a carrier: the detection counts it when it is the only line break of
    /// the node that holds it, and the normalization of the whole file rewrites it to <c>\n</c> and changes nothing else
    /// </summary>
    /// <typeparam name="TNode">The type of the node that holds the line break</typeparam>
    /// <param name="source">The fixture, written with <c>\n</c> and one placeholder</param>
    /// <param name="terminator">The line break that replaces the placeholder</param>
    /// <param name="isHolder">Selects the node that holds the line break among the nodes of type <typeparamref name="TNode"/></param>
    private static void AssertCarrier<TNode>(string source, string terminator, Func<TNode, bool> isHolder)
        where TNode : SyntaxNode
    {
        var root = CoreSyntaxTestHelper.ParseCompilationUnit(source.Replace(LineBreakPlaceholder, terminator));
        var holder = root.DescendantNodes(descendIntoTrivia: true).OfType<TNode>().Single(isHolder);

        Assert.Contains(terminator, holder.ToFullString(), "The fixture must place the line break inside the selected node.");

        // A single-character line break counts as "\n". Without a carrier the detection falls back to Environment.NewLine,
        // so the detection is only conclusive where that fallback differs from what the carrier counts as.
        var counted = terminator == "\r\n"
                          ? "\r\n"
                          : "\n";

        if (Environment.NewLine != counted)
        {
            Assert.AreEqual(counted, LineEndingUtilities.DetectEndOfLine(holder), "The detection must count the line break.");
        }

        Assert.AreEqual(source.Replace(LineBreakPlaceholder, "\n"), LineEndingUtilities.NormalizeLineEndings(root, "\n").ToFullString(), "The normalization must rewrite the line break.");
    }

    #endregion // Methods
}