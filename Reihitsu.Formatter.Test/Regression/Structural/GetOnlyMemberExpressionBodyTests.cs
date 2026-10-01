using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Tests verifying that a property or indexer whose accessor list holds nothing but an expression-capable
/// <c>get</c> accessor is formatted as an expression-bodied member, and that the accessor list is kept whenever
/// a setter, an accessor attribute or modifier, an initializer, or trivia the rewrite would move requires it
/// </summary>
[TestClass]
public class GetOnlyMemberExpressionBodyTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a get-only property with a block-bodied getter becomes an expression-bodied property
    /// </summary>
    [TestMethod]
    public void BlockGetterPropertyBecomesExpressionBodiedProperty()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get
                                     {
                                         return _value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int Value => _value;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a hand-written single-line get-only accessor list with an expression-bodied getter becomes an
    /// expression-bodied property
    /// </summary>
    [TestMethod]
    public void SingleLineExpressionGetterPropertyBecomesExpressionBodiedProperty()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value { get => _value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int Value => _value;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a hand-written multi-line get-only accessor list with an expression-bodied getter becomes an
    /// expression-bodied property
    /// </summary>
    [TestMethod]
    public void MultiLineExpressionGetterPropertyBecomesExpressionBodiedProperty()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get => _value;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int Value => _value;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a get-only property whose getter throws becomes a property with a throw-expression body
    /// </summary>
    [TestMethod]
    public void ThrowingGetterPropertyBecomesThrowExpressionBody()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 public int Value { get { throw new InvalidOperationException(); } }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    public int Value => throw new InvalidOperationException();
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a get-only property whose getter returns by reference keeps the <c>ref</c> in its expression body
    /// </summary>
    [TestMethod]
    public void RefReturningGetterPropertyBecomesRefExpressionBody()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public ref int Value { get { return ref _value; } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public ref int Value => ref _value;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a static get-only property is converted like an instance property and keeps its modifiers
    /// </summary>
    [TestMethod]
    public void StaticGetOnlyPropertyBecomesExpressionBodiedProperty()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public static int Value { get { return 1; } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public static int Value => 1;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a get-only indexer with a block-bodied getter becomes an expression-bodied indexer
    /// </summary>
    [TestMethod]
    public void BlockGetterIndexerBecomesExpressionBodiedIndexer()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _values = new int[3];

                                 public int this[long index]
                                 {
                                     get
                                     {
                                         return _values[index];
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private readonly int[] _values = new int[3];

                                    public int this[long index] => _values[index];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a get-only indexer with an expression-bodied getter becomes an expression-bodied indexer
    /// </summary>
    [TestMethod]
    public void ExpressionGetterIndexerBecomesExpressionBodiedIndexer()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _values = new int[3];

                                 public int this[int index] { get => _values[index]; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private readonly int[] _values = new int[3];

                                    public int this[int index] => _values[index];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a get-only explicit interface indexer becomes an expression-bodied indexer
    /// </summary>
    [TestMethod]
    public void ExplicitInterfaceGetOnlyIndexerBecomesExpressionBodiedIndexer()
    {
        // Arrange
        const string input = """
                             interface IValues
                             {
                                 int this[int index] { get; }
                             }

                             class C : IValues
                             {
                                 int IValues.this[int index]
                                 {
                                     get
                                     {
                                         return index;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                interface IValues
                                {
                                    int this[int index] { get; }
                                }

                                class C : IValues
                                {
                                    int IValues.this[int index] => index;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an indexer whose parameter list spans several lines takes the arrow onto the line of the
    /// closing bracket
    /// </summary>
    [TestMethod]
    public void MultiLineParameterIndexerTakesArrowOntoClosingBracketLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _values = new int[3];

                                 public int this[int first,
                                                 int second]
                                 {
                                     get => _values[first];
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private readonly int[] _values = new int[3];

                                    public int this[int first,
                                                    int second] => _values[first];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a getter returning a multi-line expression produces the same output as the hand-written
    /// expression-bodied property
    /// </summary>
    [TestMethod]
    public void MultiLineExpressionGetterMatchesHandWrittenExpressionBodiedProperty()
    {
        // Arrange
        const string blockInput = """
                                  class C
                                  {
                                      private int _value;

                                      public int Value
                                      {
                                          get
                                          {
                                              return Compute(_value,
                                                             _value);
                                          }
                                      }

                                      private static int Compute(int first, int second) => first + second;
                                  }
                                  """;
        const string handWrittenInput = """
                                        class C
                                        {
                                            private int _value;

                                            public int Value => Compute(_value,
                                                                        _value);

                                            private static int Compute(int first, int second) => first + second;
                                        }
                                        """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int Value => Compute(_value,
                                                                _value);

                                    private static int Compute(int first, int second)
                                    {
                                        return first + second;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(handWrittenInput, expected);
        AssertRuleResult(blockInput, expected);
    }

    /// <summary>
    /// Verifies that every get-only member of a type is converted in a single pass
    /// </summary>
    [TestMethod]
    public void SeveralGetOnlyMembersAreConvertedTogether()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int First { get { return _value; } }

                                 public int Second { get => _value; }

                                 public int this[int index] { get { return index; } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int First => _value;

                                    public int Second => _value;

                                    public int this[int index] => index;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an attribute and a documentation comment on the member stay on the converted member
    /// </summary>
    [TestMethod]
    public void MemberAttributeAndDocumentationStayOnConvertedMember()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 private int _value;

                                 /// <summary>
                                 /// The value
                                 /// </summary>
                                 [Obsolete]
                                 public int Value { get => _value; }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    private int _value;

                                    /// <summary>
                                    /// The value
                                    /// </summary>
                                    [Obsolete]
                                    public int Value => _value;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a property with a getter and a setter keeps its accessor list
    /// </summary>
    [TestMethod]
    public void GetterAndSetterKeepAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get => _value;
                                     set => _value = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a property with a getter and an init accessor keeps its accessor list
    /// </summary>
    [TestMethod]
    public void GetterAndInitKeepAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get => _value;
                                     init => _value = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a set-only property keeps its accessor list
    /// </summary>
    [TestMethod]
    public void SetOnlyPropertyKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     set => _value = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a getter with more than one statement keeps its block and the accessor list
    /// </summary>
    [TestMethod]
    public void MultiStatementGetterKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get
                                     {
                                         var copy = _value;

                                         return copy;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an attribute on the getter keeps the accessor list, so the attribute keeps its target
    /// </summary>
    [TestMethod]
    public void AttributedGetterKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     [Obsolete]
                                     get => _value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a modifier on the getter keeps the accessor list, so the modifier is not lost
    /// </summary>
    [TestMethod]
    public void GetterModifierKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             struct S
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     readonly get => _value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a get-only auto-property stays unchanged
    /// </summary>
    [TestMethod]
    public void AutoGetterIsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int Value { get; }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a get-only property with an initializer keeps its accessor list, because an expression-bodied
    /// property cannot carry an initializer
    /// </summary>
    [TestMethod]
    public void PropertyWithInitializerKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int Value
                                 {
                                     get => field;
                                 } = 5;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, parseOptions: new CSharpParseOptions(LanguageVersion.CSharp14));
    }

    /// <summary>
    /// Verifies that a getter returning <c>field</c> is converted when <c>field</c> is a contextual keyword
    /// </summary>
    [TestMethod]
    public void FieldKeywordGetterIsConvertedInCSharp14()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int Value { get => field; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    public int Value => field;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, new CSharpParseOptions(LanguageVersion.CSharp14));
    }

    /// <summary>
    /// Verifies that a getter returning <c>field</c> is converted when <c>field</c> is an ordinary identifier
    /// </summary>
    [TestMethod]
    public void FieldIdentifierGetterIsConvertedInCSharp13()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int field;

                                 public int Value { get => field; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int field;

                                    public int Value => field;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, new CSharpParseOptions(LanguageVersion.CSharp13));
    }

    /// <summary>
    /// Verifies that a line comment between the property name and the accessor list keeps the accessor list
    /// </summary>
    [TestMethod]
    public void LineCommentAfterSignatureKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value // Explains the value
                                 {
                                     get => _value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment between the indexer parameters and the accessor list keeps the accessor list
    /// </summary>
    [TestMethod]
    public void BlockCommentAfterIndexerSignatureKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public int this[int index] /* c */
                                 {
                                     get => index;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment line before the getter keeps the accessor list
    /// </summary>
    [TestMethod]
    public void CommentBeforeGetterKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     // Explains the value
                                     get => _value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment inside the getter block keeps the block and the accessor list
    /// </summary>
    [TestMethod]
    public void CommentInsideGetterBlockKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get
                                     {
                                         // Explains the value
                                         return _value;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment after the getter's arrow keeps the accessor list
    /// </summary>
    [TestMethod]
    public void CommentAfterGetterArrowKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get => /* c */ _value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment line before the accessor list's closing brace keeps the accessor list
    /// </summary>
    [TestMethod]
    public void CommentBeforeClosingBraceKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get => _value;

                                     // Trailing note
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment trailing the getter's expression travels with the expression
    /// </summary>
    [TestMethod]
    public void BlockCommentTrailingExpressionTravelsWithExpression()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get => _value /* c */;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int Value => _value /* c */;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing the getter's semicolon follows the member's semicolon
    /// </summary>
    [TestMethod]
    public void CommentTrailingGetterSemicolonFollowsMemberSemicolon()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get
                                     {
                                         return _value; // Explains the value
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int Value => _value; // Explains the value
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing the accessor list's closing brace follows the member's semicolon
    /// </summary>
    [TestMethod]
    public void CommentTrailingClosingBraceFollowsMemberSemicolon()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get => _value;
                                 } // Explains the value
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int Value => _value; // Explains the value
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that comments trailing both the getter's semicolon and the closing brace keep the accessor list
    /// </summary>
    [TestMethod]
    public void CommentsTrailingSemicolonAndClosingBraceKeepAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get => _value; // First
                                 } // Second
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a conditional directive choosing between two getters keeps the accessor list
    /// </summary>
    [TestMethod]
    public void ConditionalGettersKeepAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                             #if DEBUG
                                     get => _value;
                             #else
                                     get => 0;
                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a setter inside an inactive conditional branch keeps the accessor list, so the disabled
    /// accessor is not deleted
    /// </summary>
    [TestMethod]
    public void SetterInInactiveBranchKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get => _value;
                             #if NEVER_DEFINED
                                     set => _value = value;
                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a region directive inside the accessor list keeps the accessor list
    /// </summary>
    [TestMethod]
    public void RegionInsideAccessorListKeepsAccessorList()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     #region Getter

                                     get => _value;

                                     #endregion // Getter
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a get-only property is not converted below C# 6, which introduced expression-bodied properties
    /// </summary>
    [TestMethod]
    public void BlockGetterIsKeptBelowCSharp6()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get
                                     {
                                         return _value;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, parseOptions: new CSharpParseOptions(LanguageVersion.CSharp5));
    }

    /// <summary>
    /// Verifies that a returning block getter is converted to an expression-bodied property in C# 6
    /// </summary>
    [TestMethod]
    public void ReturningBlockGetterIsConvertedInCSharp6()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get
                                     {
                                         return _value;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int Value => _value;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, new CSharpParseOptions(LanguageVersion.CSharp6));
    }

    /// <summary>
    /// Verifies that a throwing block getter keeps its block in C# 6, which predates throw expressions
    /// </summary>
    [TestMethod]
    public void ThrowingBlockGetterIsKeptInCSharp6()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 public int Value
                                 {
                                     get
                                     {
                                         throw new InvalidOperationException();
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, parseOptions: new CSharpParseOptions(LanguageVersion.CSharp6));
    }

    /// <summary>
    /// Verifies that a throwing block getter is converted to a throw-expression body in C# 7
    /// </summary>
    [TestMethod]
    public void ThrowingBlockGetterIsConvertedInCSharp7()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 public int Value
                                 {
                                     get
                                     {
                                         throw new InvalidOperationException();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    public int Value => throw new InvalidOperationException();
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, new CSharpParseOptions(LanguageVersion.CSharp7));
    }

    /// <summary>
    /// Verifies that the C# 6 gate still holds when an earlier structural transform replaced the tree in the same pass
    /// </summary>
    [TestMethod]
    public void BlockGetterIsKeptBelowCSharp6AfterEarlierTransform()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _value;

                                 public int Value
                                 {
                                     get
                                     {
                                         return _value;
                                     }
                                 }

                                 public void Run(bool condition)
                                 {
                                     if (condition)
                                         _value = 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _value;

                                    public int Value
                                    {
                                        get
                                        {
                                            return _value;
                                        }
                                    }

                                    public void Run(bool condition)
                                    {
                                        if (condition)
                                        {
                                            _value = 1;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, new CSharpParseOptions(LanguageVersion.CSharp5));
    }

    #endregion // Methods
}