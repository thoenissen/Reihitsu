using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// A line inside a wrapped argument-like list that a comment or directive keeps from being joined - a leading separator, or a
/// line inside an argument that starts at a token other than the argument's first token - is aligned with the list instead
/// of the enclosing statement's block column
/// </summary>
[TestClass]
public class ArgumentContinuationLineAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a leading comma of a tuple expression kept on its own line by a line comment is aligned with the
    /// tuple's first element
    /// </summary>
    [TestMethod]
    public void TupleLeadingCommaAfterLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var pair = (1 // keep
                                                 , 2);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a leading comma of a tuple expression placed at the block column is moved under the tuple's first element
    /// </summary>
    [TestMethod]
    public void TupleLeadingCommaAtBlockColumnIsAlignedWithTheElements()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var pair = (1 // keep
                                     , 2);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        var pair = (1 // keep
                                                    , 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line inside a tuple element that a line comment holds is aligned with the element's first token
    /// </summary>
    [TestMethod]
    public void TupleElementInteriorLineHeldByLineCommentIsAlignedWithTheElement()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var pair = (- // keep
                                     1, 2);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        var pair = (- // keep
                                                    1, 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma of an argument list kept on its own line by a line comment is aligned with the arguments
    /// </summary>
    [TestMethod]
    public void ArgumentLeadingCommaAfterLineCommentIsAlignedWithTheArguments()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(1 // keep
                                     , 2);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(1 // keep
                                          , 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma of an argument list kept on its own line by a block comment is aligned with the arguments
    /// </summary>
    [TestMethod]
    public void ArgumentLeadingCommaAfterBlockCommentIsAlignedWithTheArguments()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(1 /* keep */
                                     , 2);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(1 /* keep */
                                          , 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma following a pragma directive is aligned with the arguments while the directive keeps its line
    /// </summary>
    [TestMethod]
    public void ArgumentLeadingCommaAfterPragmaDirectiveIsAlignedWithTheArguments()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(1
                                     #pragma warning disable CS0168
                                     , 2);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(1
                                #pragma warning disable CS0168
                                          , 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma of an object creation's argument list is aligned with that list's arguments
    /// </summary>
    [TestMethod]
    public void ArgumentLeadingCommaOfObjectCreationIsAlignedWithTheArguments()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var value = new Example(1 // keep
                                     , 2);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        var value = new Example(1 // keep
                                                                , 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma of a constructor initializer's argument list is aligned with that list's arguments
    /// </summary>
    [TestMethod]
    public void ArgumentLeadingCommaOfConstructorInitializerIsAlignedWithTheArguments()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal Example()
                                     : this(1 // keep
                                 , 2)
                                 {
                                 }

                                 internal Example(int first, int second)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal Example()
                                        : this(1 // keep
                                               , 2)
                                    {
                                    }

                                    internal Example(int first, int second)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line inside an argument that a line comment holds is aligned with the argument's first token
    /// </summary>
    [TestMethod]
    public void ArgumentInteriorLineHeldByLineCommentIsAlignedWithTheArgument()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(out // keep
                                     var value);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(out // keep
                                          var value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a held line inside a wrapped second argument is aligned with that argument's first token
    /// </summary>
    [TestMethod]
    public void ArgumentInteriorLineOfWrappedSecondArgumentIsAlignedWithThatArgument()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(1,
                                       out // keep
                                     var value);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(1,
                                          out // keep
                                          var value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument which starts a line is still aligned with the column after the opening parenthesis
    /// </summary>
    [TestMethod]
    public void WrappedArgumentStartStaysAlignedWithTheOpenParenthesis()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(1,
                                       out var value);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a leading comma of an element access argument list is aligned with the column after the opening bracket
    /// </summary>
    [TestMethod]
    public void ElementAccessLeadingCommaIsAlignedWithTheArguments()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var value = values[0 // keep
                                     , 1];
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        var value = values[0 // keep
                                                           , 1];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a held line inside an element access argument is aligned with the argument's first token
    /// </summary>
    [TestMethod]
    public void ElementAccessInteriorLineHeldByLineCommentIsAlignedWithTheArgument()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var value = values[^ // keep
                                     1];
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        var value = values[^ // keep
                                                           1];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma of a member attribute's argument list is aligned with the attribute arguments
    /// </summary>
    [TestMethod]
    public void MemberAttributeLeadingCommaIsAlignedWithTheArguments()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 [Obsolete("a" // keep
                                 , false)]
                                 internal void M()
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    [Obsolete("a" // keep
                                              , false)]
                                    internal void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma of a parameter attribute's argument list is aligned with that attribute's arguments rather than the parameter
    /// </summary>
    [TestMethod]
    public void ParameterAttributeLeadingCommaIsAlignedWithTheArguments()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete("a" // keep
                                 , false)] int value)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    internal void M([Obsolete("a" // keep
                                                              , false)] int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a named attribute value that a line comment keeps on its own line is aligned under the argument name
    /// </summary>
    [TestMethod]
    public void MemberAttributeNamedValueHeldByLineCommentIsAlignedUnderTheName()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 [Obsolete(message: // keep
                                 "x")]
                                 internal void M()
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    [Obsolete(message: // keep
                                              "x")]
                                    internal void M()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a named attribute value of a parameter attribute that a line comment keeps on its own line is aligned under the argument name
    /// </summary>
    [TestMethod]
    public void ParameterAttributeNamedValueHeldByLineCommentIsAlignedUnderTheName()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete(message: // keep
                                 "x")] int value)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    internal void M([Obsolete(message: // keep
                                                              "x")] int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line inside a type argument that a line comment holds is aligned with the type argument's first token
    /// </summary>
    [TestMethod]
    public void TypeArgumentInteriorLineHeldByLineCommentIsAlignedWithTheTypeArgument()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 private System.Collections.Generic.List<System. // keep
                                 Int32> _values;
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    private System.Collections.Generic.List<System. // keep
                                                                            Int32> _values;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma of a type argument list keeps its existing alignment with the first type argument
    /// </summary>
    [TestMethod]
    public void TypeArgumentLeadingCommaStaysAlignedWithTheFirstTypeArgument()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 private System.Collections.Generic.Dictionary<int // keep
                                                                               , string> _values;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a leading comma of a multi-line dictionary key is aligned with the key body
    /// </summary>
    [TestMethod]
    public void DictionaryKeyLeadingCommaIsAlignedWithTheKeyBody()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var grid = new Grid
                                                {
                                                    [1 // keep
                                     , 2] = 3,
                                                };
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        var grid = new Grid
                                                   {
                                                       [1 // keep
                                                           , 2] = 3,
                                                   };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the clauses of a query argument are aligned with the query's first token rather than the argument name
    /// </summary>
    [TestMethod]
    public void QueryClausesOfNamedArgumentAreAlignedWithTheQueryStart()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(o: from value in values
                                     where value > 0
                                     select value);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(o: from value in values
                                             where value > 0
                                             select value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a held line inside a switch expression arm of an argument is not aligned with the argument but left to the
    /// switch expression, which places it exactly as it does outside an argument
    /// </summary>
    [TestMethod]
    public void HeldLineInsideSwitchExpressionArgumentIsLeftToTheSwitchExpression()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(1,
                                       value switch
                                       {
                                           1 => // keep
                                     "one",
                                           _ => "other",
                                       });
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a held line inside an object initializer member of an argument is not aligned with the argument but left to
    /// the initializer, which places it exactly as it does outside an argument
    /// </summary>
    [TestMethod]
    public void HeldLineInsideObjectInitializerArgumentIsLeftToTheInitializer()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(new Example
                                       {
                                           A = // keep
                                     value,
                                       });
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}