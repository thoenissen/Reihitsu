using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Tests that an empty statement and the statement next to it are split where a blank-line requirement separates them, in
/// a block and in a switch section, so a section that becomes multi-line through the split is braced in the first run,
/// and that they are left as written where no such requirement applies
/// </summary>
[TestClass]
public class EmptyStatementLineBreakTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// The braced result of a switch whose first section holds an empty statement and a <c>throw</c>
    /// </summary>
    private const string BracedThrowSection = """
                                              class C
                                              {
                                                  int M(int x)
                                                  {
                                                      switch (x)
                                                      {
                                                          case 1:
                                                              {
                                                                  ;

                                                                  throw null;
                                                              }
                                                          default:
                                                              {
                                                                  return 0;
                                                              }
                                                      }
                                                  }
                                              }
                                              """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies the second FMT-BREAK-006 example: the pairs are split in a block, behind a closing brace, and in a switch
    /// section, and the section is braced in the same run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void SpecExampleSplitsEmptyStatementPairsThatABlankLineSeparates()
    {
        const string input = """
                             class C
                             {
                                 int M(int a)
                                 {
                                     a++;
                                     ; return a;
                                 }

                                 void N(bool a)
                                 {
                                     if (a) { } ;
                                 }

                                 int O(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             ; return 1;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int a)
                                    {
                                        a++;
                                        ;

                                        return a;
                                    }

                                    void N(bool a)
                                    {
                                        if (a)
                                        {
                                        }

                                        ;
                                    }

                                    int O(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    ;

                                                    return 1;
                                                }
                                            default:
                                                {
                                                    return 0;
                                                }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a switch section whose only statements are an empty statement and a <c>return</c> on one line is
    /// braced in the first run, together with the other sections
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void SectionWithEmptyStatementAndReturnOnOneLineIsBracedInFirstRun()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             ; return 1;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    ;

                                                    return 1;
                                                }
                                            default:
                                                {
                                                    return 0;
                                                }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a switch section whose only statements are an empty statement and a <c>throw</c> on one line is braced
    /// in the first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void SectionWithEmptyStatementAndThrowOnOneLineIsBracedInFirstRun()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             ; throw null;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input, BracedThrowSection);
    }

    /// <summary>
    /// Verifies that a switch section whose empty statement and <c>throw</c> stand on the label's line is braced in the
    /// first run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void SectionWithEmptyStatementAndThrowOnLabelLineIsBracedInFirstRun()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1: ; throw null;
                                         default:
                                             return 0;
                                     }
                                 }
                             }
                             """;

        AssertRuleResult(input, BracedThrowSection);
    }

    /// <summary>
    /// Verifies that a section with an empty statement and a <c>return</c> on one line in a switch that another multi-line
    /// section braces gets the same layout in one run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void SectionWithEmptyStatementAndReturnNextToMultiLineSectionIsSplit()
    {
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             ; return 1;
                                         case 2:
                                             A();
                                             B();
                                             break;
                                     }

                                     return 0;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    ;

                                                    return 1;
                                                }
                                            case 2:
                                                {
                                                    A();
                                                    B();
                                                }
                                                break;
                                        }

                                        return 0;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty statement in front of the terminal <c>break</c> of a section, which no blank-line requirement
    /// separates from it, is left on the <c>break</c>'s line and the switch is not braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void EmptyStatementInFrontOfTerminalBreakIsLeftAsWritten()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             ; break;
                                         default:
                                             A();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                ; break;

                                            default:
                                                A();
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty statement in front of a <c>continue</c> in a switch section is split from it and the switch
    /// is braced in one run
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void SectionWithEmptyStatementAndContinueOnOneLineIsBracedInFirstRun()
    {
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             ; continue;
                                         default:
                                             return;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    ;

                                                    continue;
                                                }
                                            default:
                                                {
                                                    return;
                                                }
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies both sides of the exemption in a block: an empty statement and an expression statement that no blank-line
    /// requirement separates stay on one line, while a local declaration, a <c>yield</c>, and a <c>break</c> behind an
    /// empty statement are split from it with a blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-006")]
    public void EmptyStatementIsSplitOnlyFromStatementsABlankLineSeparates()
    {
        const string input = """
                             class C
                             {
                                 System.Collections.Generic.IEnumerable<int> M(bool a)
                                 {
                                     while (a)
                                     {
                                         ; break;
                                     }

                                     ; yield return 1;
                                 }

                                 void N()
                                 {
                                     ; Foo();
                                     ; var b = 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    System.Collections.Generic.IEnumerable<int> M(bool a)
                                    {
                                        while (a)
                                        {
                                            ;

                                            break;
                                        }

                                        ;

                                        yield return 1;
                                    }

                                    void N()
                                    {
                                        ; Foo();
                                        ;

                                        var b = 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}