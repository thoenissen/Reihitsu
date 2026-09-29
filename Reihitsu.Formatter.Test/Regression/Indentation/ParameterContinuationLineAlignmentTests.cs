using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// A line that starts inside a parameter of a wrapped parameter-like list at a token other than the parameter's own
/// first token - because a comment or directive forbids joining it to the previous line - is aligned with that first
/// token instead of the enclosing declaration's block column
/// </summary>
[TestClass]
public class ParameterContinuationLineAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a positional record parameter type stays aligned after an attribute list followed by a line comment
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([Obsolete] // keep
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a parameter type that was placed at the base indentation is moved under its attribute list
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAtBaseIndentationIsAlignedWithItsAttributeList()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([Obsolete] // keep
                             int Id);
                             """;
        const string expected = """
                                using System;

                                namespace Demo;

                                internal sealed record Example([Obsolete] // keep
                                                               int Id);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a targeted attribute list followed by a line comment keeps the parameter type aligned
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterTargetedAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([property: Obsolete] // keep
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a later record parameter type stays aligned after an attribute list and a line comment, and that
    /// the parameter following it is unaffected
    /// </summary>
    [TestMethod]
    public void SecondRecordParameterTypeAfterAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example(int Other,
                                                            [Obsolete] // keep
                                                            int Id,
                                                            string Name);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an ordinary method parameter type stays aligned after an attribute list and a line comment
    /// </summary>
    [TestMethod]
    public void MethodParameterTypeAfterAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete] // keep
                                                 int value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a local function parameter type stays aligned after an attribute list and a line comment
    /// </summary>
    [TestMethod]
    public void LocalFunctionParameterTypeAfterAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     void Local([Obsolete] // keep
                                                int value)
                                     {
                                     }

                                     Local(1);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a parameter type stays aligned after an attribute list followed by a pragma directive
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterAttributeListAndPragmaStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([Obsolete]
                             #pragma warning disable CS0618
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment on its own line before the parameter type keeps the blank line the blank-line
    /// rules require, and that the comment and the type are aligned with the attribute list
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterAttributeListAndBlockCommentLineIsAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([param: Obsolete]
                                                            /* keep */ int Id);
                             """;
        const string expected = """
                                using System;

                                namespace Demo;

                                internal sealed record Example([param: Obsolete]

                                                               /* keep */ int Id);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter type stays aligned after an attribute list whose following list sits in disabled text
    /// </summary>
    [TestMethod]
    public void RecordParameterTypeAfterDisabledAttributeListStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([Obsolete]
                             #if DEBUG
                                                            [CLSCompliant(false)]
                             #endif
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a second attribute list inside an active conditional block and the parameter type after it stay
    /// aligned with the parameter's first attribute list
    /// </summary>
    [TestMethod]
    public void SecondAttributeListInActiveConditionalBlockStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal sealed record Example([Obsolete]
                             #if true
                                                            [CLSCompliant(false)]
                             #endif
                                                            int Id);
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a second attribute list after a line comment stays aligned with the first one
    /// </summary>
    [TestMethod]
    public void SecondAttributeListAfterLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Diagnostics.CodeAnalysis;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete] // keep
                                                 [NotNull] string value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a parameter modifier after an attribute list and a line comment stays aligned
    /// </summary>
    [TestMethod]
    public void ModifierAfterAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete] // keep
                                                 ref int value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a parameter type after a modifier and a line comment stays aligned, without any attribute list
    /// </summary>
    [TestMethod]
    public void TypeAfterModifierAndLineCommentStaysAligned()
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

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a parameter name after its type and a line comment stays aligned
    /// </summary>
    [TestMethod]
    public void IdentifierAfterTypeAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int // keep
                                                 value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a default value after the equals sign and a line comment stays aligned
    /// </summary>
    [TestMethod]
    public void DefaultValueAfterEqualsAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int value = // keep
                                                 1)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the second part of a qualified parameter type after a line comment stays aligned
    /// </summary>
    [TestMethod]
    public void QualifiedTypePartAfterLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(System. // keep
                                                 Int32 value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the lines of a multi-line attribute argument list keep their alignment and the parameter type
    /// after it is aligned with the attribute list
    /// </summary>
    [TestMethod]
    public void TypeAfterMultiLineAttributeArgumentsStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete("first",
                                                           true)] // keep
                                                 int value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the block of a lambda default value keeps its own alignment
    /// </summary>
    [TestMethod]
    public void LambdaBlockInDefaultValueKeepsItsAlignment()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(Func<int> factory = () =>
                                                                     {
                                                                         return 1;
                                                                     },
                                                 int other = 0)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the block of a lambda default value follows its parameter when the parameter's type is moved
    /// under the attribute list
    /// </summary>
    [TestMethod]
    public void LambdaBlockInDefaultValueFollowsMovedParameter()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete] // keep
                                 Func<int> factory = () =>
                                 {
                                     return 1;
                                 })
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    internal void M([Obsolete] // keep
                                                    Func<int> factory = () =>
                                                                        {
                                                                            return 1;
                                                                        })
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the content and closing marker of a raw-string default value follow their parameter when the
    /// parameter's type is moved under the attribute list
    /// </summary>
    [TestMethod]
    public void RawStringDefaultValueFollowsMovedParameter()
    {
        // Arrange
        const string input = """"
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M([Obsolete] // keep
                                 string text = """
                                     raw
                                     """)
                                 {
                                 }
                             }
                             """";
        const string expected = """"
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    internal void M([Obsolete] // keep
                                                    string text = """
                                                                  raw
                                                                  """)
                                    {
                                    }
                                }
                                """";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a parameter of a lambda passed as a wrapped argument stays aligned with its attribute list
    /// </summary>
    [TestMethod]
    public void LambdaParameterInWrappedArgumentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M()
                                 {
                                     Run(0,
                                         (int first,
                                          [Obsolete] // keep
                                          int second) => first);
                                 }

                                 internal void Run(int value, Func<int, int, int> function)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an indexer parameter sharing its line with a previous parameter keeps its continuation under its
    /// own first token
    /// </summary>
    [TestMethod]
    public void IndexerParameterOnSharedLineKeepsContinuationUnderItsFirstToken()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal int this[int first, [Obsolete] // keep
                                                              int second]
                                 {
                                     get => first;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a type parameter name after an attribute list and a line comment stays aligned
    /// </summary>
    [TestMethod]
    public void TypeParameterAfterAttributeListAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example<[Obsolete] // keep
                                                    T>
                             {
                                 internal T Value { get; set; }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a type parameter sharing its line with a previous type parameter keeps its continuation under its
    /// own first token
    /// </summary>
    [TestMethod]
    public void TypeParameterOnSharedLineKeepsContinuationUnderItsFirstToken()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example<TFirst, [Obsolete] // keep
                                                            TSecond>
                             {
                                 internal TSecond Value { get; set; }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a function-pointer parameter type after a modifier and a line comment stays aligned
    /// </summary>
    [TestMethod]
    public void FunctionPointerParameterTypeAfterModifierAndLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal unsafe class Example
                             {
                                 private delegate*<ref // keep
                                                   int, void> _pointer;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a lambda expression body a parameter's author wrapped without any comment is aligned with the
    /// parameter
    /// </summary>
    [TestMethod]
    public void WrappedLambdaExpressionBodyInDefaultValueIsAlignedWithItsParameter()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(Func<int> factory = () =>
                                 1)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                namespace Demo;

                                internal class Example
                                {
                                    internal void M(Func<int> factory = () =>
                                                    1)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma kept on its own line by a line comment is aligned with the parameters
    /// </summary>
    [TestMethod]
    public void LeadingCommaAfterLineCommentStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int first // keep
                                                 , int second)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a leading comma that was placed at the base indentation is moved under the parameters
    /// </summary>
    [TestMethod]
    public void LeadingCommaAtBaseIndentationIsAlignedWithTheParameters()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal sealed record Example(int First // keep
                             , int Second);
                             """;
        const string expected = """
                                namespace Demo;

                                internal sealed record Example(int First // keep
                                                               , int Second);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a leading comma inside an active conditional block is aligned with the parameters
    /// </summary>
    [TestMethod]
    public void LeadingCommaInActiveConditionalBlockStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int first
                             #if !NEVER
                                                 , int second
                             #endif
                                                 )
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a leading comma after disabled text is aligned with the parameters and the disabled text is left
    /// untouched
    /// </summary>
    [TestMethod]
    public void LeadingCommaAfterDisabledTextStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int first
                             #if NEVER
                                   , int unused
                             #endif
                                                 , int second)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comma left alone on its line after a line comment is aligned with the parameters
    /// </summary>
    [TestMethod]
    public void CommaAloneOnItsLineStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(int first // keep
                                                 ,
                                                 int second)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comma after a first parameter held on its own line by a directive is aligned with that
    /// parameter
    /// </summary>
    [TestMethod]
    public void CommaAfterDirectiveHeldFirstParameterStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal void M(
                             #if true
                                                 int first
                             #endif
                                                 ,
                                                 int second)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a leading comma of an indexer parameter list is aligned with the parameters
    /// </summary>
    [TestMethod]
    public void IndexerLeadingCommaStaysAligned()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal int this[int first
                                                   , int second]
                                 {
                                     get => first;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a leading comma of an indexer parameter list is aligned with the list's first parameter, not with
    /// the parameter that shares the previous line
    /// </summary>
    [TestMethod]
    public void IndexerLeadingCommaAfterSharedLineIsAlignedWithTheFirstParameter()
    {
        // Arrange
        const string input = """
                             namespace Demo;

                             internal class Example
                             {
                                 internal int this[int first, int second // keep
                                                   , int third]
                                 {
                                     get => first;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the continuation of a parameter that follows a leading comma is aligned with that parameter's
    /// first token
    /// </summary>
    [TestMethod]
    public void ContinuationOfParameterAfterLeadingCommaIsAlignedWithItsFirstToken()
    {
        // Arrange
        const string input = """
                             using System;

                             namespace Demo;

                             internal class Example
                             {
                                 internal int this[int first
                                                   , [Obsolete] // keep
                                                     int second]
                                 {
                                     get => first;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a leading comma of an argument list is not aligned by the parameter rule
    /// </summary>
    [TestMethod]
    public void ArgumentLeadingCommaIsNotAlignedByParameterRule()
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

                                 internal void N(int first, int second)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a leading comma of a member attribute's argument list is not aligned by the parameter rule
    /// </summary>
    [TestMethod]
    public void AttributeArgumentLeadingCommaIsNotAlignedByParameterRule()
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

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a continuation line inside an argument is not aligned by the parameter rule
    /// </summary>
    [TestMethod]
    public void ArgumentContinuationLineIsNotAlignedByParameterRule()
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

                                 internal void N(out int value)
                                 {
                                     value = 0;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a continuation line inside a member attribute's argument is not aligned by the parameter rule
    /// </summary>
    [TestMethod]
    public void MemberAttributeArgumentContinuationLineIsNotAlignedByParameterRule()
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

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a continuation line inside a field's type argument is not aligned by the parameter rule
    /// </summary>
    [TestMethod]
    public void TypeArgumentContinuationLineIsNotAlignedByParameterRule()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             namespace Demo;

                             internal class Example
                             {
                                 private List<System. // keep
                                 Int32> _values;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}