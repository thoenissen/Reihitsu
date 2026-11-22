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
/// Test methods for <see cref="RH7101DoNotCombineFieldsAnalyzer"/> and <see cref="RH7101DoNotCombineFieldsCodeFixProvider"/>: what is reported, and the fields and the layout the fix writes
/// </summary>
[TestClass]
public class RH7101DoNotCombineFieldsAnalyzerTests : BatchCodeFixTestsBase<RH7101DoNotCombineFieldsAnalyzer, RH7101DoNotCombineFieldsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that single field declarations do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForSingleFieldDeclarations()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField;
                                    private int secondField;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that combined field declarations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    [Requirement("RH7101-003")]
    [Requirement("RH7101-004")]
    public async Task VerifyCombinedFieldDeclarationsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix splits only the flagged field and leaves unrelated members in the type untouched,
    /// so the fix diff does not inherit unrelated whole-type reformatting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixDoesNotReformatUnrelatedMembers()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField, {|#0:secondField|};

                                    void Unrelated()
                                    {
                                System.Console.WriteLine();
                                    }
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;
                                     private int secondField;

                                     void Unrelated()
                                     {
                                 System.Console.WriteLine();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix indents every generated field at the member level when the declaration is documented
    /// in the documentation mode None, where a documentation comment of the declaration does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentedCombinedFieldsAreFixedAtMemberIndentation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Two fields.
                                    /// </summary>
                                    private int firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     /// <summary>
                                     /// Two fields.
                                     /// </summary>
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject), Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps the documentation comment above the declaration where the author wrote it, and
    /// places the generated fields at the member column rather than at the column of the documentation comment
    /// in the documentation mode None, where a documentation comment of the declaration does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMisalignedDocumentationCommentFixUsesFieldLineIndentation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                  /// <summary>
                                  /// Two fields.
                                  /// </summary>
                                    private int firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                   /// <summary>
                                   /// Two fields.
                                   /// </summary>
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject), Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps a documentation comment that starts on the opening brace line where the author
    /// wrote it, and places the generated fields at the member column rather than at column zero
    /// in the documentation mode None, where a documentation comment of the declaration does not withhold the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentOnOpeningBraceLineFixUsesFieldLineIndentation()
    {
        const string testData = """
                                internal class TestClass
                                { /// <summary>
                                  /// Two fields.
                                  /// </summary>
                                    private int firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 { /// <summary>
                                   /// Two fields.
                                   /// </summary>
                                     private int firstField;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, test => test.SolutionTransforms.Add(ApplyDocumentationModeNoneToTestProject), Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out a first declarator on a later line than the type, with no comment in front of
    /// it, and the later field's declarator at the column the formatter gives them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task VerifyFixLaysOutAFirstDeclaratorWithoutACommentAsTheFormatterLaysItOut()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int
                                    firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int
                                         firstField;
                                     private int
                                         secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is offered for a combined field inside an active conditional group and splits it: the
    /// <c>#if</c> directive in the declaration's leading trivia stays in front of the first field, and the group holds both
    /// fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    [Requirement("RH7101-004")]
    public async Task VerifyFixSplitsAFieldInsideAnActiveConditionalGroup()
    {
        const string testData = """
                                internal class TestClass
                                {
                                #if DEBUG
                                    private int firstField, {|#0:secondField|};
                                #endif
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                 #if DEBUG
                                     private int firstField;
                                     private int secondField;
                                 #endif
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     test => test.SolutionTransforms.Add(DefineDebug),
                     Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: combined field declarations of an interface, a struct, a record, and a
    /// fixed-size buffer are reported, constants and static fields included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    [Requirement("RH7101-003")]
    public async Task CombinedFieldDeclarationsOfEveryTypeKindAreReported()
    {
        const string source = """
                              interface I
                              {
                                  static int A, {|RH7101:B|};
                              }

                              struct S
                              {
                                  public int X, {|RH7101:Y|};
                              }

                              record R
                              {
                                  public const int A = 1, {|RH7101:B|} = 2;
                              }

                              unsafe struct U
                              {
                                  public fixed int A[2], {|RH7101:B|}[3];
                              }
                              """;

        await Verify(source, test => test.SolutionTransforms.Add(ApplyAllowUnsafeToTestProject));
    }

    /// <summary>
    /// Verifies the specification example: event field declarations and local declarations with several declarators
    /// are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-002")]
    public async Task EventFieldsAndLocalsWithSeveralDeclaratorsAreNotReported()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  public event Action E1, E2;

                                  void M()
                                  {
                                      int a, b;
                                  }
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: a declaration with three declarators is reported once, on its second
    /// declarator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-003")]
    public async Task DeclarationIsReportedOnceOnItsSecondDeclarator()
    {
        const string source = """
                              class C
                              {
                                  private int _a, {|RH7101:_b|} = 1, _c;
                              }
                              """;

        await Verify(source);
    }

    /// <summary>
    /// Verifies the specification example: the fix gives each generated field the attributes, modifiers, and type of
    /// the combined declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixRepeatsAttributesModifiersAndTypeForEachField()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  [NonSerialized]
                                  private static readonly string _a = "a", {|RH7101:_b|} = "b";
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       [NonSerialized]
                                       private static readonly string _a = "a";
                                       [NonSerialized]
                                       private static readonly string _b = "b";
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the fix spaces the split declaration as the formatter spaces it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixSpacesTheSplitDeclarationAsTheFormatterSpacesIt()
    {
        const string source = """
                              class C
                              {
                                  private  int[]  _a = { 1,2 } ,  {|RH7101:_b|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int[] _a = { 1, 2 };
                                       private int[] _b;
                                   }
                                   """;

        await Verify(source, fixedSource);

        AssertFormattingChangesNothing(fixedSource);
    }

    /// <summary>
    /// Verifies that the fix gives each generated field one attribute per list, each list on a line of its own, as the
    /// formatter does
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixSplitsTheAttributeListsOfTheSplitDeclarationAsTheFormatterSplitsThem()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  [NonSerialized, Obsolete] private static readonly string _a = "a", {|RH7101:_b|} = "b";
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       [NonSerialized]
                                       [Obsolete]
                                       private static readonly string _a = "a";
                                       [NonSerialized]
                                       [Obsolete]
                                       private static readonly string _b = "b";
                                   }
                                   """;

        await Verify(source, fixedSource);

        AssertFormattingChangesNothing(fixedSource);
    }

    /// <summary>
    /// Verifies that the fix of a declaration that starts with an attribute list on the declaration's line, which the
    /// formatter does not split, puts the field behind the list on a line of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixSplitsADeclarationThatStartsWithAnAttributeListOnItsLine()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  [NonSerialized] private int _x, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       [NonSerialized]
                                       private int _x;
                                       [NonSerialized]
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the fix splits a declaration without modifiers, whose first token is its type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixSplitsADeclarationThatStartsWithItsType()
    {
        const string source = """
                              class C
                              {
                                  int _x, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       int _x;
                                       int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the fix formats the whole of each field it creates, initializers included: the structural
    /// rewrites and the layout of every formatter phase reach them, and each fix splits only its own declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixFormatsTheInitializersOfTheFieldsItCreates()
    {
        const string source = """
                              using System;

                              class C
                              {
                                  private static bool _f;
                                  private Action _a = () => { if (_f) return; }, {|RH7101:_b|};
                                  private int[] _c = { 1, 2, }, {|RH7101:_d|};
                              }
                              """;

        const string fixedSource = """
                                   using System;

                                   class C
                                   {
                                       private static bool _f;
                                       private Action _a = () =>
                                                           {
                                                               if (_f)
                                                               {
                                                                   return;
                                                               }
                                                           };
                                       private Action _b;
                                       private int[] _c = { 1, 2 };
                                       private int[] _d;
                                   }
                                   """;

        await Verify(source, fixedSource);

        AssertFormattingChangesNothing(fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: the fix of a declaration between two members on its line, with a comment in
    /// the gap in front of it, gives every field and the member behind a line of their own, and the comment stays
    /// behind the member in front
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixGivesTheFieldsAndTheMembersOnTheirLineALineOfTheirOwn()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* c */ private int _x, {|RH7101:_y|}; private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p; /* c */
                                       private int _x;
                                       private int _y;
                                       private int _q;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the first field starts the next line when another member stands in front of the declaration on its
    /// line, with one line break and no blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixStartsTheFirstFieldOnTheNextLineBehindAMemberInFront()
    {
        const string source = """
                              class C
                              {
                                  private int _p; private int _x, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p;
                                       private int _x;
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a member behind the declaration on its line starts the next line, with one line break and no
    /// blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixStartsTheMemberBehindTheDeclarationOnTheNextLine()
    {
        const string source = """
                              class C
                              {
                                  private int _x, {|RH7101:_y|}; private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x;
                                       private int _y;
                                       private int _q;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a method in front of the declaration on its line keeps its own layout, and the first field starts
    /// the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixStartsTheFirstFieldOnTheNextLineBehindAMethodInFront()
    {
        const string source = """
                              class C
                              {
                                  void M() { } private int _x, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       void M() { }
                                       private int _x;
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a documentation comment in the gap in front of the declaration, behind the member in front on its line, which is in the declaration's leading trivia and documents it, withholds the fix in the
    /// documentation modes Parse and Diagnose, and that the diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-006")]
    public async Task DocumentationCommentInTheGapInFrontWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /** X. */ private int _x, {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
    }

    /// <summary>
    /// Verifies that a documentation comment in the gap behind the declaration goes with the member it documents onto
    /// the next line, with no blank line above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixMovesADocumentationCommentInTheGapBehindWithTheMemberItDocuments()
    {
        const string source = """
                              class C
                              {
                                  private int _a, {|RH7101:_b|}; /** Q. */ private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a;
                                       private int _b;
                                       /** Q. */ private int _q;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a declaration behind the opening brace of its type keeps its line, while the later field stands at
    /// the type's member column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixKeepsTheLineOfADeclarationBehindTheOpeningBraceOfItsType()
    {
        const string source = """
                              class C { private int _x, {|RH7101:_y|}; }
                              """;

        const string fixedSource = """
                                   class C { private int _x;
                                       private int _y; }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: a declaration off its member column keeps the column of its first line, and
    /// the later field stands at the type's member column, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixPlacesTheLaterFieldOfADeclarationOffItsMemberColumnAtTheMemberColumn()
    {
        const string source = """
                              class C
                              {
                                    private int _x, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                         private int _x;
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
    }

    /// <summary>
    /// Verifies that a tab in front of the declaration stays in front of the first field, and the later field stands at
    /// the type's member column, indented with spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixKeepsTheTabInFrontOfTheFirstFieldAndIndentsTheLaterFieldWithSpaces()
    {
        var source = ExpandWhitespaceMarkers("""
                                             class C
                                             {
                                             →private int _x, {|RH7101:_y|};
                                             }
                                             """);

        var fixedSource = ExpandWhitespaceMarkers("""
                                                  class C
                                                  {
                                                  →private int _x;
                                                  ····private int _y;
                                                  }
                                                  """);

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
    }

    /// <summary>
    /// Verifies that a tab-indented declaration in a tab-indented type gets the later field at the context's depth,
    /// with the context's tab counted as one column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixCountsTheTabOfATabIndentedTypeAsOneColumn()
    {
        var source = ExpandWhitespaceMarkers("""
                                             namespace N
                                             {
                                             →class C
                                             →{
                                             →→private int _x, {|RH7101:_y|};
                                             →}
                                             }
                                             """);

        var fixedSource = ExpandWhitespaceMarkers("""
                                                  namespace N
                                                  {
                                                  →class C
                                                  →{
                                                  →→private int _x;
                                                  ·····private int _y;
                                                  →}
                                                  }
                                                  """);

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a first field that keeps the declaration's line behind a comment that spans lines, with more than one
    /// space in front of it, keeps the column of the declaration's first token, and that its wrapped line is laid out from
    /// that column, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLaysOutTheWrappedLineOfAFirstFieldBehindACommentThatSpansLinesFromTheDeclarationsColumn()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* a
                                 b */      private int _x = 1 +
                                      2, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p; /* a
                                      b */      private int _x = 1
                                                                 + 2;
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
    }

    /// <summary>
    /// Verifies that the tabs in front of a first field that keeps the declaration's line behind a comment that spans lines
    /// count as one column each, and that its wrapped line is laid out from that column, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixCountsEachTabInFrontOfAFirstFieldBehindACommentThatSpansLinesAsOneColumn()
    {
        var source = ExpandWhitespaceMarkers("""
                                             class C
                                             {
                                                 private int _p; /* a
                                                b */→→private int _x = 1 +
                                                     2, {|RH7101:_y|};
                                             }
                                             """);

        var fixedSource = ExpandWhitespaceMarkers("""
                                                  class C
                                                  {
                                                      private int _p; /* a
                                                     b */→→private int _x = 1
                                                                            + 2;
                                                      private int _y;
                                                  }
                                                  """);

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
    }

    /// <summary>
    /// Verifies the specification example: the fix of one declaration leaves another combined declaration of the type
    /// as it is, which keeps its diagnostic, under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixOfOneDeclarationLeavesAnotherDeclarationOfTheTypeCombined()
    {
        const string source = """
                              class C
                              {
                                  private int _a, _b;
                                  private int _c, _d;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a;
                                       private int _b;
                                       private int _c, _d;
                                   }
                                   """;

        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_b");
        await Verify(fixedSource.Replace("_c, _d", "_c, {|RH7101:_d|}"));
    }

    /// <summary>
    /// Verifies that the fix of a declaration of a type leaves a combined declaration of a nested type as it is, which
    /// keeps its diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixOfADeclarationLeavesADeclarationOfANestedTypeCombined()
    {
        const string source = """
                              class C
                              {
                                  private int _a, _b;

                                  class D
                                  {
                                      private int _c, _d;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a;
                                       private int _b;

                                       class D
                                       {
                                           private int _c, _d;
                                       }
                                   }
                                   """;

        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_b");
        await Verify(fixedSource.Replace("_c, _d", "_c, {|RH7101:_d|}"));
    }

    /// <summary>
    /// Verifies that the fix of a declaration of a nested type leaves a combined declaration of the outer type as it is
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixOfADeclarationOfANestedTypeLeavesADeclarationOfTheOuterTypeCombined()
    {
        const string source = """
                              class C
                              {
                                  private int _a, _b;

                                  class D
                                  {
                                      private int _c, _d;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a, _b;

                                       class D
                                       {
                                           private int _c;
                                           private int _d;
                                       }
                                   }
                                   """;

        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_d");
    }

    /// <summary>
    /// Verifies that the fix of the first of two combined declarations on one line splits only that declaration and
    /// starts the other declaration on the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixOfTheFirstOfTwoDeclarationsOnOneLineStartsTheOtherOnTheNextLine()
    {
        const string source = """
                              class C
                              {
                                  private int _a, _b; private int _c, _d;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a;
                                       private int _b;
                                       private int _c, _d;
                                   }
                                   """;

        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_b");
    }

    /// <summary>
    /// Verifies that the fix of the second of two combined declarations on one line splits only that declaration and
    /// starts its first field on the next line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixOfTheSecondOfTwoDeclarationsOnOneLineStartsItsFirstFieldOnTheNextLine()
    {
        const string source = """
                              class C
                              {
                                  private int _a, _b; private int _c, _d;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a, _b;
                                       private int _c;
                                       private int _d;
                                   }
                                   """;

        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_d");
    }

    /// <summary>
    /// Verifies that Fix All over two combined declarations on one line, whose single fixes both write the whitespace
    /// between them, splits both in one iteration and gives every field a line of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixAllSplitsTwoDeclarationsOnOneLine()
    {
        const string source = """
                              class C
                              {
                                  private int _a, {|RH7101:_b|}; private int _c, {|RH7101:_d|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _a;
                                       private int _b;
                                       private int _c;
                                       private int _d;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that the fix is offered for and splits a combined declaration of every type kind the formatter splits
    /// </summary>
    /// <param name="keyword">The keyword that declares the type</param>
    /// <param name="modifiers">The modifiers of the fields</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    [DataRow("class", "private")]
    [DataRow("struct", "private")]
    [DataRow("record", "private")]
    [DataRow("record struct", "private")]
    [DataRow("interface", "private static")]
    public async Task FixSplitsACombinedDeclarationOfEveryTypeKind(string keyword, string modifiers)
    {
        var source = $$"""
                       {{keyword}} T
                       {
                           {{modifiers}} int _x, {|RH7101:_y|};
                       }
                       """;

        var fixedSource = $$"""
                            {{keyword}} T
                            {
                                {{modifiers}} int _x;
                                {{modifiers}} int _y;
                            }
                            """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that a comment in front of a later declarator withholds the fix under LF and CRLF line endings
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-007")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentInFrontOfALaterDeclaratorWithholdsTheFixUnderBothLineEndings(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private int _x, // horizontal
                                              /* vertical */ {|RH7101:_y|};
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source.ReplaceLineEndings(endOfLine));
    }

    /// <summary>
    /// Verifies that the fix of a declaration between two members on its line keeps the document's CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixGivesTheFieldsAndTheMembersOnTheirLineALineOfTheirOwnUnderBothLineEndings()
    {
        const string source = """
                              class C
                              {
                                  private int _p; /* c */ private int _x, _y; /* d */ private int _q;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p; /* c */
                                       private int _x;
                                       private int _y; /* d */
                                       private int _q;
                                   }
                                   """;

        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
    }

    /// <summary>
    /// Verifies that a combined declaration is not reported while the file contains a syntax error, also outside the
    /// declaration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    [Requirement("SHARED-JOIN-002")]
    public async Task CombinedDeclarationIsNotReportedInAFileWithASyntaxError()
    {
        const string source = """
                              class C
                              {
                                  private int _x, _y;

                                  void M()
                                  {
                                      var a = ;
                                  }
                              }
                              """;

        await Verify(source, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the same combined declaration is reported, with its fix, once the syntax error is removed, so the
    /// missing report above is caused by the error
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-001")]
    [Requirement("SHARED-JOIN-002")]
    public async Task CombinedDeclarationIsReportedAndFixedOnceTheSyntaxErrorIsRemoved()
    {
        const string source = """
                              class C
                              {
                                  private int _x, {|RH7101:_y|};

                                  void M()
                                  {
                                      var a = 1;
                                  }
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x;
                                       private int _y;

                                       void M()
                                       {
                                           var a = 1;
                                       }
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: a combined field declaration in an extension block, which the compiler
    /// rejects, is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-002")]
    public async Task CombinedDeclarationInAnExtensionBlockIsNotReported()
    {
        const string source = """
                              static class E
                              {
                                  extension(int i)
                                  {
                                      private static int _x, _y;
                                  }
                              }
                              """;

        await Verify(source, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the fix splits a combined declaration in a file whose header marks it as generated, when the
    /// analyzers analyze the file because the generated-code heuristic is turned off
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixSplitsADeclarationInAFileWithAGeneratedHeaderThatTheAnalyzersAnalyze()
    {
        const string source = """
                              // <auto-generated/>
                              class C
                              {
                                  private int _x, {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   // <auto-generated/>
                                   class C
                                   {
                                       private int _x;
                                       private int _y;
                                   }
                                   """;

        await Verify(source,
                     fixedSource,
                     test =>
                     {
                         test.TestState.AnalyzerConfigFiles.Add(("/.globalconfig", "is_global = true\ngenerated_code = false\n"));
                         test.FixedState.AnalyzerConfigFiles.Add(("/.globalconfig", "is_global = true\ngenerated_code = false\n"));
                     });
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

                                       private static readonly int _g = 1;
                                       private static readonly int _h;

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
    /// Verifies the change example: a line break between the last modifier and the type, which the split repeats in every field,
    /// is joined in every field the fix creates
    /// </summary>
    /// <param name="endOfLine">Line break of the document</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    [Requirement("FMT-BREAK-041")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task FixJoinsALineBreakBehindTheLastModifierInEveryField(string endOfLine)
    {
        const string source = """
                              class C
                              {
                                  private
                                      int _a, {|RH7101:_b|};
                              }
                              """;
        const string fixedSource = """
                                   class C
                                   {
                                       private int _a;
                                       private int _b;
                                   }
                                   """;

        await Verify(ConvertEndOfLine(source, endOfLine), ConvertEndOfLine(fixedSource, endOfLine));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Defines the preprocessor symbol <c>DEBUG</c> for the test project
    /// </summary>
    /// <param name="solution">Solution</param>
    /// <param name="projectId">Project ID</param>
    /// <returns>Solution</returns>
    private static Solution DefineDebug(Solution solution, ProjectId projectId)
    {
        return solution.GetProject(projectId)?.ParseOptions is CSharpParseOptions parseOptions
                   ? solution.WithProjectParseOptions(projectId, parseOptions.WithPreprocessorSymbols("DEBUG"))
                   : solution;
    }

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
    /// Gets the location of the second declarator of the first variable declaration in the syntax root
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>The location of the declarator's identifier</returns>
    private static Location GetSecondDeclaratorLocation(SyntaxNode root)
    {
        return root.DescendantNodes()
                   .OfType<VariableDeclarationSyntax>()
                   .First()
                   .Variables[1]
                   .Identifier
                   .GetLocation();
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