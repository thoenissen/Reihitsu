using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core.Enumerations;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="FluentChain"/>
/// </summary>
[TestClass]
public class FluentChainTests
{
    #region Tests

    /// <summary>
    /// Verifies that every member access is a link, invoked or not, and that argument lists are attached parts
    /// </summary>
    [TestMethod]
    public void CreateCollectsInvokedAndNonInvokedLinks()
    {
        var chain = CreateChain("list.Where(o => o).First().Name");

        Assert.AreSequenceEqual(new[] { "Where", "First", "Name" }, chain.Links.Select(link => link.Name.Identifier.ValueText).ToArray());
        Assert.AreSequenceEqual(new[] { true, true, false }, chain.Links.Select(link => link.IsInvoked).ToArray());
        Assert.AreEqual(2, chain.AttachedParts.Count(part => part.Kind == FluentChainAttachedPartKind.ArgumentList));
    }

    /// <summary>
    /// Verifies the operator tokens of <c>.</c>, <c>?.</c>, <c>!.</c> and <c>!?.</c> links
    /// </summary>
    [TestMethod]
    public void CreateCollectsOperatorTokensOfEveryLinkKind()
    {
        var chain = CreateChain("a.B()?.C()!.D()!?.E()");

        Assert.AreSequenceEqual(new[] { ".", "?.", "!.", "!?." }, chain.Links.Select(link => string.Concat(link.OperatorTokens.Select(token => token.Text))).ToArray());
        Assert.AreSequenceEqual(new[] { ".", "?", "!", "!" }, chain.Links.Select(link => link.OperatorToken.Text).ToArray());
    }

    /// <summary>
    /// Verifies that of two null-forgiving operators only the one in front of the link operator belongs to it
    /// </summary>
    [TestMethod]
    public void CreateKeepsOnlyTheLastNullForgivingInTheOperator()
    {
        var chain = CreateChain("a.B!!?.C()");

        Assert.AreEqual("!?.", string.Concat(chain.Links[1].OperatorTokens.Select(token => token.Text)));
        Assert.ContainsSingle(part => part.Kind == FluentChainAttachedPartKind.NullForgiving, chain.AttachedParts);
    }

    /// <summary>
    /// Verifies that element accesses, conditional element accesses and trailing null-forgiving operators are attached
    /// parts and not links
    /// </summary>
    [TestMethod]
    public void CreateTreatsElementAccessAndTrailingNullForgivingAsAttachedParts()
    {
        var chain = CreateChain("a.B!?[0].C()[1]!");

        Assert.AreSequenceEqual(new[] { "B", "C" }, chain.Links.Select(link => link.Name.Identifier.ValueText).ToArray());
        Assert.AreSequenceEqual(new[] { "!?[", "(", "[", "!" }, chain.AttachedParts.Select(part => string.Concat(part.Tokens.Select(token => token.Text))).ToArray());
    }

    /// <summary>
    /// Verifies that the prefix runs up to the first invoked link
    /// </summary>
    [TestMethod]
    public void CreateSplitsPrefixAndChainPart()
    {
        var chain = CreateChain("x.Items.Where(o => o).Count");

        Assert.AreEqual(1, chain.FirstInvokedLinkIndex);
        Assert.IsTrue(chain.HasPrefix);
        Assert.IsTrue(chain.IsPrefixLink(0));
        Assert.IsFalse(chain.IsPrefixLink(1));
        Assert.IsFalse(chain.IsPrefixLink(2));
    }

    /// <summary>
    /// Verifies that a chain without an invoked link consists of its prefix only
    /// </summary>
    [TestMethod]
    public void CreateReportsCallLessChain()
    {
        var chain = CreateChain("order.Customer.Address");

        Assert.IsTrue(chain.IsCallLess);
        Assert.IsFalse(chain.HasPrefix);
        Assert.IsTrue(chain.IsPrefixLink(1));
    }

    /// <summary>
    /// Verifies the prefix exemption on both sides of its boundary
    /// </summary>
    /// <param name="source">The chain source</param>
    /// <param name="expected">Whether the chain has a prefix</param>
    [TestMethod]
    [DataRow("x.Items.Where()", true)]
    [DataRow("x.Items[0].ToString()", true)]
    [DataRow("a.P?[0].B()", true)]
    [DataRow("a?.B.C()", true)]
    [DataRow("a.B!.C()", true)]
    [DataRow("x.Where()", false)]
    [DataRow("x[0].ToString()", false)]
    [DataRow("Get()?.Bar()", false)]
    [DataRow("x.Items().Where()", false)]
    public void HasPrefixMatchesTheExemption(string source, bool expected)
    {
        Assert.AreEqual(expected, CreateChain(source).HasPrefix);
    }

    /// <summary>
    /// Verifies that the root ends directly in front of the first link's operator
    /// </summary>
    [TestMethod]
    public void RootLastTokenPrecedesFirstLinkOperator()
    {
        var chain = CreateChain("a[0]?.C()");

        Assert.AreEqual("]", chain.RootLastToken.Text);
        Assert.AreEqual("?", chain.FirstLink.OperatorToken.Text);
    }

    /// <summary>
    /// Verifies that an expression without a member access is no chain
    /// </summary>
    /// <param name="source">The expression source</param>
    [TestMethod]
    [DataRow("list[0]")]
    [DataRow("Foo(1)")]
    [DataRow("a?[0]")]
    public void CreateReturnsNullWithoutLink(string source)
    {
        Assert.IsNull(FluentChain.Create(SyntaxFactory.ParseExpression(source)));
    }

    /// <summary>
    /// Verifies that only the outermost node of a chain creates a chain
    /// </summary>
    [TestMethod]
    public void CreateReturnsNullForInnerChainNode()
    {
        var expression = SyntaxFactory.ParseExpression("a.B().C()");
        var inner = expression.DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax>().First();

        Assert.IsNull(FluentChain.Create(inner));
        Assert.AreSame(expression, FluentChain.GetOutermostChainNode(inner));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses an expression and creates its chain
    /// </summary>
    /// <param name="source">The expression source</param>
    /// <returns>The chain</returns>
    private static FluentChain CreateChain(string source)
    {
        var chain = FluentChain.Create(SyntaxFactory.ParseExpression(source));

        Assert.IsNotNull(chain);

        return chain;
    }

    #endregion // Methods
}