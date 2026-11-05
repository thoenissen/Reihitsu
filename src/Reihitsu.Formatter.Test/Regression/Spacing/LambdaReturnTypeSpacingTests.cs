using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the space between an explicit lambda return type and the lambda's parameter list: one space, whatever
/// the type ends in and whether the author wrote the pair on one line or the line-break phase joined it
/// </summary>
[TestClass]
public class LambdaReturnTypeSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that keyword, array, and generic return types get one space in front of the parameter list, joined and on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-026")]
    [Requirement("FMT-BREAK-031")]
    public void KeywordArrayAndGenericReturnTypesGetOneSpace()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     N(int
                                       (x) => x);
                                     N(int(x) => x);
                                     N(int[]
                                       (x) => x);
                                     N(int[](x) => x);
                                     N(List<int>
                                       (x) => x);
                                     N(List<int>(x) => x);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        N(int (x) => x);
                                        N(int (x) => x);
                                        N(int[] (x) => x);
                                        N(int[] (x) => x);
                                        N(List<int> (x) => x);
                                        N(List<int> (x) => x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that nullable and tuple return types get one space in front of the parameter list, joined and on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-026")]
    [Requirement("FMT-BREAK-031")]
    public void NullableAndTupleReturnTypesGetOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(int?
                                       (x) => x);
                                     N(int?(x) => x);
                                     N((int, int)
                                       (x) => (x, x));
                                     N((int, int)(x) => (x, x));
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(int? (x) => x);
                                        N(int? (x) => x);
                                        N((int, int) (x) => (x, x));
                                        N((int, int) (x) => (x, x));
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the return type of a lambda with a modifier gets one space in front of the parameter list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-026")]
    public void ReturnTypeBehindAModifierGetsOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(static int(x) => x);
                                     N(static int[]  (x) => x);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(static int (x) => x);
                                        N(static int[] (x) => x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda with an explicit return type outside an argument list gets one space in front of its parameter list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-026")]
    public void ReturnTypeOfAnAssignedLambdaGetsOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var f = int(int x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var f = int (int x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the return type carries keeps its place, and the parameter list follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-026")]
    [Requirement("FMT-SPACE-013")]
    public void CommentBehindTheReturnTypeKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(int/* a */(x) => x);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(int/* a */ (x) => x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}