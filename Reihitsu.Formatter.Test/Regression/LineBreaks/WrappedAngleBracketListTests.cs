using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for joining a wrapped angle-bracket delimited list (type argument, type
/// parameter, or function-pointer parameter list) onto a single line when safe, and for aligning its
/// continuation lines to the first element's column when the join is refused, across every syntactic
/// position such a list can occupy (issue #693)
/// </summary>
[TestClass]
public class WrappedAngleBracketListTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a wrapped type-argument list inside a base-type list with exactly one base type
    /// is joined onto the declaration line
    /// </summary>
    [TestMethod]
    public void SingleBaseTypeWithWrappedTypeArgumentsJoins()
    {
        // Arrange
        const string input = """
                             public class Implementation : Base<T1,
                                                                T2>
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class Implementation : Base<T1, T2>
                                {
                                    public void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment between two type arguments refuses the join and aligns the
    /// continuation to the first type argument's column, instead of the enclosing block level
    /// </summary>
    [TestMethod]
    public void LineCommentBetweenTypeArgumentsRefusesJoinAndAligns()
    {
        // Arrange
        const string input = """
                             public class Implementation : Base<T1, // note
                                                                T2>
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class Implementation : Base<T1, // note
                                                                   T2>
                                {
                                    public void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment on a type argument refuses the join and aligns the continuation
    /// to the first type argument's column
    /// </summary>
    [TestMethod]
    public void BlockCommentOnTypeArgumentRefusesJoinAndAligns()
    {
        // Arrange
        const string input = """
                             public class Implementation : Base<T1 /* note */,
                                                                T2>
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class Implementation : Base<T1 /* note */,
                                                                   T2>
                                {
                                    public void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an active preprocessor directive between type arguments refuses the join and
    /// aligns both branches' live tokens to the first type argument's column
    /// </summary>
    [TestMethod]
    public void ActiveDirectiveBetweenTypeArgumentsRefusesJoinAndAligns()
    {
        // Arrange
        const string input = """
                             public class Implementation : Base<T1,
                             #if true
                                                                T2>
                             #else
                                                                T3>
                             #endif
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class Implementation : Base<T1,
                                #if true
                                                                   T2>
                                #else
                                                                   T3>
                                #endif
                                {
                                    public void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a nested type-argument list which is itself unjoinable — its own interior
    /// carries a comment — keeps the outer list from joining too, since the outer list still sees a
    /// multi-line element after the inner list is rewritten
    /// </summary>
    [TestMethod]
    public void NestedUnjoinableTypeArgumentRefusesOuterJoin()
    {
        // Arrange
        const string input = """
                             public class Implementation : Base<System.Collections.Generic.Dictionary<string, // note
                                                                                                       int>,
                                                                T2>
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class Implementation : Base<System.Collections.Generic.Dictionary<string, // note
                                                                                                         int>,
                                                                   T2>
                                {
                                    public void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a type-argument list already on one line, inside a wrapped base-type list, is
    /// left byte-identical
    /// </summary>
    [TestMethod]
    public void AlreadyJoinedTypeArgumentsInWrappedBaseListIsStable()
    {
        // Arrange
        const string input = """
                             public class Implementation : Base<T1, T2>,
                                                           IInterface1,
                                                           IInterface2
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a type argument inside an inactive (<c>#if false</c>-style) branch keeps its
    /// authored column, since disabled text carries no tokens for the layout model to see
    /// </summary>
    [TestMethod]
    public void DisabledTextInsideTypeArgumentListIsUntouched()
    {
        // Arrange
        const string input = """
                             public class Implementation : Base<T1,
                             #if NEVER_DEFINED
                                                                T2,
                             #endif
                                                                T3>
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a wrapped type-parameter list on a class declaration is joined onto one line
    /// </summary>
    [TestMethod]
    public void WrappedClassTypeParameterListJoins()
    {
        // Arrange
        const string input = """
                             public class Sample<T1,
                                                 T2>
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class Sample<T1, T2>
                                {
                                    public void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped type-parameter list on a method declaration is joined onto one line
    /// </summary>
    [TestMethod]
    public void WrappedMethodTypeParameterListJoins()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void Foo<TA,
                                                 TB>()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public class Sample
                                {
                                    public void Foo<TA, TB>()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped function-pointer parameter list is joined onto one line
    /// </summary>
    [TestMethod]
    public void WrappedFunctionPointerParameterListJoins()
    {
        // Arrange
        const string input = """
                             public unsafe class Sample
                             {
                                 private delegate*<int,
                                                   long,
                                                   void> _pointer;
                             }
                             """;
        const string expected = """
                                public unsafe class Sample
                                {
                                    private delegate*<int, long, void> _pointer;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped generic field type joins onto one line
    /// </summary>
    [TestMethod]
    public void WrappedGenericFieldTypeJoins()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 private Dictionary<string,
                                                    int> _field;
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    private Dictionary<string, int> _field;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped generic return type joins onto one line
    /// </summary>
    [TestMethod]
    public void WrappedGenericReturnTypeJoins()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public Dictionary<string,
                                                   int> Foo()
                                 {
                                     return null;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public Dictionary<string, int> Foo()
                                    {
                                        return null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped generic parameter type joins onto one line
    /// </summary>
    [TestMethod]
    public void WrappedGenericParameterTypeJoins()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public void Foo(Dictionary<string,
                                                            int> map)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public void Foo(Dictionary<string, int> map)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped generic local-declaration type joins onto one line
    /// </summary>
    [TestMethod]
    public void WrappedGenericLocalDeclarationTypeJoins()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public void Foo()
                                 {
                                     Dictionary<string,
                                                int> map = null;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public void Foo()
                                    {
                                        Dictionary<string, int> map = null;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped generic type inside <c>typeof</c> joins onto one line
    /// </summary>
    [TestMethod]
    public void WrappedGenericTypeOfArgumentJoins()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public void Foo()
                                 {
                                     var type = typeof(Dictionary<string,
                                                                  int>);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public void Foo()
                                    {
                                        var type = typeof(Dictionary<string, int>);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that wrapped invocation type arguments join onto one line
    /// </summary>
    [TestMethod]
    public void WrappedInvocationTypeArgumentsJoin()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public void Foo()
                                 {
                                     Bar<string,
                                         int>();
                                 }

                                 private void Bar<T1, T2>()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public void Foo()
                                    {
                                        Bar<string, int>();
                                    }

                                    private void Bar<T1, T2>()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped generic type in an object-creation expression joins onto one line
    /// </summary>
    [TestMethod]
    public void WrappedGenericObjectCreationTypeJoins()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public void Foo()
                                 {
                                     var map = new Dictionary<string,
                                                              int>();
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public void Foo()
                                    {
                                        var map = new Dictionary<string, int>();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped generic type in a generic constraint clause joins onto one line
    /// </summary>
    [TestMethod]
    public void WrappedGenericConstraintTypeJoins()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public void Foo<T>()
                                     where T : IComparable<T,
                                                          T>
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public void Foo<T>()
                                        where T : IComparable<T, T>
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped generic type in a using-alias directive joins onto one line
    /// </summary>
    [TestMethod]
    public void WrappedGenericUsingAliasTypeJoins()
    {
        // Arrange
        const string input = """
                             using Lookup = System.Collections.Generic.Dictionary<string,
                                                                                  int>;

                             public class Sample
                             {
                                 public void Foo()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using Lookup = System.Collections.Generic.Dictionary<string, int>;

                                public class Sample
                                {
                                    public void Foo()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped generic type nested inside a multi-line argument list joins onto one
    /// line while its sibling argument keeps its own alignment
    /// </summary>
    [TestMethod]
    public void WrappedGenericTypeNestedInMultiLineArgumentListJoins()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public void Foo()
                                 {
                                     Bar(
                                         Baz<string,
                                             int>(),
                                         1);
                                 }

                                 private void Bar(object a, int b)
                                 {
                                 }

                                 private T Baz<T1, T2>()
                                 {
                                     return default;
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public void Foo()
                                    {
                                        Bar(Baz<string, int>(),
                                            1);
                                    }

                                    private void Bar(object a, int b)
                                    {
                                    }

                                    private T Baz<T1, T2>()
                                    {
                                        return default;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an unbound generic (<c>Dictionary&lt;,&gt;</c>) whose omitted type arguments
    /// span multiple lines leaves the closing bracket at its pre-existing column instead of aligning
    /// to a zero-width anchor, since an omitted type argument's token carries no real column
    /// </summary>
    [TestMethod]
    public void UnboundGenericWithZeroWidthAnchorIsNotAlignedToADegenerateColumn()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 public void Foo()
                                 {
                                     var type = typeof(Dictionary<,
                                                                  >);
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    public void Foo()
                                    {
                                        var type = typeof(Dictionary<,
                                                >);
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-argument type-argument list whose join is refused by an interior
    /// comment still aligns its closing bracket to the argument's actual rendered column — not the
    /// enclosing block level — which is the falsifier for a <c>Count &lt; 2</c> early return copied
    /// from <c>BaseTypeListContributor</c>: dropping it would leave the closing bracket at the
    /// method-body block level instead of tracking the argument
    /// </summary>
    [TestMethod]
    public void SingleArgumentRefusedJoinAlignsCloseBracketToArgumentColumn()
    {
        // Arrange
        const string input = """
                             public class Sample
                             {
                                 public void Foo()
                                 {
                                     Bar(1,
                                         Baz<int // note
                                     >());
                                 }

                                 private void Bar(int a, object b)
                                 {
                                 }

                                 private T Baz<T>()
                                 {
                                     return default;
                                 }
                             }
                             """;
        const string expected = """
                                public class Sample
                                {
                                    public void Foo()
                                    {
                                        Bar(1,
                                            Baz<int // note
                                                >());
                                    }

                                    private void Bar(int a, object b)
                                    {
                                    }

                                    private T Baz<T>()
                                    {
                                        return default;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-argument type-argument list whose only argument is itself an
    /// unjoinable nested generic list stays wrapped, instead of the cascading join that would happen
    /// if the nested list joined first
    /// </summary>
    [TestMethod]
    public void SingleArgumentWithUnjoinableNestedGenericRefusesJoin()
    {
        // Arrange
        const string input = """
                             using System.Collections.Generic;

                             public class Sample
                             {
                                 private List<System.Collections.Generic.Dictionary<string, // note
                                                                                    int>> _items;
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                public class Sample
                                {
                                    private List<System.Collections.Generic.Dictionary<string, // note
                                                                                       int>> _items;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}