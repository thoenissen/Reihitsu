using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7101DoNotCombineFieldsCodeFixProvider"/> on comments, documentation comments, and directives in and around the declaration and its type, which the fix keeps or for which it is withheld
/// </summary>
[TestClass]
public class RH7101DoNotCombineFieldsCommentAndDirectiveTests : BatchCodeFixTestsBase<RH7101DoNotCombineFieldsAnalyzer, RH7101DoNotCombineFieldsCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// A document with a combined field declaration in front of a <c>/* */</c> comment that starts inside the <c>#if !X</c> group
    /// and holds the line <c>#else</c>, so a build with <c>X</c> reads the lines behind it as the content of the verbatim string
    /// <c>s</c>
    /// </summary>
    private const string CarrierDocumentSource = """
                                                 internal class OtherClass
                                                 {
                                                     private int c, {|RH7101:d|};

                                                     int M(int a)
                                                     {
                                                 #if !X
                                                         /*
                                                 #else
                                                         var s = @"
                                                         */
                                                         a = 1; //";
                                                 #endif
                                                         return a;
                                                     }
                                                 }
                                                 """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that the fix is withheld for a combined declaration in a type that holds a conditional with comments between its
    /// operators and its branches: the fix formats the type, which the formatter refuses as unsupported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task NoFixIsOfferedInATypeThatHoldsACommentBehindAConditionalOperator()
    {
        const string source = """
                              class C
                              {
                                  private int _x, {|RH7101:_y|};

                                  int M(bool a)
                                  {
                                      var x = a ? /* yes */ 1 : /* no */ 2;

                                      return x;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation-style comment in front of the semicolon withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task DocumentationStyleCommentInFrontOfTheSemicolonWithholdsTheFix()
    {
        const string source = """
                              internal class TestClass
                              {
                                  private int firstField, {|RH7101:secondField|} /** Trailing note. */;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation-style comment in front of a separating comma withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task DocumentationStyleCommentInFrontOfASeparatingCommaWithholdsTheFix()
    {
        const string source = """
                              internal class TestClass
                              {
                                  private int firstField /** First field. */, {|RH7101:secondField|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a single-line documentation comment in front of a later declarator withholds the fix: it is no
    /// documentation comment of the declaration, and the formatter refuses it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    [Requirement("RH7101-007")]
    public async Task SingleLineDocumentationCommentInFrontOfALaterDeclaratorWithholdsTheFix()
    {
        const string source = """
                              internal class TestClass
                              {
                                  private int firstField,
                                              /// <summary>Second field.</summary>
                                              {|RH7101:secondField|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment in front of a later declarator withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    [Requirement("RH7101-007")]
    public async Task DelimitedDocumentationCommentInFrontOfALaterDeclaratorWithholdsTheFix()
    {
        const string source = """
                              internal class TestClass
                              {
                                  private int firstField,
                                              /** Second field. */
                                              {|RH7101:secondField|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a comment behind a binary operator, in an initializer value or elsewhere in the type,
    /// withholds the fix for every combined declaration of the type
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SpecExampleCommentBehindABinaryOperatorWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a = 1 + /* c */ 2, {|RH7101:_b|};

                                  private int _x, {|RH7101:_y|};

                                  int M(int value)
                                  {
                                      return value + /* c */ 1;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment behind a binary operator in an initializer value of the declaration withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentBehindABinaryOperatorInAnInitializerValueWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _a = 1 + /* c */ 2, {|RH7101:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment behind a binary operator in a method of the type, far from the declaration, withholds the fix, also
    /// in a nested type
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int M(int value)\n    {\n        return value + /* c */ 1;\n    }\n}", DisplayName = "In a method of the type")]
    [DataRow("#nullable enable\n\nclass C\n{\n    private int _x, {|RH7101:_y|};\n\n    class D\n    {\n        bool M(bool a, bool b)\n        {\n            return a &&\n                // c\n                b;\n        }\n    }\n}", DisplayName = "In a nested type")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int M(int value)\n    {\n        return value +\n#pragma warning disable CS0001\n            1;\n    }\n}", DisplayName = "Directive in a method of the type")]
    public async Task CommentBehindABinaryOperatorElsewhereInTheTypeWithholdsTheFix(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a comment behind the arrow of an expression body elsewhere in the type withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SpecExampleCommentBehindTheArrowOfAnExpressionBodyWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a, {|RH7101:_b|};

                                  int A => // c
                                      _a;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment or directive behind the arrow of an expression body withholds the fix wherever the type holds it:
    /// in a method, in an accessor, in a local function in an initializer value of the declaration, and in a nested type
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int M() => /* c */ 1;\n}", DisplayName = "Block comment behind the arrow of a method on one line")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int P\n    {\n        get =>\n            // c\n            _x;\n    }\n}", DisplayName = "Comment line behind the arrow of a getter")]
    [DataRow("using System;\n\nclass C\n{\n    private Func<int> _f = () =>\n    {\n        int L() => /* c */ 1;\n\n        return L();\n    }, {|RH7101:_g|};\n}", DisplayName = "Local function in an initializer value")]
    [DataRow("#nullable enable\n\nclass C\n{\n    private int _x, {|RH7101:_y|};\n\n    class D\n    {\n        int P =>\n#pragma warning disable CS0001\n            1;\n    }\n}", DisplayName = "Directive behind the arrow in a nested type")]
    public async Task CommentBehindTheArrowOfAnExpressionBodyElsewhereInTheTypeWithholdsTheFix(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment behind a conditional expression's operator, or between a list element and a comma on a later line,
    /// elsewhere in the type withholds the fix
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int M(bool b)\n    {\n        return b ? /* c */ 1 : 2;\n    }\n}", DisplayName = "Behind a conditional operator")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    void M()\n    {\n        N(1 // c\n          , 2);\n    }\n\n    void N(int a, int b)\n    {\n    }\n}", DisplayName = "In front of a comma on a later line")]
    public async Task CommentOfAnotherRefusedShapeElsewhereInTheTypeWithholdsTheFix(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the change example: a comment between the <c>?</c> and the <c>.</c> of a link in a declarator's value withholds the
    /// fix, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SpecExampleCommentInsideALinkOperatorWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private static string Source = "";

                                  private int? _a = Source? // one
                                                          .Length, {|RH7101:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment between the tokens of a <c>?.</c>, a <c>!.</c>, or a <c>!?.</c> elsewhere in the type withholds the
    /// fix
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int? M(string s)\n    {\n        return s? /* c */ .Length;\n    }\n}", DisplayName = "Inside a ?.")]
    [DataRow("#nullable enable\n\nclass C\n{\n    private int _x, {|RH7101:_y|};\n\n    int M(string? s)\n    {\n        return s! /* c */ .Length;\n    }\n}", DisplayName = "Inside a !.")]
    [DataRow("#nullable enable\n\nclass C\n{\n    private int _x, {|RH7101:_y|};\n\n    int? M(string? s)\n    {\n        return s!\n            // c\n            ?.Length;\n    }\n}", DisplayName = "Inside a !?.")]
    public async Task CommentInsideALinkOperatorElsewhereInTheTypeWithholdsTheFix(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the change example: a comment between the <c>.</c> of a member access and its name in a declarator's value
    /// withholds the fix, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task ChangeExampleCommentBetweenTheDotAndTheMemberNameWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private static string Source = "";

                                  private int _a = Source. // one
                                                   Length, {|RH7101:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment between the <c>.</c> of a member access or a member binding and its name elsewhere in the type
    /// withholds the fix
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int M(string s)\n    {\n        return s. /* c */ Length;\n    }\n}", DisplayName = "Behind the dot of a member access")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int? M(string s)\n    {\n        return s?.\n            // c\n            Length;\n    }\n}", DisplayName = "In front of the name of a member binding")]
    public async Task CommentBetweenTheDotAndTheMemberNameElsewhereInTheTypeWithholdsTheFix(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment in front of a binary operator elsewhere in the type does not withhold the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentInFrontOfABinaryOperatorElsewhereInTheTypeDoesNotWithholdTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _x, {|RH7101:_y|};

                                  int M(int value)
                                  {
                                      return value /* c */ + 1;
                                  }
                              }
                              """;
        const string fixedSource = """
                                   class C
                                   {
                                       private int _x;
                                       private int _y;

                                       int M(int value)
                                       {
                                           return value /* c */ + 1;
                                       }
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
    }

    /// <summary>
    /// Verifies that a comment written at column zero in front of the first declarator's name, on a later line than the type,
    /// withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentAtColumnZeroInFrontOfTheFirstNameWithholdsTheFix()
    {
        const string source = """
                              internal class TestClass
                              {
                                  private int
                              /* c */ firstField, {|RH7101:secondField|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment behind a separating comma withholds the fix: the formatter refuses the type the fix formats the
    /// declaration in, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentBehindASeparatingCommaWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _first, // first
                                              {|RH7101:_second|}; // second
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a region in front of one declaration and a conditional group around another do not
    /// withhold the fix, which splits both declarations and keeps each directive in front of the first field
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    [Requirement("RH7101-004")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task SpecExampleDirectiveInFrontOfTheDeclarationDoesNotWithholdTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  #region Fields

                                  private int _x, {|RH7101:_y|};

                                  #endregion // Fields

                              #if !RELEASE
                                  private int _p, {|RH7101:_q|} = 1;
                              #endif
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       #region Fields

                                       private int _x;
                                       private int _y;

                                       #endregion // Fields

                                   #if !RELEASE
                                       private int _p;
                                       private int _q = 1;
                                   #endif
                                   }
                                   """;

        await Verify(ConvertEndOfLine(source, endOfLine), ConvertEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that a region whose directives stand directly in front of and behind the declaration does not withhold the fix,
    /// and the fix leaves both directives where they are
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    [Requirement("RH7101-004")]
    public async Task RegionDirectiveDirectlyInFrontOfTheDeclarationDoesNotWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  #region Fields
                                  private int _a, {|RH7101:_b|};
                                  #endregion
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       #region Fields
                                       private int _a;
                                       private int _b;
                                       #endregion
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a pragma directive between the declarators withholds the fix, and the diagnostic is still
    /// reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    public async Task PragmaDirectiveBetweenTheDeclaratorsWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _a,
                              #pragma warning disable CS0169
                                              {|RH7101:_b|};
                              #pragma warning restore CS0169
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment in front of a later declarator, behind a comment that trails the separating comma, withholds
    /// the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentInFrontOfALaterDeclaratorWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _x, // horizontal
                                              /* vertical */ {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment behind a blank line in front of a later declarator withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentBehindABlankLineInFrontOfALaterDeclaratorWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _x, // horizontal

                                              /* vertical */ {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that documentation comments in front of later declarators withhold the fix in every documentation mode
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    [Requirement("RH7101-007")]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.Diagnose)]
    [DataRow(DocumentationMode.None)]
    public async Task DocumentationCommentsInFrontOfLaterDeclaratorsWithholdTheFixInEveryDocumentationMode(DocumentationMode documentationMode)
    {
        const string source = """
                              class C
                              {
                                  private int _x,
                                              /// <summary>Second field.</summary>
                                              {|RH7101:_y|},
                                              /** Third field. */
                                              _z;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, documentationMode);
    }

    /// <summary>
    /// Verifies the specification example with a comment that spans lines in each gap: the fix lays out the declaration
    /// as the formatter lays it out, so the first field stays behind the closing <c>*/</c> in front, the member behind the
    /// closing <c>*/</c> behind stays where it is, and only the later field starts a line of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLeavesTheMembersBehindACommentThatSpansLinesInTheGapsWhereTheyAre()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* a
                                     b */ private int _x, {|RH7101:_y|}; /* c
                                     d */ private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p; /* a
                                          b */ private int _x;
                                       private int _y; /* c
                                          d */ private int _q;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
        AssertFormattingChangesNothing(fixedSource);
    }

    /// <summary>
    /// Verifies that the fix leaves the first field behind the closing <c>*/</c> of a comment that spans lines in the gap
    /// in front of the declaration, as the formatter leaves it, and does not change the member in front
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLeavesTheFirstFieldBehindACommentThatSpansLinesInTheGapInFront()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* a
                                     b */ private int _x, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p; /* a
                                          b */ private int _x;
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
        AssertFormattingChangesNothing(fixedSource);
    }

    /// <summary>
    /// Verifies that the fix leaves a member behind the closing <c>*/</c> of a comment that spans lines behind the
    /// declaration where it is, as the formatter leaves it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLeavesTheMemberBehindACommentThatSpansLinesInTheGapBehind()
    {
        const string source = """
                              class C
                              {
                                  private int _x, {|RH7101:_y|}; /* a
                                     b */ private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x;
                                       private int _y; /* a
                                          b */ private int _q;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
        AssertFormattingChangesNothing(fixedSource);
    }

    /// <summary>
    /// Verifies that a comment that spans lines behind the separating comma withholds the fix, also with a member in front of
    /// the declaration on its line
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentThatSpansLinesBehindTheSeparatorWithholdsTheFixBehindAMemberInFront(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _p; private int _x, /* a
                                     b */ {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source.ReplaceLineEndings(endOfLine));
    }

    /// <summary>
    /// Verifies that a comment that spans lines behind the separating comma withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentThatSpansLinesBehindTheSeparatorWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _x, /* a
                                     b */ {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source.ReplaceLineEndings(endOfLine));
    }

    /// <summary>
    /// Verifies that a documentation comment that spans lines in the gap in front of the declaration, with another one in the gap behind it, which is in the declaration's leading trivia and documents it, withholds the fix in the
    /// documentation modes Parse and Diagnose, and that the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentThatSpansLinesInTheGapInFrontWithholdsTheFixAlsoWithOneInTheGapBehind()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /** a
                                     b */ private int _x, {|RH7101:_y|}; /** c
                                     d */ private int _q;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment that spans lines in the gap in front of the declaration, which is in the declaration's leading trivia and documents it, withholds the fix in the
    /// documentation modes Parse and Diagnose, and that the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentThatSpansLinesInTheGapInFrontWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /** a
                                     b */ private int _x, {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the member behind the target starts the next line with its documentation comment when the gap behind
    /// the target holds a documentation comment that spans lines, and that no blank line is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixStartsTheMemberBehindADocumentationCommentThatSpansLinesInTheGapBehindOnTheNextLine()
    {
        const string source = """
                              class C
                              {
                                  private int _x, {|RH7101:_y|}; /** c
                                     d */ private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x;
                                       private int _y;
                                       /** c
                                          d */ private int _q;
                                   }
                                   """;

        const string formattedSource = """
                                       class C
                                       {
                                           private int _x;
                                           private int _y;

                                           /** c
                                              d */ private int _q;
                                       }
                                       """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
        AssertFormattingAddsOnlyBlankLines(source, fixedSource, formattedSource);
    }

    /// <summary>
    /// Verifies that a documentation comment behind a comment that spans lines in the gap in front of the declaration, with the same pair in the gap behind it, which is in the declaration's leading trivia and documents it, withholds the fix in the
    /// documentation modes Parse and Diagnose, and that the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentBehindACommentThatSpansLinesInTheGapInFrontWithholdsTheFixAlsoWithOneInTheGapBehind()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* a
                                     b */ /** c */ private int _x, {|RH7101:_y|}; /* d
                                     e */ /** f */ private int _q;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment behind a comment that spans lines in the gap in front of the declaration, which is in the declaration's leading trivia and documents it, withholds the fix in the
    /// documentation modes Parse and Diagnose, and that the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentBehindACommentThatSpansLinesInTheGapInFrontWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* a
                                     b */ /** c */ private int _x, {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the member behind the target starts the next line with its documentation comment when the gap behind
    /// the target holds a comment that spans lines and then a one-line documentation comment, and that no blank line is
    /// written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixStartsTheMemberBehindACommentThatSpansLinesAndADocumentationCommentInTheGapBehindOnTheNextLine()
    {
        const string source = """
                              class C
                              {
                                  private int _x, {|RH7101:_y|}; /* d
                                     e */ /** f */ private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x;
                                       private int _y; /* d
                                          e */
                                       /** f */ private int _q;
                                   }
                                   """;

        const string formattedSource = """
                                       class C
                                       {
                                           private int _x;
                                           private int _y; /* d
                                              e */

                                           /** f */ private int _q;
                                       }
                                       """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
        AssertFormattingAddsOnlyBlankLines(source, fixedSource, formattedSource);
    }

    /// <summary>
    /// Verifies that a documentation comment that spans lines behind a one-line comment in the gap in front of the declaration, with the same pair in the gap behind it, which is in the declaration's leading trivia and documents it, withholds the fix in the
    /// documentation modes Parse and Diagnose, and that the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentThatSpansLinesBehindAOneLineCommentInTheGapInFrontWithholdsTheFixAlsoWithOneInTheGapBehind()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* a */ /** b
                                     c */ private int _x, {|RH7101:_y|}; /* d */ /** e
                                     f */ private int _q;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment that spans lines behind a one-line comment in the gap in front of the declaration, which is in the declaration's leading trivia and documents it, withholds the fix in the
    /// documentation modes Parse and Diagnose, and that the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentThatSpansLinesBehindAOneLineCommentInTheGapInFrontWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* a */ /** b
                                     c */ private int _x, {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the member behind the target starts the next line with its documentation comment when the gap behind
    /// the target holds a one-line comment and then a documentation comment that spans lines, and that no blank line is
    /// written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixStartsTheMemberBehindAOneLineCommentAndADocumentationCommentThatSpansLinesInTheGapBehindOnTheNextLine()
    {
        const string source = """
                              class C
                              {
                                  private int _x, {|RH7101:_y|}; /* d */ /** e
                                     f */ private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x;
                                       private int _y; /* d */
                                       /** e
                                          f */ private int _q;
                                   }
                                   """;

        const string formattedSource = """
                                       class C
                                       {
                                           private int _x;
                                           private int _y; /* d */

                                           /** e
                                              f */ private int _q;
                                       }
                                       """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
        AssertFormattingAddsOnlyBlankLines(source, fixedSource, formattedSource);
    }

    /// <summary>
    /// Verifies that a documentation comment that spans lines behind a comment that spans lines in the gap in front of the declaration, with the same pair in the gap behind it, which is in the declaration's leading trivia and documents it, withholds the fix in the
    /// documentation modes Parse and Diagnose, and that the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentThatSpansLinesBehindACommentThatSpansLinesInTheGapInFrontWithholdsTheFixAlsoWithOneInTheGapBehind()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* a
                                     b */ /** c
                                     d */ private int _x, {|RH7101:_y|}; /* e
                                     f */ /** g
                                     h */ private int _q;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment that spans lines behind a comment that spans lines in the gap in front of the declaration, which is in the declaration's leading trivia and documents it, withholds the fix in the
    /// documentation modes Parse and Diagnose, and that the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentThatSpansLinesBehindACommentThatSpansLinesInTheGapInFrontWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* a
                                     b */ /** c
                                     d */ private int _x, {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that the member behind the target starts the next line with its documentation comment when the gap behind
    /// the target holds a comment that spans lines and then a documentation comment that spans lines, and that no blank line
    /// is written above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixStartsTheMemberBehindACommentThatSpansLinesAndADocumentationCommentThatSpansLinesInTheGapBehindOnTheNextLine()
    {
        const string source = """
                              class C
                              {
                                  private int _x, {|RH7101:_y|}; /* e
                                     f */ /** g
                                     h */ private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x;
                                       private int _y; /* e
                                          f */
                                       /** g
                                          h */ private int _q;
                                   }
                                   """;

        const string formattedSource = """
                                       class C
                                       {
                                           private int _x;
                                           private int _y; /* e
                                              f */

                                           /** g
                                              h */ private int _q;
                                       }
                                       """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
        AssertFormattingAddsOnlyBlankLines(source, fixedSource, formattedSource);
    }

    /// <summary>
    /// Verifies the specification example on the other side of the condition: the target's first token and the first token
    /// of the member behind it already start their lines, so the documentation comments that span lines behind the code in
    /// front stay where they stand, although document-level formatting moves each onto a line of its own
    /// in the documentation mode None, where a documentation comment of the declaration does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLeavesDocumentationCommentsThatSpanLinesInFrontOfMembersThatStartTheirLinesWhereTheyStand()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /** a
                                     b */
                                  private int _x, {|RH7101:_y|}; /** c
                                     d */
                                  private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p; /** a
                                          b */
                                       private int _x;
                                       private int _y; /** c
                                          d */
                                       private int _q;
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y", DocumentationMode.None);
    }

    /// <summary>
    /// Verifies the other side of the condition with a <c>///</c> comment behind the code in front of the target and behind
    /// the target: the first token behind each comment already starts its line, so the comments stay where they stand
    /// in the documentation mode None, where a documentation comment of the declaration does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLeavesSingleLineDocumentationCommentsInFrontOfMembersThatStartTheirLinesWhereTheyStand()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /// a
                                  private int _x, {|RH7101:_y|}; /// c
                                  private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p; /// a
                                       private int _x;
                                       private int _y; /// c
                                       private int _q;
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y", DocumentationMode.None);
    }

    /// <summary>
    /// Verifies that the fix leaves a documentation comment that starts its line in front of the target and in front of the
    /// member behind it where it stands, although the token behind it shares its line: the first field keeps the target's line
    /// and column behind the comment
    /// in the documentation mode None, where a documentation comment of the declaration does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLeavesDocumentationCommentsThatStartTheirLinesInFrontOfMembersOnTheSameLineWhereTheyStand()
    {
        const string source = """
                              class C
                              {
                                  private int _p;
                                  /** x */ private int _x, {|RH7101:_y|};
                                  /** q */ private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p;
                                       /** x */ private int _x;
                                       private int _y;
                                       /** q */ private int _q;
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y", DocumentationMode.None);
    }

    /// <summary>
    /// Verifies the gap in front of the target alone: the fix leaves a documentation comment that starts its line in front of
    /// the target, with the target's first token behind it on that line, where it stands
    /// in the documentation mode None, where a documentation comment of the declaration does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLeavesTheFirstFieldBehindADocumentationCommentThatStartsItsLineInTheGapInFrontOnTheTargetsLine()
    {
        const string source = """
                              class C
                              {
                                  private int _p;
                                  /** x */ private int _x, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p;
                                       /** x */ private int _x;
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y", DocumentationMode.None);
    }

    /// <summary>
    /// Verifies the specification example: a declaration with a <c>///</c> and one with a <c>/** */</c> documentation
    /// comment in their leading trivia are reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentedDeclarationsAreReportedWithoutAFix()
    {
        const string source = """
                              class C
                              {
                                  /// <summary>The coordinates</summary>
                                  public int X, {|RH7101:Y|};

                                  /** <summary>The sizes</summary> */
                                  public int Width, {|RH7101:Height|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example in the documentation mode Diagnose: a documented declaration is reported, and
    /// no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentedDeclarationsAreReportedWithoutAFixInDocumentationModeDiagnose()
    {
        const string source = """
                              class C
                              {
                                  /// <summary>The coordinates</summary>
                                  public int X, {|RH7101:Y|};

                                  /** <summary>The sizes</summary> */
                                  public int Width, {|RH7101:Height|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, DocumentationMode.Diagnose);
    }

    /// <summary>
    /// Verifies that a documentation comment in front of an attribute list of the declaration withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentInFrontOfTheAttributesOfTheDeclarationWithholdsTheFix()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  /// <summary>The coordinates</summary>
                                  [NonSerialized]
                                  public int X, {|RH7101:Y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example in the documentation mode None: the comments are ordinary comments, and the
    /// fix splits both declarations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    [Requirement("RH7101-006")]
    public async Task FixSplitsDeclarationsWithDocumentationStyleCommentsInDocumentationModeNone()
    {
        const string source = """
                              class C
                              {
                                  /// <summary>The coordinates</summary>
                                  public int X, {|RH7101:Y|};

                                  /** <summary>The sizes</summary> */
                                  public int Width, {|RH7101:Height|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       /// <summary>The coordinates</summary>
                                       public int X;
                                       public int Y;

                                       /** <summary>The sizes</summary> */
                                       public int Width;
                                       public int Height;
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
    }

    /// <summary>
    /// Verifies that in the documentation mode None a <c>/** */</c> comment that starts behind the code of another member on
    /// the declaration's line is an ordinary comment between the two members, which stays behind the code in front of it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    [Requirement("RH7101-006")]
    public async Task FixKeepsAnOrdinaryCommentInTheGapInFrontBehindTheMemberInFrontInDocumentationModeNone()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /** X. */ private int _x, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p; /** X. */
                                       private int _x;
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject));
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y", DocumentationMode.None);
    }

    /// <summary>
    /// Verifies the specification example: an ordinary comment in front of the declaration does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task OrdinaryCommentInFrontOfTheDeclarationDoesNotWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  // The coordinates
                                  public int X, {|RH7101:Y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       // The coordinates
                                       public int X;
                                       public int Y;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: a declaration with a comment between a modifier and the type, inside its
    /// attribute list, behind the type, in front of a value, or behind a separating comma is reported, and no code fix is
    /// offered for any of them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task SpecExampleCommentInsideTheDeclarationWithholdsTheFix()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private /* c */ int _a, {|RH7101:_b|};

                                  [Obsolete /* why */]
                                  private int _c, {|RH7101:_d|};

                                  private int /* c */ _first, {|RH7101:_second|};

                                  private int _g = /* c */ 1, {|RH7101:_h|};

                                  private int _x, // horizontal
                                              {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: a comment that the formatter refuses elsewhere in the type withholds the fix
    /// of a declaration that holds no comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [Requirement("SHARED-FIX-010")]
    public async Task SpecExampleCommentElsewhereInTheTypeWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _a, {|RH7101:_b|};

                                  void M()
                                  {
                                      int /* c */ x = 1;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment that the formatter refuses in a nested type withholds the fix of a declaration of the outer
    /// type, whose context holds the nested type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentInANestedTypeWithholdsTheFixOfADeclarationOfTheOuterType()
    {
        const string source = """
                              class C
                              {
                                  private int _a, {|RH7101:_b|};

                                  class D
                                  {
                                      private int /* c */ _x;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment that the formatter refuses in the outer type does not withhold the fix of a declaration of a
    /// nested type: the fix formats the declaration in the nested type, which holds no such comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    public async Task CommentInTheOuterTypeDoesNotWithholdTheFixOfADeclarationOfANestedType()
    {
        const string source = """
                              class C
                              {
                                  private int /* c */ _a;

                                  class D
                                  {
                                      private int _x, {|RH7101:_y|};
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int /* c */ _a;

                                       class D
                                       {
                                           private int _x;
                                           private int _y;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that Fix All splits the declaration of a type the formatter does not refuse and leaves the declaration of a
    /// type it refuses reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-010")]
    [Requirement("SHARED-FIX-002")]
    public async Task FixAllSplitsOnlyTheDeclarationsOfTypesTheFormatterDoesNotRefuse()
    {
        const string source = """
                              class A
                              {
                                  private int _a, {|RH7101:_b|};

                                  void M()
                                  {
                                      int /* c */ x = 1;
                                  }
                              }

                              class B
                              {
                                  private int _c, {|RH7101:_d|};
                                  private int _e, {|RH7101:_f|};
                              }
                              """;

        const string fixedSource = """
                                   class A
                                   {
                                       private int _a, {|RH7101:_b|};

                                       void M()
                                       {
                                           int /* c */ x = 1;
                                       }
                                   }

                                   class B
                                   {
                                       private int _c;
                                       private int _d;
                                       private int _e;
                                       private int _f;
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies the specification example: a comment inside an initializer value does not withhold the fix, and stays in
    /// that value. The space between the comment and the argument is the one the formatter writes behind a comment that
    /// <c>(</c> carries (FMT-SPACE-013)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [Requirement("RH7101-004")]
    public async Task SpecExampleCommentInsideAnInitializerValueDoesNotWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  private static int _first = Offset(/* c */ 1), {|RH7101:_second|};

                                  private static int Offset(int value) => value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private static int _first = Offset(/* c */1);
                                       private static int _second;

                                       private static int Offset(int value) => value;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: a comment in front of the declaration stays on the first field, a comment inside
    /// an initializer value stays in that value, and a comment behind the semicolon stays behind the last field. The space
    /// between the comment and the argument is the one the formatter writes behind a comment that <c>(</c> carries
    /// (FMT-SPACE-013)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task SpecExampleFixKeepsTheCommentsInFrontOfTheDeclarationInsideAValueAndBehindTheSemicolon()
    {
        const string source = """
                              class C
                              {
                                  // The coordinates
                                  private static int _x = Offset(/* horizontal */ 1), {|RH7101:_y|}; // vertical

                                  private static int Offset(int value) => value;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       // The coordinates
                                       private static int _x = Offset(/* horizontal */1);
                                       private static int _y; // vertical

                                       private static int Offset(int value) => value;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a comment behind the attribute list, on the attribute list's line, withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentBehindTheAttributeListOnItsLineWithholdsTheFix()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  [Obsolete] /* c */
                                  private int _s, {|RH7101:_t|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment on its own line between the attribute list and the modifiers withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentOnItsOwnLineBetweenTheAttributeListAndTheModifiersWithholdsTheFix()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  [Obsolete]
                                  // c
                                  private int _e, {|RH7101:_f|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment between two modifiers withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentBetweenTwoModifiersWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  public /* c */ static int _q, {|RH7101:_r|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment inside a qualified type withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentInsideTheTypeWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private System. /* c */ Int32 _o, {|RH7101:_p|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation-style comment between the attribute list and the modifiers, which is no documentation
    /// comment of the declaration, withholds the fix as any comment in that slot does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task DocumentationStyleCommentBehindTheAttributeListWithholdsTheFix()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  [Obsolete]
                                  /// <summary>m</summary>
                                  public int U, {|RH7101:V|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that in the documentation mode None a documentation-style comment between the attribute list and the
    /// modifiers is an ordinary comment in the part every field repeats, and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task DocumentationStyleCommentBehindTheAttributeListWithholdsTheFixInDocumentationModeNone()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  [Obsolete]
                                  /// <summary>m</summary>
                                  public int U, {|RH7101:V|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, DocumentationMode.None);
    }

    /// <summary>
    /// Verifies that a comment behind the type withholds the fix for a declaration of a nested type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task CommentBehindTheTypeOfADeclarationOfANestedTypeWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  class D
                                  {
                                      private int /* c */ _a, {|RH7101:_b|};
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment between the first declarator's <c>=</c> and its value withholds the fix: it stands in front of
    /// the value, not inside it
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentInFrontOfTheFirstValueWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _first = /* c */ 1, {|RH7101:_second|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source.ReplaceLineEndings(endOfLine));
    }

    /// <summary>
    /// Verifies that a comment on its own line in front of the first declarator's name withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentOnItsOwnLineInFrontOfTheFirstNameWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int
                                      /* c */ _first, {|RH7101:_second|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source.ReplaceLineEndings(endOfLine));
    }

    /// <summary>
    /// Verifies that a documentation-style comment on its own line in front of the first declarator's name withholds the fix
    /// in every documentation mode
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.Diagnose)]
    [DataRow(DocumentationMode.None)]
    public async Task DocumentationStyleCommentOnItsOwnLineInFrontOfTheFirstNameWithholdsTheFix(DocumentationMode documentationMode)
    {
        const string source = """
                              class C
                              {
                                  private int
                                      /** c */ _first, {|RH7101:_second|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, documentationMode);
    }

    /// <summary>
    /// Verifies that a documentation-style comment behind the type on the type's line withholds the fix when documentation
    /// comments are parsed, as it does when they are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task DocumentationStyleCommentBehindTheTypeWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int /** c */ _first, {|RH7101:_second|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a single-line documentation-style comment behind the type withholds the fix in the documentation mode
    /// Diagnose
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task SingleLineDocumentationStyleCommentBehindTheTypeWithholdsTheFixInDocumentationModeDiagnose()
    {
        const string source = """
                              class C
                              {
                                  private int /// <summary>c</summary>
                                      _g, {|RH7101:_h|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, DocumentationMode.Diagnose);
    }

    /// <summary>
    /// Verifies the counterpart of the specification example: in the documentation mode None, a <c>/** */</c> or <c>///</c>
    /// comment behind the type on the type's line is an ordinary comment in the part every field repeats; the diagnostic is
    /// reported and the fix is withheld
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task DocumentationStyleCommentBehindTheTypeWithholdsTheFixInDocumentationModeNone()
    {
        const string source = """
                              class C
                              {
                                  private int /** c */ _first, {|RH7101:_second|};

                                  private int /// <summary>c</summary>
                                      _g, {|RH7101:_h|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, DocumentationMode.None);
    }

    /// <summary>
    /// Verifies that an ordinary comment behind a documentation-style comment behind the type withholds the fix when
    /// documentation comments are parsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task OrdinaryCommentBehindADocumentationStyleCommentBehindTheTypeWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int /** c */ /* d */ _first, {|RH7101:_second|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example in the documentation mode None: the <c>/** */</c> comment and the ordinary comment
    /// behind it are trailing trivia of the type, in the part every field repeats; the diagnostic is reported and the fix is
    /// withheld
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task OrdinaryCommentBehindADocumentationStyleCommentBehindTheTypeWithholdsTheFixInDocumentationModeNone()
    {
        const string source = """
                              class C
                              {
                                  private int /** c */ /* d */ _first, {|RH7101:_second|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, DocumentationMode.None);
    }

    /// <summary>
    /// Verifies the specification example: an ordinary comment in front of a <c>/** */</c> comment behind the type is trailing
    /// trivia of the type, in the part every field repeats; the diagnostic is reported and the fix is withheld in every
    /// documentation mode
    /// </summary>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow(DocumentationMode.Parse)]
    [DataRow(DocumentationMode.Diagnose)]
    [DataRow(DocumentationMode.None)]
    public async Task OrdinaryCommentInFrontOfADocumentationStyleCommentBehindTheTypeWithholdsTheFix(DocumentationMode documentationMode)
    {
        const string source = """
                              class C
                              {
                                  private int /* d */ /** c */ _first, {|RH7101:_second|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, documentationMode);
    }

    /// <summary>
    /// Verifies that a line comment behind a documentation-style comment behind the type withholds the fix in the
    /// documentation mode Diagnose
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    public async Task LineCommentBehindADocumentationStyleCommentBehindTheTypeWithholdsTheFixInDocumentationModeDiagnose()
    {
        const string source = """
                              class C
                              {
                                  private int /** c */ // d
                                      _g, {|RH7101:_h|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source, DocumentationMode.Diagnose);
    }

    /// <summary>
    /// Verifies that documentation-style comments behind the types of two declarations withhold the fix for every declaration
    /// of the type, also for the declaration without a comment: the fix formats each declaration in the type, which the
    /// formatter refuses
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [Requirement("SHARED-FIX-010")]
    public async Task DocumentationStyleCommentsBehindTheTypesWithholdTheFixForEveryDeclarationOfTheType()
    {
        const string source = """
                              class C
                              {
                                  private int /** c */ /* d */ _a, {|RH7101:_b|};
                                  private int /* d */ /** c */ _c, {|RH7101:_d|};
                                  private int _e, {|RH7101:_f|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment in the declaration's leading trivia, on the declaration's line, does not withhold the fix,
    /// under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [Requirement("RH7101-004")]
    public async Task CommentInFrontOfTheDeclarationDoesNotWithholdTheFix()
    {
        const string source = """
                              class C
                              {
                                  /* c */ private int _first, {|RH7101:_second|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       /* c */ private int _first;
                                       private int _second;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_second");
    }

    /// <summary>
    /// Verifies that a comment behind the first declarator's name, in front of the separating comma, withholds the fix
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentBehindTheFirstNameWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _first /* c */, {|RH7101:_second|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source.ReplaceLineEndings(endOfLine));
    }

    /// <summary>
    /// Verifies that a comment inside one declaration withholds the fix for every combined declaration of the type: the fix
    /// formats each declaration in the type, which the formatter refuses as a whole
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [Requirement("SHARED-FIX-010")]
    public async Task CommentInOneDeclarationWithholdsTheFixForEveryDeclarationOfTheType()
    {
        const string source = """
                              class C
                              {
                                  private int /* c */ _a, {|RH7101:_b|};
                                  private int _c, {|RH7101:_d|};
                                  private int _e, {|RH7101:_f|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a Fix All in the project, started from a document without a hiding carrier, splits the declaration in that
    /// document and leaves a second document that holds one as it is, with its diagnostic still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-012")]
    public async Task FixAllInTheProjectLeavesADocumentWithAHidingCarrierUnchanged()
    {
        const string source = """
                              internal class TestClass
                              {
                                  private int a, {|RH7101:b|};
                              }
                              """;
        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       private int a;
                                       private int b;
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.Sources.Add(("/0/Test1.cs", CarrierDocumentSource));
                         test.FixedState.Sources.Add(("/0/Test1.cs", CarrierDocumentSource));

                         // The diagnostic in the carrier document stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                         test.NumberOfFixAllInDocumentIterations = 1;
                         test.NumberOfFixAllInProjectIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies that a Fix All in the solution, started from a document without a hiding carrier, splits the declaration in that
    /// document and leaves a document of another project that holds a hiding carrier as it is, with its diagnostic still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-012")]
    public async Task FixAllInTheSolutionLeavesADocumentWithAHidingCarrierInAnotherProjectUnchanged()
    {
        const string source = """
                              internal class TestClass
                              {
                                  private int a, {|RH7101:b|};
                              }
                              """;
        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       private int a;
                                       private int b;
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.AdditionalProjects["Other"].Sources.Add(("/Other/Test1.cs", CarrierDocumentSource));
                         test.FixedState.AdditionalProjects["Other"].Sources.Add(("/Other/Test1.cs", CarrierDocumentSource));

                         // The diagnostic in the carrier document stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                         test.NumberOfIncrementalIterations = 1;
                         test.NumberOfFixAllIterations = 1;
                         test.NumberOfFixAllInDocumentIterations = 1;
                         test.NumberOfFixAllInProjectIterations = 1;
                     });
    }

    /// <summary>
    /// Verifies that a field declaration with a conditional directive around a declarator between its other declarators
    /// is reported on the second declarator the analyzed build compiles, whether the symbol is defined or not, and no
    /// code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task FieldDeclarationWithAConditionalDeclaratorBetweenItsDeclaratorsIsReportedOnTheSecondCompiledDeclarator()
    {
        const string withoutSymbol = """
                                     internal class TestClass
                                     {
                                         private int _a,
                                     #if DEBUG
                                             _debug,
                                     #endif
                                             {|RH7101:_b|};
                                     }
                                     """;

        const string withSymbol = """
                                  internal class TestClass
                                  {
                                      private int _a,
                                  #if DEBUG
                                          {|RH7101:_debug|},
                                  #endif
                                          _b;
                                  }
                                  """;

        await VerifyNoCodeFixIsOffered(withoutSymbol);
        await VerifyNoCodeFixIsOffered(withSymbol, DocumentationMode.Parse, "DEBUG");
    }

    /// <summary>
    /// Verifies the specification example: in a build that does not define <c>DEBUG</c>, the declaration has the
    /// declarators <c>_a</c> and <c>_c</c>, is reported on <c>_c</c>, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task SpecificationExampleConditionalDeclaratorIsReportedOnTheSecondCompiledDeclaratorWithoutFix()
    {
        const string testData = """
                                class C
                                {
                                    private int _a,
                                #if DEBUG
                                                _b,
                                #endif
                                                {|RH7101:_c|};
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a field declaration with an empty conditional directive pair between its declarators is reported,
    /// and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task FieldDeclarationWithAnEmptyConditionalDirectivePairBetweenItsDeclaratorsIsReportedWithoutFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a,
                                #if DEBUG
                                #endif
                                        {|RH7101:_b|};
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a field declaration with a conditional directive in the initializer of a declarator that is not
    /// the last one is reported, and no code fix is offered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task FieldDeclarationWithAConditionalDirectiveInTheInitializerOfANonLastDeclaratorIsReportedWithoutFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a = 1
                                #if DEBUG
                                        + 1
                                #endif
                                        , {|RH7101:_b|};
                                }
                                """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that a field declaration is reported on the second declarator the analyzed build compiles, and no code
    /// fix is offered, for every shape of conditional directive between its declarators
    /// </summary>
    /// <param name="directives">The conditional directives, and the declarators they hold, between <c>_a,</c> and the last declarator</param>
    /// <param name="lastDeclarator">The last declarator, with the markup of the expected diagnostic where it is reported</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("#if FEATURE\n#endif", "{|RH7101:_d|}", DisplayName = "Empty block")]
    [DataRow("#if NEVER\n        _b,\n#endif", "{|RH7101:_d|}", DisplayName = "Excluded declarator")]
    [DataRow("#if !NEVER\n        {|RH7101:_b|},\n#endif", "_d", DisplayName = "Active declarator")]
    [DataRow("#if NEVER\n        _b,\n#else\n        {|RH7101:_c|},\n#endif", "_d", DisplayName = "If and else")]
    [DataRow("#if NEVER\n        _b,\n#elif !NEVER\n        {|RH7101:_c|},\n#endif", "_d", DisplayName = "If and elif")]
    [DataRow("#if false\n        _b,\n#endif", "{|RH7101:_d|}", DisplayName = "If false")]
    [Requirement("RH7101-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task EveryShapeOfConditionalDirectiveBetweenTheDeclaratorsKeepsTheReportAndWithholdsTheFix(string directives, string lastDeclarator)
    {
        var testData = $$"""
                         internal class TestClass
                         {
                             private int _a,
                         {{directives}}
                                 {{lastDeclarator}};
                         }
                         """;

        await VerifyNoCodeFixIsOffered(testData);
    }

    /// <summary>
    /// Verifies that Fix All splits the declarations without a conditional directive between their declarators and
    /// leaves the one with such a directive reported and unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    [Requirement("SHARED-DIAG-008")]
    public async Task FixAllSplitsOnlyTheDeclarationsWithoutAConditionalDirectiveBetweenTheirDeclarators()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a,
                                #if FEATURE
                                #endif
                                        {|RH7101:_b|};
                                }

                                internal class D
                                {
                                    private int _c, {|RH7101:_d|};
                                }

                                internal class E
                                {
                                    private int _e, {|RH7101:_f|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int _a,
                                 #if FEATURE
                                 #endif
                                         {|RH7101:_b|};
                                 }

                                 internal class D
                                 {
                                     private int _c;
                                     private int _d;
                                 }

                                 internal class E
                                 {
                                     private int _e;
                                     private int _f;
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that a field declaration with a conditional directive in the initializer of its last declarator is
    /// still reported, because the directive does not stand between its declarators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    public async Task FieldDeclarationWithAConditionalDirectiveInTheInitializerOfItsLastDeclaratorIsReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a, {|RH7101:_b|} = 1
                                #if DEBUG
                                        + 1
                                #endif
                                        ;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the specification example: a declaration with a directive between its declarators, which the formatter refuses,
    /// withholds the fix of another declaration of the same type as well, and both stay reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    [Requirement("SHARED-FIX-010")]
    public async Task SpecExampleDirectiveInsideAnotherDeclarationOfTheTypeWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _p, {|RH7101:_q|};

                                  private int _a,
                              #pragma warning disable CS0169
                                              {|RH7101:_b|};
                              #pragma warning restore CS0169
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the specification example: in a build that does not define <c>DEBUG</c>, disabled text in front of the
    /// declaration withholds the fix, and the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    public async Task SpecExampleDisabledTextInFrontOfTheDeclarationWithholdsTheFix()
    {
        const string source = """
                              using System.Diagnostics;

                              class C
                              {
                              #if DEBUG
                                  [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                              #endif
                                  private int _a, {|RH7101:_b|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that disabled text in front of the declaration withholds the fix whatever it holds: a modifier, attribute lists
    /// in two branches, a member of its own, the declaration's own branch for another build, or an attribute list in a fully
    /// inactive group
    /// </summary>
    /// <param name="source">The source text with markup for the expected diagnostic</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    [DataRow("class C\n{\n#if NETSTANDARD\n    static\n#endif\n    private int _a, {|RH7101:_b|};\n}", DisplayName = "Disabled modifier")]
    [DataRow("using System;\n\nclass C\n{\n#if DEBUG\n    [Obsolete]\n#elif TRACE\n    [NonSerialized]\n#endif\n    private int _a, {|RH7101:_b|};\n}", DisplayName = "Disabled attribute lists in two branches")]
    [DataRow("class C\n{\n#if DEBUG\n    private int _dbg;\n#endif\n    private int _a, {|RH7101:_b|};\n}", DisplayName = "Disabled member of its own")]
    [DataRow("class C\n{\n#if X\n    private long _a, _b;\n#else\n    private int _a, {|RH7101:_b|};\n#endif\n}", DisplayName = "Declaration in the active else branch")]
    [DataRow("using System;\n\nclass C\n{\n#if false\n    [Obsolete]\n#endif\n    private int _a, {|RH7101:_b|};\n}", DisplayName = "Fully inactive group")]
    public async Task DisabledTextInFrontOfTheDeclarationWithholdsTheFix(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a directive inside the declaration outside its leading trivia withholds the fix: between a modifier and the
    /// type, and inside the value of the last declarator, where the rule still reports the declaration
    /// </summary>
    /// <param name="source">The source text with markup for the expected diagnostic</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    [DataRow("class C\n{\n    private\n#pragma warning disable CS0169\n    int _a, {|RH7101:_b|};\n#pragma warning restore CS0169\n}", DisplayName = "Pragma directive between a modifier and the type")]
    [DataRow("using System;\n\nclass C\n{\n    private Func<int> _a, {|RH7101:_b|} = () =>\n    {\n#if DEBUG\n        return 1;\n#else\n        return 2;\n#endif\n    };\n}", DisplayName = "Conditional group inside the value of the last declarator")]
    public async Task DirectiveInsideTheDeclarationWithholdsTheFix(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a directive inside a declaration of a nested type withholds the fix of a declaration of the outer type, whose
    /// context holds the nested type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    public async Task DirectiveInANestedTypeWithholdsTheFixOfADeclarationOfTheOuterType()
    {
        const string source = """
                              class C
                              {
                                  private int _p, {|RH7101:_q|};

                                  class D
                                  {
                                      private int _a,
                              #pragma warning disable CS0169
                                                  {|RH7101:_b|};
                              #pragma warning restore CS0169
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a directive inside a declaration of the outer type does not withhold the fix of a declaration of a nested
    /// type: the fix formats the declaration in the nested type, which holds no such directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    [Requirement("SHARED-FIX-010")]
    public async Task DirectiveInTheOuterTypeDoesNotWithholdTheFixOfADeclarationOfANestedType()
    {
        const string source = """
                              class C
                              {
                                  private int _a,
                              #pragma warning disable CS0169
                                              {|RH7101:_b|};
                              #pragma warning restore CS0169

                                  class D
                                  {
                                      private int _x, {|RH7101:_y|};
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a,
                                   #pragma warning disable CS0169
                                                   {|RH7101:_b|};
                                   #pragma warning restore CS0169

                                       class D
                                       {
                                           private int _x;
                                           private int _y;
                                       }
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         // The diagnostic whose fix is withheld stays and is marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies that Fix All splits the declarations of a type whose directives stand only in front of its declarations, and
    /// leaves the declarations of a type with a directive between declarators reported without a fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    [Requirement("SHARED-FIX-010")]
    [Requirement("SHARED-FIX-002")]
    public async Task FixAllSplitsTheDeclarationsBehindDirectivesAndLeavesTheTypeWithADirectiveInsideADeclaration()
    {
        const string source = """
                              class A
                              {
                                  private int _a, {|RH7101:_b|};

                                  private int _c,
                              #pragma warning disable CS0169
                                              {|RH7101:_d|};
                              #pragma warning restore CS0169
                              }

                              class B
                              {
                                  #region Fields

                                  private int _e, {|RH7101:_f|};

                                  #endregion // Fields

                              #if !RELEASE
                                  private int _g, {|RH7101:_h|};
                              #endif
                              }
                              """;

        const string fixedSource = """
                                   class A
                                   {
                                       private int _a, {|RH7101:_b|};

                                       private int _c,
                                   #pragma warning disable CS0169
                                                   {|RH7101:_d|};
                                   #pragma warning restore CS0169
                                   }

                                   class B
                                   {
                                       #region Fields

                                       private int _e;
                                       private int _f;

                                       #endregion // Fields

                                   #if !RELEASE
                                       private int _g;
                                       private int _h;
                                   #endif
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         // The diagnostics whose fixes are withheld stay and are marked in the expected source
                         test.FixedState.MarkupHandling = MarkupMode.Allow;
                         test.BatchFixedState.MarkupHandling = MarkupMode.Allow;
                     });
    }

    /// <summary>
    /// Verifies the change example: a comment in front of the arrow of an expression body elsewhere in the type withholds the fix,
    /// while the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentInFrontOfTheArrowOfAnExpressionBodyWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a, {|RH7101:_b|};

                                  int A // c
                                      => _a;
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment or directive in front of the arrow of an expression body withholds the fix wherever the type holds
    /// it: in a method, in an accessor, in a local function in an initializer value of the declaration, and in a nested type
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int M() /* c */ => 1;\n}", DisplayName = "Block comment in front of the arrow of a method on one line")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int P\n    {\n        get\n            // c\n            => _x;\n    }\n}", DisplayName = "Comment line in front of the arrow of a getter")]
    [DataRow("using System;\n\nclass C\n{\n    private Func<int> _f = () =>\n    {\n        int L() /* c */ => 1;\n\n        return L();\n    }, {|RH7101:_g|};\n}", DisplayName = "Local function in an initializer value")]
    [DataRow("#nullable enable\n\nclass C\n{\n    private int _x, {|RH7101:_y|};\n\n    class D\n    {\n        int P\n#pragma warning disable CS0001\n            => 1;\n    }\n}", DisplayName = "Directive in front of the arrow in a nested type")]
    public async Task CommentInFrontOfTheArrowOfAnExpressionBodyElsewhereInTheTypeWithholdsTheFix(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies the change example: a line comment that keeps an argument list of an initializer value on the line below withholds
    /// the fix, and the diagnostic stays
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentInFrontOfAnArgumentListInAnInitializerValueWithholdsTheFix(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _a = Offset // one
                                                   (1), {|RH7101:_b|};

                                  private static int Offset(int value)
                                  {
                                      return value;
                                  }
                              }
                              """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    /// <summary>
    /// Verifies that a comment in front of an argument list on one line, in front of an element access, or between the <c>?</c> and
    /// the <c>[</c> of a conditional element access anywhere in the type withholds the fix for its combined declaration
    /// </summary>
    /// <param name="source">The source text with markup</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("class C\n{\n    private int _a = Offset /* one */ (1), {|RH7101:_b|};\n\n    private static int Offset(int value)\n    {\n        return value;\n    }\n}", DisplayName = "Block comment in front of an argument list in an initializer value")]
    [DataRow("class C\n{\n    private int _x, {|RH7101:_y|};\n\n    int M(int[] arr)\n    {\n        return arr // c\n            [0];\n    }\n}", DisplayName = "Comment in front of an element access in a method of the type")]
    [DataRow("#nullable enable\n\nclass C\n{\n    private int _x, {|RH7101:_y|};\n\n    class D\n    {\n        int? M(int[]? n)\n        {\n            return n? /* c */ [0];\n        }\n    }\n}", DisplayName = "Comment between the question mark and the bracket in a nested type")]
    public async Task CommentInFrontOfAnAttachedPartWithholdsTheFix(string source)
    {
        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a comment in a gap the line-break phase joins, a comment inside a list of omitted type arguments, and,
    /// as the change doc's examples show, a comment or a directive behind an assignment operator, behind the <c>=</c> of a
    /// property, a parameter's default value, a variable declarator, or an enum member, behind the <c>=&gt;</c> of a switch arm,
    /// or behind the <c>=</c> of an attribute argument, anywhere in the type the fix formats, withhold the fix, because the
    /// formatter refuses the type, and the diagnostic stays
    /// </summary>
    /// <param name="member">The member that holds the refused shape</param>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [Requirement("SHARED-FIX-010")]
    [DataRow("    object M()\n    {\n        return new /* c */ object();\n    }", "\n", DisplayName = "Gap behind new, LF")]
    [DataRow("    object M()\n    {\n        return new /* c */ object();\n    }", "\r\n", DisplayName = "Gap behind new, CRLF")]
    [DataRow("    System.Type M()\n    {\n        return typeof(System.Collections.Generic.Dictionary</* c */,>);\n    }", "\n", DisplayName = "Omitted type arguments, LF")]
    [DataRow("    object M(int x)\n    {\n        return new { A = x += /* c */ 3 };\n    }", "\n", DisplayName = "Assignment in an anonymous-object member, LF")]
    [DataRow("    int[] G { get; } = // values\n        { 1, 2 };", "\n", DisplayName = "Property initializer, LF")]
    [DataRow("    int[] G { get; } = // values\n        { 1, 2 };", "\r\n", DisplayName = "Property initializer, CRLF")]
    [DataRow("    int M(int y = /* default */ 1)\n    {\n        return y + _a + _b;\n    }", "\n", DisplayName = "Parameter default, LF")]
    [DataRow("    int M(int x)\n    {\n        x += // add\n            3;\n\n        return x;\n    }", "\n", DisplayName = "Compound assignment, LF")]
    [DataRow("    int M()\n    {\n        int x =\n#if DEBUG\n            1;\n#else\n            2;\n#endif\n\n        return x;\n    }", "\n", DisplayName = "Directive behind a local declarator, LF")]
    [DataRow("    enum E\n    {\n        A = // one\n            1\n    }", "\n", DisplayName = "Enum member in a nested type, LF")]
    [DataRow("    int M(int v)\n    {\n        return v switch\n               {\n                   1 => // one\n                       2,\n                   _ => 0\n               };\n    }", "\n", DisplayName = "Switch arm, LF")]
    [DataRow("    [System.Obsolete(DiagnosticId = // name\n                     \"X\")]\n    int M()\n    {\n        return _a + _b;\n    }", "\n", DisplayName = "Attribute argument, LF")]
    [DataRow("    [System.Obsolete(DiagnosticId = // name\n                     \"X\")]\n    int M()\n    {\n        return _a + _b;\n    }", "\r\n", DisplayName = "Attribute argument, CRLF")]
    public async Task VerifyNoFixIsOfferedForAShapeTheFormatterRefusesInTheType(string member, string endOfLine)
    {
        var source = $"class C\n{{\n    private int _a, {{|RH7101:_b|}};\n\n{member}\n}}";

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(source, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Applies the fix of the declaration whose declarator has the given name, under LF and under CRLF, and asserts the
    /// document's text after the fix, with the document's line ending kept
    /// </summary>
    /// <param name="source">Source text without markup, with LF line endings</param>
    /// <param name="fixedSource">Expected text after the fix, with LF line endings</param>
    /// <param name="reportedDeclaratorName">Name of the declarator the diagnostic is reported on</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task AssertSingleFixUnderBothLineEndings(string source, string fixedSource, string reportedDeclaratorName)
    {
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, reportedDeclaratorName, DocumentationMode.Parse);
    }

    /// <summary>
    /// Applies the fix of the declaration whose declarator has the given name, under LF and under CRLF, with the syntax
    /// tree parsed in the given documentation mode, and asserts the document's text after the fix, with the document's
    /// line ending kept
    /// </summary>
    /// <param name="source">Source text without markup, with LF line endings</param>
    /// <param name="fixedSource">Expected text after the fix, with LF line endings</param>
    /// <param name="reportedDeclaratorName">Name of the declarator the diagnostic is reported on</param>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private static async Task AssertSingleFixUnderBothLineEndings(string source, string fixedSource, string reportedDeclaratorName, DocumentationMode documentationMode)
    {
        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        Assert.AreEqual(fixedSource, await ApplyFixOfDeclarationAsync(plainSource, reportedDeclaratorName, documentationMode), "Unexpected fix result under LF line endings.");
        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedSource), await ApplyFixOfDeclarationAsync(NormalizeToCarriageReturnLineFeed(plainSource), reportedDeclaratorName, documentationMode), "Unexpected fix result under CRLF line endings.");
    }

    /// <summary>
    /// Applies the one fix offered for the declaration whose declarator has the given name
    /// </summary>
    /// <param name="source">Source text without markup</param>
    /// <param name="reportedDeclaratorName">Name of the declarator the diagnostic is reported on</param>
    /// <param name="documentationMode">Documentation mode of the syntax tree</param>
    /// <returns>The document's text after the fix</returns>
    private static async Task<string> ApplyFixOfDeclarationAsync(string source, string reportedDeclaratorName, DocumentationMode documentationMode)
    {
        var actions = await GetCodeFixActionsAsync(source,
                                                   RH7101DoNotCombineFieldsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == reportedDeclaratorName)
                                                               .Identifier
                                                               .GetLocation(),
                                                   documentationMode);

        Assert.HasCount(1, actions, "Exactly one fix must be offered for the declaration.");

        var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
        var changedDocument = operations.OfType<ApplyChangesOperation>()
                                        .Single()
                                        .ChangedSolution
                                        .Projects
                                        .Single()
                                        .Documents
                                        .Single();

        return (await changedDocument.GetTextAsync(CancellationToken.None)).ToString();
    }

    /// <summary>
    /// Asserts that formatting the source and formatting the fix result both give the formatted source, and that the fix
    /// result differs from it only by the blank lines the formatter writes, which the fix does not write in the gaps
    /// around the declaration
    /// </summary>
    /// <param name="source">Source text with markup, with LF line endings</param>
    /// <param name="fixedSource">Expected text after the fix, with LF line endings</param>
    /// <param name="formattedSource">Expected text after formatting, with LF line endings</param>
    private static void AssertFormattingAddsOnlyBlankLines(string source, string fixedSource, string formattedSource)
    {
        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        Assert.AreEqual(formattedSource, Format(plainSource), "Formatting the source must give the formatted source.");
        Assert.AreEqual(formattedSource, Format(fixedSource), "Formatting the fix result must give the formatted source.");
        Assert.AreEqual(fixedSource, string.Join("\n", formattedSource.Split('\n').Where(line => line.Trim().Length > 0)), "The fix result must differ from the formatted source only by blank lines.");
    }

    #endregion // Methods

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstFieldA, {|#0:secondFieldA|};
                                    private int firstFieldB, {|#1:secondFieldB|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstFieldA;
                                     private int secondFieldA;
                                     private int firstFieldB;
                                     private int secondFieldB;
                                 }
                                 """;

        // Two combined field declarations sit on adjacent lines with no blank line between them; the Fix All provider
        // applies the fix of one declaration after the other, and each fix only splits its own declaration, so Fix All
        // converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}