using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for the LineBreaks/StructuralTransforms consolidation cleanup: removing
/// <c>BracePlacer.EnsureCloseBraceContinuation</c>, hoisting one shared <c>EnsureStatementsStartOnSeparateLines</c>
/// implementation, and replacing <c>LineBreakContainedBlockRewriter.NormalizeBlockBraces</c> with the shared
/// <c>BracePlacer.NormalizeContainedBlock</c>. The removed method was dead at ten of its eleven call sites — proven
/// by each site's close token being the rewriter's own detached node's last token, so <c>GetNextToken()</c> could
/// only return <see langword="default"/> — and those ten are covered below as pure stability assertions. The
/// eleventh site, <c>LineBreakInitializerRewriter.VisitListPattern</c>, was not dead: a list pattern's
/// <c>Designation</c> lies inside the visited node, unlike every other site, so removing the call is a genuine
/// (and, on inspection, desirable — it now matches <c>VisitRecursivePattern</c>'s sibling behavior) output change,
/// asserted explicitly rather than folded into the stability claim above
/// </summary>
[TestClass]
public class LineBreaksConsolidationStabilityTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a statement immediately following a block's closing brace on the same line still moves to
    /// its own line after <c>BracePlacer.EnsureCloseBraceContinuation</c> is removed — the separation is produced
    /// by <c>LineBreakBlockRewriter.EnsureStatementsStartOnSeparateLines</c> and <c>BlankLinePhase</c>, not by the
    /// removed method, which was unreachable at this call site (the close brace is always the block's last token
    /// in the detached node the rewriter holds)
    /// </summary>
    [TestMethod]
    public void StatementAfterBlockCloseBraceStillMovesToOwnLine()
    {
        const string input = """
                             public class C
                             {
                                 public void M()
                                 {
                                     if (true) { N(); } N();
                                 }

                                 public void N()
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                public class C
                                {
                                    public void M()
                                    {
                                        if (true)
                                        {
                                            N();
                                        }

                                        N();
                                    }

                                    public void N()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a recursive pattern with a designation keeps its close brace on the same line as the
    /// designation once the designation guard around the removed
    /// <c>BracePlacer.EnsureCloseBraceContinuation</c> call is deleted along with the call itself. The guard's
    /// only reachable purpose was suppressing this exact call, so removing both together must not force the
    /// designation onto its own line
    /// </summary>
    [TestMethod]
    public void RecursivePatternDesignationStaysOnCloseBraceLine()
    {
        const string input = """
                             public class C
                             {
                                 public void M(object value)
                                 {
                                     if (value is { } shape)
                                     {
                                         N(shape);
                                     }
                                 }

                                 public void N(object shape)
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line list pattern with a designation keeps its close bracket and the designation on
    /// one line — the one call site (<c>LineBreakInitializerRewriter.VisitListPattern</c>) where removing
    /// <c>BracePlacer.EnsureCloseBraceContinuation</c> is a genuine output change rather than dead-code removal.
    /// A list pattern's <c>Designation</c> lies inside the visited node, unlike every other removed call site, so
    /// the close bracket's <c>GetNextToken()</c> resolved to the designation itself and the removed method used to
    /// push it onto its own line. The new behavior — designation stays with the close bracket — matches
    /// <see cref="RecursivePatternDesignationStaysOnCloseBraceLine"/>'s sibling assertion for recursive patterns,
    /// which is the behavior this test locks in
    /// </summary>
    [TestMethod]
    public void ListPatternDesignationStaysOnCloseBracketLine()
    {
        const string input = """
                             public class C
                             {
                                 public void M(int[] value)
                                 {
                                     if (value is
                                     [
                                         1,
                                         2
                                     ] items)
                                     {
                                         N(items);
                                     }
                                 }

                                 public void N(int[] items)
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that two statements on one line inside a block are still split after
    /// <c>LineBreakBlockRewriter</c>'s two <c>EnsureStatementsStartOnSeparateLines</c> overloads are hoisted into
    /// one shared generic implementation
    /// </summary>
    [TestMethod]
    public void TwoStatementsOnOneLineInBlockAreSplit()
    {
        const string input = """
                             public class C
                             {
                                 public void M()
                                 {
                                     N(); N();
                                 }

                                 public void N()
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                public class C
                                {
                                    public void M()
                                    {
                                        N();
                                        N();
                                    }

                                    public void N()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>if</c> block whose open brace is glued to the condition normalizes identically to a
    /// <c>while</c> block in the same shape, now that both go through the shared
    /// <c>BracePlacer.NormalizeContainedBlock</c> instead of the deleted, near-duplicate
    /// <c>LineBreakContainedBlockRewriter.NormalizeBlockBraces</c>
    /// </summary>
    [TestMethod]
    public void IfAndWhileGluedOpenBraceNormalizeIdentically()
    {
        const string ifInput = """
                               public class C
                               {
                                   public void M(bool flag)
                                   {
                                       if (flag) {
                                           N();
                                       }
                                   }

                                   public void N()
                                   {
                                   }
                               }
                               """;
        const string ifExpected = """
                                  public class C
                                  {
                                      public void M(bool flag)
                                      {
                                          if (flag)
                                          {
                                              N();
                                          }
                                      }

                                      public void N()
                                      {
                                      }
                                  }
                                  """;

        const string whileInput = """
                                  public class C
                                  {
                                      public void M(bool flag)
                                      {
                                          while (flag) {
                                              N();
                                          }
                                      }

                                      public void N()
                                      {
                                      }
                                  }
                                  """;
        const string whileExpected = """
                                     public class C
                                     {
                                         public void M(bool flag)
                                         {
                                             while (flag)
                                             {
                                                 N();
                                             }
                                         }

                                         public void N()
                                         {
                                         }
                                     }
                                     """;

        AssertRuleResult(ifInput, ifExpected);
        AssertRuleResult(whileInput, whileExpected);
    }

    /// <summary>
    /// Verifies that an <c>if</c> block preceded by a <c>#region</c> gap normalizes identically to the same
    /// <c>while</c> shape (region formatting normalizes indentation, the surrounding blank lines, and the
    /// matching <c>#endregion</c> comment independently of the contained-block brace consolidation)
    /// </summary>
    [TestMethod]
    public void IfAndWhileRegionBeforeBlockNormalizeIdentically()
    {
        const string ifInput = """
                               public class C
                               {
                                   public void M(bool flag)
                                   {
                               #region Guard
                                       if (flag)
                                       {
                                           N();
                                       }
                               #endregion
                                   }

                                   public void N()
                                   {
                                   }
                               }
                               """;
        const string ifExpected = """
                                  public class C
                                  {
                                      public void M(bool flag)
                                      {
                                          #region Guard

                                          if (flag)
                                          {
                                              N();
                                          }

                                          #endregion // Guard
                                      }

                                      public void N()
                                      {
                                      }
                                  }
                                  """;

        const string whileInput = """
                                  public class C
                                  {
                                      public void M(bool flag)
                                      {
                                  #region Guard
                                          while (flag)
                                          {
                                              N();
                                          }
                                  #endregion
                                      }

                                      public void N()
                                      {
                                      }
                                  }
                                  """;
        const string whileExpected = """
                                     public class C
                                     {
                                         public void M(bool flag)
                                         {
                                             #region Guard

                                             while (flag)
                                             {
                                                 N();
                                             }

                                             #endregion // Guard
                                         }

                                         public void N()
                                         {
                                         }
                                     }
                                     """;

        AssertRuleResult(ifInput, ifExpected);
        AssertRuleResult(whileInput, whileExpected);
    }

    #endregion // Methods
}