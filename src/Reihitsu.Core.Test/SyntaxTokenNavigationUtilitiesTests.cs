using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="SyntaxTokenNavigationUtilities"/>: for every token of a tree, the tokens inside
/// directives, disabled code, and documentation comments included, the helper must return exactly what Roslyn's default
/// <see cref="SyntaxToken.GetPreviousToken"/> and <see cref="SyntaxToken.GetNextToken"/> return
/// </summary>
[TestClass]
public class SyntaxTokenNavigationUtilitiesTests
{
    #region Methods

    /// <summary>
    /// Asserts that the helper agrees with Roslyn in both directions on every token below the root, the tokens of structured
    /// trivia included
    /// </summary>
    /// <param name="root">The root whose tokens are checked</param>
    private static void AssertAgreesWithRoslyn(SyntaxNode root)
    {
        var tokens = root.DescendantTokens(descendIntoTrivia: true).ToList();

        Assert.IsNotEmpty(tokens);

        foreach (var token in tokens)
        {
            var description = $"'{token.Text}' ({token.Kind()}) at {token.SpanStart}";

            Assert.AreEqual(token.GetPreviousToken(), SyntaxTokenNavigationUtilities.GetPreviousToken(token), $"The token in front of {description} differs from Roslyn's.");
            Assert.AreEqual(token.GetNextToken(), SyntaxTokenNavigationUtilities.GetNextToken(token), $"The token behind {description} differs from Roslyn's.");
        }
    }

