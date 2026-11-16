using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Full-pipeline tests for the layout of a C# 15 union declaration: its braces and members start their own lines, the opening
/// parenthesis of its case-type list joins its name, and each of its <c>where</c> clauses starts its own line, as for a class
/// </summary>
[TestClass]
public class UnionDeclarationLayoutTests : FormatterTestsBase
{
    #region Fields

    /// <summary>
    /// The parse options of C# 15, the version that introduces union declarations
    /// </summary>
    private static readonly CSharpParseOptions _cSharp15 = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp15);

    #endregion // Fields

    #region Tests

    /// <summary>
    /// Verifies the union example of FMT-BREAK-002: the braces of a union declaration start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public void UnionBracesStartTheirOwnLines()
    {
        const string input = """
                             union U(int, string) { public int X => 1; }
                             """;
        const string expected = """
                                union U(int, string)
                                {
                                    public int X => 1;
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies the union example of FMT-BREAK-007: each member of a union declaration starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-007")]
    public void UnionMembersStartTheirOwnLines()
    {
        const string input = """
                             union U(int, string)
                             {
                                 public int X => 1; public int Y => 2;
                             }
                             """;
        const string expected = """
                                union U(int, string)
                                {
                                    public int X => 1;
                                    public int Y => 2;
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that the braces and members of a one-line union start their own lines, also when a member gets a block body and
    /// the union has a base list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    [Requirement("FMT-BREAK-007")]
    public void OneLineUnionsGetTheirBracesAndMembersOnTheirOwnLines()
    {
        const string input = """
                             public union U1(int, string) { public int X => 1; public int Y => 2; }
                             public union U4(int, string) : System.IComparable { public int CompareTo(object o) => 0; }
                             """;
        const string expected = """
                                public union U1(int, string)
                                {
                                    public int X => 1;
                                    public int Y => 2;
                                }
                                public union U4(int, string) : System.IComparable
                                {
                                    public int CompareTo(object o)
                                    {
                                        return 0;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a union with an explicit constructor and a constructor initializer is laid out as a class with a primary
    /// constructor is
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public void UnionWithAConstructorIsLaidOutAsAClass()
    {
        const string input = """
                             namespace N
                             {
                                 public union A7(int, string) { public A7(long x) : this((int)x) { } }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    public union A7(int, string)
                                    {
                                        public A7(long x)
                                            : this((int)x)
                                        {
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that the members of a <c>readonly</c> union start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    [Requirement("FMT-BREAK-007")]
    public void ReadonlyUnionIsLaidOut()
    {
        const string input = """
                             public readonly union R1(int, string) { public int X => 1; public int Y => 2; }
                             """;
        const string expected = """
                                public readonly union R1(int, string)
                                {
                                    public int X => 1;
                                    public int Y => 2;
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a union nested in a class and a union nested in that union are laid out, each one level further in
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    [Requirement("FMT-BREAK-007")]
    public void NestedUnionsAreLaidOut()
    {
        const string input = """
                             class Outer { union In(int, string) { public int X => 1; union Deep(long, string) { public int Y => 2; } } }
                             """;
        const string expected = """
                                class Outer
                                {
                                    union In(int, string)
                                    {
                                        public int X => 1;
                                        union Deep(long, string)
                                        {
                                            public int Y => 2;
                                        }
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a union whose body holds a region gets its first member on the line after its opening brace, and that the
    /// region keeps its place
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public void UnionWithARegionGetsItsFirstMemberOnTheLineAfterItsBrace()
    {
        const string input = """
                             namespace N
                             {
                                 public union C1(int, string) { public int Z => 0;
                             #region R
                                     public int X => 1;
                                     #endregion
                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    public union C1(int, string)
                                    {
                                        public int Z => 0;

                                        #region R

                                        public int X => 1;

                                        #endregion // R
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a union already laid out as a class stays unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    [Requirement("FMT-BREAK-007")]
    public void LaidOutUnionStaysUnchanged()
    {
        const string input = """
                             namespace N
                             {
                                 public union Pet(Cat, Dog)
                                 {
                                     public int X => 1;
                                     public int Y => 2;
                                 }

                                 public class Cat;

                                 public class Dog;
                             }
                             """;

        AssertRuleResult(input, null, _cSharp15);
    }

    /// <summary>
    /// Verifies the union example of FMT-BREAK-011: the opening parenthesis of a case-type list on the line after the union's name
    /// joins the name, and a wrapped list with leading commas keeps one case type per line, aligned under the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void UnionCaseTypeListIsLaidOutAsAPrimaryConstructorParameterList()
    {
        const string input = """
                             union U1
                                 (int, string);

                             union U2(int
                                      , string
                                      , long
                                 );
                             """;
        const string expected = """
                                union U1(int, string);

                                union U2(int,
                                         string,
                                         long);
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that the opening parenthesis of a case-type list on the line after the name of a union with a body joins the name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void UnionCaseTypeListOnTheNextLineJoinsTheNameOfAUnionWithABody()
    {
        const string input = """
                             public union U2
                                 (int, string)
                             {
                                 public int X => 1;
                             }
                             """;
        const string expected = """
                                public union U2(int, string)
                                {
                                    public int X => 1;
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a wrapped case-type list keeps one case type per line, aligned under the first
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-011")]
    public void WrappedUnionCaseTypeListKeepsItsAlignedLayout()
    {
        const string input = """
                             internal union Nested(int,
                                 string,
                                 long);
                             """;
        const string expected = """
                                internal union Nested(int,
                                                      string,
                                                      long);
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a block comment between a union's name and its case-type list stays there, as for a class
    /// </summary>
    [TestMethod]
    public void BlockCommentBetweenUnionNameAndCaseTypeListStays()
    {
        const string input = """
                             public union B1 /* c */ (int, string);
                             """;

        AssertRuleResult(input, null, _cSharp15);
    }

    /// <summary>
    /// Verifies that an end-of-line comment between a union's name and its case-type list keeps the line break behind it, as for a
    /// class
    /// </summary>
    [TestMethod]
    public void EndOfLineCommentBetweenUnionNameAndCaseTypeListKeepsItsLineBreak()
    {
        const string input = """
                             namespace N
                             {
                                 public union B2 // c
                                     (int, string);
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    public union B2 // c
                                    (int, string);
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies the union example of FMT-BREAK-023: each <c>where</c> clause of a union starts its own line, on a union declared
    /// with a semicolon and on one with a body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-023")]
    public void UnionConstraintClausesStartTheirOwnLines()
    {
        const string input = """
                             union U<T>(T, string)where T : class;

                             union V<T, S>(T, S) where T : class where S : struct
                             {
                                 public int X => 1;
                             }
                             """;
        const string expected = """
                                union U<T>(T, string)
                                    where T : class;

                                union V<T, S>(T, S)
                                    where T : class
                                    where S : struct
                                {
                                    public int X => 1;
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that the constraint clauses and the braces of a one-line union start their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    [Requirement("FMT-BREAK-023")]
    public void OneLineUnionWithConstraintClausesIsLaidOut()
    {
        const string input = """
                             public union U6<T>(T, string)where T:class;
                             public union U8<T, S>(T, S) where T : class where S : struct { public bool IsOk => true; }
                             """;
        const string expected = """
                                public union U6<T>(T, string)
                                    where T : class;
                                public union U8<T, S>(T, S)
                                    where T : class
                                    where S : struct
                                {
                                    public bool IsOk => true;
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a <c>where</c> clause behind a block comment starts its own line and the comment stays behind the case-type
    /// list, as for a class
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-023")]
    public void ConstraintClauseBehindABlockCommentStartsItsOwnLine()
    {
        const string input = """
                             namespace N
                             {
                                 public union B4<T>(T, string) /* c */ where T : class { public int X => 1; }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    public union B4<T>(T, string) /* c */
                                        where T : class
                                    {
                                        public int X => 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a directive between a union's case-type list and its opening brace stays there, and the braces and member
    /// behind it are laid out, as for a class
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    public void DirectiveInFrontOfTheOpeningBraceOfAUnionStays()
    {
        const string input = """
                             namespace N
                             {
                                 public union D1(int, string)
                             #if DEBUG
                             #endif
                                 { public int X => 1; }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    public union D1(int, string)
                                #if DEBUG
                                #endif
                                    {
                                        public int X => 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a directive in front of a union's <c>where</c> clause stays there, and the clause, braces, and member are laid
    /// out, as for a class
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-002")]
    [Requirement("FMT-BREAK-023")]
    public void DirectiveInFrontOfAUnionConstraintClauseStays()
    {
        const string input = """
                             namespace N
                             {
                                 public union D2<T>(T, string)
                             #if DEBUG
                             #endif
                                 where T : class { public int X => 1; }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    public union D2<T>(T, string)
                                #if DEBUG
                                #endif
                                        where T : class
                                    {
                                        public int X => 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a <c>where</c> clause behind an end-of-line comment keeps its line and stays unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-023")]
    public void ConstraintClauseBehindAnEndOfLineCommentStaysUnchanged()
    {
        const string input = """
                             namespace N
                             {
                                 public union B3<T>(T, string) // c
                                     where T : class;
                             }
                             """;

        AssertRuleResult(input, null, _cSharp15);
    }

    #endregion // Tests
}