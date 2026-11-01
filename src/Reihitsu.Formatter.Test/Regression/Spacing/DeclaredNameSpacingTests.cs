using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Full-pipeline tests for the space between a type or pattern and the name it declares: one space, for every declaration
/// that names a variable behind a type or pattern
/// </summary>
[TestClass]
public class DeclaredNameSpacingTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies that parameters whose type ends in <c>?</c>, <c>]</c>, <c>&gt;</c>, or <c>)</c> get one space in front of their
    /// name, and a run of whitespace behind a keyword type becomes one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void ParameterNamesGetOneSpaceBehindTheirType()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(int?a, int[]b, List<int>c, (int, int)d, int  e)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(int? a, int[] b, List<int> c, (int, int) d, int e)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter whose pointer type ends in <c>*</c> gets one space in front of its name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void PointerParameterNameGetsOneSpaceBehindItsType()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M(int*p)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    unsafe void M(int* p)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the names of tuple type elements get one space behind their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void TupleElementNamesGetOneSpaceBehindTheirType()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M((int?A, int[]B) t)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M((int? A, int[] B) t)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the names of local variable declarations get one space behind their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void LocalVariableNamesGetOneSpaceBehindTheirType()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     int?z = null;
                                     int[]y = null;
                                     List<int>x = null;
                                     (int, int)w = default;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M()
                                    {
                                        int? z = null;
                                        int[] y = null;
                                        List<int> x = null;
                                        (int, int) w = default;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the names of declaration expressions, in an <c>out</c> argument and in a deconstruction, get one space
    /// behind their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void DeclarationExpressionNamesGetOneSpaceBehindTheirType()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     N(out int?w, out int[]v);

                                     (int?a, int?b) = (1, 2);
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        N(out int? w, out int[] v);

                                        (int? a, int? b) = (1, 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the designations of declaration patterns get one space behind their type, in an <c>is</c> expression and in
    /// a <c>case</c> label
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void DeclarationPatternDesignationsGetOneSpaceBehindTheirType()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M(object o)
                                 {
                                     var p = o is int[]d;
                                     var q = o is List<int>l;

                                     switch (o)
                                     {
                                         case int[]e:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M(object o)
                                    {
                                        var p = o is int[] d;
                                        var q = o is List<int> l;

                                        switch (o)
                                        {
                                            case int[] e:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the designations behind a recursive pattern and a list pattern get one space behind the pattern
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void RecursiveAndListPatternDesignationsGetOneSpaceBehindThePattern()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(object o, int[] a)
                                 {
                                     var p = o is { }e;
                                     var q = o is string { Length: 1 }s;
                                     var r = a is [1]f;
                                     var t = a is [1, 2]g;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o, int[] a)
                                    {
                                        var p = o is { } e;
                                        var q = o is string { Length: 1 } s;
                                        var r = a is [1] f;
                                        var t = a is [1, 2] g;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the variable of a <c>foreach</c> statement gets one space behind its type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void ForeachVariableGetsOneSpaceBehindItsType()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int?[] xs, int[][] ys)
                                 {
                                     foreach (int?x in xs)
                                     {
                                     }

                                     foreach (int[]y in ys)
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int?[] xs, int[][] ys)
                                    {
                                        foreach (int? x in xs)
                                        {
                                        }

                                        foreach (int[] y in ys)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the variable of a <c>catch</c> declaration gets one space behind its type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void CatchVariableGetsOneSpaceBehindItsType()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     try
                                     {
                                     }
                                     catch (System.Exception?e)
                                     {
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
                                        }
                                        catch (System.Exception? e)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the range variables of a <c>from</c> and a <c>join</c> clause get one space behind their type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void QueryRangeVariablesGetOneSpaceBehindTheirType()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int?[] xs)
                                 {
                                     var q = from int?i in xs join int?j in xs on i equals j select i;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(int?[] xs)
                                    {
                                        var q = from int? i in xs join int? j in xs on i equals j select i;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a gap inside the type is decided by its own rule while the gap in front of the name gets one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    public void GapInsideTheTypeIsNotDecidedByTheRule()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int ?a)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int? a)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter wrapped between a type that ends in <c>?</c> or <c>]</c> and its name is joined and spaced like
    /// the same parameter on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedParameterIsSpacedLikeTheSameParameterOnOneLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int?
                                        a, int[]
                                        b, int?c, int[]d)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int? a, int[] b, int? c, int[] d)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a range variable wrapped between its type and its name inside an argument is joined and spaced like the same
    /// range variable on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    [Requirement("FMT-BREAK-031")]
    public void JoinedRangeVariableIsSpacedLikeTheSameRangeVariableOnOneLine()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 void M(int?[] xs)
                                 {
                                     N(from int?
                                       y in xs select y);
                                     N(from int?z in xs select z);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                class C
                                {
                                    void M(int?[] xs)
                                    {
                                        N(from int? y in xs select y);
                                        N(from int? z in xs select z);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment the type carries in front of the name keeps its place, and the name follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    [Requirement("FMT-SPACE-013")]
    public void CommentBehindTheTypeKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int?/* a */b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int?/* a */ b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment the name carries keeps the whitespace in front of it, and the name
    /// follows it after one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SPACE-021")]
    [Requirement("FMT-SPACE-014")]
    public void DocumentationCommentInFrontOfTheNameKeepsItsPlace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int? /** a */b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int? /** a */ b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}