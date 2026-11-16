using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank line above a comment that is the first content behind an opening token: none behind
/// an opening brace, parenthesis, bracket, or generic <c>&lt;</c>, one behind a comma or an operator (FMT-BLANK-011)
/// </summary>
[TestClass]
public class FirstCommentBehindOpeningTokenTests : FormatterTestsBase
{
    #region Fields

    /// <summary>
    /// The parse options of C# 15, the version that introduces union declarations
    /// </summary>
    private static readonly CSharpParseOptions _cSharp15 = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp15);

    /// <summary>
    /// The default parse options without documentation parsing, under which <c>///</c> is an ordinary comment
    /// </summary>
    private static readonly CSharpParseOptions _documentationModeNone = CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None);

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Verifies the specification example: a comment that is the first content behind the opening parenthesis of a
    /// record's parameter list, a union's case-type list, a method's parameter list, and an argument list, behind the
    /// opening bracket of a collection expression and a list pattern, and behind the <c>&lt;</c> of a type parameter list
    /// gets no blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void SpecExampleFirstCommentBehindOpeningParenthesisBracketOrLessThanGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             public record R(
                                 // first
                                 int A,
                                 int B);

                             public union U(
                                 // first
                                 int,
                                 string);

                             class C
                             {
                                 int M(
                                     // first
                                     int a,
                                     int b)
                                 {
                                     return M(
                                         // first
                                         a,
                                         b);
                                 }

                                 int[] F = [
                                     // first
                                     1,
                                     2
                                 ];

                                 bool N(int[] a)
                                 {
                                     return a is [
                                         // first
                                         1,
                                         ..
                                     ];
                                 }

                                 void O<
                                     // first
                                     T>()
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public record R(
                                                // first
                                                int A,
                                                int B);

                                public union U(
                                               // first
                                               int,
                                               string);

                                class C
                                {
                                    int M(
                                          // first
                                          int a,
                                          int b)
                                    {
                                        return M(
                                                 // first
                                                 a,
                                                 b);
                                    }

                                    int[] F = [
                                                  // first
                                                  1,
                                                  2
                                              ];

                                    bool N(int[] a)
                                    {
                                        return a is [
                                                        // first
                                                        1,
                                                        ..
                                                    ];
                                    }

                                    void O<
                                    // first
                                    T>()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, _cSharp15);
    }

    /// <summary>
    /// Verifies the specification example: a documentation comment that starts its line as the first content behind an
    /// opening parenthesis gets no blank line, single-line or delimited
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void SpecExampleFirstDocumentationCommentBehindOpeningParenthesisGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             public record R(
                                 /// <summary>first</summary>
                                 int A,
                                 int B);

                             class C
                             {
                                 void M(
                                     /** first */ int a, int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                public record R(
                                                /// <summary>
                                                /// first
                                                /// </summary>
                                                int A,
                                                int B);

                                class C
                                {
                                    void M(
                                           /** first */ int a,
                                           int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the first comment behind the opening brace of an anonymous object gets no
    /// blank line, as behind any other opening brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void SpecExampleFirstCommentBehindOpeningBraceOfAnonymousObjectGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 object M()
                                 {
                                     return new
                                     {
                                         // first
                                         A = 1,
                                     };
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    object M()
                                    {
                                        return new
                                               {
                                                   // first
                                                   A = 1,
                                               };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the first comment behind the opening token of an attribute argument list, an indexer's parameter
    /// list, a type parameter list, a tuple, an array size, and an element access gets no blank line, and that nothing
    /// else of the layout changes
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void FirstCommentBehindOtherOpeningTokensGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 [Obsolete(
                                     // first
                                     "x")]
                                 int this[
                                     // first
                                     int i] => i;

                                 void M<
                                     // first
                                     T>()
                                 {
                                     var t = (
                                         // first
                                         1,
                                         2);
                                     var a = new int[
                                         // first
                                         3];
                                     _ = a[
                                         // first
                                         0];
                                     _ = t;
                                 }
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    [Obsolete(
                                              // first
                                              "x")]
                                    int this[
                                             // first
                                             int i] => i;

                                    void M<
                                    // first
                                    T>()
                                    {
                                        var t = (
                                                 // first
                                                 1,
                                                 2);
                                        var a = new int[
                                        // first
                                        3];
                                        _ = a[
                                              // first
                                              0];
                                        _ = t;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written behind the opening parenthesis is moved onto a line of its
    /// own as before and gets no blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void DelimitedDocumentationCommentWrittenBehindOpeningParenthesisIsMovedWithoutBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(/** first */ int a, int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(
                                           /** first */ int a,
                                           int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies both sides of the boundary in one list: the first comment behind the opening parenthesis gets no blank
    /// line, a comment behind a comma of the same list gets one, for line and block comments
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentBehindCommaGetsBlankLineWhileFirstCommentBehindOpeningParenthesisGetsNone()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(
                                     // first
                                     int a,
                                     // second
                                     int b,
                                     /* third */
                                     int c)
                                 {
                                 }

                                 void N(
                                     /* first */
                                     int a,
                                     int b)
                                 {
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(
                                           // first
                                           int a,

                                           // second
                                           int b,

                                           /* third */
                                           int c)
                                    {
                                    }

                                    void N(
                                           /* first */
                                           int a,
                                           int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment behind a comma of a parameter list and of a collection expression gets a blank line when no
    /// comment stands behind the opening token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentBehindCommaInParenthesizedOrBracketedListGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a,
                                        // second
                                        int b)
                                 {
                                 }

                                 int[] F = [
                                     1,
                                     // second
                                     2
                                 ];
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a,

                                           // second
                                           int b)
                                    {
                                    }

                                    int[] F = [
                                                  1,

                                                  // second
                                                  2
                                              ];
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line the author wrote behind an opening parenthesis, bracket, or <c>&lt;</c> is kept, while
    /// the one behind an opening brace is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    [Requirement("FMT-BLANK-007")]
    public void AuthorsBlankLineBehindOpeningParenthesisBracketOrLessThanIsKept()
    {
        // Arrange
        const string input = """
                             public record R(

                                 // first
                                 int A,
                                 int B);

                             class C
                             {
                                 int[] F = [

                                     // first
                                     1,
                                     2
                                 ];

                                 void O<

                                     // first
                                     T>()
                                 {

                                     // first
                                     O<T>();
                                 }
                             }
                             """;
        const string expected = """
                                public record R(

                                                // first
                                                int A,
                                                int B);

                                class C
                                {
                                    int[] F = [

                                                  // first
                                                  1,
                                                  2
                                              ];

                                    void O<

                                    // first
                                    T>()
                                    {
                                        // first
                                        O<T>();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the first comment behind the opening parenthesis of a statement header, a cast, a parenthesized
    /// expression, a positional and a parenthesized pattern, and a lambda's parameter list, behind the opening bracket of
    /// an attribute list and of a nested collection expression, and behind the <c>&lt;</c> of a type argument list and a
    /// function pointer parameter list gets no blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void FirstCommentBehindEveryOtherOpeningTokenGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 [
                                     // attribute
                                     Obsolete]
                                 List<
                                     // return type
                                     int> M(object o, int a, (int, int) p)
                                 {
                                     if (
                                         // header
                                         a > 0)
                                     {
                                         a = (
                                             // cast
                                             int)o;
                                     }

                                     a = (
                                         // parenthesized
                                         a + 1) * 2;
                                     _ = p is (
                                         // positional
                                         1, 2);
                                     _ = a is (
                                         // pattern
                                         > 0);
                                     Func<int, int> f = (
                                         // lambda
                                         int x) => x;
                                     _ = M<
                                         // invocation
                                         int>(o);
                                     int[][] n = [
                                         [
                                             // nested
                                             1
                                         ]
                                     ];
                                     _ = f;
                                     _ = n;

                                     return null;
                                 }

                                 T M<T>(object o) => default;

                                 unsafe void P(delegate*<
                                     // function pointer
                                     int, void> f)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertBlankLinesAbove(input,
                              ("// attribute", false),
                              ("// return type", false),
                              ("// header", false),
                              ("// cast", false),
                              ("// parenthesized", false),
                              ("// positional", false),
                              ("// pattern", false),
                              ("// lambda", false),
                              ("// invocation", false),
                              ("// nested", false),
                              ("// function pointer", false));
    }

    /// <summary>
    /// Verifies that a comment behind a <c>&lt;</c> that is an operator, binary or relational pattern, gets a blank line,
    /// since such a <c>&lt;</c> opens nothing
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentBehindLessThanOperatorGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool N(int a, int b)
                                 {
                                     return a <
                                         // compared
                                         b;
                                 }

                                 bool P(int a)
                                 {
                                     return a is <
                                         // bound
                                         5;
                                 }
                             }
                             """;

        // Act & Assert
        AssertBlankLinesAbove(input,
                              ("// compared", true),
                              ("// bound", true));
    }

    /// <summary>
    /// Verifies that a comment in an empty parameter list or collection expression, which the closing token owns, gets no
    /// blank line behind the opening token
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentInEmptyListBehindOpeningTokenGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(
                                     // no parameters yet
                                     )
                                 {
                                     int[] a = [
                                         // no elements yet
                                     ];
                                     _ = a;
                                 }
                             }
                             """;

        // Act & Assert
        AssertBlankLinesAbove(input,
                              ("// no parameters yet", false),
                              ("// no elements yet", false));
    }

    /// <summary>
    /// Verifies that the first comment on its own line below an opening parenthesis that a trailing comment ends gets no
    /// blank line, since the token in front of it is still the opening parenthesis
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void FirstCommentBelowTrailingCommentOfOpeningParenthesisGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     M( // note
                                         // first
                                         a);
                                 }
                             }
                             """;

        // Act & Assert
        AssertBlankLinesAbove(input, ("// first", false));
    }

    /// <summary>
    /// Verifies that a comment directly below a directive that follows an opening bracket gets no blank line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void CommentBelowDirectiveBehindOpeningBracketGetsNoBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int[] M()
                                 {
                                     return [
                             #pragma warning disable CS0168
                                         // first
                                         1,
                                         2
                                     ];
                                 }
                             }
                             """;

        // Act & Assert
        AssertBlankLinesAbove(input, ("// first", false));
    }

    /// <summary>
    /// Verifies that under documentation mode <c>None</c>, where <c>///</c> is an ordinary comment, the first one behind an
    /// opening parenthesis gets no blank line and one behind a comma gets one
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public void TripleSlashCommentWithoutDocumentationParsingFollowsTheOpeningTokenRule()
    {
        // Arrange
        const string input = """
                             public record R(
                                 /// first
                                 int A,
                                 /// second
                                 int B);
                             """;
        const string expected = """
                                public record R(
                                                /// first
                                                int A,

                                                /// second
                                                int B);
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, _documentationModeNone);
    }

    #endregion // Methods
}