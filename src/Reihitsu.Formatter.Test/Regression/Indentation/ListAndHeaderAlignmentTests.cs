using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for the alignment of wrapped lists, base lists, declaration headers, and <c>when</c> clauses
/// </summary>
[TestClass]
public class ListAndHeaderAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: wrapped arguments and parameters are indented to the column directly after the opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void WrappedArgumentsAndParametersAlignAfterTheOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Call(1,
                                        2,
                                             3);
                                 }

                                 void Call(int a,
                                       int b,
                                     int c)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Call(1,
                                             2,
                                             3);
                                    }

                                    void Call(int a,
                                              int b,
                                              int c)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a closing parenthesis that starts a line is indented to the column directly after the opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void ClosingParenthesisOnItsOwnLineAlignsAfterTheOpeningParenthesis()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Call(1,
                                          2 // last
                                     );
                                 }

                                 void Call(int a, int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        Call(1,
                                             2 // last
                                             );
                                    }

                                    void Call(int a, int b)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the wrapped parameters of an indexer align to the column directly after the opening bracket
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-012")]
    public void WrappedBracketedParametersAlignAfterTheOpeningBracket()
    {
        const string input = """
                             class C
                             {
                                 int this[int a, // first
                                     int b] => a + b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int this[int a, // first
                                             int b] => a + b;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a wrapped attribute list and type argument list are indented to the column of their first element
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void AttributeListsAndTypeArgumentListsAlignToTheirFirstElement()
    {
        const string input = """
                             class C
                             {
                                 [System.Obsolete("a"), // first
                                     System.CLSCompliant(true)]
                                 void M()
                                 {
                                 }

                                 System.Collections.Generic.Dictionary<int, // first
                                     string> _map;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    [System.Obsolete("a"), // first
                                     System.CLSCompliant(true)]
                                    void M()
                                    {
                                    }

                                    System.Collections.Generic.Dictionary<int, // first
                                                                          string> _map;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped type parameter list is indented to the column of its first type parameter
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void WrappedTypeParameterListAlignsToItsFirstTypeParameter()
    {
        const string input = """
                             class C<TFirst, // first
                                 TSecond>
                             {
                                 TFirst _first;
                             }
                             """;
        const string expected = """
                                class C<TFirst, // first
                                        TSecond>
                                {
                                    TFirst _first;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped function-pointer parameter list is indented to the column of its first parameter
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-015")]
    public void WrappedFunctionPointerParameterListAlignsToItsFirstParameter()
    {
        const string input = """
                             unsafe class C
                             {
                                 delegate*<int, // first
                                     string, void> _callback;
                             }
                             """;
        const string expected = """
                                unsafe class C
                                {
                                    delegate*<int, // first
                                              string, void> _callback;
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a later base type that starts a line is indented to the column of the first base type
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-016")]
    public void BaseTypesAlignToTheFirstBaseType()
    {
        const string input = """
                             class C : System.IDisposable, // first
                                 System.ICloneable
                             {
                                 public void Dispose()
                                 {
                                 }

                                 public object Clone()
                                 {
                                     return null;
                                 }
                             }
                             """;
        const string expected = """
                                class C : System.IDisposable, // first
                                          System.ICloneable
                                {
                                    public void Dispose()
                                    {
                                    }

                                    public object Clone()
                                    {
                                        return null;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a constructor initializer and the <c>where</c> clauses are indented one level deeper than their declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-017")]
    public void ConstructorInitializersAndConstraintClausesAreIndentedOneLevel()
    {
        const string input = """
                             class B
                             {
                                 public B(int x)
                                 {
                                 }
                             }

                             class C<T> : B
                                 where T : class
                             {
                                 public C()
                                 : base(1)
                                 {
                                 }

                                 void M<U>()
                                         where U : struct
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class B
                                {
                                    public B(int x)
                                    {
                                    }
                                }

                                class C<T> : B
                                    where T : class
                                {
                                    public C()
                                        : base(1)
                                    {
                                    }

                                    void M<U>()
                                        where U : struct
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>: this(...)</c> initializer and a misindented type constraint are indented one level deeper than their declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-017")]
    public void ThisInitializerAndTypeConstraintAreIndentedOneLevel()
    {
        const string input = """
                             class C<T>
                             where T : new()
                             {
                                 public C(int x)
                                 {
                                 }

                                 public C()
                                             : this(1)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C<T>
                                    where T : new()
                                {
                                    public C(int x)
                                    {
                                    }

                                    public C()
                                        : this(1)
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a wrapped <c>when</c> clause is indented one level deeper than its <c>case</c> keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-021")]
    public void WrappedWhenClauseIsIndentedOneLevelDeeperThanCase()
    {
        const string input = """
                             class C
                             {
                                 void M(object o)
                                 {
                                     switch (o)
                                     {
                                         case int i // positive
                                         when i > 0:
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(object o)
                                    {
                                        switch (o)
                                        {
                                            case int i // positive
                                                when i > 0:
                                                break;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}