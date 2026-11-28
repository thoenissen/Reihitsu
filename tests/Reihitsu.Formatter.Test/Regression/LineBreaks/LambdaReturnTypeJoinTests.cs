using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the join of a parenthesized lambda's explicit return type and the <c>(</c> of its parameter list:
/// the line breaks and blank lines between them are removed wherever the lambda stands, while a comment, a directive, or
/// disabled text in the gap keeps them
/// </summary>
[TestClass]
public class LambdaReturnTypeJoinTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the spec example: as a declaration's value, behind a modifier and across a blank line, in a conditional branch, as an expression body, and with a block body, whose braces then align under the joined <c>(</c>, the <c>(</c> of a lambda's parameter list joins the line of the lambda's explicit return type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void SpecExampleReturnTypeJoinsTheParameterListInEveryPosition()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M(bool b)
                                 {
                                     Func<int, int> i = int
                                         (int x) => x;
                                     Func<int, int> h = static int

                                         (int x) => x;
                                     Func<int, int> g = b
                                         ? int
                                             (int x) => x
                                         : static int (int x) => x;
                                     Func<int, int> m = int
                                         (int x) =>
                                         {
                                             return x;
                                         };
                                 }

                                 Func<int, int> P() => int
                                     (int x) => x;
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M(bool b)
                                    {
                                        Func<int, int> i = int (int x) => x;
                                        Func<int, int> h = static int (int x) => x;
                                        Func<int, int> g = b
                                                               ? int (int x) => x
                                                               : static int (int x) => x;
                                        Func<int, int> m = int (int x) =>
                                                               {
                                                                   return x;
                                                               };
                                    }

                                    Func<int, int> P()
                                    {
                                        return int (int x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a comment in the gap between the return type and the parameter list keeps the line break, and a lambda without a return type has no such gap; both are left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void SpecExampleCommentInTheGapKeepsTheLineBreakAndLambdaWithoutReturnTypeIsUnchanged()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 void M()
                                 {
                                     Func<int, int> i = int // c
                                         (int x) => x;
                                     Func<int, int> j = (int x) => x;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a generic return type joins the parameter list behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void GenericReturnTypeJoinsTheParameterList()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, List<int>> g = List<int>
                                         (int x) => null;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        Func<int, List<int>> g = List<int> (int x) => null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tuple return type joins the parameter list behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void TupleReturnTypeJoinsTheParameterList()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, (int, int)> t = (int, int)
                                         (int x) => (x, x);
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        Func<int, (int, int)> t = (int, int) (int x) => (x, x);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an array return type joins the parameter list behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void ArrayReturnTypeJoinsTheParameterList()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, int[]> a = int[]
                                         (int x) => null;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        Func<int, int[]> a = int[] (int x) => null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nullable return type joins the parameter list behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void NullableReturnTypeJoinsTheParameterList()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, int?> n = int?
                                         (int x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        Func<int, int?> n = int? (int x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>ref</c> return type joins the parameter list behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void RefReturnTypeJoinsTheParameterList()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     R r = ref int
                                         (ref int x) => ref x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        R r = ref int (ref int x) => ref x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the return type of an <c>async</c> lambda joins the parameter list behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void ReturnTypeOfAsyncLambdaJoinsTheParameterList()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, Task<int>> s = async Task<int>
                                         (int x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        Func<int, Task<int>> s = async Task<int> (int x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the gap behind a modifier and the gap behind the return type are both joined, so a lambda written over three lines stands on one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void ModifierReturnTypeAndParameterListOnThreeLinesJoinIntoOne()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, int> h = static
                                         int
                                         (int x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        Func<int, int> h = static int (int x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the return type of a lambda assigned to a variable joins the parameter list behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void ReturnTypeOfAssignedLambdaJoinsTheParameterList()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, int> q;
                                     q = int
                                         (int x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        Func<int, int> q;
                                        q = int (int x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the return type of a lambda that is the expression body of another lambda joins the parameter list behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void ReturnTypeOfLambdaInExpressionBodyOfLambdaJoinsTheParameterList()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, Func<int, int>> h = y => int
                                         (int x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        Func<int, Func<int, int>> h = y => int (int x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment behind the return type keeps the line break in front of the parameter list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void DelimitedCommentInTheGapKeepsTheLineBreak()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, int> c = int /* c */
                                         (int x) => x;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment on its own line between the return type and the parameter list keeps the line breaks around it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void CommentLineInTheGapKeepsTheLineBreaks()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, int> l = int
                                         // c
                                         (int x) => x;
                                 }
                             }
                             """;
        const string expected = """
                                using System;
                                using System.Collections.Generic;
                                using System.Threading.Tasks;

                                class C
                                {
                                    delegate ref int R(ref int x);

                                    void M()
                                    {
                                        Func<int, int> l = int

                                            // c
                                            (int x) => x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive between the return type and the parameter list keeps the line breaks around it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void DirectiveInTheGapKeepsTheLineBreaks()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, int> d = int
                             #if DEBUG
                                         (int x) => x;
                             #else
                                         (int x) => x + 1;
                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that disabled text between the return type and the parameter list keeps the line breaks around it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void DisabledTextInTheGapKeepsTheLineBreaks()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;
                             using System.Threading.Tasks;

                             class C
                             {
                                 delegate ref int R(ref int x);

                                 void M()
                                 {
                                     Func<int, int> d = int
                             #if NEVER
                                         (long x) => x
                             #endif
                                         (int x) => x;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the return type of a lambda that is the expression body of a property joins the parameter list behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void ReturnTypeOfPropertyExpressionBodyJoinsTheParameterList()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 Func<int, int> P => int
                                     (int x) => x;
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    Func<int, int> P => int (int x) => x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the join also applies in a node-level root: <see cref="ReihitsuFormatter.FormatNode"/> of a local declaration joins the lambda's parameter list onto its return type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-045")]
    public void JoinAppliesInANodeLevelRoot()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings("class C\n{\n    void M()\n    {\n        System.Func<int, int> i = int\n            (int x) => x;\n    }\n}", endOfLine), cancellationToken: TestContext.CancellationToken);
            var statement = tree.GetRoot(TestContext.CancellationToken).DescendantNodes().OfType<LocalDeclarationStatementSyntax>().Single();

            var result = ReihitsuFormatter.FormatNode(statement, cancellationToken: TestContext.CancellationToken);

            Assert.IsNull(result.Unsupported);
            Assert.AreEqual("System.Func<int, int> i = int (int x) => x;", result.Output.ToString().Trim(), $"The node-level root must be joined under {DescribeLineEnding(endOfLine)} line endings.");
        }
    }

    #endregion // Tests
}