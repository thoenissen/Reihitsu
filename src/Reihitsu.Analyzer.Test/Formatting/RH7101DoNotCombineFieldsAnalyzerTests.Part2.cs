using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7101DoNotCombineFieldsAnalyzer"/> and <see cref="RH7101DoNotCombineFieldsCodeFixProvider"/>
/// </summary>
public partial class RH7101DoNotCombineFieldsAnalyzerTests
{
    #region Tests

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
    /// Verifies that no code fix is offered for a combined declaration in a block-scoped namespace, where the fix would
    /// change nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-005")]
    public async Task NoCodeFixForACombinedDeclarationInABlockScopedNamespace()
    {
        const string source = """
                              namespace N
                              {
                                  int a, b;
                              }
                              """;

        var actions = await GetCodeFixActionsAsync(source, RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, GetSecondDeclaratorLocation);

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that no code fix is offered for a combined declaration in a file-scoped namespace, where the fix would
    /// change nothing
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-005")]
    public async Task NoCodeFixForACombinedDeclarationInAFileScopedNamespace()
    {
        const string source = """
                              namespace N;

                              int a, b;
                              """;

        var actions = await GetCodeFixActionsAsync(source, RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, GetSecondDeclaratorLocation);

        Assert.IsEmpty(actions);
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
    /// Verifies that in a CRLF document the fix writes the LF behind the split declaration as CRLF behind its last field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-PIPE-010")]
    [Requirement("SHARED-FIX-008")]
    public async Task FixWritesTheDetectedLineEndingBehindTheLastFieldInAMixedDocument()
    {
        const string testData = "internal class TestClass\r\n{\r\n    private int _a, _b;\n    private int _c;\r\n}\r\n";
        const string resultData = "internal class TestClass\r\n{\r\n    private int _a;\r\n    private int _b;\r\n    private int _c;\r\n}\r\n";

        Assert.AreEqual(resultData, await ApplyCodeFixAsync(testData));
    }

    /// <summary>
    /// Verifies that the fix removes the whitespace behind the split declaration, under LF and CRLF
    /// </summary>
    /// <param name="endOfLine">The line ending of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [Requirement("FMT-PIPE-010")]
    public async Task FixRemovesWhitespaceBehindTheLastField(string endOfLine)
    {
        const string testData = "internal class TestClass\n{\n    private int _a, {|RH7101:_b|};   \n    private int _c;\n}\n";
        const string resultData = "internal class TestClass\n{\n    private int _a;\n    private int _b;\n    private int _c;\n}\n";

        await VerifyCodeFixWithEndOfLine(testData, resultData, endOfLine);
    }

    /// <summary>
    /// Verifies that a combined static field declaration in a union declaration is reported and split by the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task VerifyCombinedStaticFieldInAUnionIsSplit()
    {
        const string testData = """
                                public union U7(int, string)
                                {
                                    public static int A = 1, {|#0:B|} = 2;
                                }
                                """;

        const string fixedData = """
                                 public union U7(int, string)
                                 {
                                     public static int A = 1;
                                     public static int B = 2;
                                 }
                                 """;

        // The .NET 9 reference assemblies lack the union runtime types (CS0518, CS0656), so compiler diagnostics are not verified
        await Verify(testData, fixedData, test => test.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix splits a combined static field declaration in a union declaration under CRLF line endings, and that
    /// the result keeps them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task VerifyCombinedStaticFieldInAUnionIsSplitWithCarriageReturnLineFeed()
    {
        const string testData = """
                                public union U7(int, string)
                                {
                                    public static int A = 1, {|RH7101:B|} = 2;
                                }
                                """;

        const string fixedData = """
                                 public union U7(int, string)
                                 {
                                     public static int A = 1;
                                     public static int B = 2;
                                 }
                                 """;

        var source = ConvertEndOfLine(testData, "\r\n");
        var expected = ConvertEndOfLine(fixedData, "\r\n");

        // The .NET 9 reference assemblies lack the union runtime types (CS0518, CS0656), so compiler diagnostics are not verified
        await Verify(source, expected, test => test.CompilerDiagnostics = CompilerDiagnostics.None);

        Assert.AreEqual(expected, await ApplyCodeFixAsync(RemoveMarkup(source)));
    }

    /// <summary>
    /// Verifies that the fix splits a combined static and a combined constant declaration in one union declaration, each through
    /// its own diagnostic, and that Fix All splits both
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task VerifyCombinedFieldsInAUnionAreSplitTogether()
    {
        const string testData = """
                                public union U7(int, string)
                                {
                                    public static int A = 1, {|#0:B|} = 2;
                                    public const int C = 3, {|#1:D|} = 4;
                                }
                                """;

        const string fixedData = """
                                 public union U7(int, string)
                                 {
                                     public static int A = 1;
                                     public static int B = 2;
                                     public const int C = 3;
                                     public const int D = 4;
                                 }
                                 """;

        // The .NET 9 reference assemblies lack the union runtime types (CS0518, CS0656), so compiler diagnostics are not verified
        await Verify(testData, fixedData, test => test.CompilerDiagnostics = CompilerDiagnostics.None, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat, 2));
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
    /// Verifies the specification example through the code fix and Fix All: a line break behind the type and a line break
    /// between two modifiers are repeated in every field the fix creates, and a line break behind a <c>,</c> stands in no
    /// field, under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    [Requirement("RH7101-004")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixRepeatsLineBreaksInsideAndBehindTheHeadInEveryField(string endOfLine)
    {
        const string source = """
                              using System;
                              using System.Collections.Generic;

                              class C
                              {
                                  private Dictionary<string, List<int>>
                                      _map1, {|RH7101:_map2|} = new();

                                  private static
                                      readonly int _g = 1, {|RH7101:_h|};

                                  [NonSerialized]
                                  private int _x,
                                      {|RH7101:_y|};
                              }
                              """;
        const string fixedSource = """
                                   using System;
                                   using System.Collections.Generic;

                                   class C
                                   {
                                       private Dictionary<string, List<int>>
                                       _map1;
                                       private Dictionary<string, List<int>>
                                       _map2 = new();

                                       private static
                                       readonly int _g = 1;
                                       private static
                                       readonly int _h;

                                       [NonSerialized]
                                       private int _x;
                                       [NonSerialized]
                                       private int _y;
                                   }
                                   """;

        await Verify(ConvertEndOfLine(source, endOfLine), ConvertEndOfLine(fixedSource, endOfLine));
    }

    /// <summary>
    /// Verifies that the fix repeats a line break behind the type in every field it creates, with the first declarator on a
    /// deeper column, and writes every line break as the document's line ending
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    [Requirement("RH7101-004")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixRepeatsALineBreakBehindTheTypeInEveryField(string endOfLine)
    {
        const string source = """
                              internal class TestClass
                              {
                                  private int
                                      _g, {|RH7101:_h|};
                              }
                              """;
        const string fixedSource = """
                                   internal class TestClass
                                   {
                                       private int
                                       _g;
                                       private int
                                       _h;
                                   }
                                   """;

        await VerifyCodeFixWithEndOfLine(source, fixedSource, endOfLine);
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

    #endregion // Tests
}