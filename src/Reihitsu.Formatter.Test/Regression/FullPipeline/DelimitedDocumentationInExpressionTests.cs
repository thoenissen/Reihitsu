using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline tests for delimited documentation comments written behind an operator inside an expression: behind the
/// <c>?</c> or <c>:</c> of a conditional expression, where the operator has to start its own line, and behind other
/// operators, where it stays at the end of the line in front of the moved comment
/// </summary>
[TestClass]
public class DelimitedDocumentationInExpressionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a one-line conditional expression in an
    /// expression body starts a new line, and <c>?</c> and <c>:</c> each start their own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindColonOfOneLineConditionalInExpressionBody()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? 1 : /** d */ 2;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b)
                                    {
                                        return b
                                                   ? 1
                                                   :

                                        /** d */ 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a one-line conditional expression in a
    /// return statement starts a new line, and <c>?</c> and <c>:</c> each start their own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindColonOfOneLineConditionalInReturnStatement()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? 1 : /** d */ 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b, bool c)
                                    {
                                        return b
                                                   ? 1
                                                   :

                                        /** d */ 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment behind the <c>:</c> of a one-line
    /// conditional expression in an expression body starts a new line, the block comment stays behind <c>:</c>, and
    /// <c>?</c> and <c>:</c> each start their own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindBlockCommentBehindColonOfConditionalInExpressionBody()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? 1 : /* c */ /** d */ 2;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b)
                                    {
                                        return b
                                                   ? 1
                                                   : /* c */

                                        /** d */ 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment behind the <c>:</c> of a one-line
    /// conditional expression in a return statement starts a new line, the block comment stays behind <c>:</c>, and
    /// <c>?</c> and <c>:</c> each start their own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindBlockCommentBehindColonOfConditionalInReturnStatement()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? 1 : /* c */ /** d */ 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b, bool c)
                                    {
                                        return b
                                                   ? 1
                                                   : /* c */

                                        /** d */ 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a one-line conditional expression in an
    /// expression body starts a new line, and <c>?</c> and <c>:</c> each start their own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindQuestionMarkOfOneLineConditionalInExpressionBody()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? /** d */ 1 : 2;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b)
                                    {
                                        return b
                                                   ?

                                        /** d */ 1
                                                   : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a one-line conditional expression in a
    /// return statement starts a new line, and <c>?</c> and <c>:</c> each start their own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindQuestionMarkOfOneLineConditionalInReturnStatement()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? /** d */ 1 : 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b, bool c)
                                    {
                                        return b
                                                   ?

                                        /** d */ 1
                                                   : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment behind the <c>?</c> of a one-line
    /// conditional expression in an expression body starts a new line, the block comment stays behind <c>?</c>, and
    /// <c>?</c> and <c>:</c> each start their own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindBlockCommentBehindQuestionMarkOfConditionalInExpressionBody()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? /* c */ /** d */ 1 : 2;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b)
                                    {
                                        return b
                                                   ? /* c */

                                        /** d */ 1
                                                   : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment behind the <c>?</c> of a one-line
    /// conditional expression in a return statement starts a new line, the block comment stays behind <c>?</c>, and
    /// <c>?</c> and <c>:</c> each start their own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindBlockCommentBehindQuestionMarkOfConditionalInReturnStatement()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? /* c */ /** d */ 1 : 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b, bool c)
                                    {
                                        return b
                                                   ? /* c */

                                        /** d */ 1
                                                   : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a conditional expression in an expression
    /// body, wrapped in front of <c>?</c> only, starts a new line, and <c>:</c> starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindColonOfPartlyWrappedConditionalInExpressionBody()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b
                                     ? 1 : /** d */ 2;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b)
                                    {
                                        return b
                                                   ? 1
                                                   :

                                        /** d */ 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a conditional expression in a return
    /// statement, wrapped in front of <c>?</c> only, starts a new line, and <c>:</c> starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindColonOfPartlyWrappedConditionalInReturnStatement()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b)
                                 {
                                     return b
                                         ? 1 : /** d */ 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b)
                                    {
                                        return b
                                                   ? 1
                                                   :

                                        /** d */ 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a conditional expression in an expression
    /// body, wrapped in front of <c>:</c> only, starts a new line, and <c>?</c> starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindQuestionMarkOfPartlyWrappedConditionalInExpressionBody()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? /** d */ 1
                                     : 2;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b)
                                    {
                                        return b
                                                   ?

                                        /** d */ 1
                                                   : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a conditional expression in a return
    /// statement, wrapped in front of <c>:</c> only, starts a new line, and <c>?</c> starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindQuestionMarkOfPartlyWrappedConditionalInReturnStatement()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b)
                                 {
                                     return b ? /** d */ 1
                                         : 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b)
                                    {
                                        return b
                                                   ?

                                        /** d */ 1
                                                   : 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a parenthesized conditional nested in the
    /// <c>?</c> branch of a conditional in an expression body starts a new line, and every <c>?</c> and <c>:</c> starts
    /// its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindColonOfParenthesizedNestedConditionalInExpressionBody()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c) => b ? (c ? 1 : /** d */ 2) : 3;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b, bool c)
                                    {
                                        return b
                                                   ? (c
                                                          ? 1
                                                          :

                                        /** d */ 2)
                                                   : 3;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a parenthesized conditional nested in the
    /// <c>?</c> branch of a conditional in a return statement starts a new line, and every <c>?</c> and <c>:</c> starts
    /// its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindColonOfParenthesizedNestedConditionalInReturnStatement()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? (c ? 1 : /** d */ 2) : 3;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b, bool c)
                                    {
                                        return b
                                                   ? (c
                                                          ? 1
                                                          :

                                        /** d */ 2)
                                                   : 3;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a conditional nested in the <c>:</c>
    /// branch of a conditional in an expression body starts a new line, and every <c>?</c> and <c>:</c> starts its own
    /// line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindQuestionMarkOfNestedConditionalInExpressionBody()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c) => b ? 1 : c ? /** d */ 2 : 3;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b, bool c)
                                    {
                                        return b
                                                   ? 1
                                                   : c
                                                       ?

                                        /** d */ 2
                                                       : 3;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a conditional nested in the <c>:</c>
    /// branch of a conditional in a return statement starts a new line, and every <c>?</c> and <c>:</c> starts its own
    /// line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindQuestionMarkOfNestedConditionalInReturnStatement()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? 1 : c ? /** d */ 2 : 3;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b, bool c)
                                    {
                                        return b
                                                   ? 1
                                                   : c
                                                       ?

                                        /** d */ 2
                                                       : 3;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a <c>:</c> that already starts its line in a return
    /// statement starts a new line, and <c>:</c> keeps starting its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindColonStartingItsLineInReturnStatement()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b)
                                 {
                                     return b
                                         ? 1
                                         : /** d */ 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(bool b)
                                    {
                                        return b
                                                   ? 1
                                                   :

                                        /** d */ 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a <c>:</c> that already starts its line in a local
    /// declaration starts a new line, and <c>:</c> keeps starting its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-018")]
    public void MovesDelimitedDocumentationBehindColonStartingItsLineInLocalDeclaration()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal void M(bool b)
                                 {
                                     var x = b
                                         ? 1
                                         : /** d */ 2;
                                 }
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal void M(bool b)
                                    {
                                        var x = b
                                                    ? 1
                                                    :

                                        /** d */ 2;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a <c>+</c> starts a new line and the operator stays at
    /// the end of the line in front of it
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBehindAdditionOperator()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(int a, int b, int? n)
                                 {
                                     return a + /** d */ b;
                                 }

                                 internal int N(int x, int y) => x;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(int a, int b, int? n)
                                    {
                                        return a +

                                        /** d */ b;
                                    }

                                    internal int N(int x, int y)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a <c>??</c> starts a new line and the operator stays at
    /// the end of the line in front of it
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBehindCoalesceOperator()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(int a, int b, int? n)
                                 {
                                     return n ?? /** d */ b;
                                 }

                                 internal int N(int x, int y) => x;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(int a, int b, int? n)
                                    {
                                        return n ??

                                        /** d */ b;
                                    }

                                    internal int N(int x, int y)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a <c>&amp;&amp;</c> starts a new line and the operator
    /// stays at the end of the line in front of it
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBehindConditionalAndOperator()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(int a, int b, int? n)
                                 {
                                     var t = a > 0 && /** d */ b > 0;
                                     return a;
                                 }

                                 internal int N(int x, int y) => x;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(int a, int b, int? n)
                                    {
                                        var t = a > 0 &&

                                        /** d */ b > 0;

                                        return a;
                                    }

                                    internal int N(int x, int y)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>=</c> of an assignment starts a new line and the
    /// operator stays at the end of the line in front of it
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBehindAssignmentOperator()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(int a, int b, int? n)
                                 {
                                     int x;
                                     x = /** d */ a;
                                     return x;
                                 }

                                 internal int N(int x, int y) => x;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(int a, int b, int? n)
                                    {
                                        int x;
                                        x =

                                        /** d */ a;

                                        return x;
                                    }

                                    internal int N(int x, int y)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>=&gt;</c> of a lambda starts a new line and the
    /// arrow stays at the end of the line in front of it
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBehindLambdaArrow()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(int a, int b, int? n)
                                 {
                                     System.Func<int> f = () => /** d */ a;
                                     return f();
                                 }

                                 internal int N(int x, int y) => x;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(int a, int b, int? n)
                                    {
                                        System.Func<int> f = () =>

                                        /** d */ a;

                                        return f();
                                    }

                                    internal int N(int x, int y)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a named argument starts a new line and
    /// the colon stays at the end of the line in front of it
    /// </summary>
    [TestMethod]
    public void MovesDelimitedDocumentationBehindNamedArgumentColon()
    {
        // Arrange
        const string input = """
                             internal class C
                             {
                                 internal int M(int a, int b, int? n)
                                 {
                                     return N(x: a, y: /** d */ b);
                                 }

                                 internal int N(int x, int y) => x;
                             }
                             """;
        const string expected = """
                                internal class C
                                {
                                    internal int M(int a, int b, int? n)
                                    {
                                        return N(x: a,
                                                 y:

                                                 /** d */ b);
                                    }

                                    internal int N(int x, int y)
                                    {
                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}