using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Core;
using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Analyzer.Test.Core;

/// <summary>
/// Unit tests for <see cref="CommentPositionUtilities"/>
/// </summary>
[TestClass]
public class CommentPositionUtilitiesTests
{
    #region Methods

    /// <summary>
    /// Gets the single <c>/*X*/</c> marker comment of a statement placed inside a method body
    /// </summary>
    /// <param name="statement">Statement</param>
    /// <returns>The marker comment</returns>
    private static SyntaxTrivia GetMarkerComment(string statement)
    {
        var source = $"class Sample\n{{\n    void Method(bool a, bool b, object o, int[] items)\n    {{\n        {statement}\n    }}\n}}";
        var root = CSharpSyntaxTree.ParseText(source).GetRoot();

        return root.DescendantTrivia().Single(trivia => trivia.ToString() == "/*X*/");
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that the analyzer decides ownership of a gap inside a list element exactly like the formatter does
    /// before it joins the line of the token after the gap
    /// </summary>
    /// <param name="statement">Statement containing the <c>/*X*/</c> marker comment inside an argument</param>
    [TestMethod]
    [DataRow("Run(new\n/*X*/{ A = 1 });")]
    [DataRow("Run(new System.Collections.Generic.List<int>()\n/*X*/{ 1 });")]
    [DataRow("Run(o\n/*X*/switch { _ => 1 });")]
    [DataRow("Run(o is string\n/*X*/{ Length: 1 });")]
    [DataRow("Run(o is\n/*X*/(1 or 2));")]
    [DataRow("Run(items is\n/*X*/[1]);")]
    [DataRow("Run([1,\n/*X*/2]);")]
    [DataRow("Run(1 +\n/*X*/[2].Length);")]
    [DataRow("Run((System.Action)(() =>\n/*X*/{\n}));")]
    [DataRow("Run((System.Func<int>)(() =>\n/*X*/1));")]
    [DataRow("Run(from item in items\n/*X*/where item > 0 select item);")]
    [DataRow("Run(from item\n/*X*/in items select item);")]
    [DataRow("Run($\"{a /*X*/}\");")]
    [DataRow("Run(a\n/*X*/&& b);")]
    [DataRow("Run(items.Where(item => item > 0)\n/*X*/.ToList());")]
    public void IsOwnedByNestedConstructMatchesFormatterOwnership(string statement)
    {
        var comment = GetMarkerComment(statement);

        Assert.IsTrue(CommentPositionUtilities.TryGetSurroundingTokens(comment, out _, out var nextToken));

        var element = nextToken.Parent.AncestorsAndSelf().First(ListElementInteriorUtilities.IsListElement);

        Assert.AreNotEqual(element.GetFirstToken(), nextToken, "The fixture must not place the marker on the boundary before an element.");

        var isOwnedByFormatter = ListElementInteriorUtilities.FindOwningConstruct(nextToken, element) != null;
        var isOwnedByAnalyzer = CommentPositionUtilities.IsOwnedByNestedConstruct(element.Parent, nextToken);

        Assert.AreEqual(isOwnedByFormatter, isOwnedByAnalyzer);
    }

    /// <summary>
    /// Verifies that a gap before the first token of a list element belongs to the list, even when the element starts
    /// with a construct that owns its own lines
    /// </summary>
    /// <param name="statement">Statement containing the <c>/*X*/</c> marker comment before an argument</param>
    [TestMethod]
    [DataRow("Run(\n/*X*/new { A = 1 });")]
    [DataRow("Run(1,\n/*X*/o switch { _ => 1 });")]
    [DataRow("Run(\n/*X*/[1]);")]
    public void IsOwnedByNestedConstructReturnsFalseBeforeElement(string statement)
    {
        var comment = GetMarkerComment(statement);

        Assert.IsTrue(CommentPositionUtilities.TryGetSurroundingTokens(comment, out _, out var nextToken));

        var element = nextToken.Parent.AncestorsAndSelf().First(ListElementInteriorUtilities.IsListElement);

        Assert.IsFalse(CommentPositionUtilities.IsOwnedByNestedConstruct(element.Parent, nextToken));
    }

    #endregion // Tests
}