using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    /// <summary>
    /// Verifies that the anchor is the first invoked link in front of the first link that starts a line, also when later
    /// links share its line, or the first link when the first link itself starts a line
    /// </summary>
    /// <param name="source">The expression source</param>
    /// <param name="expectedName">The member name of the expected anchor link</param>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    [DataRow("a.B().C()\n.D()", "B")]
    [DataRow("a.Prop.Foo()\n.Bar()", "Foo")]
    [DataRow("a?.B.C()\n.D()", "C")]
    [DataRow("items.Where(x => x.A).Select(x => x.B)\n.ToList()", "Where")]
    [DataRow("x.Next.Self().Next\n.Self()", "Self")]
    [DataRow("a\n.B()\n.C()", "B")]
    [DataRow("a\n.Prop.Foo()", "Prop")]
    public void GetAnchorLinkReturnsFirstInvokedLinkBeforeFirstWrappedLink(string source, string expectedName)
    {
        Assert.AreEqual(expectedName, CreateChain(source).GetAnchorLink().Name.Identifier.Text);
    }

    /// <summary>
    /// Verifies that, when no invoked link comes before the first link that starts a line, the anchor is the last link in
    /// front of that link: behind a prefix of property reads, in a chain without a call, behind conditional and
    /// null-forgiving links, behind attached parts and a root that spans lines, and behind a directive; a link behind the
    /// first wrapped link never becomes the anchor, one link in front of it stays the anchor, and of three or more links in
    /// front of it the last one is the anchor, not the second. A line break inside the text of a delimited comment does not
    /// make the link behind its <c>*/</c> start a line, so that link is the anchor when the comment starts behind code, while
    /// a link behind a comment that starts its own line starts a line
    /// </summary>
    /// <param name="source">The expression source</param>
    /// <param name="expectedIndex">The index of the expected anchor link among the chain's links</param>
    [TestMethod]
    [Requirement("SHARED-CHAIN-007")]
    [DataRow("x.Items.Count\n.ToString()", 1)]
    [DataRow("x.Items.Length\n.ToString()", 1)]
    [DataRow("order.Customer.Address\n.City", 1)]
    [DataRow("order.Customer.Address\n.City.Street\n.Name", 1)]
    [DataRow("this.Settings.Items\n.Where(x => x.A)", 1)]
    [DataRow("System.String.Empty\n.Trim()", 1)]
    [DataRow("x.Next.Next\n.Next\n.Self()", 1)]
    [DataRow("x.Next?.Next\n?.Name\n.Trim()", 1)]
    [DataRow("x?.Next?.Next\n?.Self()", 1)]
    [DataRow("x!.Next.Next\n.Self()", 1)]
    [DataRow("a.B?.B\n.C", 1)]
    [DataRow("x.Arr[0].Next\n.Self()", 1)]
    [DataRow("get(1,\n2\n).Next.Next\n.Self()", 1)]
    [DataRow("new C\n{\n}.Next.Next\n.Self()", 1)]
    [DataRow("x.Next.Next\n#if DEBUG\n.Self()\n#endif\n.Self()", 1)]
    [DataRow("x.Next.Next.Next\n.Self()", 2)]
    [DataRow("x.Next.Next.Next.Next\n.Next", 3)]
    [DataRow("order.Customer.Address.Street\n.Name", 2)]
    [DataRow("x.Next?.Next.Next\n.Self()", 2)]
    [DataRow("a.B.D?.D.D\n.E", 3)]
    [DataRow("x.A.B.C\n.D.E\n.F", 2)]
    [DataRow("x.Next.Next.Next /* c\n*/ .Next\n.Self()", 3)]
    [DataRow("x.Next.Next\n.Next.Next\n.Self()", 1)]
    [DataRow("x.Items\n.Count.Length\n.ToString()", 0)]
    [DataRow("x.Next\n.Next.Next\n.Self()", 0)]
    [DataRow("x?[0, 1].Next\n.Self()", 0)]
    [DataRow("a\n.Next.Next\n.Self()", 0)]
    [DataRow("x.Next /* c\n*/ .Next\n.Self()", 1)]
    [DataRow("x.Next /* c\n*/ .Next\n.Next\n.Self()", 1)]
    [DataRow("x.Next\n/* c\n*/ .Next\n.Self()", 0)]
    [DataRow("x.Next\n\n/* c\n*/ .Next\n.Self()", 0)]
    public void GetAnchorLinkReturnsLastLinkInFrontOfFirstWrappedLinkWithoutInvokedLink(string source, int expectedIndex)
    {
        var chain = CreateChain(source);

        Assert.AreSame(chain.Links[expectedIndex], chain.GetAnchorLink());
    }

    /// <summary>
    /// Verifies that a chain whose first link is kept on its own line by a comment or a directive aligns to its root
    /// </summary>
    /// <param name="source">The expression source</param>
    [TestMethod]
    [DataRow("a // c\n.B()\n.C()")]
    [DataRow("a\n#if DEBUG\n#endif\n.B()")]
    public void GetAnchorLinkReturnsNullWhenFirstLinkIsBlocked(string source)
    {
        Assert.IsNull(CreateChain(source).GetAnchorLink());
    }

    /// <summary>
    /// Verifies that a comment in front of a later link does not make the chain align to its root
    /// </summary>
    [TestMethod]
    public void GetAnchorLinkIgnoresCommentInFrontOfLaterLink()
    {
        Assert.AreEqual("B", CreateChain("a.B() // c\n.C()").GetAnchorLink().Name.Identifier.Text);
    }

    /// <summary>
    /// Verifies that the left side of a null-conditional assignment continues the chain of its conditional access, so
    /// its member bindings and member accesses are links of that chain
    /// </summary>
    [TestMethod]
    public void CreateCollectsLinksOfNullConditionalAssignmentTarget()
    {
        var chain = CreateChain("a.B()?.C().D = 5");

        Assert.AreSequenceEqual(new[] { "B", "C", "D" }, chain.Links.Select(link => link.Name.Identifier.ValueText).ToArray());
        Assert.AreSequenceEqual(new[] { ".", "?.", "." }, chain.Links.Select(link => string.Concat(link.OperatorTokens.Select(token => token.Text))).ToArray());
    }

    /// <summary>
    /// Verifies that the left side of a null-conditional assignment is not the outermost node of a chain of its own
    /// </summary>
    [TestMethod]
    public void LeftSideOfNullConditionalAssignmentIsNoOutermostChainNode()
    {
        var expression = (ConditionalAccessExpressionSyntax)SyntaxFactory.ParseExpression("a?.B.C += 1");
        var left = ((AssignmentExpressionSyntax)expression.WhenNotNull).Left;

        Assert.IsFalse(FluentChain.IsOutermostChainNode(left));
        Assert.IsNull(FluentChain.Create(left));
        Assert.AreSame(expression, FluentChain.GetOutermostChainNode(left));
    }

    /// <summary>
    /// Verifies that the right side of a null-conditional assignment is no part of the chain
    /// </summary>
    [TestMethod]
    public void RightSideOfNullConditionalAssignmentIsNoPartOfTheChain()
    {
        var chain = CreateChain("a?.B = x.Y()");

        Assert.AreSequenceEqual(new[] { "B" }, chain.Links.Select(link => link.Name.Identifier.ValueText).ToArray());
        Assert.IsEmpty(chain.AttachedParts);
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