    /// <summary>
    /// Parses the source into a compilation unit and asserts that the helper agrees with Roslyn on every token
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The parsed compilation unit</returns>
    private static CompilationUnitSyntax AssertAgreesWithRoslyn(string source)
    {
        var root = CoreSyntaxTestHelper.ParseCompilationUnit(source);

        AssertAgreesWithRoslyn(root);

        return root;
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that the helper agrees with Roslyn on every token of parents with many children: an enum, an array initializer,
    /// an argument list, a class with many fields, and a block with many statements
    /// </summary>
    [TestMethod]
    public void AgreesWithRoslynOnLongLists()
    {
        var members = string.Join(",\n", Enumerable.Range(0, 100).Select(index => $"    V{index}"));
        var numbers = string.Join(", ", Enumerable.Range(0, 100));
        var fields = string.Concat(Enumerable.Range(0, 100).Select(index => $"    private int _field{index};\n"));
        var statements = string.Concat(Enumerable.Range(0, 100).Select(_ => "        M(1);\n"));

        AssertAgreesWithRoslyn($"internal enum E\n{{\n{members}\n}}\n\ninternal class C\n{{\n{fields}\n    internal void M(params int[] values)\n    {{\n        var array = new[] {{ {numbers} }};\n        M({numbers});\n{statements}    }}\n}}\n");
    }

    /// <summary>
    /// Verifies that the helper agrees with Roslyn on a long list with CRLF line endings
    /// </summary>
    [TestMethod]
    public void AgreesWithRoslynUnderCrlfLineEndings()
    {
        var members = string.Join(",\r\n", Enumerable.Range(0, 100).Select(index => $"    // Member {index}\r\n    V{index}"));

        AssertAgreesWithRoslyn($"internal enum E\r\n{{\r\n{members}\r\n}}\r\n");
    }

    /// <summary>
    /// Verifies that the helper skips zero-width tokens, such as omitted array sizes and omitted type arguments, as Roslyn does
    /// </summary>
    [TestMethod]
    public void SkipsZeroWidthTokensAsRoslynDoes()
    {
        var root = AssertAgreesWithRoslyn("""
                                          internal class C
                                          {
                                              private object _array = new int[,] { { 1 } };
                                              private object _type = typeof(System.Collections.Generic.Dictionary<,>);
                                          }
                                          """);
        var rank = root.DescendantNodes().OfType<ArrayRankSpecifierSyntax>().First();

        Assert.IsTrue(rank.Sizes.All(size => size.IsKind(SyntaxKind.OmittedArraySizeExpression)));
        Assert.AreEqual(2, rank.Rank);
        Assert.AreEqual(rank.Rank - 1, rank.Sizes.SeparatorCount);
        Assert.AreEqual(rank.Sizes.GetSeparator(0), SyntaxTokenNavigationUtilities.GetNextToken(rank.OpenBracketToken));
        Assert.AreEqual(rank.Sizes.GetSeparator(0), SyntaxTokenNavigationUtilities.GetPreviousToken(rank.CloseBracketToken));
    }

    /// <summary>
    /// Verifies that the helper skips the end of the file, a zero-width token, also when a final comment gives it trivia, and
    /// finds no token behind the last token of the text and none in front of the first, as Roslyn does
    /// </summary>
    [TestMethod]
    public void FindsNoTokenBeyondTheEndsOfTheText()
    {
        var root = AssertAgreesWithRoslyn("""
                                          // Header
                                          internal class C
                                          {
                                          }

                                          // Final comment
                                          """);
        var tokens = root.DescendantTokens().ToList();

        Assert.IsTrue(root.EndOfFileToken.HasLeadingTrivia);
        Assert.AreEqual(default, SyntaxTokenNavigationUtilities.GetNextToken(tokens[^2]));
        Assert.AreEqual(tokens[^2], SyntaxTokenNavigationUtilities.GetPreviousToken(root.EndOfFileToken));
        Assert.AreEqual(default, SyntaxTokenNavigationUtilities.GetPreviousToken(tokens[0]));
    }

    /// <summary>
    /// Verifies that the helper agrees with Roslyn on the tokens of conditional directives, disabled code, regions, and pragmas,
    /// and steps over a directive from the code around it, as Roslyn does
    /// </summary>
    [TestMethod]
    public void AgreesWithRoslynAroundDirectivesAndDisabledCode()
    {
        AssertAgreesWithRoslyn("""
                               #define FEATURE
                               using System;

                               internal class C
                               {
                                   #region Fields

                               #pragma warning disable CS0169
                                   private int _first;
                               #pragma warning restore CS0169

                                   #endregion // Fields

                                   internal int[] M()
                                   {
                                       return new[]
                                              {
                                                  1,
                               #if FEATURE
                                                  2,
                               #elif OTHER
                                                  3,
                               #else
                                                  4,
                               #endif
                                                  5
                                              };
                                   }
                               }
                               """);
    }

    /// <summary>
    /// Verifies that the helper agrees with Roslyn on the tokens of single-line and delimited documentation comments and steps
    /// over a documentation comment from the code around it, as Roslyn does
    /// </summary>
    [TestMethod]
    public void AgreesWithRoslynAroundDocumentationComments()
    {
        AssertAgreesWithRoslyn("""
                               /// <summary>
                               /// A class with a <see cref="M(int)"/> method
                               /// </summary>
                               internal class C
                               {
                                   /** <summary>A method</summary>
                                       <param name="value">A value</param> */
                                   internal void M(int value)
                                   {
                                   }
                               }
                               """);
    }

    /// <summary>
    /// Verifies that the helper skips missing tokens and skipped tokens of a text with syntax errors, as Roslyn does
    /// </summary>
    [TestMethod]
    public void AgreesWithRoslynOnMissingAndSkippedTokens()
    {
        var root = AssertAgreesWithRoslyn("""
                                          internal class C
                                          {
                                              internal void M()
                                              {
                                                  int value = ;
                                                  M(1 2)
                                              }
                                              ) ]
                                          }
                                          """);

        Assert.Contains(token => token.IsMissing, root.DescendantTokens());
        Assert.Contains(trivia => trivia.IsKind(SyntaxKind.SkippedTokensTrivia), root.DescendantTrivia());
    }

    /// <summary>
    /// Verifies that the helper agrees with Roslyn on an empty text and on a text that holds only a comment
    /// </summary>
    [TestMethod]
    public void AgreesWithRoslynOnEmptyAndCommentOnlyText()
    {
        AssertAgreesWithRoslyn(string.Empty);
        AssertAgreesWithRoslyn("// Only a comment\n");
        AssertAgreesWithRoslyn("/// <summary>Only a documentation comment</summary>\n");
    }

    /// <summary>
    /// Verifies that the helper agrees with Roslyn on a node without a tree around it, whose first and last tokens have no
    /// neighbour, and on a node replaced inside a tree
    /// </summary>
    [TestMethod]
    public void AgreesWithRoslynOnDetachedAndRewrittenNodes()
    {
        var expression = SyntaxFactory.ParseExpression("a ? b : c ? d : (e ? f : g)");

        AssertAgreesWithRoslyn(expression);

        Assert.AreEqual(default, SyntaxTokenNavigationUtilities.GetPreviousToken(expression.GetFirstToken()));
        Assert.AreEqual(default, SyntaxTokenNavigationUtilities.GetNextToken(expression.GetLastToken()));

        var root = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                             internal class C
                                                             {
                                                                 internal int M(bool a)
                                                                 {
                                                                     return a ? 1 : 2;
                                                                 }
                                                             }
                                                             """);
        var conditional = root.DescendantNodes().OfType<ConditionalExpressionSyntax>().Single();
        var rewritten = root.ReplaceNode(conditional, expression.WithTriviaFrom(conditional));

        AssertAgreesWithRoslyn(rewritten);
        AssertAgreesWithRoslyn(rewritten.DescendantNodes().OfType<ReturnStatementSyntax>().Single());
    }

    #endregion // Tests
}