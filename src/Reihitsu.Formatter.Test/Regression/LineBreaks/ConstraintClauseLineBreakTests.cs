using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Tests for the line breaks inside a generic constraint clause: the joins that structure nothing, the one-line or
/// one-constraint-per-line shape of the clause, and the comments and directives that block a join
/// </summary>
[TestClass]
public class ConstraintClauseLineBreakTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the reported input: a partly wrapped clause, a clause wrapped behind a comma, and a clause wrapped behind
    /// its colon become one constraint per line, aligned under the first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void ReportedWrappedConstraintClausesAreLaidOutOneConstraintPerLine()
    {
        const string input = """
                             using System;

                             namespace N
                             {
                                 public class A<TKey, TValue>
                                     where TKey : class,
                                     new()
                                     where TValue : struct
                                 {
                                     public void M<TFirst, TSecond>(TFirst first)
                                         where TFirst : class, IComparable<TFirst>,
                                             IEquatable<TFirst>, new()
                                         where TSecond :
                                             IDisposable
                                     {
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                namespace N
                                {
                                    public class A<TKey, TValue>
                                        where TKey : class,
                                                     new()
                                        where TValue : struct
                                    {
                                        public void M<TFirst, TSecond>(TFirst first)
                                            where TFirst : class,
                                                           IComparable<TFirst>,
                                                           IEquatable<TFirst>,
                                                           new()
                                            where TSecond : IDisposable
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a clause wrapped between two constraints becomes one constraint per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void PartlyWrappedClauseBecomesOneConstraintPerLine()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T : class, IComparable<T>,
                                         IEquatable<T>, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M<T>()
                                        where T : class,
                                                  IComparable<T>,
                                                  IEquatable<T>,
                                                  new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a single constraint wrapped behind the colon is joined onto the colon's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void SingleConstraintWrappedBehindColonIsJoined()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T :
                                         IDisposable
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M<T>()
                                        where T : IDisposable
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: the first of several constraints wrapped behind the colon is joined onto the colon's
    /// line, and the clause stays one constraint per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void FirstOfSeveralConstraintsWrappedBehindColonIsJoined()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T :
                                         class,
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
    /// Verifies the spec example: a comma that starts a line moves to the end of the line of the constraint in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void CommaAtLineStartMovesToEndOfPreviousLine()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class
                                         , new()
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
    /// Verifies the spec example: a clause written on one line stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ClauseOnOneLineStaysUnchanged()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T : class, IComparable<T>, IEquatable<T>, new()
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a clause already laid out one constraint per line and aligned under its first constraint stays unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void ClauseAlreadyOneConstraintPerLineStaysUnchanged()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T : class,
                                               IComparable<T>,
                                               new()
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: a colon wrapped below the type parameter joins the type parameter's line, and the clause
    /// then ends on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ColonWrappedBelowTypeParameterJoinsItsLine()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T
                                         : class, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class, new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change-doc example: a colon wrapped below the type parameter joins its line, and a clause that is also
    /// wrapped between constraints becomes one constraint per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ColonWrappedBelowTypeParameterJoinsAndWrappedClauseIsLaidOut()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T
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
                                        where T : class,
                                                  new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a wrapped type argument list inside a constraint is joined, and the clause is judged
    /// after the join, so it stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void WrappedTypeArgumentListInsideConstraintJoinsAndClauseStaysOnOneLine()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M<T>()
                                     where T : class, IDictionary<int,
                                         string>, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M<T>()
                                        where T : class, IDictionary<int, string>, new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a wrapped qualified name inside a constraint is joined, and the clause stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void WrappedQualifiedNameInsideConstraintJoins()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class, System.
                                         IDisposable, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class, System.IDisposable, new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped alias-qualified name inside a constraint is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void WrappedAliasQualifiedNameInsideConstraintJoins()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class, global::
                                         System.IDisposable, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class, global::System.IDisposable, new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break inside a <c>new()</c> constraint is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void WrappedConstructorConstraintJoins()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class, new(
                                         )
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class, new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break inside a <c>class?</c> constraint is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void WrappedNullableClassConstraintJoins()
    {
        const string input = """
                             #nullable enable

                             class C
                             {
                                 void M<T>()
                                     where T : class
                                         ?, System.IDisposable
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                #nullable enable

                                class C
                                {
                                    void M<T>()
                                        where T : class?, System.IDisposable
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break inside an <c>allows ref struct</c> constraint is joined
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void WrappedAllowsRefStructConstraintJoins()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T : IDisposable, allows ref
                                         struct
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M<T>()
                                        where T : IDisposable, allows ref struct
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a clause whose inner wrap is joined is still wrapped by a line break behind a comma, so it becomes one
    /// constraint per line after the join
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ClauseWithJoinedInnerWrapIsStillWrappedByCommaWrap()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class, System.
                                         IDisposable,
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
                                                  System.IDisposable,
                                                  new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a constraint that a comment keeps over two lines wraps its clause, and the constraint's
    /// inner line follows the constraint to its new column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void ConstraintHeldOverTwoLinesByCommentWrapsItsClause()
    {
        const string input = """
                             using System.Collections.Generic;

                             class C
                             {
                                 void M<T>()
                                     where T : class, IDictionary<int, // c
                                         string>, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System.Collections.Generic;

                                class C
                                {
                                    void M<T>()
                                        where T : class,
                                                  IDictionary<int, // c
                                                              string>,
                                                  new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a comment behind the type parameter keeps the colon on the next line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void CommentBehindTypeParameterKeepsColonOnItsOwnLine()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
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
    /// Verifies the spec example: a comment behind the colon keeps the first constraint on the next line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void CommentBehindColonKeepsFirstConstraintOnNextLine()
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
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment behind the colon wraps a clause whose constraints are written on one line, so they become
    /// one constraint per line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void CommentBehindColonWrapsClauseWithConstraintsOnOneLine()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : // c
                                         class, new()
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
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a directive behind the colon keeps the first constraint on the next line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void DirectiveBehindColonKeepsFirstConstraintOnNextLine()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T :
                             #if !X
                                         class,
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
                                        where T :
                                #if !X
                                                  class,
                                #endif
                                                  new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a comment in front of a comma keeps the comma at the start of its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void CommentInFrontOfCommaKeepsCommaAtLineStart()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class // c
                                         , new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class // c
                                                  , new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a directive in front of a comma keeps the comma at the start of its line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void DirectiveInFrontOfCommaKeepsCommaAtLineStart()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T : IDisposable
                             #if !X
                                         , allows ref struct
                             #endif
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M<T>()
                                        where T : IDisposable
                                #if !X
                                                  , allows ref struct
                                #endif
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a block comment behind a comma stays behind it, and the constraint behind the comment
    /// starts its own line in a wrapped clause
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void BlockCommentBehindCommaStaysBehindIt()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T : class, /* c */ new(),
                                         IDisposable
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M<T>()
                                        where T : class, /* c */
                                                  new(),
                                                  IDisposable
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the change-doc example: an end-of-line comment behind a comma stays behind it, and the next constraint is aligned
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void LineCommentBehindCommaStaysBehindIt()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class, // c
                                         new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class, // c
                                                  new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comma that starts a line moves onto the previous line when the comment stands behind the comma, not
    /// in front of it, and the comment stays behind the comma
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void CommaWithCommentBehindItMovesToEndOfPreviousLine()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class
                                         , // c
                                         new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T : class, // c
                                                  new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line in a comma gap that is not joined is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void BlankLineBehindCommaIsKept()
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
    /// Verifies that a blank line behind the colon goes with the join of the first constraint onto the colon's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void BlankLineBehindColonGoesWithTheJoin()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T :

                                         IDisposable
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M<T>()
                                        where T : IDisposable
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a clause on one line with a comment at the end of the line stays unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ClauseOnOneLineWithTrailingCommentStaysUnchanged()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class, new() // c
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a clause on one line with a block comment between two constraints stays unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ClauseOnOneLineWithInlineBlockCommentStaysUnchanged()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T : class, /* c */ new()
                                 {
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that <c>allows ref struct</c> as the last constraint of a wrapped clause starts its own line and is aligned
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void AllowsRefStructAsLastConstraintOfWrappedClauseIsLaidOut()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T : IDisposable,
                                         allows ref struct
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M<T>()
                                        where T : IDisposable,
                                                  allows ref struct
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two wrapped clauses of one declaration whose type parameter names differ in width are each laid out
    /// one constraint per line under their own first constraint
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void TwoWrappedClausesOfOneDeclarationAreEachLaidOut()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<TKey, TLongValueName>()
                                     where TKey : class, IDisposable,
                                         new()
                                     where TLongValueName : struct,
                                         IComparable<TLongValueName>, IEquatable<TLongValueName>
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M<TKey, TLongValueName>()
                                        where TKey : class,
                                                     IDisposable,
                                                     new()
                                        where TLongValueName : struct,
                                                               IComparable<TLongValueName>,
                                                               IEquatable<TLongValueName>
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a wrapped clause whose type parameter is a verbatim identifier aligns under the first constraint behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-INDENT-032")]
    public void WrappedClauseOfVerbatimTypeParameterIsLaidOut()
    {
        const string input = """
                             class C
                             {
                                 void M<@class>()
                                     where @class : class, System.IDisposable
                                         , new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<@class>()
                                        where @class : class,
                                                       System.IDisposable,
                                                       new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a colon that a comment keeps on its own line wraps its clause, also when its constraints
    /// are written on one line, while a clause whose colon is joined stays on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ColonKeptOnItsOwnLineByCommentWrapsClauseWrittenOnOneLine()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T // c
                                         : class, new()
                                 {
                                 }

                                 void N<T>()
                                     where T
                                         : class, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T // c
                                            : class,
                                              new()
                                    {
                                    }

                                    void N<T>()
                                        where T : class, new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a colon a comment keeps on its own line wraps a clause of three constraints written on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ColonKeptOnItsOwnLineByCommentWrapsEveryConstraint()
    {
        const string input = """
                             using System;

                             class C
                             {
                                 void M<T>()
                                     where T // c
                                         : class, IDisposable, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    void M<T>()
                                        where T // c
                                            : class,
                                              IDisposable,
                                              new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a colon a directive keeps on its own line wraps its clause, also when its constraints are written on
    /// one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ColonKeptOnItsOwnLineByDirectiveWrapsClause()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T
                             #if !X
                             #endif
                                         : class, new()
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
                                #endif
                                            : class,
                                              new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a colon that disabled text keeps on its own line wraps its clause
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ColonKeptOnItsOwnLineByDisabledTextWrapsClause()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T
                             #if X
                                         , unused
                             #endif
                                         : class, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T
                                #if X
                                            , unused
                                #endif
                                            : class,
                                              new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a colon a comment keeps on its own line leaves a single constraint behind it on the colon's line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    public void ColonKeptOnItsOwnLineByCommentKeepsSingleConstraintOnItsLine()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T // c
                                         : class
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T // c
                                            : class
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tight colon a comment keeps on its own line gets its space, and the clause is wrapped and aligned
    /// behind it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-037")]
    [Requirement("FMT-SPACE-009")]
    public void TightColonKeptOnItsOwnLineByCommentIsSpacedAndWrapsClause()
    {
        const string input = """
                             class C
                             {
                                 void M<T>()
                                     where T // c
                                         :class, new()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M<T>()
                                        where T // c
                                            : class,
                                              new()
                                    {
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}