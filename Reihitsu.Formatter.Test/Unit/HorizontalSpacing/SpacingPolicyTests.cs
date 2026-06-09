using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.HorizontalSpacing;

namespace Reihitsu.Formatter.Test.Unit.HorizontalSpacing;

/// <summary>
/// Tests for <see cref="SpacingPolicy"/>, the policy half of the horizontal spacing phase. These
/// pin the spacing decisions and the rule precedence independently of the trivia mechanics
/// </summary>
[TestClass]
public class SpacingPolicyTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the closing bracket of an attribute list is followed by a single space
    /// </summary>
    [TestMethod]
    public void AttributeListCloseBracketRequiresSingleSpace()
    {
        AssertDesiredSpaces("[System.Obsolete]void M(){}", SyntaxKind.CloseBracketToken, 1);
    }

    /// <summary>
    /// Verifies that an opening parenthesis is followed by no space
    /// </summary>
    [TestMethod]
    public void OpenParenthesisRequiresNoSpace()
    {
        AssertDesiredSpaces("class C{void M(){M( 1);}}", SyntaxKind.OpenParenToken, 0, secondOccurrence: true);
    }

    /// <summary>
    /// Verifies that a comma in an argument list is followed by a single space
    /// </summary>
    [TestMethod]
    public void CommaRequiresSingleSpace()
    {
        AssertDesiredSpaces("class C{void M(int a,int b){}}", SyntaxKind.CommaToken, 1);
    }

    /// <summary>
    /// Verifies that commas in a rank-only array specifier stay compact
    /// </summary>
    [TestMethod]
    public void RankOnlyArrayCommaRequiresNoSpace()
    {
        AssertDesiredSpaces("class C{int[,] _t;}", SyntaxKind.CommaToken, 0);
    }

    /// <summary>
    /// Verifies that a binary operator is surrounded by single spaces
    /// </summary>
    [TestMethod]
    public void BinaryOperatorRequiresSingleSpace()
    {
        AssertDesiredSpaces("class C{void M(){var x=1+2;}}", SyntaxKind.PlusToken, 1);
    }

    /// <summary>
    /// Verifies that a semicolon in a for-loop header is followed by a single space
    /// </summary>
    [TestMethod]
    public void ForLoopSemicolonRequiresSingleSpace()
    {
        AssertDesiredSpaces("class C{void M(){for(var i=0;i<1;i++){}}}", SyntaxKind.SemicolonToken, 1);
    }

    /// <summary>
    /// Verifies that a spaced keyword such as <c>if</c> is followed by a single space
    /// </summary>
    [TestMethod]
    public void SpacedKeywordRequiresSingleSpace()
    {
        AssertDesiredSpaces("class C{void M(){if(true){}}}", SyntaxKind.IfKeyword, 1);
    }

    /// <summary>
    /// Verifies that the <c>return</c> keyword sits directly before its semicolon
    /// </summary>
    [TestMethod]
    public void ReturnBeforeSemicolonRequiresNoSpace()
    {
        AssertDesiredSpaces("class C{void M(){return;}}", SyntaxKind.ReturnKeyword, 0);
    }

    /// <summary>
    /// Verifies that no rule applies between two ordinary identifiers, so the policy returns
    /// <see langword="null"/> and only the collapse-multiple-spaces fallback applies
    /// </summary>
    [TestMethod]
    public void NoRuleAppliesBetweenTypeAndIdentifier()
    {
        var policy = new SpacingPolicy();

        var tree = CSharpSyntaxTree.ParseText("class C{void M(){Foo bar=null;}}", cancellationToken: TestContext.CancellationToken);
        var left = tree.GetRoot(TestContext.CancellationToken)
                       .DescendantTokens()
                       .First(token => token.IsKind(SyntaxKind.IdentifierToken) && token.Text == "Foo");
        var right = left.GetNextToken();

        var result = policy.GetDesiredSpacesAfter(left, right);

        Assert.IsFalse(result.HasValue);
    }

    /// <summary>
    /// Verifies that the no-space rule wins over the keyword rule: a <c>new[]</c> implicit array
    /// keeps the bracket adjacent even though <c>new</c> is a spaced keyword
    /// </summary>
    [TestMethod]
    public void NoSpaceRuleTakesPrecedenceOverKeywordForImplicitArray()
    {
        AssertDesiredSpaces("class C{void M(){var a=new[]{1};}}", SyntaxKind.NewKeyword, 0);
    }

    /// <summary>
    /// Asserts that the spacing policy returns the expected desired space count after the first
    /// (or second) token of the given kind
    /// </summary>
    /// <param name="code">The C# code to parse</param>
    /// <param name="leftKind">The kind of the left token of the pair</param>
    /// <param name="expected">The expected desired space count</param>
    /// <param name="secondOccurrence">Whether to use the second occurrence of the token kind</param>
    private void AssertDesiredSpaces(string code, SyntaxKind leftKind, int expected, bool secondOccurrence = false)
    {
        var policy = new SpacingPolicy();
        var (left, right) = FindPair(code, leftKind, secondOccurrence);

        var result = policy.GetDesiredSpacesAfter(left, right);

        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Finds the first (or second) token of the given kind in the parsed code and returns it paired
    /// with the token that immediately follows it
    /// </summary>
    /// <param name="code">The C# code to parse</param>
    /// <param name="leftKind">The kind of the left token of the pair</param>
    /// <param name="secondOccurrence">Whether to use the second occurrence of the token kind</param>
    /// <returns>The matching token and the token that follows it</returns>
    private (SyntaxToken Left, SyntaxToken Right) FindPair(string code, SyntaxKind leftKind, bool secondOccurrence = false)
    {
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.CancellationToken);
        var matches = tree.GetRoot(TestContext.CancellationToken)
                          .DescendantTokens()
                          .Where(token => token.IsKind(leftKind))
                          .ToList();

        var left = secondOccurrence ? matches[1] : matches[0];

        return (left, left.GetNextToken());
    }

    #endregion // Methods
}