using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Full-pipeline tests for the structural rewrites of a C# 15 union declaration: an empty body becomes a semicolon unless trivia
/// keeps its braces, and a field declaration with several declarators is split into one field per declarator
/// </summary>
[TestClass]
public class UnionDeclarationStructuralTests : FormatterTestsBase
{
    #region Fields

    /// <summary>
    /// The parse options of C# 15, the version that introduces union declarations
    /// </summary>
    private static readonly CSharpParseOptions _cSharp15 = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp15);

    #endregion // Fields

    #region Tests

    /// <summary>
    /// Verifies the union example of FMT-STRUCT-017: an empty body over two lines and an empty body on one line behind a
    /// <c>where</c> clause become a semicolon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void EmptyUnionBodyBecomesASemicolon()
    {
        const string input = """
                             union U(int, string)
                             {
                             }

                             union V<T>(T, string) where T : class { }
                             """;
        const string expected = """
                                union U(int, string);

                                union V<T>(T, string)
                                    where T : class;
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that an empty union body followed by a semicolon, on one line and over two lines, becomes one semicolon behind
    /// the header
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void EmptyUnionBodyFollowedByASemicolonBecomesOneSemicolon()
    {
        const string input = """
                             union S1(int, string) { };

                             union U(int, string)
                             {
                             };
                             """;
        const string expected = """
                                union S1(int, string);

                                union U(int, string);
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that an empty union body on one line and one over two lines become a semicolon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void EmptyUnionBodiesOnOneAndTwoLinesBecomeASemicolon()
    {
        const string input = """
                             public union U2(int, string) { }
                             public union U3(int, string)
                             {
                             }
                             """;
        const string expected = """
                                public union U2(int, string);
                                public union U3(int, string);
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that the empty body of a <c>partial</c> union becomes a semicolon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void EmptyPartialUnionBodyBecomesASemicolon()
    {
        const string input = """
                             public partial union B8(int, string) { }
                             """;
        const string expected = """
                                public partial union B8(int, string);
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a union declared with a semicolon stays unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    public void UnionDeclaredWithASemicolonStaysUnchanged()
    {
        const string input = """
                             public union Pet(Cat, Dog);

                             public class Cat;

                             public class Dog;
                             """;

        AssertRuleResult(input, null, _cSharp15);
    }

    /// <summary>
    /// Verifies that an empty union body that holds a block comment keeps its braces, which then start their own lines, as a
    /// class's do
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-018")]
    [Requirement("FMT-BREAK-002")]
    public void EmptyUnionBodyWithABlockCommentKeepsItsBraces()
    {
        const string input = """
                             public union U9(int, string) { /* reserved */ }
                             """;
        const string expected = """
                                public union U9(int, string)
                                { /* reserved */
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that an empty union body with an end-of-line comment behind its opening brace keeps its braces, and the brace
    /// starts its own line together with the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-018")]
    [Requirement("FMT-BREAK-002")]
    public void EmptyUnionBodyWithAnEndOfLineCommentKeepsItsBraces()
    {
        const string input = """
                             namespace N
                             {
                                 public union B6(int, string) { // trailing
                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    public union B6(int, string)
                                    { // trailing
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that an empty union body that holds a directive keeps its braces and stays unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-018")]
    public void EmptyUnionBodyWithADirectiveKeepsItsBraces()
    {
        const string input = """
                             namespace N
                             {
                                 public union B7(int, string)
                                 {
                             #if DEBUG
                             #endif
                                 }
                             }
                             """;

        AssertRuleResult(input, null, _cSharp15);
    }

    /// <summary>
    /// Verifies the union example of FMT-STRUCT-021: a static field declaration with two declarators in a union is split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void StaticFieldInAUnionIsSplit()
    {
        const string input = """
                             union U(int, string)
                             {
                                 public static int A = 1, B = 2;
                             }
                             """;
        const string expected = """
                                union U(int, string)
                                {
                                    public static int A = 1;
                                    public static int B = 2;
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a constant declaration with two declarators in a one-line union is split, and the union's braces start their
    /// own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    [Requirement("FMT-BREAK-002")]
    public void ConstantInAOneLineUnionIsSplit()
    {
        const string input = """
                             public union B10(int, string) { public const int A = 1, B = 2; }
                             """;
        const string expected = """
                                public union B10(int, string)
                                {
                                    public const int A = 1;
                                    public const int B = 2;
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that a static field declaration with two declarators in a one-line union is split, and the union's braces start
    /// their own lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    [Requirement("FMT-BREAK-002")]
    public void StaticFieldInAOneLineUnionIsSplit()
    {
        const string input = """
                             public union B9(int, string) { public static int A = 1, B = 2; }
                             """;
        const string expected = """
                                public union B9(int, string)
                                {
                                    public static int A = 1;
                                    public static int B = 2;
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies the union example of FMT-STRUCT-028: a union declaration is a type, so its attribute list with two attributes
    /// is split into one list per attribute, each on its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void UnionAttributeListIsSplit()
    {
        const string input = """
                             [A, B] union U(int, string);
                             """;
        const string expected = """
                                [A]
                                [B]
                                union U(int, string);
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that each list split from a union's list keeps the <c>type:</c> specifier
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void UnionAttributeListWithTypeSpecifierIsSplitKeepingTheSpecifier()
    {
        const string input = """
                             [type: A, B] union U(int, string);
                             """;
        const string expected = """
                                [type: A]
                                [type: B]
                                union U(int, string);
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that the attribute list of a union nested in a class, in front of a union with a body, is split, and that the
    /// created lists take the union's indentation
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void NestedUnionAttributeListIsSplit()
    {
        const string input = """
                             class C
                             {
                                 [A, B] union N(int, string)
                                 {
                                     public int X => 1;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    [A]
                                    [B]
                                    union N(int, string)
                                    {
                                        public int X => 1;
                                    }
                                }
                                """;

        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies that the attribute list of a union with a comment inside stays one list and stays as written, as a struct's
    /// does
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-028")]
    public void UnionAttributeListWithACommentInsideStaysOneList()
    {
        const string input = """
                             [A /* c */, B] struct S;

                             [A /* c */, B] union U(int, string);
                             """;

        AssertRuleResult(input, null, _cSharp15);
    }

    #endregion // Tests
}