using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for <see cref="Pipeline.StructuralTransforms.Rewriter.ControlFlowBraceTransform"/> —
/// inserting braces around an unbraced <c>if</c>/<c>else</c> body must terminate the closing brace's own
/// line so the following token (an <c>else</c> keyword, a directive, or the next statement) is never glued
/// to it, and must not consume a blank line that separated the statement from what followed it
/// </summary>
[TestClass]
public class ControlFlowBraceElsePlacementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that when only the then-branch is unbraced and the else-branch is already braced, the
    /// inserted closing brace still starts a new line before <c>else</c>
    /// </summary>
    [TestMethod]
    public void ThenBranchUnbracedElseBranchAlreadyBracedPlacesElseOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x)
                                         Foo();
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
    /// Verifies that every branch of an unbraced <c>else if</c> chain is braced and every <c>else</c>/
    /// <c>else if</c> keeps starting its own line
    /// </summary>
    [TestMethod]
    public void ElseIfChainWithAllBranchesUnbracedPlacesEveryElseOnItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     if (x == 1)
                                         Foo();
                                     else if (x == 2)
                                         Bar();
                                     else
                                         Baz();
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
    /// Verifies that a fully single-line <c>if</c>/<c>else</c> statement is split into Allman form with
    /// <c>else</c> starting its own line, even though the input carried no line break before it
    /// </summary>
    [TestMethod]
    public void SingleLineIfElseSplitsElseOntoItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x) Foo(); else Bar();
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
    /// Verifies that a <c>#region</c> between the unbraced then-branch and <c>else</c> is preserved and
    /// ends up exactly as the region-formatting phase already lays out a hand-braced counterpart —
    /// a blank line around both the region and its <c>else</c> content, and a matching <c>#endregion</c>
    /// comment
    /// </summary>
    [TestMethod]
    public void RegionBetweenUnbracedThenBranchAndElseIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x)
                                 {
                                     if (x)
                                         Foo();
                                     #region Else branch
                                     else
                                         Bar();
                                     #endregion
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
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line separating an unbraced <c>if</c>/<c>else</c> statement from the next
    /// statement survives brace insertion
    /// </summary>
    [TestMethod]
    public void BlankLineAfterUnbracedIfElseIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool x, bool y)
                                 {
                                     if (x)
                                         Foo();
                                     else
                                         Bar();

                                     if (y)
                                     {
                                         Baz();
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool x, bool y)
                                    {
                                        if (x)
                                        {
                                            Foo();
                                        }
                                        else
                                        {
                                            Bar();
                                        }

                                        if (y)
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
    /// Verifies that an unbraced <c>if</c> with no <c>else</c>, immediately followed by the enclosing
    /// block's own closing brace, is already formatted correctly and stays unchanged
    /// </summary>
    [TestMethod]
    public void UnbracedIfWithNoElseStaysUnchanged()
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
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a then-branch already braced with an unbraced else-branch, immediately followed by
    /// the enclosing block's own closing brace, still braces the else-branch and stays otherwise unaffected
    /// </summary>
    [TestMethod]
    public void ThenBranchAlreadyBracedElseBranchUnbracedBracesTheElseBranch()
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
                                         Bar();
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

    #endregion // Methods
}