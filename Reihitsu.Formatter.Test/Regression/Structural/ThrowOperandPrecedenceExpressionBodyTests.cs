using System.Linq;
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
    /// Verifies that the reported conditional getter operand and assignment setter operand stay compilable
    /// </summary>
    [TestMethod]
    public void ConditionalGetterAndAssignmentSetterOperandsStayCompilable()
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
    /// Verifies that a conditional getter operand alone stays compilable
    /// </summary>
    [TestMethod]
    public void ConditionalGetterOperandStaysCompilable()
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
    /// Verifies that an assignment setter operand alone stays compilable
    /// </summary>
    [TestMethod]
    public void AssignmentSetterOperandStaysCompilable()
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
    /// Verifies that an already parenthesized conditional getter operand stays compilable
    /// </summary>
    [TestMethod]
    public void ParenthesizedConditionalGetterOperandStaysCompilable()
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
    /// Verifies that a null-coalescing assignment setter operand stays compilable
    /// </summary>
    [TestMethod]
    public void CoalesceAssignmentSetterOperandStaysCompilable()
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
    /// Verifies that a query operand in a getter stays compilable
    /// </summary>
    [TestMethod]
    public void QueryGetterOperandStaysCompilable()
    {
        // Arrange
        const string input = """
                             using System.Linq;

                             public class Test
                             {
                                 private System.Exception[] _exceptions;

                                 public int X
                                 {
                                     get { throw from e in _exceptions select e; }
                                 }
                             }
                             """;
        const string expected = """
                                using System.Linq;

                                public class Test
                                {
                                    private System.Exception[] _exceptions;

                                    public int X => throw (from e in _exceptions select e);
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
    /// Verifies under CRLF line endings that the formatted reported getter and setter carry no bare low-precedence throw operand
    /// </summary>
    [TestMethod]
    public void ReportedInputFormattedWithCrlfHasNoBareLowPrecedenceThrowOperand()
    {
        // Arrange
        var input = NormalizeLineEndings("""
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
                                         """,
                                         "\r\n");
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: CancellationToken.None);
        var context = new FormattingContext("\r\n", languageVersion: LanguageVersionResolver.Resolve(tree.Options));

        // Act
        var actual = FormattingPipeline.Execute(tree.GetRoot(CancellationToken.None), context, CancellationToken.None).ToFullString();

        // Assert
        Assert.IsFalse(actual.Contains("throw _condition ?") || actual.Contains("throw _exception ="), $"Output: {actual.Replace("\r\n", "\\r\\n")}");
    }

    #endregion // Methods
}