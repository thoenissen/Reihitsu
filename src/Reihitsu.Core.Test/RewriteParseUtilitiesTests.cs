using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="RewriteParseUtilities"/>. Each rewrite is built as syntax nodes, as the code
/// fixes that use the check build theirs: removing a <c>this.</c> or <c>base.</c> prefix, swapping the operands of a
/// comparison, or removing a comment between two tokens
/// </summary>
[TestClass]
public class RewriteParseUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that a root that is not rewritten at all parses as written
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsTrueForUnchangedRoot()
    {
        var root = Parse("""
                         class C
                         {
                             int M(int a, int b) => a + b;
                         }
                         """);

        Assert.IsTrue(RewriteParseUtilities.ParsesAsWritten(root, root, CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing a <c>this.</c> prefix whose name keeps its reading parses as written
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsTrueForRemovedPrefixThatKeepsItsReading()
    {
        var root = Parse("""
                         class C
                         {
                             int a;

                             bool M(int b) => this.a < b;
                         }
                         """);

        Assert.IsTrue(RewriteParseUtilities.ParsesAsWritten(root, RemovePrefix(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing a <c>this.</c> prefix behind a <c>&lt;</c> that a later <c>&gt;</c> turns into a type
    /// argument list does not parse as written: <c>F(a &lt; b, c &gt; (d))</c> reads as a call of the generic
    /// method <c>a&lt;b, c&gt;</c>
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsFalseForRemovedPrefixThatReadsAsGenericName()
    {
        var root = Parse("""
                         class C
                         {
                             int b;

                             void M(int a, int c, int d)
                             {
                                 F(a < this.b, c > (d));
                             }

                             void F(bool x, bool y)
                             {
                             }
                         }
                         """);

        Assert.IsFalse(RewriteParseUtilities.ParsesAsWritten(root, RemovePrefix(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing the same prefix parses as written when no token after the <c>&gt;</c> makes the
    /// comparisons a type argument list
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsTrueForRemovedPrefixBetweenComparisonsThatStayComparisons()
    {
        var root = Parse("""
                         class C
                         {
                             int b;

                             void M(int a, int c, int d)
                             {
                                 F(a < this.b, c > d);
                             }

                             void F(bool x, bool y)
                             {
                             }
                         }
                         """);

        Assert.IsTrue(RewriteParseUtilities.ParsesAsWritten(root, RemovePrefix(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that swapping the operands of a comparison so that a <c>&lt;</c> follows a name does not parse as
    /// written when a later <c>&gt;</c> turns them into a type argument list
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsFalseForSwappedComparisonThatReadsAsGenericName()
    {
        var root = Parse("""
                         class C
                         {
                             void M(int a, int b, int c, int d)
                             {
                                 F(b > a, c > (d));
                             }

                             void F(bool x, bool y)
                             {
                             }
                         }
                         """);
        var comparison = root.DescendantNodes().OfType<BinaryExpressionSyntax>().First();
        var swapped = SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression,
                                                     comparison.Right.WithTriviaFrom(comparison.Left),
                                                     SyntaxFactory.Token(SyntaxKind.LessThanToken).WithTriviaFrom(comparison.OperatorToken),
                                                     comparison.Left.WithTriviaFrom(comparison.Right));

        Assert.IsFalse(RewriteParseUtilities.ParsesAsWritten(root, root.ReplaceNode(comparison, swapped), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing a <c>base.</c> prefix from a parenthesized member that is invoked does not parse as
    /// written: <c>(A)(x)</c> reads as a cast of <c>(x)</c> to <c>A</c>
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsFalseForRemovedPrefixThatReadsAsCast()
    {
        var root = Parse("""
                         class C : B
                         {
                             object M(int x) => (base.A)(x);
                         }
                         """);

        Assert.IsFalse(RewriteParseUtilities.ParsesAsWritten(root, RemovePrefix(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing a <c>base.</c> prefix from a parenthesized name that is followed by a binary operator
    /// parses as written, since <c>(A) + x</c> does not read as a cast
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsTrueForRemovedPrefixInParenthesesFollowedByBinaryOperator()
    {
        var root = Parse("""
                         class C : B
                         {
                             int M(int x) => (base.A) + x;
                         }
                         """);

        Assert.IsTrue(RewriteParseUtilities.ParsesAsWritten(root, RemovePrefix(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing a comment between a keyword and an identifier that touch it on both sides does not parse
    /// as written: <c>return</c> and <c>a</c> lex as the one identifier <c>returna</c>
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsFalseForRemovedCommentThatMergesKeywordAndIdentifier()
    {
        var root = Parse("""
                         class C
                         {
                             int M(int a)
                             {
                                 return/**/a;
                             }
                         }
                         """);

        Assert.IsFalse(RewriteParseUtilities.ParsesAsWritten(root, RemoveComments(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing a comment between two operators that touch it on both sides does not parse as written:
    /// <c>+</c> and <c>+</c> lex as the one token <c>++</c>
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsFalseForRemovedCommentThatMergesOperators()
    {
        var root = Parse("""
                         class C
                         {
                             int M(int a, int b) => a+/**/+b;
                         }
                         """);

        Assert.IsFalse(RewriteParseUtilities.ParsesAsWritten(root, RemoveComments(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that removing a comment between two tokens that do not merge parses as written: <c>a+b</c>
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsTrueForRemovedCommentBetweenTokensThatDoNotMerge()
    {
        var root = Parse("""
                         class C
                         {
                             int M(int a, int b) => a+/**/b;
                         }
                         """);

        Assert.IsTrue(RewriteParseUtilities.ParsesAsWritten(root, RemoveComments(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that trivia is not compared: removing a comment with whitespace on both sides parses as written
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsTrueForRemovedCommentWithWhitespaceOnBothSides()
    {
        var root = Parse("""
                         class C
                         {
                             int M(int a)
                             {
                                 return /**/ a;
                             }
                         }
                         """);

        Assert.IsTrue(RewriteParseUtilities.ParsesAsWritten(root, RemoveComments(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a rewrite outside an <c>#if</c> block of a document with directives parses as written
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsTrueForRewriteOutsideConditionalDirectives()
    {
        var root = Parse("""
                         class C
                         {
                             int a;

                             int M()
                             {
                         #if DEBUG
                                 var debug = 1;
                         #else
                                 var release = 2;
                         #endif
                                 return this.a;
                             }
                         }
                         """);

        Assert.IsTrue(RewriteParseUtilities.ParsesAsWritten(root, RemovePrefix(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a rewrite inside the active branch of an <c>#if</c> parses as written when the branch is active
    /// only through a preprocessor symbol of the original parse options, so the check re-parses with those options
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsTrueForRewriteInBranchActiveThroughPreprocessorSymbol()
    {
        var root = Parse("""
                         class C
                         {
                             int a;

                             int M()
                             {
                         #if DEBUG
                                 return this.a;
                         #else
                                 return 0;
                         #endif
                             }
                         }
                         """,
                         "DEBUG");

        Assert.IsTrue(RewriteParseUtilities.ParsesAsWritten(root, RemovePrefix(root), CancellationToken.None));
    }

    /// <summary>
    /// Verifies that a rewrite inside an <c>#if</c> branch that reads as a generic name does not parse as written
    /// </summary>
    [TestMethod]
    public void ParsesAsWrittenReturnsFalseForRewriteInConditionalBranchThatReadsAsGenericName()
    {
        var root = Parse("""
                         class C
                         {
                             int b;

                             void M(int a, int c, int d)
                             {
                         #if DEBUG
                                 F(a < this.b, c > (d));
                         #endif
                             }

                             void F(bool x, bool y)
                             {
                             }
                         }
                         """,
                         "DEBUG");

        Assert.IsFalse(RewriteParseUtilities.ParsesAsWritten(root, RemovePrefix(root), CancellationToken.None));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Parses the source with the given preprocessor symbols
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="preprocessorSymbols">Preprocessor symbols</param>
    /// <returns>The root of the parsed tree</returns>
    private static SyntaxNode Parse(string source, params string[] preprocessorSymbols)
    {
        var options = new CSharpParseOptions(LanguageVersion.Latest, preprocessorSymbols: preprocessorSymbols);

        return CSharpSyntaxTree.ParseText(source, options).GetRoot();
    }

    /// <summary>
    /// Replaces the single <c>this.</c> or <c>base.</c> member access by its name, keeping the access's trivia
    /// </summary>
    /// <param name="root">Root</param>
    /// <returns>The rewritten root</returns>
    private static SyntaxNode RemovePrefix(SyntaxNode root)
    {
        var memberAccess = root.DescendantNodes()
                               .OfType<MemberAccessExpressionSyntax>()
                               .Single(node => node.Expression is ThisExpressionSyntax or BaseExpressionSyntax);

        return root.ReplaceNode(memberAccess, memberAccess.Name.WithTriviaFrom(memberAccess));
    }

    /// <summary>
    /// Removes every multi-line comment from the trailing trivia of the tokens, leaving the whitespace around it
    /// </summary>
    /// <param name="root">Root</param>
    /// <returns>The rewritten root</returns>
    private static SyntaxNode RemoveComments(SyntaxNode root)
    {
        var tokens = root.DescendantTokens().Where(token => token.TrailingTrivia.Any(SyntaxKind.MultiLineCommentTrivia));

        return root.ReplaceTokens(tokens,
                                  (token, _) => token.WithTrailingTrivia(token.TrailingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) == false)));
    }

    #endregion // Methods
}