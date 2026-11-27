using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the opening parenthesis of a parenthesized lambda that starts its line: no parameter-list rule
/// moves it, so the requirement that owns the gap in front of the lambda decides that gap, as for any other expression
/// </summary>
[TestClass]
public class ParenthesizedLambdaLineStartTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the spec example: the opening parenthesis of an anonymous method moves onto the <c>delegate</c> line, the
    /// comma and closing parenthesis of a lambda's parameter list are placed like any other, and a lambda argument on its
    /// own line keeps that line and the blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void SpecExampleAnonymousMethodParenthesisJoinsWhileLambdaArgumentKeepsItsLine()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Linq;

                             class C
                             {
                                 void M(int[] items)
                                 {
                                     Func<int, int, int> f = delegate
                                         (int a, int b) { return a; };
                                     var total = items.Aggregate(0,
                                                                 (sum
                                                                  , item
                                                                 ) => sum + item);
                                     var other = items.Aggregate(0,

                                                                 (sum, item) => sum + item);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Linq;

                                class C
                                {
                                    void M(int[] items)
                                    {
                                        Func<int, int, int> f = delegate(int a, int b)
                                                                {
                                                                    return a;
                                                                };
                                        var total = items.Aggregate(0,
                                                                    (sum,
                                                                     item) => sum + item);
                                        var other = items.Aggregate(0,

                                                                    (sum, item) => sum + item);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a lambda element of a collection initializer and of a wrapped collection expression
    /// starts its own line like any other element
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void SpecExampleLambdaElementsOfInitializerAndCollectionExpressionStartTheirOwnLines()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var list = new List<Func<int, int>> { (x) => x, (x) => x };
                                     Func<int, int>[] col = [(x) => x,
                                                             (x) => x];
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
                                        var list = new List<Func<int, int>>
                                                   {
                                                       (x) => x,
                                                       (x) => x
                                                   };
                                        Func<int, int>[] col = [
                                                                   (x) => x,
                                                                   (x) => x
                                                               ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda argument on its own line, without a blank line above it, keeps its line and column, and
    /// that no phase run alone changes that output
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void LambdaArgumentOnItsOwnLineIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 int M(int[] items)
                                 {
                                     return items.Aggregate(0,
                                                            (sum, item) => sum + item);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the blank line in front of a lambda argument that starts its line behind the comma of an argument
    /// list is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    [Requirement("FMT-BREAK-030")]
    public void BlankLineInFrontOfLambdaArgumentIsKept()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 int M(int[] items)
                                 {
                                     return items.Aggregate(0,

                                                            (sum, item) => sum + item);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the blank line between two lambda elements of an array initializer is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    [Requirement("FMT-BREAK-030")]
    public void BlankLineBetweenLambdaElementsOfArrayInitializerIsKept()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     var l = new Func<int, int>[]
                                             {
                                                 (x) => x,

                                                 (x) => x
                                             };
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a lambda behind the arrow of an expression lambda and a blank line joins the arrow, and the blank line goes
    /// with the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    [Requirement("FMT-BREAK-030")]
    public void BlankLineInFrontOfLambdaBehindExpressionLambdaArrowGoesWithTheJoin()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int, Func<int, int>> h = x =>

                                         (y) => y;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, Func<int, int>> h = x => (y) => y;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda behind <c>return</c> and a blank line joins the <c>return</c>, and the blank line goes with the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void BlankLineInFrontOfLambdaBehindReturnGoesWithTheJoin()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 Func<int, int> M()
                                 {
                                     return

                                         (x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    Func<int, int> M()
                                    {
                                        return (x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda that starts its line behind <c>return</c> joins the <c>return</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-041")]
    public void LambdaBehindReturnJoinsTheReturn()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 Func<int, int> M()
                                 {
                                     return
                                         (x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    Func<int, int> M()
                                    {
                                        return (x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change-doc entry for collections: a one-line collection initializer of lambdas, a wrapped collection
    /// expression whose first element is a lambda, two lambdas on one line of a wrapped collection expression, and a
    /// blank line between lambda elements get the layout of any other element, the blank line included
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void LineBreaksInFrontOfLambdaElementsOfCollectionsAreKept()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 void M()
                                 {
                                     var list = new List<Func<int, int>> { (x) => x, (x) => x };
                                     Func<int, int>[] col = [(x) => x,
                                                             (x) => x];
                                     Func<int, int>[] two = [
                                         (x) => x, (x) => x];
                                     Func<int, int>[] gap = [(x) => x,

                                                             (x) => x];
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
                                        var list = new List<Func<int, int>>
                                                   {
                                                       (x) => x,
                                                       (x) => x
                                                   };
                                        Func<int, int>[] col = [
                                                                   (x) => x,
                                                                   (x) => x
                                                               ];
                                        Func<int, int>[] two = [
                                                                   (x) => x,
                                                                   (x) => x
                                                               ];
                                        Func<int, int>[] gap = [
                                                                   (x) => x,

                                                                   (x) => x
                                                               ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda that starts the line behind the <c>=</c> of a variable declarator is joined onto the
    /// <c>=</c> line, its blank line included, because the assignment requirement owns that gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void LambdaBehindAssignmentIsJoinedOntoTheAssignmentLine()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int, int> f =

                                         (x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = (x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda that is the first argument of a wrapped argument list is joined onto the line of the
    /// opening parenthesis, because the list layout owns that gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void LambdaAsFirstArgumentIsJoinedOntoTheOpeningParenthesisLine()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void Run(Func<int, int> f, int v)
                                 {
                                     Run(
                                         (x) => x,
                                         1);
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void Run(Func<int, int> f, int v)
                                    {
                                        Run((x) => x,
                                            1);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda behind the name colon of a named argument is joined onto the line of the name, because the
    /// join inside a list element owns that gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void LambdaBehindNamedArgumentColonIsJoinedOntoTheNameLine()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void Run(Func<int, int> f)
                                 {
                                     Run(f:
                                         (x) => x);
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void Run(Func<int, int> f)
                                    {
                                        Run(f: (x) => x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda argument whose opening parenthesis starts the line behind its <c>static</c> modifier is
    /// joined onto the modifier's line, because the join inside a list element owns that gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void LambdaParenthesisBehindStaticModifierInArgumentIsJoinedOntoTheModifierLine()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void Run(Func<int, int> f)
                                 {
                                     Run(static
                                         (x) => x);
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void Run(Func<int, int> f)
                                    {
                                        Run(static (x) => x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment line, with the blank line above it, in front of a lambda argument keeps the lambda on its
    /// own line, the comment and the lambda aligned with the other arguments
    /// </summary>
    [TestMethod]
    public void CommentLineInFrontOfLambdaArgumentIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 int M(int[] items)
                                 {
                                     return items.Aggregate(0,

                                                            // note
                                                            (sum, item) => sum + item);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that conditional directives in front of lambda arguments keep each lambda on its own line, aligned with
    /// the other arguments
    /// </summary>
    [TestMethod]
    public void DirectiveInFrontOfLambdaArgumentIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             class C
                             {
                                 int M(int[] items)
                                 {
                                     return items.Aggregate(0,
                             #if DEBUG
                                                            (sum, item) => sum + item);
                             #else
                                                            (sum, item) => sum - item);
                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Tests
}