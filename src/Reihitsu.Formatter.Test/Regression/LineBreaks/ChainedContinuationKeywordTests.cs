using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// A statement keyword that chains directly from a preceding block's closing brace
/// (<c>else</c>, <c>catch</c>, <c>finally</c>, or a <c>do</c> statement's <c>while</c>) must
/// start its own line, even when the braces around that chain were already present in the
/// source rather than synthesized by <c>ControlFlowBraceTransform</c>
/// </summary>
[TestClass]
public class ChainedContinuationKeywordTests : FormatterTestsBase
{
    #region Fields

    /// <summary>
    /// Preprocessor symbols that keep a <c>DEBUG</c>-guarded directive branch active
    /// </summary>
    private static readonly string[] _preprocessorSymbols = ["DEBUG"];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies that every <c>else</c>/<c>else if</c> keyword in a fully hand-braced <c>else if</c>
    /// chain, glued to the preceding closing brace, moves onto its own line
    /// </summary>
    [TestMethod]
    public void GluedElseIfChainMovesEveryElseOntoItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     if (x == 1)
                                     {
                                         Foo();
                                     } else if (x == 2)
                                     {
                                         Bar();
                                     } else
                                     {
                                         Baz();
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        if (x == 1)
                                        {
                                            Foo();
                                        }
                                        else if (x == 2)
                                        {
                                            Bar();
                                        }
                                        else
                                        {
                                            Baz();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a fully single-line but already-braced <c>if</c>/<c>else</c> statement is split
    /// into Allman form with <c>else</c> on its own line
    /// </summary>
    [TestMethod]
    public void SingleLineAlreadyBracedIfElseSplitsElseOntoItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x) { Foo(); } else { Bar(); }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool x)
                                    {
                                        if (x)
                                        {
                                            Foo();
                                        }
                                        else
                                        {
                                            Bar();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every <c>catch</c> keyword and a trailing <c>finally</c> keyword, each glued to
    /// the closing brace of the block it chains from, move onto their own line
    /// </summary>
    [TestMethod]
    public void GluedCatchClausesAndFinallyMoveOntoTheirOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     try
                                     {
                                         Foo();
                                     } catch (ArgumentException)
                                     {
                                         Bar();
                                     } catch (Exception ex) when (ex != null)
                                     {
                                         Baz();
                                     } finally
                                     {
                                         Qux();
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        try
                                        {
                                            Foo();
                                        }
                                        catch (ArgumentException)
                                        {
                                            Bar();
                                        }
                                        catch (Exception ex) when (ex != null)
                                        {
                                            Baz();
                                        }
                                        finally
                                        {
                                            Qux();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>finally</c> keyword directly following a <c>try</c> block (no <c>catch</c>
    /// clauses), glued to the closing brace, moves onto its own line
    /// </summary>
    [TestMethod]
    public void GluedFinallyAfterTryBlockWithNoCatchesMovesOntoItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     try
                                     {
                                         Foo();
                                     } finally
                                     {
                                         Bar();
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        try
                                        {
                                            Foo();
                                        }
                                        finally
                                        {
                                            Bar();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>do</c> statement's <c>while</c> keyword, glued to the closing brace of the
    /// block body, moves onto its own line
    /// </summary>
    [TestMethod]
    public void GluedDoWhileMovesWhileOntoItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     do
                                     {
                                         Foo();
                                     } while (x);
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool x)
                                    {
                                        do
                                        {
                                            Foo();
                                        }
                                        while (x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>do</c> statement whose body is a single unbraced statement is left
    /// completely unchanged, including its glued <c>while</c> keyword — splitting only <c>while</c>
    /// would half-format a statement the formatter otherwise leaves alone
    /// </summary>
    [TestMethod]
    public void UnbracedDoWhileBodyStaysUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     do Foo(); while (x);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a hand-braced <c>if</c>/<c>else</c> nested inside the then-branch of an outer
    /// hand-braced <c>if</c>/<c>else</c>, with every keyword glued, has both levels' <c>else</c>
    /// keywords moved onto their own line
    /// </summary>
    [TestMethod]
    public void NestedGluedIfElseMovesBothLevelsElseOntoItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a, bool b)
                                 {
                                     if (a)
                                     {
                                         if (b)
                                         {
                                             Foo();
                                         } else
                                         {
                                             Bar();
                                         }
                                     } else
                                     {
                                         Baz();
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool a, bool b)
                                    {
                                        if (a)
                                        {
                                            if (b)
                                            {
                                                Foo();
                                            }
                                            else
                                            {
                                                Bar();
                                            }
                                        }
                                        else
                                        {
                                            Baz();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment glued to the closing brace on the same line as a glued <c>else</c>
    /// stays on the closing brace's line while <c>else</c> still moves onto its own line
    /// </summary>
    [TestMethod]
    public void GluedElseWithBlockCommentOnBraceLineKeepsCommentOnBraceLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x)
                                     {
                                         Foo();
                                     } /* note */ else
                                     {
                                         Bar();
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool x)
                                    {
                                        if (x)
                                        {
                                            Foo();
                                        } /* note */
                                        else
                                        {
                                            Bar();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a trailing line comment on a glued <c>else</c> keyword's own line travels with
    /// <c>else</c> onto its new line rather than being dropped or left behind
    /// </summary>
    [TestMethod]
    public void GluedElseWithTrailingLineCommentKeepsCommentWithElse()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x)
                                     {
                                         Foo();
                                     } else // note
                                     {
                                         Bar();
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool x)
                                    {
                                        if (x)
                                        {
                                            Foo();
                                        }
                                        else // note
                                        {
                                            Bar();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>else</c> keyword that already starts its own line is left byte-identical
    /// </summary>
    [TestMethod]
    public void ElseAlreadyOnItsOwnLineStaysUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x)
                                     {
                                         Foo();
                                     }
                                     else
                                     {
                                         Bar();
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>catch</c> keyword that already starts its own line is left byte-identical
    /// </summary>
    [TestMethod]
    public void CatchAlreadyOnItsOwnLineStaysUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     try
                                     {
                                         Foo();
                                     }
                                     catch (Exception)
                                     {
                                         Bar();
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>#region</c>/<c>#endregion</c> pair already correctly laid out between a
    /// hand-braced if-block and its <c>else</c> — matching the layout the region-formatting phase
    /// already produces for the same shape when braces are synthesized — is left byte-identical, so
    /// the new own-line step neither adds nor removes any of the blank lines that layout requires
    /// </summary>
    [TestMethod]
    public void RegionBetweenAlreadyBracedIfAndElseStaysUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x)
                                     {
                                         Foo();
                                     }

                                     #region Else branch

                                     else
                                     {
                                         Bar();
                                     }

                                     #endregion // Else branch
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an own-line comment already separating a hand-braced if-block from its <c>else</c>
    /// is left unchanged by the new own-line step — the blank line the formatter's blank-line phase
    /// already inserts before a standalone comment in this position is pre-existing, unrelated behavior
    /// </summary>
    [TestMethod]
    public void OwnLineCommentBeforeElseStaysUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x)
                                     {
                                         Foo();
                                     }
                                     // otherwise
                                     else
                                     {
                                         Bar();
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool x)
                                    {
                                        if (x)
                                        {
                                            Foo();
                                        }

                                        // otherwise
                                        else
                                        {
                                            Bar();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an active <c>#if</c>/<c>#endif</c> pair between a hand-braced if-block and its
    /// <c>else</c> — which already forces <c>else</c> onto its own line — is left byte-identical
    /// </summary>
    [TestMethod]
    public void DirectiveBeforeElseStaysUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x)
                                     {
                                         Foo();
                                     }
                             #if DEBUG
                                     else
                                     {
                                         Bar();
                                     }
                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, expected: null, parseOptions: new CSharpParseOptions(preprocessorSymbols: _preprocessorSymbols));
    }

    #endregion // Methods
}