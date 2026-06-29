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
    /// Verifies that the canonical rank of the summary element is the lowest
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankReturnsZeroForSummary()
    {
        Assert.AreEqual(0, XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("summary"));
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
    /// Verifies that the canonical rank lookup is case insensitive
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankIsCaseInsensitive()
    {
        Assert.AreEqual(XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("summary"),
                        XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("SUMMARY"));
    }

    /// <summary>
    /// Verifies that an unknown element receives the unknown element rank
    /// </summary>
    [TestMethod]
    public void GetCanonicalElementRankReturnsUnknownRankForUnknownElement()
    {
        Assert.AreEqual(XmlDocumentationElementOrderingUtilities.UnknownElementRank,
                        XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank("seealso"));
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