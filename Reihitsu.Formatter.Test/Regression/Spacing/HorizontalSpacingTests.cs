using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Spacing;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class HorizontalSpacingTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a single space is ensured around the binary plus operator.
    /// </summary>
    [TestMethod]
    public void BinaryOperatorPlusEnsuresSingleSpace()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(int a, int b)
                {
                    var y = a+b;
                }
            }
            """;
        const string expected = """
            class C
            {
                void M(int a, int b)
                {
                    var y = a + b;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single space is ensured around the binary minus operator.
    /// </summary>
    [TestMethod]
    public void BinaryOperatorMinusEnsuresSingleSpace()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(int a, int b)
                {
                    var y = a-b;
                }
            }
            """;
        const string expected = """
            class C
            {
                void M(int a, int b)
                {
                    var y = a - b;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single space is ensured around the simple assignment operator.
    /// </summary>
    [TestMethod]
    public void AssignmentOperatorEnsuresSingleSpace()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    int x;
                    x=1;
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    int x;
                    x = 1;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single space is ensured around compound assignment operators.
    /// </summary>
    [TestMethod]
    public void CompoundAssignmentEnsuresSingleSpace()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    int x = 0;
                    x+=1;
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    int x = 0;
                    x += 1;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single space is ensured around the equals sign in a variable declaration.
    /// </summary>
    [TestMethod]
    public void EqualsValueClauseEnsuresSingleSpace()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x=1;
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var x = 1;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a space is ensured after a comma token.
    /// </summary>
    [TestMethod]
    public void CommaTokenEnsuresSpaceAfter()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(int a,int b)
                {
                }
            }
            """;
        const string expected = """
            class C
            {
                void M(int a, int b)
                {
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a space is ensured after semicolons inside a for-statement header.
    /// </summary>
    [TestMethod]
    public void SemicolonInForEnsuresSpaceAfter()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    for (int i = 0;i < 10;i++)
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
                    for (int i = 0; i < 10; i++)
                    {
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that semicolons outside a for-statement are not modified.
    /// </summary>
    [TestMethod]
    public void SemicolonOutsideForNoChange()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    int x = 1;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a space is ensured after the <c>if</c> keyword.
    /// </summary>
    [TestMethod]
    public void IfKeywordEnsuresSpaceAfter()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(bool x)
                {
                    if(x)
                    {
                    }
                }
            }
            """;
        const string expected = """
            class C
            {
                void M(bool x)
                {
                    if (x)
                    {
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a space is ensured after the <c>for</c> keyword.
    /// </summary>
    [TestMethod]
    public void ForKeywordEnsuresSpaceAfter()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    for(int i = 0; i < 10; i++)
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
                    for (int i = 0; i < 10; i++)
                    {
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a space is ensured after the <c>foreach</c> keyword.
    /// </summary>
    [TestMethod]
    public void ForEachKeywordEnsuresSpaceAfter()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(int[] items)
                {
                    foreach(var x in items)
                    {
                    }
                }
            }
            """;
        const string expected = """
            class C
            {
                void M(int[] items)
                {
                    foreach (var x in items)
                    {
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a space is ensured after the <c>while</c> keyword.
    /// </summary>
    [TestMethod]
    public void WhileKeywordEnsuresSpaceAfter()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(bool x)
                {
                    while(x)
                    {
                    }
                }
            }
            """;
        const string expected = """
            class C
            {
                void M(bool x)
                {
                    while (x)
                    {
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a space is ensured after the <c>return</c> keyword before an expression.
    /// </summary>
    [TestMethod]
    public void ReturnKeywordEnsuresSpaceAfter()
    {
        // Arrange
        const string input = """
            class C
            {
                int M(int x)
                {
                    return(x);
                }
            }
            """;
        const string expected = """
            class C
            {
                int M(int x)
                {
                    return (x);
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no space is added after the <c>return</c> keyword when followed by a semicolon.
    /// </summary>
    [TestMethod]
    public void ReturnKeywordBeforeSemicolonNoSpace()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    return;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no space is added after the <c>throw</c> keyword when followed by a semicolon.
    /// </summary>
    [TestMethod]
    public void ThrowKeywordBeforeSemicolonNoSpace()
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
                    catch
                    {
                        throw;
                    }
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the <c>new</c> keyword does not add a space before <c>(</c> in target-typed new expressions.
    /// </summary>
    [TestMethod]
    public void NewKeywordDoesNotAddSpaceBeforeParenInTargetTypedNew()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    object x = new();
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a space is ensured after the <c>new</c> keyword when followed by a type name.
    /// </summary>
    [TestMethod]
    public void NewKeywordEnsuresSpaceBeforeTypeName()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = new object();
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that anonymous type creation in a local variable is spaced correctly.
    /// </summary>
    [TestMethod]
    public void AnonymousTypeLocalVariableEnsuresSpacing()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var person = new
                    {
                        FirstName="Max",
                        LastName="Mustermann"
                    };
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var person = new
                                 {
                                     FirstName = "Max",
                                     LastName = "Mustermann"
                                 };
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that anonymous type creation in a LINQ <c>Select</c> projection is spaced correctly.
    /// </summary>
    [TestMethod]
    public void AnonymousTypeInLinqSelectEnsuresSpacing()
    {
        // Arrange
        const string input = """
            using System.Linq;

            class C
            {
                void M(int[] items)
                {
                    var result = items.Select(x => new
                    {
                        Value=x,
                        Text=x.ToString()
                    });
                }
            }
            """;
        const string expected = """
            using System.Linq;

            class C
            {
                void M(int[] items)
                {
                    var result = items.Select(x => new
                                                   {
                                                       Value = x,
                                                       Text = x.ToString()
                                                   });
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that inferred anonymous type members are spaced correctly.
    /// </summary>
    [TestMethod]
    public void AnonymousTypeWithInferredMembersEnsuresSpacing()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var firstName = "Max";
                    var lastName = "Mustermann";
                    var person = new
                    {
                        firstName,
                        lastName
                    };
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var firstName = "Max";
                    var lastName = "Mustermann";
                    var person = new
                                 {
                                     firstName,
                                     lastName
                                 };
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that explicit anonymous type member names are spaced correctly.
    /// </summary>
    [TestMethod]
    public void AnonymousTypeWithExplicitMemberNamesEnsuresSpacing()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var firstName = "Max";
                    var person = new
                                 {
                                     Name=firstName
                                 };
                }
            }
            """;
        const string expected = """
            class C
            {
                void M()
                {
                    var firstName = "Max";
                    var person = new
                                 {
                                     Name = firstName
                                 };
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a space after an opening parenthesis is removed.
    /// </summary>
    [TestMethod]
    public void OpenParenRemovesSpaceAfter()
    {
        // Arrange
        const string input = """
            class C
            {
                void M( int x)
                {
                }
            }
            """;
        const string expected = """
            class C
            {
                void M(int x)
                {
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a space before a closing parenthesis is removed.
    /// </summary>
    [TestMethod]
    public void CloseParenRemovesSpaceBefore()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(int x )
                {
                }
            }
            """;
        const string expected = """
            class C
            {
                void M(int x)
                {
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that multiple consecutive spaces are normalized to a single space.
    /// </summary>
    [TestMethod]
    public void MultipleSpacesNormalizedToSingle()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(int a, int b)
                {
                    var y = a  +  b;
                }
            }
            """;
        const string expected = """
            class C
            {
                void M(int a, int b)
                {
                    var y = a + b;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line binary expression is not modified by horizontal spacing.
    /// </summary>
    [TestMethod]
    public void MultiLineExpressionNoSpacingChange()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(int a, int b)
                {
                    var y = a
                            + b;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that already correctly spaced code is not modified.
    /// </summary>
    [TestMethod]
    public void AlreadyCorrectSpacingNoChange()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(int a, int b)
                {
                    var y = a + b;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the space between new and () in a generic constraint is removed.
    /// </summary>
    [TestMethod]
    public void NewConstraintSpaceRemoved()
    {
        // Arrange
        const string input = """
            class C
            {
                T M<T>()
                    where T : new ()
                {
                    return new T();
                }
            }
            """;

        const string expected = """
            class C
            {
                T M<T>()
                    where T : new()
                {
                    return new T();
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}