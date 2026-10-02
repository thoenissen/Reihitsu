using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// A line that starts inside an element of an argument-, parameter-, attribute-, tuple- or type-argument-like list at a token
/// other than the element's first token is joined onto the previous line, unless a comment, a directive, disabled text,
/// or a construct that owns that line break keeps it
/// </summary>
[TestClass]
public class ListElementInteriorJoinTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that an argument wrapped after its <c>out</c> modifier is joined onto one line with a separating space
    /// </summary>
    [TestMethod]
    public void OutModifierWrappedBeforeDeclarationIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(out
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
                                        N(out var value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a named argument whose value was wrapped after the colon is joined, so the argument list is no longer split
    /// </summary>
    [TestMethod]
    public void NamedArgumentValueWrappedAfterColonIsJoinedAndTheListStaysOnOneLine()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(first:
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
                                        N(first: 1, 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda argument whose expression body was wrapped after the arrow is joined onto the lambda's line
    /// </summary>
    [TestMethod]
    public void LambdaExpressionBodyWrappedAfterArrowIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     return values.Any(static value =>
                                     value > 0);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        return values.Any(static value => value > 0);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument wrapped after <c>await</c> is joined without merging the keyword into its operand
    /// </summary>
    [TestMethod]
    public void AwaitWrappedBeforeOperandIsJoinedWithSeparatingSpace()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(await
                                     GetAsync());
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(await GetAsync());
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an object creation argument wrapped after <c>new</c> is joined
    /// </summary>
    [TestMethod]
    public void NewWrappedBeforeTypeIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     Use(new
                                     Example());
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        Use(new Example());
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nested invocation argument wrapped before its opening parenthesis is joined without a space
    /// </summary>
    [TestMethod]
    public void InvocationWrappedBeforeOpenParenthesisIsJoinedWithoutSpace()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(Create
                                     (1), 2);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(Create(1), 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>typeof</c> argument wrapped before its opening parenthesis is joined without a space
    /// </summary>
    [TestMethod]
    public void TypeofWrappedBeforeOpenParenthesisIsJoinedWithoutSpace()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     Use(typeof
                                     (int));
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        Use(typeof(int));
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an index-from-end argument of an element access wrapped after the hat operator is joined
    /// </summary>
    [TestMethod]
    public void IndexFromEndOperandWrappedInElementAccessIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var last = values[^
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
                                        var last = values[^1];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that joining a negated negation keeps the two minus operators apart, so the token stream is unchanged
    /// </summary>
    [TestMethod]
    public void PrefixUnaryOperatorsAreNotMergedByTheJoin()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(-
                                     -value);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(- -value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a named tuple element whose value was wrapped after the colon is joined
    /// </summary>
    [TestMethod]
    public void TupleExpressionNamedElementValueWrappedAfterColonIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     var pair = (A:
                                     1, B: 2);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        var pair = (A: 1, B: 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an argument wrapped across blank lines only is joined
    /// </summary>
    [TestMethod]
    public void BlankLinesInsideAnArgumentAreJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(out

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
                                        N(out var value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a collection expression argument wrapped before its opening bracket is joined after the argument name
    /// </summary>
    [TestMethod]
    public void CollectionExpressionOpeningBracketWrappedAfterNamedArgumentColonIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     UseList(values:
                                     [1, 2]);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        UseList(values: [1, 2]);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a qualified type argument wrapped after its dot is joined
    /// </summary>
    [TestMethod]
    public void QualifiedTypeArgumentWrappedAfterDotIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 private System.Collections.Generic.List<System.
                                 Int32> _values;
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    private System.Collections.Generic.List<System.Int32> _values;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the value of a named attribute argument of a parameter attribute is pulled up onto the argument name's line
    /// </summary>
    [TestMethod]
    public void NamedAttributeArgumentValueWrappedAfterColonIsJoined()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete(message:
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
                                    internal void M([Obsolete(message: "x")] int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the value of a named attribute property wrapped after the equals sign is joined while the wrap between the arguments stays
    /// </summary>
    [TestMethod]
    public void NamedAttributeArgumentValueWrappedAfterEqualsIsJoined()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 [AttributeUsage(AttributeTargets.All,
                                                 AllowMultiple =
                                 true)]
                                 internal sealed class MarkerAttribute : Attribute;
                             }
                             """;

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    [AttributeUsage(AttributeTargets.All,
                                                    AllowMultiple = true)]
                                    internal sealed class MarkerAttribute : Attribute;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an attribute of a parameter wrapped before its argument list is joined without a space
    /// </summary>
    [TestMethod]
    public void AttributeWrappedBeforeItsArgumentListIsJoined()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete
                                 ("x")] int value)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    internal void M([Obsolete("x")] int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter wrapped after its <c>ref</c> modifier is joined
    /// </summary>
    [TestMethod]
    public void ParameterWrappedAfterRefModifierIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(ref
                                 int value)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M(ref int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter wrapped between its type and its name is joined
    /// </summary>
    [TestMethod]
    public void ParameterWrappedBetweenTypeAndNameIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int
                                 value)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M(int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter whose default value was wrapped before the equals sign is joined
    /// </summary>
    [TestMethod]
    public void ParameterDefaultValueWrappedBeforeEqualsIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int value
                                 = 0)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M(int value = 0)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nullable parameter type wrapped before its question mark is joined without a space
    /// </summary>
    [TestMethod]
    public void NullableParameterTypeWrappedBeforeQuestionMarkIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int
                                 ? value)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M(int? value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an indexer parameter wrapped between its type and its name is joined
    /// </summary>
    [TestMethod]
    public void IndexerParameterWrappedBetweenTypeAndNameIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal int this[int
                                 index]
                                 {
                                     get => index;
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal int this[int index]
                                    {
                                        get => index;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a type parameter wrapped after its variance keyword is joined
    /// </summary>
    [TestMethod]
    public void TypeParameterWrappedAfterVarianceIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal interface IExample<in
                             T>;
                             """;

        const string expected = """
                                namespace Demo;

                                internal interface IExample<in T>;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a function pointer parameter wrapped after its <c>ref</c> modifier is joined
    /// </summary>
    [TestMethod]
    public void FunctionPointerParameterWrappedAfterRefModifierIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal unsafe void M(delegate*<ref
                                 int, void> callback)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal unsafe void M(delegate*<ref int, void> callback)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a local function parameter wrapped after its <c>in</c> modifier is joined
    /// </summary>
    [TestMethod]
    public void LocalFunctionParameterWrappedAfterModifierIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     int Local(in
                                     int value)
                                     {
                                         return value;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        int Local(in int value)
                                        {
                                            return value;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tuple type element wrapped between its type and its name is joined
    /// </summary>
    [TestMethod]
    public void TupleTypeElementWrappedBetweenTypeAndNameIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 private (int
                                 First, int Second) _pair;
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    private (int First, int Second) _pair;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrap between two arguments is not an interior wrap and stays
    /// </summary>
    [TestMethod]
    public void WrapBetweenArgumentsIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(1,
                                       2);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a binary operator wrap inside an argument stays, because operator wraps keep their own layout
    /// </summary>
    [TestMethod]
    public void BinaryOperatorWrapInsideArgumentIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(first
                                       + second,
                                       1);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the block body of a lambda argument keeps its own lines
    /// </summary>
    [TestMethod]
    public void BlockLambdaArgumentBodyIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(value =>
                                       {
                                           Use(value);
                                       });
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional operator wrap inside an argument stays
    /// </summary>
    [TestMethod]
    public void ConditionalOperatorWrapInsideArgumentIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(flag
                                           ? 1
                                           : 2);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a wrap inside an interpolation hole of an argument is not joined
    /// </summary>
    [TestMethod]
    public void InterpolationHoleWrapIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N($"{first
                                          + second}");
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an object initializer brace of an argument keeps its own line while the creation itself is joined
    /// </summary>
    [TestMethod]
    public void ObjectInitializerOfArgumentIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     Use(new
                                     Example
                                         {
                                             Value = 1,
                                         });
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        Use(new Example
                                            {
                                                Value = 1,
                                            });
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an interior wrap a line comment holds is not joined and is aligned with the argument's first token
    /// </summary>
    [TestMethod]
    public void InteriorWrapHeldByLineCommentIsAlignedInsteadOfJoined()
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
    /// Verifies that an interior wrap held by a block comment on its own line is not joined
    /// </summary>
    [TestMethod]
    public void InteriorWrapHeldByOwnLineBlockCommentIsAlignedInsteadOfJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(out
                                     /* keep */
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
                                        N(out

                                          /* keep */
                                          var value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter wrapped after a modifier that a line comment holds keeps the wrap and is aligned with the modifier
    /// </summary>
    [TestMethod]
    public void ParameterInteriorWrapHeldByLineCommentIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(ref // keep
                                 int value)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M(ref // keep
                                                    int value)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a cast argument wrapped after the cast's closing parenthesis is joined without a space
    /// </summary>
    [TestMethod]
    public void CastOperandWrappedAfterClosingParenthesisIsJoinedWithoutSpace()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N((int)
                                     value);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N((int)value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tuple argument wrapped after an argument name is joined with a space, unlike an argument list's parenthesis
    /// </summary>
    [TestMethod]
    public void TupleArgumentWrappedAfterNamedArgumentColonKeepsItsSpace()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(pair:
                                     (1, 2));
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(pair: (1, 2));
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a lambda argument wrapped between its modifier and its parameter list is joined with a space
    /// </summary>
    [TestMethod]
    public void LambdaParameterListWrappedAfterStaticModifierKeepsItsSpace()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(static
                                     (int value) => value);
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(static (int value) => value);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the links of a method chain argument keep their own lines, because chain wraps keep their own layout
    /// </summary>
    [TestMethod]
    public void MethodChainWrapInsideArgumentIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(text.Trim()
                                           .ToUpper()
                                           .ToLower());
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a named argument whose value is a verbatim string spanning lines keeps its wrap
    /// </summary>
    [TestMethod]
    public void MultiLineVerbatimStringArgumentIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(text:
                                       @"first
                                     second");
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a named argument whose value is a single-line verbatim string is joined
    /// </summary>
    [TestMethod]
    public void SingleLineVerbatimStringArgumentIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(text:
                                     @"first");
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(text: @"first");
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the clauses of a query expression argument keep their own lines
    /// </summary>
    [TestMethod]
    public void QueryClauseWrapInsideArgumentIsNotJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(from value in values
                                       where value > 0
                                       select value);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single-line interpolated string argument wrapped after the argument name is joined
    /// </summary>
    [TestMethod]
    public void InterpolatedStringArgumentWrappedAfterNameIsJoined()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     N(text:
                                     $"{first}");
                                 }
                             }
                             """;

        const string expected = """
                                namespace Demo;

                                internal class Example
                                {
                                    internal void M()
                                    {
                                        N(text: $"{first}");
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}