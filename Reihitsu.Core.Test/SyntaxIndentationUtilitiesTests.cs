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

    #endregion // Tests
}