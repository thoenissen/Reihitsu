using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Test.Helpers;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Tests verifying that converting a throw statement into an expression body never produces code that re-binds a
/// low-precedence operand
/// </summary>
[TestClass]
public class ThrowOperandPrecedenceExpressionBodyTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the reported conditional getter operand and assignment setter operand are parenthesized
    /// </summary>
    [TestMethod]
    public void ConditionalGetterAndAssignmentSetterOperandsAreParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;
                                 private System.Exception _exception;

                                 public int X
                                 {
                                     get { throw _condition ? new System.Exception() : new System.InvalidOperationException(); }
                                 }

                                 public int Y
                                 {
                                     set { throw _exception = new System.Exception(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;
                                    private System.Exception _exception;

                                    public int X => throw (_condition ? new System.Exception() : new System.InvalidOperationException());

                                    public int Y
                                    {
                                        set => throw (_exception = new System.Exception());
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional getter operand alone is parenthesized
    /// </summary>
    [TestMethod]
    public void ConditionalGetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get { throw _condition ? new System.Exception() : new System.InvalidOperationException(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X => throw (_condition ? new System.Exception() : new System.InvalidOperationException());
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an assignment setter operand alone is parenthesized
    /// </summary>
    [TestMethod]
    public void AssignmentSetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private System.Exception _exception;

                                 public int Y
                                 {
                                     set { throw _exception = new System.Exception(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int Y
                                    {
                                        set => throw (_exception = new System.Exception());
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an already parenthesized conditional getter operand keeps exactly one pair of parentheses
    /// </summary>
    [TestMethod]
    public void ParenthesizedConditionalGetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get { throw (_condition ? new System.Exception() : new System.InvalidOperationException()); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X => throw (_condition ? new System.Exception() : new System.InvalidOperationException());
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a null-coalescing assignment setter operand is parenthesized
    /// </summary>
    [TestMethod]
    public void CoalesceAssignmentSetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private System.Exception _exception;

                                 public int Y
                                 {
                                     set { throw _exception ??= new System.Exception(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int Y
                                    {
                                        set => throw (_exception ??= new System.Exception());
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a query operand in a getter is parenthesized, because the bare form draws CS8848 as a throw
    /// expression operand
    /// </summary>
    [TestMethod]
    public void QueryGetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private Source _source;

                                 public int X
                                 {
                                     get { throw from item in _source select item; }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private Source _source;

                                    public int X => throw (from item in _source select item);
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional operand in a block-bodied method is left as a block body
    /// </summary>
    [TestMethod]
    public void ConditionalMethodOperandKeepsBlockBody()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int M()
                                 {
                                     throw _condition ? new System.Exception() : new System.InvalidOperationException();
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a compound assignment setter operand is parenthesized as a whole
    /// </summary>
    [TestMethod]
    public void CompoundAssignmentSetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private int _count;

                                 public int Y
                                 {
                                     set { throw _count += value; }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private int _count;

                                    public int Y
                                    {
                                        set => throw (_count += value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an assignment operand in an init accessor is parenthesized
    /// </summary>
    [TestMethod]
    public void AssignmentInitOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private System.Exception _exception;

                                 public int Y
                                 {
                                     init { throw _exception = new System.Exception(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int Y
                                    {
                                        init => throw (_exception = new System.Exception());
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional getter operand is parenthesized when the getter stays beside a setter
    /// </summary>
    [TestMethod]
    public void ConditionalGetterOperandBesideSetterIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get { throw _condition ? new System.Exception() : new System.InvalidOperationException(); }
                                     set { }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X
                                    {
                                        get => throw (_condition ? new System.Exception() : new System.InvalidOperationException());
                                        set
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional operand of a get-only indexer getter is parenthesized when the getter is lifted
    /// </summary>
    [TestMethod]
    public void ConditionalGetOnlyIndexerOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int this[int index]
                                 {
                                     get { throw _condition ? new System.Exception() : new System.InvalidOperationException(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int this[int index] => throw (_condition ? new System.Exception() : new System.InvalidOperationException());
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an assignment operand of an indexer setter is parenthesized
    /// </summary>
    [TestMethod]
    public void AssignmentIndexerSetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private System.Exception _exception;

                                 public int this[int index]
                                 {
                                     get => index;
                                     set { throw _exception = new System.Exception(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int this[int index]
                                    {
                                        get => index;
                                        set => throw (_exception = new System.Exception());
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a simple lambda operand is parenthesized, because the bare form does not parse as a throw
    /// expression
    /// </summary>
    [TestMethod]
    public void SimpleLambdaGetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 public int X
                                 {
                                     get { throw x => x; }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    public int X => throw (x => x);
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parenthesized lambda operand is parenthesized, because the bare form does not parse as a
    /// throw expression
    /// </summary>
    [TestMethod]
    public void ParenthesizedLambdaGetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 public int X
                                 {
                                     get { throw () => new System.Exception(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    public int X => throw (() => new System.Exception());
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an anonymous method operand stays bare, because it parses as a primary term
    /// </summary>
    [TestMethod]
    public void AnonymousMethodGetterOperandStaysBare()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 public int X
                                 {
                                     get { throw delegate { }; }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    public int X => throw delegate
                                    {
                                    };
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a query operand in a setter is parenthesized
    /// </summary>
    [TestMethod]
    public void QuerySetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private Source _source;

                                 public int Y
                                 {
                                     set { throw from item in _source select item; }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private Source _source;

                                    public int Y
                                    {
                                        set => throw (from item in _source select item);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a query below a top-level null-coalescing operator stays bare, because only the operand's
    /// top-level node decides
    /// </summary>
    [TestMethod]
    public void QueryBelowCoalesceGetterOperandStaysBare()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private System.Exception _exception;
                                 private Source _source;

                                 public int X
                                 {
                                     get { throw _exception ?? from item in _source select item; }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private System.Exception _exception;
                                    private Source _source;

                                    public int X => throw _exception ?? from item in _source select item;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an already parenthesized query operand keeps exactly one pair of parentheses
    /// </summary>
    [TestMethod]
    public void ParenthesizedQueryGetterOperandKeepsOnePair()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private Source _source;

                                 public int X
                                 {
                                     get { throw (from item in _source select item); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private Source _source;

                                    public int X => throw (from item in _source select item);
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a null-coalescing operand stays bare, because it binds as tightly as a throw expression's operand
    /// </summary>
    [TestMethod]
    public void CoalesceGetterOperandStaysBare()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private System.Exception _exception;

                                 public int X
                                 {
                                     get { throw _exception ?? new System.Exception(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int X => throw _exception ?? new System.Exception();
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional below a top-level null-coalescing operator stays as written
    /// </summary>
    [TestMethod]
    public void ConditionalBelowCoalesceGetterOperandStaysBare()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;
                                 private System.Exception _exception;

                                 public int X
                                 {
                                     get { throw _exception ?? (_condition ? new System.Exception() : null); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;
                                    private System.Exception _exception;

                                    public int X => throw _exception ?? (_condition ? new System.Exception() : null);
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>as</c> operand stays bare, because it binds more tightly than a throw expression's operand
    /// </summary>
    [TestMethod]
    public void AsGetterOperandStaysBare()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private object _value;

                                 public int X
                                 {
                                     get { throw _value as System.Exception; }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private object _value;

                                    public int X => throw _value as System.Exception;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a switch expression operand stays bare, because it binds more tightly than a throw expression's
    /// operand
    /// </summary>
    [TestMethod]
    public void SwitchExpressionGetterOperandStaysBare()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private int _value;

                                 public int X
                                 {
                                     get { throw _value switch { 0 => new System.Exception(), _ => new System.InvalidOperationException() }; }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private int _value;

                                    public int X => throw _value switch
                                                          {
                                                              0 => new System.Exception(), _ => new System.InvalidOperationException()
                                                          };
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nested conditional operand receives exactly one outer pair of parentheses
    /// </summary>
    [TestMethod]
    public void NestedConditionalGetterOperandGetsOneOuterPair()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _first;
                                 private bool _second;

                                 public int X
                                 {
                                     get { throw _first ? new System.Exception() : _second ? new System.ArgumentException() : new System.InvalidOperationException(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _first;
                                    private bool _second;

                                    public int X => throw (_first
                                                               ? new System.Exception()
                                                               : _second
                                                                   ? new System.ArgumentException()
                                                                   : new System.InvalidOperationException());
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment after the throw keyword stays between the keyword and the opening parenthesis
    /// </summary>
    [TestMethod]
    public void CommentAfterThrowKeywordStaysBeforeOpeningParenthesis()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get { throw /* Comment */ _condition ? new System.Exception() : new System.InvalidOperationException(); }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X => throw /* Comment */ (_condition ? new System.Exception() : new System.InvalidOperationException());
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment trailing the operand follows the closing parenthesis
    /// </summary>
    [TestMethod]
    public void BlockCommentBeforeSemicolonFollowsClosingParenthesis()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get { throw _condition ? new System.Exception() : new System.InvalidOperationException() /* Comment */; }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X => throw (_condition ? new System.Exception() : new System.InvalidOperationException()) /* Comment */;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line comment trailing the operand keeps the block body, because the comment would
    /// otherwise end up inside the parentheses
    /// </summary>
    [TestMethod]
    public void LineCommentBeforeSemicolonKeepsBlockBody()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get
                                     {
                                         throw _condition ? new System.Exception() : new System.InvalidOperationException() // Comment
                                         ;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment trailing the closing brace follows the new semicolon after the closing parenthesis
    /// </summary>
    [TestMethod]
    public void CommentAfterClosingBraceFollowsSemicolon()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private System.Exception _exception;

                                 public int Y
                                 {
                                     set { throw _exception = new System.Exception(); } // Comment
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private System.Exception _exception;

                                    public int Y
                                    {
                                        set => throw (_exception = new System.Exception()); // Comment
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional operand spanning several lines is parenthesized and aligned like authored
    /// parentheses
    /// </summary>
    [TestMethod]
    public void MultiLineConditionalGetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get
                                     {
                                         throw _condition
                                             ? new System.Exception()
                                             : new System.InvalidOperationException();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X => throw (_condition
                                                               ? new System.Exception()
                                                               : new System.InvalidOperationException());
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line comment inside a multi-line conditional operand stays inside the parentheses
    /// </summary>
    [TestMethod]
    public void InteriorLineCommentConditionalGetterOperandIsParenthesized()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get
                                     {
                                         throw _condition // Comment
                                             ? new System.Exception()
                                             : new System.InvalidOperationException();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X => throw (_condition // Comment
                                                               ? new System.Exception()
                                                               : new System.InvalidOperationException());
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an operand on the line after the throw keyword, preceded by a comment, is formatted exactly like
    /// the same operand written with parentheses
    /// </summary>
    [TestMethod]
    public void NextLineOperandWithCommentFormatsLikeAuthoredParentheses()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get
                                     {
                                         throw
                                             // Comment
                                             _condition ? new System.Exception() : new System.InvalidOperationException();
                                     }
                                 }
                             }
                             """;
        const string authored = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X
                                    {
                                        get
                                        {
                                            throw
                                                // Comment
                                                (_condition ? new System.Exception() : new System.InvalidOperationException());
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, Format(authored));
    }

    /// <summary>
    /// Verifies that a conditional operand carrying a balanced conditional-compilation block is formatted exactly like
    /// the same operand written with parentheses, keeping the directives and the disabled text inside them
    /// </summary>
    [TestMethod]
    public void DirectiveInsideOperandFormatsLikeAuthoredParentheses()
    {
        // Arrange
        const string input = """
                             public class Test
                             {
                                 private bool _condition;

                                 public int X
                                 {
                                     get
                                     {
                                         throw _condition
                             #if DEBUG
                                             ? new System.Exception()
                             #else
                                             ? new System.ArgumentException()
                             #endif
                                             : new System.InvalidOperationException();
                                     }
                                 }
                             }
                             """;
        const string authored = """
                                public class Test
                                {
                                    private bool _condition;

                                    public int X
                                    {
                                        get
                                        {
                                            throw (_condition
                                #if DEBUG
                                                ? new System.Exception()
                                #else
                                                ? new System.ArgumentException()
                                #endif
                                                : new System.InvalidOperationException());
                                        }
                                    }
                                }
                                """;
        var expected = Format(authored);

        // Act & Assert
        Assert.Contains("public int X => throw (_condition", expected);
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Formats the source with LF line endings, so a test can compare a bare operand against the formatter's own
    /// output for the same operand written with parentheses
    /// </summary>
    /// <param name="source">The source text to format</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(source, "\n"), cancellationToken: CancellationToken.None);
        var context = new FormattingContext("\n", languageVersion: LanguageVersionResolver.Resolve(tree.Options));

        return FormattingPipeline.Execute(tree.GetRoot(CancellationToken.None), context, CancellationToken.None).ToFullString();
    }

    #endregion // Methods
}