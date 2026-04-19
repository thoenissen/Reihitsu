using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class BlankLineBeforeStatementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a blank line is inserted before an <c>if</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void IfStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     if (x > 0)
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
                                        var x = 1;

                                        if (x > 0)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an intentional blank line before a suppression comment is preserved.
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeSuppressionCommentIsPreserved()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;
                             using System.Linq;

                             class ValidationService
                             {
                                 void Initialize()
                                 {
                                     var map = GetItems().Where(item => item.RoleId != null)

                                                         // ReSharper disable once PossibleInvalidOperationException
                                                         .ToDictionary(item => item.Id, item => item.RoleId.Value);
                                 }

                                 IEnumerable<ItemRecord> GetItems()
                                 {
                                     return new List<ItemRecord>();
                                 }
                             }

                             class ItemRecord
                             {
                                 public ulong? RoleId { get; set; }
                                 public ulong Id { get; set; }
                             }
                             """;

        const string expected = input;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that blank lines are inserted before statements inside
    /// a statement lambda expression used in LINQ.
    /// </summary>
    [TestMethod]
    public void StatementLambdaInLinqInsertsBlankLinesBeforeStatements()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int[] M()
                                 {
                                     var values = System.Linq.Enumerable.Select(new[] { 1, 2 },
                                                                                x =>
                                                                                {
                                                                                    var y = x;
                                                                                    if (y > 1)
                                                                                    {
                                                                                        y++;
                                                                                    }
                                                                                    return y;
                                                                                });
                                     return System.Linq.Enumerable.ToArray(values);
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int[] M()
                                    {
                                        var values = System.Linq.Enumerable.Select(new[] { 1, 2 },
                                                                                   x =>
                                                                                   {
                                                                                       var y = x;

                                                                                       if (y > 1)
                                                                                       {
                                                                                           y++;
                                                                                       }

                                                                                       return y;
                                                                                   });

                                        return System.Linq.Enumerable.ToArray(values);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted when an <c>if</c> statement
    /// is the first statement in a block.
    /// </summary>
    [TestMethod]
    public void IfStatementFirstInBlockNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     if (true)
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no additional blank line is inserted before an
    /// <c>else if</c> clause.
    /// </summary>
    [TestMethod]
    public void ElseIfNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     if (true)
                                     {
                                     }
                                     else if (false)
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>try</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void TryStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     try
                                     {
                                     }
                                     catch
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
                                        var x = 1;

                                        try
                                        {
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>while</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void WhileStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     while (x > 0)
                                     {
                                         x--;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        while (x > 0)
                                        {
                                            x--;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>do</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void DoStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     do
                                     {
                                         x--;
                                     }
                                     while (x > 0);
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        do
                                        {
                                            x--;
                                        }
                                        while (x > 0);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>for</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ForStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     for (var i = 0; i < x; i++)
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
                                        var x = 1;

                                        for (var i = 0; i < x; i++)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>foreach</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ForEachStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var items = new int[0];
                                     foreach (var item in items)
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
                                        var items = new int[0];

                                        foreach (var item in items)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>return</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ReturnStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     var x = 1;
                                     return x;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    int M()
                                    {
                                        var x = 1;

                                        return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted when a <c>return</c> statement
    /// is the first statement in a block.
    /// </summary>
    [TestMethod]
    public void ReturnStatementFirstInBlockNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M()
                                 {
                                     return 1;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>throw</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ThrowStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     throw new System.Exception();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        throw new System.Exception();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>break</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void BreakStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     while (true)
                                     {
                                         var x = 1;
                                         break;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        while (true)
                                        {
                                            var x = 1;

                                            break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>continue</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void ContinueStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     while (true)
                                     {
                                         var x = 1;
                                         continue;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        while (true)
                                        {
                                            var x = 1;

                                            continue;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>goto</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void GotoStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     goto end;
                                     end:
                                     return;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        goto end;
                                        end:
                                        return;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>switch</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void SwitchStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     switch (x)
                                     {
                                         default:
                                             break;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        switch (x)
                                        {
                                            default:
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>checked</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void CheckedStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     checked
                                     {
                                         x++;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1;

                                        checked
                                        {
                                            x++;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>fixed</c> statement
    /// that follows another statement inside an <c>unsafe</c> context.
    /// </summary>
    [TestMethod]
    public void FixedStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 unsafe void M()
                                 {
                                     var arr = new int[1];
                                     fixed (int* p = arr)
                                     {
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    unsafe void M()
                                    {
                                        var arr = new int[1];

                                        fixed (int* p = arr)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>lock</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void LockStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var obj = new object();
                                     lock (obj)
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
                                        var obj = new object();

                                        lock (obj)
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>using</c> statement
    /// that follows another statement.
    /// </summary>
    [TestMethod]
    public void UsingStatementInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;
                                     using (var s = new System.IO.MemoryStream())
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
                                        var x = 1;

                                        using (var s = new System.IO.MemoryStream())
                                        {
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted before a <c>yield return</c>
    /// statement that is the first yield in a method.
    /// </summary>
    [TestMethod]
    public void YieldReturnInsertsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 System.Collections.Generic.IEnumerable<int> M()
                                 {
                                     var x = 1;
                                     yield return x;
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    System.Collections.Generic.IEnumerable<int> M()
                                    {
                                        var x = 1;

                                        yield return x;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line is inserted between consecutive
    /// <c>yield return</c> statements.
    /// </summary>
    [TestMethod]
    public void YieldReturnAfterYieldReturnNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 System.Collections.Generic.IEnumerable<int> M()
                                 {
                                     yield return 1;
                                     yield return 2;
                                     yield return 3;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no duplicate blank line is inserted when a blank line
    /// already exists before the statement.
    /// </summary>
    [TestMethod]
    public void AlreadyHasBlankLineNoDoubleInsert()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1;

                                     if (x > 0)
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}