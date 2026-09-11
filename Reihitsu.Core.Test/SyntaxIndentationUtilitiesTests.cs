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
    /// Verifies that a switch section's own label gains no indentation level, only its statements do. This is the
    /// boundary a switch-section-as-interval rewrite must preserve: routing the section through the same
    /// <c>&gt;</c> comparison used for brace scopes rather than the inclusive <c>&gt;=</c> the statements interval
    /// needs would silently misclassify the section's own start (issue #748)
    /// </summary>
    [TestMethod]
    public void GetChildIndentLevelAddsNoLevelForASwitchSectionLabel()
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

        Assert.AreEqual(0, SyntaxIndentationUtilities.GetChildIndentLevel(section, section.Labels[0], 0));
        Assert.AreEqual(1, SyntaxIndentationUtilities.GetChildIndentLevel(section, section.Statements[0], 0));
    }

    /// <summary>
    /// Verifies that a switch section with no statements of its own adds no level for its label and does not throw.
    /// An interval built from <c>Statements[0]</c> must guard this empty case explicitly rather than indexing into
    /// an empty list (issue #748)
    /// </summary>
    [TestMethod]
    public void GetChildIndentLevelHandlesEmptySwitchSectionLabelWithoutThrowing()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1:
                                      }
                                  }
                              }
                              """;

        var section = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source);

        Assert.IsEmpty(section.Statements);
        Assert.AreEqual(0, SyntaxIndentationUtilities.GetChildIndentLevel(section, section.Labels[0], 0));
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
        Assert.IsFalse(SyntaxIndentationUtilities.IsIndentingScope(block));
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
    /// Verifies which node kinds own an indenting scope. Initializers are deliberately absent: their columns are
    /// owned by the formatter's alignment contributors, not by block indentation
    /// </summary>
    [TestMethod]
    public void IsIndentingScopeRecognizesBraceScopesOnly()
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

        Assert.IsTrue(SyntaxIndentationUtilities.IsIndentingScope(CoreSyntaxTestHelper.GetSingleTypeDeclaration(source)));
        Assert.IsTrue(SyntaxIndentationUtilities.IsIndentingScope(root.DescendantNodes().OfType<AccessorListSyntax>().Single()));
        Assert.IsFalse(SyntaxIndentationUtilities.IsIndentingScope(root.DescendantNodes().OfType<InitializerExpressionSyntax>().Single()));
    }

    /// <summary>
    /// Verifies that a non-empty switch section owns an indenting scope, expressed as the interval spanning its
    /// own statements rather than as a brace pair, since it owns no braces of its own. RH5204's own output is
    /// unaffected: no brace token's direct parent is ever a switch section (issue #748)
    /// </summary>
    [TestMethod]
    public void IsIndentingScopeRecognizesNonEmptySwitchSection()
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

        Assert.IsTrue(SyntaxIndentationUtilities.IsIndentingScope(section));
    }

    /// <summary>
    /// Verifies that an empty switch section owns no indenting scope
    /// </summary>
    [TestMethod]
    public void IsIndentingScopeReturnsFalseForEmptySwitchSection()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1:
                                      }
                                  }
                              }
                              """;

        var section = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source);

        Assert.IsFalse(SyntaxIndentationUtilities.IsIndentingScope(section));
    }

    /// <summary>
    /// Verifies that a position at the section's own start, or anywhere inside its label region, is recognized
    /// (issue #786)
    /// </summary>
    [TestMethod]
    public void IsWithinSwitchSectionLabelRegionRecognizesPositionsInsideTheLabel()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1: Consume();
                                              break;
                                      }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var section = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source);
        var label = section.Labels[0];

        Assert.IsTrue(SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion(section, section.SpanStart));
        Assert.IsTrue(SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion(section, label.Span.End - 1));
    }

    /// <summary>
    /// Verifies that the label's own end position is excluded, since the section's statements start there and sit
    /// one indentation level deeper than the label (issue #786)
    /// </summary>
    [TestMethod]
    public void IsWithinSwitchSectionLabelRegionExcludesTheLabelsOwnEndBoundary()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1: Consume();
                                              break;
                                      }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var section = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source);
        var label = section.Labels[0];

        Assert.IsFalse(SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion(section, label.Span.End));
        Assert.IsFalse(SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion(section, section.Statements[0].SpanStart));
    }

    /// <summary>
    /// Verifies that a position at an earlier sibling section's label - such as a preceding section's label
    /// sharing the same physical line as this section's own label - is recognized: sibling sections of one
    /// <c>switch</c> statement share the same nesting depth, so their labels share the same one-level
    /// relationship to this section's statements (issue #786)
    /// </summary>
    [TestMethod]
    public void IsWithinSwitchSectionLabelRegionRecognizesAnEarlierSiblingSectionsLabel()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1: break;
                                          case 2: Consume();
                                              break;
                                      }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var sections = CoreSyntaxTestHelper.ParseCompilationUnit(source).DescendantNodes().OfType<SwitchSectionSyntax>().ToArray();
        var firstSection = sections[0];
        var secondSection = sections[1];

        Assert.IsTrue(SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion(secondSection, firstSection.SpanStart));
    }

    /// <summary>
    /// Verifies that a position at an earlier sibling section's own statement - as opposed to that section's
    /// label - is excluded: only a label itself, never a statement sharing the same textual stretch between two
    /// labels, carries the one-level relationship (issue #786)
    /// </summary>
    [TestMethod]
    public void IsWithinSwitchSectionLabelRegionExcludesAnEarlierSiblingSectionsStatement()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1:
                                              Consume(); case 2: Consume();
                                                  break;
                                      }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var sections = CoreSyntaxTestHelper.ParseCompilationUnit(source).DescendantNodes().OfType<SwitchSectionSyntax>().ToArray();
        var firstSection = sections[0];
        var secondSection = sections[1];

        Assert.IsFalse(SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion(secondSection, firstSection.Statements[0].SpanStart));
    }

    /// <summary>
    /// Verifies that a position before the enclosing switch statement's own opening brace - such as the
    /// <c>switch</c> keyword itself - is excluded, since it owns no label relationship to any section's
    /// statements (issue #786)
    /// </summary>
    [TestMethod]
    public void IsWithinSwitchSectionLabelRegionExcludesPositionsBeforeTheEnclosingSwitchStatement()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1: Consume();
                                              break;
                                      }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var switchStatement = CoreSyntaxTestHelper.GetSingleNode<SwitchStatementSyntax>(source);
        var section = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source);

        Assert.IsFalse(SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion(section, switchStatement.SwitchKeyword.SpanStart));
    }

    /// <summary>
    /// Verifies that a position at the last of several labels belonging to the same section is recognized, and
    /// not only a position at the first: a section's statements sit one level below whichever label immediately
    /// precedes them on a shared line (issue #786)
    /// </summary>
    [TestMethod]
    public void IsWithinSwitchSectionLabelRegionRecognizesTheLastOfSeveralLabels()
    {
        const string source = """
                              internal class C
                              {
                                  void M(int value)
                                  {
                                      switch (value)
                                      {
                                          case 1:
                                          case 2: Consume();
                                              break;
                                      }
                                  }

                                  void Consume()
                                  {
                                  }
                              }
                              """;

        var section = CoreSyntaxTestHelper.GetSingleNode<SwitchSectionSyntax>(source);
        var lastLabel = section.Labels[section.Labels.Count - 1];

        Assert.AreEqual(2, section.Labels.Count);
        Assert.IsTrue(SyntaxIndentationUtilities.IsWithinSwitchSectionLabelRegion(section, lastLabel.SpanStart));
    }

    #endregion // Tests
}