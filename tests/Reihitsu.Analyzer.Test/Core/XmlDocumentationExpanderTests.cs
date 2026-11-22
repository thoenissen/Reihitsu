using System.Linq;
using System.Xml.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Test.Core;

/// <summary>
/// Unit tests for <see cref="XmlDocumentationExpander"/>
/// </summary>
[TestClass]
public class XmlDocumentationExpanderTests
{
    #region Tests

    /// <summary>
    /// Verifies that direct full elements are matched case-insensitively
    /// </summary>
    [TestMethod]
    public void HasTagReturnsTrueForCaseInsensitiveDirectFullElement()
    {
        var documentationComment = GetDocumentationComment("<SuMmArY>Summary.</SuMmArY>");

        Assert.IsTrue(XmlDocumentationExpander.HasTag(documentationComment, expandedDocumentation: null, tagName: "summary"));
    }

    /// <summary>
    /// Verifies that direct empty elements are matched
    /// </summary>
    [TestMethod]
    public void HasTagReturnsTrueForDirectEmptyElement()
    {
        var documentationComment = GetDocumentationComment("<inheritdoc/>");

        Assert.IsTrue(XmlDocumentationExpander.HasTag(documentationComment, expandedDocumentation: null, tagName: "inheritdoc"));
    }

    /// <summary>
    /// Verifies that nested full elements do not satisfy declaration-level tag queries
    /// </summary>
    [TestMethod]
    public void HasTagReturnsFalseForNestedFullElement()
    {
        var documentationComment = GetDocumentationComment("<remarks><summary>Nested.</summary></remarks>");

        Assert.IsFalse(XmlDocumentationExpander.HasTag(documentationComment, expandedDocumentation: null, tagName: "summary"));
    }

    /// <summary>
    /// Verifies that nested empty elements do not satisfy declaration-level tag queries
    /// </summary>
    [TestMethod]
    public void HasTagReturnsFalseForNestedEmptyElement()
    {
        var documentationComment = GetDocumentationComment("<remarks><inheritdoc/></remarks>");

        Assert.IsFalse(XmlDocumentationExpander.HasTag(documentationComment, expandedDocumentation: null, tagName: "inheritdoc"));
    }

    /// <summary>
    /// Verifies that expanded top-level elements remain eligible
    /// </summary>
    [TestMethod]
    public void HasTagReturnsTrueForExpandedTopLevelElement()
    {
        var documentationComment = GetDocumentationComment("<remarks>Local remarks.</remarks>");
        var expandedDocumentation = XElement.Parse("<member><summary>Expanded summary.</summary></member>");

        Assert.IsTrue(XmlDocumentationExpander.HasTag(documentationComment, expandedDocumentation, "summary"));
    }

    /// <summary>
    /// Verifies that expanded nested elements remain ineligible
    /// </summary>
    [TestMethod]
    public void HasTagReturnsFalseForExpandedNestedElement()
    {
        var documentationComment = GetDocumentationComment("<remarks>Local remarks.</remarks>");
        var expandedDocumentation = XElement.Parse("<member><remarks><summary>Nested.</summary></remarks></member>");

        Assert.IsFalse(XmlDocumentationExpander.HasTag(documentationComment, expandedDocumentation, "summary"));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets a documentation comment containing the provided XML
    /// </summary>
    /// <param name="xml">XML content</param>
    /// <returns>The documentation comment</returns>
    private static DocumentationCommentTriviaSyntax GetDocumentationComment(string xml)
    {
        var source = $$"""
                       /// {{xml}}
                       class Sample
                       {
                       }
                       """;
        var declaration = CSharpSyntaxTree.ParseText(source)
                                          .GetRoot()
                                          .DescendantNodes()
                                          .OfType<ClassDeclarationSyntax>()
                                          .Single();

        return DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);
    }

    #endregion // Methods
}