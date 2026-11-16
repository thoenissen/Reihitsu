using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the <c>with(…)</c> element of a collection expression: no space between <c>with</c> and the opening
/// parenthesis of its argument list, and the element is laid out like any other element of a wrapped collection expression
/// </summary>
[TestClass]
public class WithElementSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the example of FMT-SPACE-028: the whitespace between <c>with</c> and <c>(</c> is removed in front of an argument
    /// list with spaces inside, a named argument, and an empty argument list, and behind a comment in the gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-028")]
    public void SpecExampleRemovesTheSpaceBetweenWithAndItsArgumentList()
    {
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(string[] values)
                                 {
                                     HashSet<string> a = [with ( StringComparer.Ordinal ) , "x"];
                                     List<string> b = [with  (capacity: values.Length), .. values];
                                     List<string> c = [with ()];
                                     List<string> d = [with /* size */ (4)];
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(string[] values)
                                    {
                                        HashSet<string> a = [with(StringComparer.Ordinal), "x"];
                                        List<string> b = [with(capacity: values.Length), .. values];
                                        List<string> c = [with()];
                                        List<string> d = [with /* size */(4)];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between <c>with</c> and an argument list is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-028")]
    public void SpaceInFrontOfTheArgumentListIsRemoved()
    {
        const string input = """
                             class C
                             {
                                 void M(System.StringComparer comparer)
                                 {
                                     System.Collections.Generic.HashSet<string> set = [with (comparer), "Hello", "HELLO"];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(System.StringComparer comparer)
                                    {
                                        System.Collections.Generic.HashSet<string> set = [with(comparer), "Hello", "HELLO"];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the space between <c>with</c> and an empty argument list is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-028")]
    public void SpaceInFrontOfAnEmptyArgumentListIsRemoved()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Collections.Generic.List<string> e = [with ()];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Collections.Generic.List<string> e = [with()];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab between <c>with</c> and its argument list is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-028")]
    public void TabInFrontOfTheArgumentListIsRemoved()
    {
        const string input = "class C\n{\n    void M()\n    {\n        System.Collections.Generic.List<int> e = [with\t(4), 1];\n    }\n}";
        const string expected = "class C\n{\n    void M()\n    {\n        System.Collections.Generic.List<int> e = [with(4), 1];\n    }\n}";

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in the gap stays where it is and the space behind it is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-028")]
    public void CommentInTheGapStaysAndTheSpaceBehindItIsRemoved()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Collections.Generic.List<string> d = [with /* size */ (4)];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Collections.Generic.List<string> d = [with /* size */(4)];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>with(…)</c> element written without a space, and the elements behind it, stay unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-028")]
    public void ElementWithoutASpaceStaysUnchanged()
    {
        const string input = """
                             class C
                             {
                                 void M(System.StringComparer comparer)
                                 {
                                     System.Collections.Generic.HashSet<string> a = [with(comparer), "x"];
                                     System.Collections.Generic.List<string> b = [with()];
                                 }
                             }
                             """;

        AssertRuleResult(input, null);
    }

    /// <summary>
    /// Verifies that a <c>with(…)</c> element behind the opening bracket of a collection expression with a space in front of it
    /// loses both spaces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-028")]
    public void SpaceBehindTheOpeningBracketAndInFrontOfTheArgumentListAreRemoved()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Collections.Generic.List<int> e = [ with (4), 1];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Collections.Generic.List<int> e = [with(4), 1];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the <c>with(…)</c> example of FMT-BREAK-022: in a wrapped collection expression the element starts the first line
    /// after the opening bracket, and every other element starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-022")]
    public void WrappedCollectionExpressionStartingWithAWithElementGetsOneElementPerLine()
    {
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     HashSet<string> s = [with(StringComparer.Ordinal),
                                                          "a", "b"];
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        HashSet<string> s = [
                                                                with(StringComparer.Ordinal),
                                                                "a",
                                                                "b"
                                                            ];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>with (…)</c> element with a space at the start of a wrapped collection expression starts its own line
    /// and loses the space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-022")]
    [Requirement("FMT-SPACE-028")]
    public void WrappedCollectionExpressionStartingWithASpacedWithElementIsLaidOut()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Collections.Generic.List<string> x = [with (4),
                                         "a"];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Collections.Generic.List<string> x = [
                                                                                        with(4),
                                                                                        "a"
                                                                                    ];
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}