using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Unit tests for <see cref="XmlDocumentationElementOrderingUtilities"/>
/// </summary>
[TestClass]
public class XmlDocumentationElementOrderingUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that the canonical rank of the inheritdoc element is the lowest
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankReturnsZeroForInheritdoc()
    {
        Assert.AreEqual(0, XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("inheritdoc"));
    }

    /// <summary>
    /// Verifies that the canonical rank of the include element is higher than the rank of the inheritdoc element
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankRanksIncludeAfterInheritdoc()
    {
        var inheritdocRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("inheritdoc");
        var includeRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("include");

        Assert.IsGreaterThan(inheritdocRank, includeRank);
    }

    /// <summary>
    /// Verifies that the canonical rank of the include element is lower than the rank of the summary element
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankRanksIncludeBeforeSummary()
    {
        var includeRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("include");
        var summaryRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("summary");

        Assert.IsGreaterThan(includeRank, summaryRank);
    }

    /// <summary>
    /// Verifies that the canonical rank of the seealso element is higher than the rank of the remarks element
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankRanksSeealsoAfterRemarks()
    {
        var remarksRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("remarks");
        var seealsoRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("seealso");

        Assert.IsGreaterThan(remarksRank, seealsoRank);
    }

    /// <summary>
    /// Verifies that the canonical rank of the remarks element is higher than the rank of the returns element
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankRanksRemarksAfterReturns()
    {
        var returnsRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("returns");
        var remarksRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("remarks");

        Assert.IsGreaterThan(returnsRank, remarksRank);
    }

    /// <summary>
    /// Verifies that the canonical rank of the permission element is higher than the rank of the exception element
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankRanksPermissionAfterException()
    {
        var exceptionRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("exception");
        var permissionRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("permission");

        Assert.IsGreaterThan(exceptionRank, permissionRank);
    }

    /// <summary>
    /// Verifies that the canonical rank of the permission element is lower than the rank of the example element
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankRanksPermissionBeforeExample()
    {
        var permissionRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("permission");
        var exampleRank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("example");

        Assert.IsGreaterThan(permissionRank, exampleRank);
    }

    /// <summary>
    /// Verifies that the canonical rank lookup is case insensitive
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankIsCaseInsensitive()
    {
        Assert.AreEqual(XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("summary"),
                        XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("SUMMARY"));
    }

    /// <summary>
    /// Verifies that a non-standard custom element receives the unknown element rank
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankReturnsUnknownRankForUnknownElement()
    {
        Assert.AreEqual(XmlDocumentationElementOrderingUtilities.UnknownElementRank,
                        XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("custom"));
    }

    /// <summary>
    /// Verifies that the tag name of an XML element node is returned
    /// </summary>
    [TestMethod]
    public void GetTagNameReturnsTagNameForElement()
    {
        var element = GetSingleXmlNode("""
                                       class Sample
                                       {
                                           /// <summary>Text</summary>
                                           public void Execute()
                                           {
                                           }
                                       }
                                       """);

        Assert.AreEqual("summary", XmlDocumentationElementOrderingUtilities.GetTagName(element));
    }

    /// <summary>
    /// Verifies that the tag name of an empty XML element node is returned
    /// </summary>
    [TestMethod]
    public void GetTagNameReturnsTagNameForEmptyElement()
    {
        var element = GetSingleXmlNode("""
                                       class Sample
                                       {
                                           /// <inheritdoc/>
                                           public void Execute()
                                           {
                                           }
                                       }
                                       """);

        Assert.AreEqual("inheritdoc", XmlDocumentationElementOrderingUtilities.GetTagName(element));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the single XML documentation node from the provided source
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The XML node</returns>
    private static XmlNodeSyntax GetSingleXmlNode(string source)
    {
        return CSharpSyntaxTree.ParseText(source)
                               .GetRoot()
                               .DescendantNodes(descendIntoTrivia: true)
                               .OfType<XmlNodeSyntax>()
                               .First(static node => node is XmlElementSyntax or XmlEmptyElementSyntax);
    }

    #endregion // Methods
}