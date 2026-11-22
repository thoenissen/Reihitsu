using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the <c>=</c> of an enum member's value and of a parameter's default value, which the formatter
/// lays out as the initializer of a variable (FMT-PIPE-014, FMT-BREAK-015, FMT-BREAK-016)
/// </summary>
[TestClass]
public class EnumMemberValueLayoutTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the specification example: an enum member's <c>=</c> below its name and a value below its <c>=</c> are
    /// joined, as are a parameter's default values, a compound assignment, and a coalescing assignment, while a wrapped
    /// binary operator keeps its wrap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    [Requirement("FMT-BREAK-015")]
    [Requirement("FMT-PIPE-014")]
    public void SpecificationExampleIsLaidOutAsAssignments()
    {
        const string input = """
                             enum E
                             {
                                 A
                                     = 1,
                                 B =
                                     2
                             }

                             class C
                             {
                                 void M(int a
                                            = 1, string s =
                                                null)
                                 {
                                     a
                                         += 1;
                                     s ??=
                                         "x";
                                     a = a
                                         + 1;
                                 }
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A = 1,
                                    B = 2
                                }

                                class C
                                {
                                    void M(int a = 1, string s = null)
                                    {
                                        a += 1;
                                        s ??= "x";
                                        a = a
                                            + 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported example: an enum member's <c>=</c> below its name is joined, and whole-document formatting
    /// removes the trailing comma of the last member
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void EnumMemberEqualsBelowNameIsJoined()
    {
        const string input = """
                             enum E
                             {
                                 A
                                     = 1,
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A = 1
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the reported example: a lone <c>=</c> of an enum member is joined with its name and its value
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void LoneEnumMemberEqualsIsJoined()
    {
        const string input = """
                             enum E
                             {
                                 A
                                     =
                                     1,
                                 B
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A = 1,
                                    B
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>=</c> of the last enum member, which has no trailing comma, is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void LastEnumMemberEqualsIsJoined()
    {
        const string input = """
                             enum E
                             {
                                 A = 1,
                                 B
                                     = 2
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A = 1,
                                    B = 2
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>=</c> of an enum member with an attribute is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void EnumMemberWithAttributeEqualsIsJoined()
    {
        const string input = """
                             enum E
                             {
                                 A = 1,
                                 [System.Obsolete]
                                 B
                                     = 2
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A = 1,
                                    [System.Obsolete]
                                    B = 2
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>=</c> of an enum member behind a documentation comment is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void DocumentedEnumMemberEqualsIsJoined()
    {
        const string input = """
                             enum E
                             {
                                 A = 1,

                                 /// <summary>
                                 /// Doc
                                 /// </summary>
                                 B
                                     = 2
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A = 1,

                                    /// <summary>
                                    /// Doc
                                    /// </summary>
                                    B = 2
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an enum member whose name line ends with a comment keeps its wrap, so the comment is not lost
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void EnumMemberBehindEndOfLineCommentKeepsItsWrap()
    {
        const string input = """
                             enum E
                             {
                                 A // c
                                     = 1,
                                 B
                             }
                             """;

        AssertBlankLinesAbove(input, ("A // c", false), ("= 1,", false));
    }

    /// <summary>
    /// Verifies that an enum member written on one line stays as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-016")]
    public void OneLineEnumMemberStays()
    {
        const string input = """
                             enum E
                             {
                                 A = 1,
                                 B = 2
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a parameter's default <c>=</c> below its name is joined, as a wrap inside a list element
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-015")]
    public void ParameterDefaultEqualsBelowNameIsJoined()
    {
        const string input = """
                             class C
                             {
                                 void M(int a, int b
                                                   = 1)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a, int b = 1)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}