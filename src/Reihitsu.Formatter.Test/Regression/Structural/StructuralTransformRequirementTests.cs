using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Full-pipeline tests for the structural-transform behavior no narrower test pins: the comments of a braced
/// body, the language-version independence of the expression-body conversion, the expression bodies and empty
/// bodies that are kept, the trailing commas that stay, and the declarations the field split leaves alone
/// </summary>
[TestClass]
public class StructuralTransformRequirementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a comment above an unbraced body moves into the new block in front of the statement, and that
    /// a line or block comment trailing the statement stays on its line, for an <c>if</c> and an <c>else</c> body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-002")]
    public void CommentsOfBracedIfAndElseBodiesStayWithTheirStatements()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                                         // leading
                                         Foo(); // trailing
                                     else
                                         Bar(); /* block */
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(bool a)
                                    {
                                        if (a)
                                        {
                                            // leading
                                            Foo(); // trailing
                                        }
                                        else
                                        {
                                            Bar(); /* block */
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>if</c> body whose trivia carries a conditional group with an active and a disabled
    /// branch is not braced
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-003")]
    public void IfBodyInsideConditionalGroupWithDisabledBranchIsNotBraced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool a)
                                 {
                                     if (a)
                             #if DEBUG
                                         Foo();
                             #else
                                         Bar();
                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that expression-bodied methods and local functions are converted under the oldest language version
    /// that can express them, so the conversion is not gated by a version
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-004")]
    public void ExpressionBodiedMethodsAndLocalFunctionsConvertUnderTheirOldestLanguageVersion()
    {
        // Arrange
        const string methodInput = """
                                   class C
                                   {
                                       int Get() => 42;
                                   }
                                   """;
        const string methodExpected = """
                                      class C
                                      {
                                          int Get()
                                          {
                                              return 42;
                                          }
                                      }
                                      """;
        const string localFunctionInput = """
                                          class C
                                          {
                                              void M()
                                              {
                                                  int Local() => 42;
                                              }
                                          }
                                          """;
        const string localFunctionExpected = """
                                             class C
                                             {
                                                 void M()
                                                 {
                                                     int Local()
                                                     {
                                                         return 42;
                                                     }
                                                 }
                                             }
                                             """;

        // Act & Assert
        AssertRuleResult(methodInput, methodExpected, new CSharpParseOptions(LanguageVersion.CSharp6));
        AssertRuleResult(localFunctionInput, localFunctionExpected, new CSharpParseOptions(LanguageVersion.CSharp7));
    }

    /// <summary>
    /// Verifies that a comment between the expression and the semicolon stays in front of the created statement's
    /// semicolon, and that an expression written on the line after the arrow joins the created <c>return</c> keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-006")]
    public void ConvertedBodyKeepsCommentBeforeSemicolonAndJoinsNextLineExpression()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int Get() => 42 /* why */;

                                 int Other() =>
                                     Compute(1, 2);
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int Get()
                                    {
                                        return 42 /* why */;
                                    }

                                    int Other()
                                    {
                                        return Compute(1, 2);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that expression-bodied properties, indexers, and lambdas are left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-008")]
    public void ExpressionBodiedPropertyIndexerAndLambdaAreKept()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 int P => 1;

                                 int this[int i] => i;

                                 Func<int> F = () => 1;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an accessor whose only statement carries an attribute keeps its block
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-010")]
    public void AccessorWithAttributedStatementKeepsBlock()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 private int _value;

                                 int Value
                                 {
                                     get
                                     {
                                         [Obsolete]
                                         return _value;
                                     }
                                     set => _value = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an empty enum keeps its braces while the other empty types beside it use the semicolon form
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void EmptyEnumKeepsItsBraces()
    {
        // Arrange
        const string input = """
                             class Empty
                             {
                             }

                             enum E
                             {
                             }
                             """;
        const string expected = """
                                class Empty;

                                enum E
                                {
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that only the two records are converted under C# 11
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void OnlyRecordsUseTheSemicolonFormUnderCSharp11()
    {
        // Arrange
        const string input = """
                             class Empty
                             {
                             }

                             struct EmptyStruct
                             {
                             }

                             interface IEmpty
                             {
                             }

                             record EmptyRecord(int Value)
                             {
                             }

                             record struct EmptyRecordStruct
                             {
                             }
                             """;
        const string expected = """
                                class Empty
                                {
                                }

                                struct EmptyStruct
                                {
                                }

                                interface IEmpty
                                {
                                }

                                record EmptyRecord(int Value);

                                record struct EmptyRecordStruct;
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, new CSharpParseOptions(LanguageVersion.CSharp11));
    }

    /// <summary>
    /// Verifies that an empty type body holding a comment or a directive keeps its braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-018")]
    public void EmptyTypeBodyWithCommentOrDirectiveInsideKeepsItsBraces()
    {
        // Arrange
        const string input = """
                             class Inside
                             {
                                 // reserved
                             }

                             class Directive
                             {
                             #if DEBUG
                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the trailing comma of an object initializer, an anonymous object creation, and a collection
    /// expression is kept, while the one of an array initializer beside them is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-020")]
    public void ObjectInitializerAnonymousObjectAndCollectionExpressionKeepTheirTrailingComma()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int[] _numbers = new[] { 1, 2, 3, };

                                 Point _point = new Point
                                                {
                                                    X = 1,
                                                    Y = 2,
                                                };

                                 object _anonymous = new
                                                     {
                                                         A = 1,
                                                     };

                                 int[] _expression = [1, 2, 3,];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int[] _numbers = new[] { 1, 2, 3 };

                                    Point _point = new Point
                                                   {
                                                       X = 1,
                                                       Y = 2,
                                                   };

                                    object _anonymous = new
                                                        {
                                                            A = 1,
                                                        };

                                    int[] _expression = [1, 2, 3,];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every generated field repeats the attributes, modifiers, and type in source order and keeps
    /// its own initializer
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void SplitFieldsRepeatAttributesModifiersAndTypeAndKeepTheirInitializers()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 private int _x, _y = 1, _z;

                                 [NonSerialized]
                                 private static readonly string _a = "a", _b = "b";
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    private int _x;
                                    private int _y = 1;
                                    private int _z;

                                    [NonSerialized]
                                    private static readonly string _a = "a";
                                    [NonSerialized]
                                    private static readonly string _b = "b";
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that combined fields in a struct and a record are split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void CombinedStructAndRecordFieldsAreSplit()
    {
        // Arrange
        const string input = """
                             struct S
                             {
                                 private int _a, _b;
                             }

                             record R
                             {
                                 private int _c, _d;
                             }
                             """;
        const string expected = """
                                struct S
                                {
                                    private int _a;
                                    private int _b;
                                }

                                record R
                                {
                                    private int _c;
                                    private int _d;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an event field declaration and a local variable declaration with several declarators are not
    /// split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void EventFieldAndLocalDeclarationsAreNotSplit()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 public event EventHandler First, Second;

                                 void M()
                                 {
                                     int a = 1, b = 2;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the leading trivia of a split declaration, here an ordinary comment above it, stays on the first
    /// generated field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void LeadingCommentOfSplitDeclarationStaysOnTheFirstField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 // The coordinates
                                 private int _x, _y;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // The coordinates
                                    private int _x;
                                    private int _y;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}