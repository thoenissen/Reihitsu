using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for the columns of the lines of a wrapped generic constraint clause that start with the colon, a constraint, or a
/// comma between two constraints, for every owner of a clause and in every enclosing scope
/// </summary>
[TestClass]
public class ConstraintClauseAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: the constraints of a type's, a method's, and an extension block's clauses that start a
    /// line align under the first constraint, and a later <c>where</c> clause keeps its own column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-032")]
    public void ConstraintsOfTypeMethodAndExtensionBlockAlignUnderFirstConstraint()
    {
        const string input = """
                             class A<TKey, TValue>
                                 where TKey : class,
                                 new()
                                 where TValue : struct
                             {
                                 void M<T>()
                                     where T : class,
                                     new()
                                 {
                                 }
                             }

                             static class E
                             {
                                 extension<T>(T value)
                                     where T : class,
                                     new()
                                 {
                                     public void X()
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class A<TKey, TValue>
                                    where TKey : class,
                                                 new()
                                    where TValue : struct
                                {
                                    void M<T>()
                                        where T : class,
                                                  new()
                                    {
                                    }
                                }

                                static class E
                                {
                                    extension<T>(T value)
                                        where T : class,
                                                  new()
                                    {
                                        public void X()
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a constraint and a comma that a comment or a directive pushes onto the next line take
    /// the first constraint's column, and a colon that a comment pushes onto the next line is indented one level deeper
    /// than <c>where</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-032")]
    public void LinesPushedDownByCommentsAndDirectivesTakeTheFirstConstraintColumn()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : // c
                                     class,
                                     new()
                                 {
                                 }

                                 void N<T>()
                                     where T :
                             #if !X
                                     class,
                             #endif
                                     new()
                                 {
                                 }

                                 void O<T>()
                                     where T : class // c
                                 , new()
                                 {
                                 }

                                 void P<T>()
                                     where T // c
                                     : class,
                                     new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : // c
                                                  class,
                                                  new()
                                    {
                                    }

                                    void N<T>()
                                        where T :
                                #if !X
                                                  class,
                                #endif
                                                  new()
                                    {
                                    }

                                    void O<T>()
                                        where T : class // c
                                                  , new()
                                    {
                                    }

                                    void P<T>()
                                        where T // c
                                            : class,
                                              new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a colon that a directive keeps below the type parameter is indented one level deeper than
    /// <c>where</c>, and the constraints align under the first constraint behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-032")]
    public void ColonPushedDownByDirectiveIsIndentedOneLevelDeeperThanWhere()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T
                             #if !X
                                 : class,
                             #endif
                                 new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T
                                #if !X
                                            : class,
                                #endif
                                              new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the constraints of an interface member without a body, a local function, and a delegate align under
    /// their first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void ConstraintsOfMemberWithoutBodyLocalFunctionAndDelegateAlign()
    {
        const string input = """
                             interface I
                             {
                                 void M<T>()
                                     where T : class,
                                     new();
                             }

                             class C
                             {
                                 void M()
                                 {
                                     void L<T>()
                                         where T : class,
                                         new()
                                     {
                                     }
                                 }
                             }

                             delegate void D<T>()
                                 where T : class,
                                 new();
                             """;
        const string expected = """
                                interface I
                                {
                                    void M<T>()
                                        where T : class,
                                                  new();
                                }

                                class C
                                {
                                    void M()
                                    {
                                        void L<T>()
                                            where T : class,
                                                      new()
                                        {
                                        }
                                    }
                                }

                                delegate void D<T>()
                                    where T : class,
                                              new();
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the partly wrapped clause of a struct becomes one constraint per line, aligned under the first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void StructClauseIsLaidOut()
    {
        const string input = """
                             using System;

                             struct S<T>
                                 where T : class, IDisposable,
                                 new()
                             {
                                 void M()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                struct S<T>
                                    where T : class,
                                              IDisposable,
                                              new()
                                {
                                    void M()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the partly wrapped clause of an interface becomes one constraint per line, aligned under the first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void InterfaceClauseIsLaidOut()
    {
        const string input = """
                             using System;

                             interface I<T>
                                 where T : class, IDisposable,
                                 new()
                             {
                                 void M();
                             }
                             """;
        const string expected = """
                                using System;

                                interface I<T>
                                    where T : class,
                                              IDisposable,
                                              new()
                                {
                                    void M();
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the partly wrapped clause of a record becomes one constraint per line, aligned under the first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void RecordClauseIsLaidOut()
    {
        const string input = """
                             using System;

                             record R<T>(T Value)
                                 where T : class, IDisposable,
                                 new();
                             """;
        const string expected = """
                                using System;

                                record R<T>(T Value)
                                    where T : class,
                                              IDisposable,
                                              new();
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the partly wrapped clause of a record struct becomes one constraint per line, aligned under the first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void RecordStructClauseIsLaidOut()
    {
        const string input = """
                             using System;

                             record struct R<T>(T Value)
                                 where T : IDisposable, IComparable<T>,
                                 IEquatable<T>;
                             """;
        const string expected = """
                                using System;

                                record struct R<T>(T Value)
                                    where T : IDisposable,
                                              IComparable<T>,
                                              IEquatable<T>;
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a clause wrapped in front of a comma on an extension block becomes one constraint per line, aligned
    /// under the first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void ExtensionBlockClauseWrappedInFrontOfCommaIsLaidOut()
    {
        const string input = """
                             static class E
                             {
                                 extension<T>(T value)
                                     where T : class
                                         , new()
                                 {
                                     public void X()
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                static class E
                                {
                                    extension<T>(T value)
                                        where T : class,
                                                  new()
                                    {
                                        public void X()
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an empty type whose body becomes a semicolon keeps its wrapped clause laid out one constraint per line,
    /// with the semicolon behind the last constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void EmptyInterfaceWithWrappedClauseIsLaidOut()
    {
        const string input = """
                             interface I<T>
                                 where T : class
                                     , new()
                             {
                             }
                             """;
        const string expected = """
                                interface I<T>
                                    where T : class,
                                              new();
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the constraints are aligned under the first constraint as it stands after the <c>where</c> line moves
    /// right to its column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-032")]
    public void ConstraintsFollowWhereLineMovedRight()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                 where T : class,
                                           new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class,
                                                  new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the constraints are aligned under the first constraint as it stands after the <c>where</c> line moves
    /// left to its column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-032")]
    public void ConstraintsFollowWhereLineMovedLeft()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                             where T : class,
                                                       new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class,
                                                  new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the clause of a nested type in a file-scoped namespace aligns under its first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-032")]
    public void NestedTypeInFileScopedNamespaceClauseAligns()
    {
        const string input = """
                             namespace N;

                             class O
                             {
                                 class I<T>
                                     where T : class,
                                 new()
                                 {
                                     void M()
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                namespace N;

                                class O
                                {
                                    class I<T>
                                        where T : class,
                                                  new()
                                    {
                                        void M()
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the clause of a local function inside a lambda body aligns under its first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-032")]
    public void LocalFunctionInLambdaClauseAligns()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.Action a = () =>
                                     {
                                         void L<T>()
                                             where T : class,
                                         new()
                                         {
                                         }
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        System.Action a = () =>
                                        {
                                            void L<T>()
                                                where T : class,
                                                          new()
                                            {
                                            }
                                        };
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}