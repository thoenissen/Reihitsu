using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Test.Base;
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
    /// Verifies that a gap inside a list element belongs to a nested construct exactly when the token after the gap is one of the
    /// tokens of a construct that lays out its own lines, for the analyzer and the formatter alike: an anonymous object, an
    /// initializer, a switch expression, a property pattern, a parenthesized pattern, a list pattern, a collection expression after
    /// its own opening bracket, a block, a query body, and an interpolation own it; the opening bracket of a collection expression,
    /// a lambda's expression body, the <c>in</c> of a <c>from</c> clause, a binary operator, and a chain link do not
    /// </summary>
    /// <param name="statement">Statement containing the <c>/*X*/</c> marker comment inside an argument</param>
    /// <param name="isOwned">Whether a nested construct owns the gap</param>
    [TestMethod]
    [Requirement("SHARED-COMMENT-006")]
    [DataRow("Run(new\n/*X*/{ A = 1 });", true, DisplayName = "Anonymous object")]
    [DataRow("Run(new System.Collections.Generic.List<int>()\n/*X*/{ 1 });", true, DisplayName = "Collection initializer")]
    [DataRow("Run(o\n/*X*/switch { _ => 1 });", true, DisplayName = "Switch expression")]
    [DataRow("Run(o is string\n/*X*/{ Length: 1 });", true, DisplayName = "Property pattern")]
    [DataRow("Run(o is\n/*X*/(1 or 2));", true, DisplayName = "Parenthesized pattern")]
    [DataRow("Run(items is\n/*X*/[1]);", true, DisplayName = "List pattern")]
    [DataRow("Run([1,\n/*X*/2]);", true, DisplayName = "Collection expression after its opening bracket")]
    [DataRow("Run(1 +\n/*X*/[2].Length);", false, DisplayName = "Opening bracket of a collection expression")]
    [DataRow("Run((System.Action)(() =>\n/*X*/{\n}));", true, DisplayName = "Block")]
    [DataRow("Run((System.Func<int>)(() =>\n/*X*/1));", false, DisplayName = "Lambda expression body")]
    [DataRow("Run(from item in items\n/*X*/where item > 0 select item);", true, DisplayName = "Query body")]
    [DataRow("Run(from item\n/*X*/in items select item);", false, DisplayName = "In of a from clause")]
    [DataRow("Run($\"{a /*X*/}\");", true, DisplayName = "Interpolation")]
    [DataRow("Run(a\n/*X*/&& b);", false, DisplayName = "Binary operator")]
    [DataRow("Run(items.Where(item => item > 0)\n/*X*/.ToList());", false, DisplayName = "Chain link")]
    public void IsOwnedByNestedConstructReturnsTheOwnershipOfEachConstruct(string statement, bool isOwned)
    {
        var comment = GetMarkerComment(statement);

        Assert.IsTrue(CommentPositionUtilities.TryGetSurroundingTokens(comment, out _, out var nextToken));

        var element = nextToken.Parent.AncestorsAndSelf().First(ListElementInteriorUtilities.IsListElement);

        Assert.AreNotEqual(element.GetFirstToken(), nextToken, "The fixture must not place the marker on the boundary before an element.");
        Assert.AreEqual(isOwned, CommentPositionUtilities.IsOwnedByNestedConstruct(element.Parent, nextToken), "Analyzer");
        Assert.AreEqual(isOwned, ListElementInteriorUtilities.FindOwningConstruct(nextToken, element) != null, "Formatter");
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