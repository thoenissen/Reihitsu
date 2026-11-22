using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for the wrapped bracketed argument list of a conditional element access (<c>c?[1, 2]</c>), whose parent in
/// the syntax tree is an element binding: it is joined onto one line like the list of an element access (<c>m[1, 2]</c>,
/// <c>d![1, 2]</c>), in every context it stands in, unless a comment, a directive, or an argument that spans several lines keeps
/// it as written
/// </summary>
[TestClass]
public class ConditionalElementAccessArgumentListTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: the list of an element access behind a null-forgiving operator and the list of a conditional
    /// element access, with an opening bracket on its own and a list behind another conditional element access, are joined alike,
    /// while a comment between the brackets keeps the list as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    [Requirement("FMT-BREAK-014")]
    public void SpecExampleJoinsConditionalAndNullForgivingElementAccessListsAlike()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             class C
                             {
                                 void M(int[,]? c, int[,] d, int[,][,]? e)
                                 {
                                     var a = c?[1,
                                                2];
                                     var g = c?[
                                         1, 2];
                                     var i = e?[0,
                                                1]?[1,
                                                    2];
                                     var h = c?[1, // note
                                                2];
                                     var b = d![1,
                                                2];
                                 }
                             }
                             """;
        const string expected = """
                                #nullable enable

                                class C
                                {
                                    void M(int[,]? c, int[,] d, int[,][,]? e)
                                    {
                                        var a = c?[1, 2];
                                        var g = c?[1, 2];
                                        var i = e?[0, 1]?[1, 2];
                                        var h = c?[1, // note
                                                   2];
                                        var b = d![1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional element access whose list wraps behind a comma is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void WrappedConditionalElementAccessListJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c?[1,
                                                2];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional element access whose opening bracket ends its line is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListWithOpeningBracketAloneJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c?[
                                         1, 2];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional element access whose closing bracket stands on its own line is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListWithClosingBracketAloneJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c?[1, 2
                                         ];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the list of a conditional element access behind a null-forgiving operator (<c>c!?[</c>) is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListBehindNullForgivingOperatorJoins()
    {
        // Arrange
        const string input = """
                             #nullable enable

                             class C
                             {
                                 void M(int[,]? c)
                                 {
                                     var a = c!?[1,
                                                 2];
                                 }
                             }
                             """;
        const string expected = """
                                #nullable enable

                                class C
                                {
                                    void M(int[,]? c)
                                    {
                                        var a = c!?[1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the list of a conditional element access behind a member binding (<c>o?.Q?[</c>) is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListBehindMemberBindingJoins()
    {
        // Arrange
        const string input = """
                             class D
                             {
                                 public int[,] Q;
                             }

                             class C
                             {
                                 void M(D o)
                                 {
                                     var a = o?.Q?[1,
                                                   2];
                                 }
                             }
                             """;
        const string expected = """
                                class D
                                {
                                    public int[,] Q;
                                }

                                class C
                                {
                                    void M(D o)
                                    {
                                        var a = o?.Q?[1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the list of a conditional element access behind an invocation (<c>this.Get()?[</c>) is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListBehindInvocationJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int[,] Get()
                                 {
                                     return null;
                                 }

                                 void M()
                                 {
                                     var a = this.Get()?[1,
                                                         2];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[,] Get()
                                    {
                                        return null;
                                    }

                                    void M()
                                    {
                                        var a = this.Get()?[1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the list of a conditional element access whose result is invoked (<c>f?[1, 2](3)</c>) is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListFollowedByInvocationJoins()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M(Func<int, int>[,] f)
                                 {
                                     var a = f?[1,
                                                2](3);
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M(Func<int, int>[,] f)
                                    {
                                        var a = f?[1, 2](3);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the list of a conditional element access followed by a member access (<c>c?[1, 2].ToString()</c>) is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListFollowedByMemberAccessJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c?[1,
                                                2].ToString();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1, 2].ToString();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that where an element binding is the receiver of an element access (<c>e?[0, 1][1, 2]</c>), the lists of both are
    /// joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ElementBindingAsReceiverOfElementAccessJoinsBothLists()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,][,] e)
                                 {
                                     var a = e?[0,
                                                1][1,
                                                   2];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,][,] e)
                                    {
                                        var a = e?[0, 1][1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped conditional element access that is an argument of an invocation is joined in one pass, so the
    /// invocation's argument list, which no longer holds a multi-line argument, stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListInInvocationArgumentJoinsInOnePass()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     N(c?[1,
                                          2] ?? 0, 3);
                                 }

                                 void N(int a, int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        N(c?[1, 2] ?? 0, 3);
                                    }

                                    void N(int a, int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped conditional element access inside the argument list of an implicit element access in an object
    /// initializer is joined in one pass, the initializer being laid out as for an element access in the same place
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListInImplicitElementAccessJoinsInOnePass()
    {
        // Arrange
        const string input = """
                             class D
                             {
                                 public int this[int a, int b]
                                 {
                                     get => 0;
                                     set
                                     {
                                     }
                                 }
                             }

                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = new D { [c?[1,
                                                         2] ?? 0, 1] = 3 };
                                 }
                             }
                             """;
        const string expected = """
                                class D
                                {
                                    public int this[int a, int b]
                                    {
                                        get => 0;
                                        set
                                        {
                                        }
                                    }
                                }

                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = new D
                                                {
                                                    [c?[1, 2] ?? 0, 1] = 3
                                                };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped conditional element access that is the body of a lambda is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListInLambdaBodyJoins()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     Func<int?> a = () => c?[1,
                                                             2];
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        Func<int?> a = () => c?[1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped conditional element access inside an interpolation hole is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void ConditionalElementAccessListInInterpolationHoleJoins()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = $"{c?[1,
                                                   2]}";
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = $"{c?[1, 2]}";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break between the <c>?</c> and the <c>[</c> is removed and the wrapped list behind it is joined, so the
    /// conditional element access ends on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    [Requirement("FMT-BREAK-040")]
    public void LineBreakBetweenQuestionMarkAndBracketAndWrappedListAreBothJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c?
                                         [1,
                                          2];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break in front of the <c>?</c> is removed and the wrapped list behind it is joined, so the conditional
    /// element access ends on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    [Requirement("FMT-BREAK-040")]
    public void LineBreakInFrontOfQuestionMarkAndWrappedListAreBothJoined()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c
                                         ?[1,
                                           2];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1, 2];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment between the brackets of a conditional element access keeps the list as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    [Requirement("FMT-BREAK-014")]
    public void BlockCommentBetweenBracketsKeepsConditionalElementAccessListAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c?[1,
                                                /* c */ 2];
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a directive between the brackets of a conditional element access keeps the list as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-001")]
    [Requirement("FMT-BREAK-014")]
    public void DirectiveBetweenBracketsKeepsConditionalElementAccessListAsWritten()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c?[1,
                             #if X
                                                2
                             #else
                                                3
                             #endif
                                                ];
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an argument that spans several lines keeps the list of a conditional element access wrapped, as it keeps the
    /// list of an element access
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-014")]
    public void MultiLineArgumentKeepsConditionalElementAccessListWrapped()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c?[1,
                                                2 +
                                                3];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int[,] c)
                                    {
                                        var a = c?[1,
                                                   2
                                                   + 3];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment in front of the <c>?</c> of a conditional element access with a wrapped list is refused as
    /// unsupported, the input being returned unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-014")]
    public void CommentInFrontOfQuestionMarkOfWrappedConditionalElementAccessIsUnsupported()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int[,] c)
                                 {
                                     var a = c /* c */ ?[1,
                                                         2];
                                 }
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(AttachedPartCommentReason, input, "/* c */");
    }

    /// <summary>
    /// Characterizes the size list of a fixed-size buffer, which holds no index and whose layout the join of index lists does not
    /// decide: it stays wrapped, its second line aligned behind the opening bracket
    /// </summary>
    [TestMethod]
    public void FixedSizeBufferSizeListStaysWrapped()
    {
        // Arrange
        const string input = """
                             unsafe struct S
                             {
                                 fixed int b[
                                     3];
                             }
                             """;
        const string expected = """
                                unsafe struct S
                                {
                                    fixed int b[
                                                3];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}