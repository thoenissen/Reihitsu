using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the spaces inside an accessor list on one line: one space behind its <c>{</c> and between two
/// accessors, whether the author wrote the list on one line or the line-break phase collapsed it
/// </summary>
[TestClass]
public class AccessorListSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that one-line auto-property accessor lists written without spaces get one space behind <c>{</c> and between the
    /// accessors
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public void TightAccessorListsGetOneSpaceBehindTheBraceAndBetweenAccessors()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P {get;set;}

                                 int Q {get;}

                                 int R {get;init;}

                                 int S {get;private set;}
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P { get; set; }

                                    int Q { get; }

                                    int R { get; init; }

                                    int S { get; private set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that runs of whitespace inside a one-line accessor list become one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public void WhitespaceRunsInsideAOneLineAccessorListAreCollapsed()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P {   get;   set;   }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P { get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a one-line accessor list with an attributed accessor is spaced like every other one-line list, wherever
    /// the attribute stands
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public void OneLineAccessorListsWithAttributedAccessorsAreSpaced()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 int P {[Obsolete] get;set;}

                                 int Q {[Obsolete]get;set;}

                                 int R { get;[Obsolete] set;}
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    int P { [Obsolete] get; set; }

                                    int Q { [Obsolete] get; set; }

                                    int R { get; [Obsolete] set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an accessor list the line-break phase collapses onto the declaration's line is spaced like the same list
    /// written on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    [Requirement("FMT-BREAK-031")]
    public void CollapsedAccessorListIsSpacedLikeTheSameListOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P
                                 {
                                     get;
                                     set;
                                 }

                                 int Q {get;set;}
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P { get; set; }

                                    int Q { get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an auto-property with an initializer keeps its accessor list spaced in front of the initializer
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public void AccessorListInFrontOfAnInitializerIsSpaced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P {get;set;} = 5;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int P { get; set; } = 5;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the other side of the rule: the braces of an array initializer are not an accessor list, so the gap behind
    /// <c>{</c> keeps the author's spacing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-022")]
    public void InitializerBracesAreNotAnAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int[] R = new[] {1, 2};
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] R = new[] {1, 2 };
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}