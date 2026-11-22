using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the colon behind an attribute target: no space before it, one space behind it, whatever the
/// target and however the author spaced it
/// </summary>
[TestClass]
public class AttributeTargetColonSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that every keyword target gets no space before its colon and one space behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    public void KeywordTargetsGetNoSpaceBeforeAndOneSpaceBehindTheColon()
    {
        // Arrange
        const string input = """
                             using System;

                             [assembly:CLSCompliant(true)]
                             [module :CLSCompliant(true)]

                             class C
                             {
                                 [event :Obsolete]
                                 event Action E;

                                 [return:Obsolete]
                                 int M()
                                 {
                                     return 1;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                [assembly: CLSCompliant(true)]
                                [module: CLSCompliant(true)]

                                class C
                                {
                                    [event: Obsolete]
                                    event Action E;

                                    [return: Obsolete]
                                    int M()
                                    {
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every identifier target, including one the compiler does not know, gets no space before its colon and
    /// one space behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    public void IdentifierTargetsGetNoSpaceBeforeAndOneSpaceBehindTheColon()
    {
        // Arrange
        const string input = """
                             using System;

                             [type :Obsolete]
                             class C<[typevar:Obsolete] T>
                             {
                                 [field :NonSerialized]
                                 int _f;

                                 [property:Obsolete]
                                 int P { get; set; }

                                 [method:Obsolete]
                                 void M([param :Obsolete] int a)
                                 {
                                 }

                                 [custom:Obsolete]
                                 void N()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                [type: Obsolete]
                                class C<[typevar: Obsolete] T>
                                {
                                    [field: NonSerialized]
                                    int _f;

                                    [property: Obsolete]
                                    int P { get; set; }

                                    [method: Obsolete]
                                    void M([param: Obsolete] int a)
                                    {
                                    }

                                    [custom: Obsolete]
                                    void N()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that runs of whitespace on either side of the colon are normalized to no space before and one space behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    public void WhitespaceRunsAroundTheColonAreNormalized()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 [return  :  Obsolete]
                                 int M()
                                 {
                                     return 1;
                                 }

                                 [method : Obsolete]
                                 void N()
                                 {
                                 }

                                 [field:  NonSerialized]
                                 int _f;
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    [return: Obsolete]
                                    int M()
                                    {
                                        return 1;
                                    }

                                    [method: Obsolete]
                                    void N()
                                    {
                                    }

                                    [field: NonSerialized]
                                    int _f;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that attribute targets already spaced as the rule requires are left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    public void AttributeTargetsAlreadySpacedStayUnchanged()
    {
        // Arrange
        const string input = """
                             using System;

                             [assembly: CLSCompliant(true)]

                             class C
                             {
                                 [field: NonSerialized]
                                 int _f;

                                 [return: Obsolete]
                                 int M([param: Obsolete] int a)
                                 {
                                     return a;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the target of an attribute on an accessor inside a one-line accessor list is spaced like every other
    /// target
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    public void AccessorAttributeTargetInsideAOneLineAccessorListIsSpaced()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 int P { [method:Obsolete] get; set; }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    int P { [method: Obsolete] get; set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the attribute targets of a local function and of its parameter are spaced like the targets of a member
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    public void AttributeTargetsOfALocalFunctionAndItsParameterAreSpaced()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     [return :Obsolete]
                                     int L([param:Obsolete] int a)
                                     {
                                         return a;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        [return: Obsolete]
                                        int L([param: Obsolete] int a)
                                        {
                                            return a;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter attribute target wrapped before a tight colon is joined, and the colon gets no space before it
    /// and one space behind it, as on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedAttributeTargetIsSpacedLikeTheSameTargetOnOneLine()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M([param
                                         :Obsolete] int a, [param:Obsolete] int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M([param: Obsolete] int a, [param: Obsolete] int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the colon of a named attribute argument behind an attribute target is decided by the name-colon rule, not
    /// by the attribute-target rule
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    [Requirement("FMT-SPACE-019")]
    public void NamedAttributeArgumentColonBehindATargetIsANameColon()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 [method:Obsolete(message :"x", error:true)]
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    [method: Obsolete(message: "x", error: true)]
                                    void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the comment example of the rule: a delimited documentation comment the colon carries in front of it keeps the
    /// whitespace in front of it and loses the whitespace between it and the colon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentInFrontOfTheColonKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 [return /** x */ :Obsolete]
                                 int M()
                                 {
                                     return 1;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    [return /** x */: Obsolete]
                                    int M()
                                    {
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the target carries in front of the colon keeps the whitespace in front of it, and the colon
    /// follows the comment without a space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    [Requirement("FMT-SPACE-013")]
    public void CommentTheTargetCarriesInFrontOfTheColonKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 [return /* x */  :Obsolete]
                                 int M()
                                 {
                                     return 1;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    [return /* x */: Obsolete]
                                    int M()
                                    {
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the comment example of the rule: a comment the colon carries behind it keeps its place, and the attribute
    /// follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-016")]
    [Requirement("FMT-SPACE-013")]
    public void CommentTheColonCarriesBehindItKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 [return:/* x */Obsolete]
                                 int M()
                                 {
                                     return 1;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    [return:/* x */ Obsolete]
                                    int M()
                                    {
                                        return 1;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}