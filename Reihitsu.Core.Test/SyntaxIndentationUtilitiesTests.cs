using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="SyntaxIndentationUtilities"/>
/// </summary>
[TestClass]
public class SyntaxIndentationUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies indentation follows syntax ownership when an explicit switch-section block starts after other code
    /// on the same physical line
    /// </summary>
    [TestMethod]
    public void ComputeStatementIndentLevelCountsInlineExplicitSwitchSectionBlock()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1: { Consume(); break; }
                                      }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var breakStatement = CoreSyntaxTestHelper.GetSingleNode<BreakStatementSyntax>(source);

        Assert.AreEqual(5, SyntaxIndentationUtilities.ComputeStatementIndentLevel(breakStatement));
    }

    /// <summary>
    /// Verifies indentation includes unbraced embedded ancestors between a statement and its containing block
    /// </summary>
    [TestMethod]
    public void ComputeStatementIndentLevelCountsUnbracedEmbeddedAncestor()
    {
        const string source = """
                              internal class C
                              {
                                  void M(bool first, bool second)
                                  {
                                      while (first)
                                          if (second) { Consume(); break; }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var breakStatement = CoreSyntaxTestHelper.GetSingleNode<BreakStatementSyntax>(source);

        Assert.AreEqual(4, SyntaxIndentationUtilities.ComputeStatementIndentLevel(breakStatement));
    }

    /// <summary>
    /// Verifies that a statement directly inside a block gains exactly one indentation level from its brace range
    /// </summary>
    [TestMethod]
    public void GetChildIndentLevelAddsOneLevelInsideABlock()
    {
        const string source = """
                              internal class C
                              {
                                  void M()
                                  {
                                      Consume();
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var block = CoreSyntaxTestHelper.GetSingleNode<ExpressionStatementSyntax>(source).Parent;

        Assert.AreEqual(1, SyntaxIndentationUtilities.GetChildIndentLevel(block, block.ChildNodes().First(), 0));
    }

    /// <summary>
    /// Verifies that a token outside the brace range keeps the parent's level. The opening brace itself sits at the
    /// range's own start, so it must not be counted as being inside it
    /// </summary>
    [TestMethod]
    public void GetChildIndentLevelKeepsParentLevelOutsideTheBraceRange()
    {
        const string source = """
                              internal class C
                              {
                                  void M()
                                  {
                                      Consume();
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var block = (BlockSyntax)CoreSyntaxTestHelper.GetSingleNode<ExpressionStatementSyntax>(source).Parent;

        Assert.AreEqual(0, SyntaxIndentationUtilities.GetChildIndentLevel(block, block.OpenBraceToken, 0));
        Assert.AreEqual(0, SyntaxIndentationUtilities.GetChildIndentLevel(block, block.CloseBraceToken, 0));
    }

    /// <summary>
    /// Verifies that an unbraced embedded statement gains one indentation level from its owner
    /// </summary>
    [TestMethod]
    public void GetChildIndentLevelAddsOneLevelForAnUnbracedEmbeddedStatement()
    {
        const string source = """
                              internal class C
                              {
                                  void M(bool flag)
                                  {
                                      if (flag)
                                          Consume();
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var ifStatement = CoreSyntaxTestHelper.GetSingleNode<IfStatementSyntax>(source);

        Assert.AreEqual(1, SyntaxIndentationUtilities.GetChildIndentLevel(ifStatement, ifStatement.Statement, 0));
    }

    /// <summary>
    /// Verifies that a braced embedded statement gains no extra level from its owner. The block owns its own brace
    /// range, so counting the owner too would indent the body twice
    /// </summary>
    [TestMethod]
    public void GetChildIndentLevelAddsNoLevelForABracedEmbeddedStatement()
    {
        const string source = """
                              internal class C
                              {
                                  void M(bool flag)
                                  {
                                      if (flag)
                                      {
                                          Consume();
                                      }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var ifStatement = CoreSyntaxTestHelper.GetSingleNode<IfStatementSyntax>(source);

        Assert.AreEqual(0, SyntaxIndentationUtilities.GetChildIndentLevel(ifStatement, ifStatement.Statement, 0));
    }

    /// <summary>
    /// Verifies that an <c>else if</c> does not cascade one indentation level per branch
    /// </summary>
    [TestMethod]
    public void GetChildIndentLevelAddsNoLevelForAnElseIfChain()
    {
        const string source = """
                              internal class C
                              {
                                  void M(bool first, bool second)
                                  {
                                      if (first)
                                          Consume();
                                      else if (second)
                                          Consume();
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var elseClause = CoreSyntaxTestHelper.ParseCompilationUnit(source).DescendantNodes().OfType<ElseClauseSyntax>().Single();

        Assert.AreEqual(0, SyntaxIndentationUtilities.GetChildIndentLevel(elseClause, elseClause.Statement, 0));
    }

    /// <summary>
    /// Verifies that a statement owned directly by a switch section gains one level for the section on top of the
    /// level the switch braces already contribute
    /// </summary>
    [TestMethod]
    public void GetChildIndentLevelAddsOneLevelForASwitchSectionStatement()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1:
                                              Consume();
                                              break;
                                      }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var section = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source);

        Assert.AreEqual(1, SyntaxIndentationUtilities.GetChildIndentLevel(section, section.Statements[0], 0));
    }

    /// <summary>
    /// Verifies that a scope whose braces are missing adds no indentation level. Malformed source must not shift the
    /// whole file, and the brace-range lookup is the single guard that prevents it
    /// </summary>
    [TestMethod]
    public void GetChildIndentLevelAddsNoLevelWhenABraceIsMissing()
    {
        const string source = "internal class C\n{\n    void M()\n    {\n        Consume();\n";

        var block = (BlockSyntax)CoreSyntaxTestHelper.ParseCompilationUnit(source)
                                                     .DescendantNodes()
                                                     .OfType<ExpressionStatementSyntax>()
                                                     .Single()
                                                     .Parent;

        Assert.IsTrue(block.CloseBraceToken.IsMissing);
        Assert.AreEqual(0, SyntaxIndentationUtilities.GetChildIndentLevel(block, block.Statements[0], 0));
        Assert.IsFalse(SyntaxIndentationUtilities.IsIndentingBraceScope(block));
    }

    /// <summary>
    /// Verifies that trivia inside a brace range inherits one indentation level while trivia outside it does not.
    /// This is the transition both the RH5204 analyzer and the formatter's layout pass use to place region directives
    /// </summary>
    [TestMethod]
    public void GetTriviaIndentLevelFollowsTheBraceRange()
    {
        const string source = """
                              #region Outer

                              internal class C
                              {
                                  #region Members

                                  internal bool Value => true;

                                  #endregion // Members
                              }

                              #endregion // Outer
                              """;

        var declaration = CoreSyntaxTestHelper.GetSingleTypeDeclaration(source);
        var property = CoreSyntaxTestHelper.GetSingleMember<PropertyDeclarationSyntax>(source);
        var innerDirective = property.GetLeadingTrivia().First(SyntaxTriviaUtilities.IsRegionDirective);
        var outerDirective = declaration.GetLeadingTrivia().First(SyntaxTriviaUtilities.IsRegionDirective);

        Assert.AreEqual(1, SyntaxIndentationUtilities.GetTriviaIndentLevel(declaration, innerDirective, 0));
        Assert.AreEqual(0, SyntaxIndentationUtilities.GetTriviaIndentLevel(declaration, outerDirective, 0));
    }

    /// <summary>
    /// Verifies which node kinds own an indenting brace range. Initializers are deliberately absent: their columns are
    /// owned by the formatter's alignment contributors, not by block indentation
    /// </summary>
    [TestMethod]
    public void IsIndentingBraceScopeRecognizesBlockScopesOnly()
    {
        const string source = """
                              internal class C
                              {
                                  internal int[] Values { get; } = new int[]
                                                                   {
                                                                       1
                                                                   };
                              }
                              """;

        var root = CoreSyntaxTestHelper.ParseCompilationUnit(source);

        Assert.IsTrue(SyntaxIndentationUtilities.IsIndentingBraceScope(CoreSyntaxTestHelper.GetSingleTypeDeclaration(source)));
        Assert.IsTrue(SyntaxIndentationUtilities.IsIndentingBraceScope(root.DescendantNodes().OfType<AccessorListSyntax>().Single()));
        Assert.IsFalse(SyntaxIndentationUtilities.IsIndentingBraceScope(root.DescendantNodes().OfType<InitializerExpressionSyntax>().Single()));
    }

    #endregion // Tests
}