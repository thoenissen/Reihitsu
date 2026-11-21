using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Full-pipeline tests for delimited documentation comments written behind an operator inside an expression: behind the
/// <c>?</c> or <c>:</c> of a conditional expression, which the formatter refuses as unsupported (FMT-UNSUP-008), behind a
/// binary operator, which it refuses as unsupported as well (FMT-UNSUP-011), and behind other operators, where the operator stays
/// at the end of the line in front of the moved comment
/// </summary>
[TestClass]
public class DelimitedDocumentationInExpressionTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a one-line conditional in an expression body is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindColonOfOneLineConditionalInExpressionBodyIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? 1 : /** d */ 2;
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a one-line conditional in a return statement is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindColonOfOneLineConditionalInReturnStatementIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? 1 : /** d */ 2;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment behind the <c>:</c> of a conditional in an expression body is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindBlockCommentBehindColonOfConditionalInExpressionBodyIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? 1 : /* c */ /** d */ 2;
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/* c */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment behind the <c>:</c> of a conditional in a return statement is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindBlockCommentBehindColonOfConditionalInReturnStatementIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? 1 : /* c */ /** d */ 2;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/* c */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a one-line conditional in an expression body is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindQuestionMarkOfOneLineConditionalInExpressionBodyIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? /** d */ 1 : 2;
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a one-line conditional in a return statement is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindQuestionMarkOfOneLineConditionalInReturnStatementIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? /** d */ 1 : 2;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment behind the <c>?</c> of a conditional in an expression body is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindBlockCommentBehindQuestionMarkOfConditionalInExpressionBodyIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? /* c */ /** d */ 1 : 2;
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/* c */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a block comment behind the <c>?</c> of a conditional in a return statement is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindBlockCommentBehindQuestionMarkOfConditionalInReturnStatementIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? /* c */ /** d */ 1 : 2;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/* c */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a partly wrapped conditional in an expression body is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindColonOfPartlyWrappedConditionalInExpressionBodyIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b
                                     ? 1 : /** d */ 2;
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a partly wrapped conditional in a return statement is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindColonOfPartlyWrappedConditionalInReturnStatementIsUnsupported()
    {
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

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a partly wrapped conditional in an expression body is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindQuestionMarkOfPartlyWrappedConditionalInExpressionBodyIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b) => b ? /** d */ 1
                                     : 2;
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a partly wrapped conditional in a return statement is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindQuestionMarkOfPartlyWrappedConditionalInReturnStatementIsUnsupported()
    {
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

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a parenthesized nested conditional in an expression body is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindColonOfParenthesizedNestedConditionalInExpressionBodyIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c) => b ? (c ? 1 : /** d */ 2) : 3;
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> of a parenthesized nested conditional in a return statement is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindColonOfParenthesizedNestedConditionalInReturnStatementIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? (c ? 1 : /** d */ 2) : 3;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a nested conditional in an expression body is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindQuestionMarkOfNestedConditionalInExpressionBodyIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c) => b ? 1 : c ? /** d */ 2 : 3;
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>?</c> of a nested conditional in a return statement is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindQuestionMarkOfNestedConditionalInReturnStatementIsUnsupported()
    {
        const string input = """
                             internal class C
                             {
                                 internal int M(bool b, bool c)
                                 {
                                     return b ? 1 : c ? /** d */ 2 : 3;
                                 }
                             }
                             """;

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> starting its line in a return statement is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindColonStartingItsLineInReturnStatementIsUnsupported()
    {
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

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind the <c>:</c> starting its line in a local declaration is a comment between a
    /// conditional operator and its branch: the input is refused as unsupported, and the first comment behind the operator is
    /// reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-008")]
    public void DelimitedDocumentationBehindColonStartingItsLineInLocalDeclarationIsUnsupported()
    {
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

        AssertUnsupportedFor(ConditionalOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a <c>+</c> is a comment between a binary operator and its right
    /// operand: the input is refused as unsupported, and the comment is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    public void DelimitedDocumentationBehindAdditionOperatorIsUnsupported()
    {
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

        AssertUnsupportedFor(BinaryOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a <c>??</c> is a comment between a binary operator and its right
    /// operand: the input is refused as unsupported, and the comment is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    public void DelimitedDocumentationBehindCoalesceOperatorIsUnsupported()
    {
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

        AssertUnsupportedFor(BinaryOperatorCommentReason, input, "/** d */");
    }

    /// <summary>
    /// Verifies that a delimited documentation comment behind a <c>&amp;&amp;</c> is a comment between a binary operator and its right
    /// operand: the input is refused as unsupported, and the comment is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-011")]
    public void DelimitedDocumentationBehindConditionalAndOperatorIsUnsupported()
    {
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

        AssertUnsupportedFor(BinaryOperatorCommentReason, input, "/** d */");
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
    /// Verifies that a delimited documentation comment behind the <c>=&gt;</c> of a lambda refuses the input as unsupported,
    /// because the line-break phase joins the gap behind the arrow
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-017")]
    public void DelimitedDocumentationBehindLambdaArrowIsUnsupported()
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

        // Act & Assert
        AssertUnsupportedFor(LambdaArrowCommentReason, input, "/** d */");
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