using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
/// Test methods for <see cref="RH7101DoNotCombineFieldsAnalyzer"/> and <see cref="RH7101DoNotCombineFieldsCodeFixProvider"/>
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

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that comments attached to declarators and their separators are preserved when the fix splits the fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task VerifyCommentsArePreservedWhenSplitting()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField, // first
                                                {|#0:secondField|}; // second
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField; // first
                                     private int secondField; // second
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a documentation comment written before the semicolon appears exactly once and keeps its
    /// position
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentBeforeSemicolonIsNotDuplicated()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField, {|#0:secondField|} /** Trailing note. */;
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;
                                     private int secondField /** Trailing note. */;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a documentation comment written before the separator is preserved rather than dropped by the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDocumentationCommentBeforeSeparatorIsPreserved()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField /** First field. */, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField /** First field. */;
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix indents every generated field at the member level when the declaration is documented
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

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps the documentation comment above the declaration where the author wrote it, and
    /// places the generated fields at the member column rather than at the column of the documentation comment
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

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix keeps a documentation comment that starts on the opening brace line where the author
    /// wrote it, and places the generated fields at the member column rather than at column zero
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

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a single-line documentation comment re-attached to a generated field is laid out as the formatter
    /// lays it out: a blank line above it, the summary expanded over three lines, and no blank line between the
    /// comment and its field
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task VerifySingleLineDocumentationCommentBeforeDeclaratorIsLaidOutAsTheFormatterLaysItOut()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField,
                                                /// <summary>Second field.</summary>
                                                {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;

                                     /// <summary>
                                     /// Second field.
                                     /// </summary>
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a delimited documentation comment re-attached to a generated field keeps a line break after it
    /// and gets the blank line above it that the formatter writes. Unlike a single-line documentation comment it
    /// carries no line break of its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task VerifyMultiLineDocumentationCommentBeforeDeclaratorKeepsItsLineBreak()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int firstField,
                                                /** Second field. */
                                                {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int firstField;

                                     /** Second field. */
                                     private int secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment written before the first declarator, on a later line than the type, is preserved by
    /// the fix, and that the later field's declarator gets the column the formatter gives it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task VerifyCommentBeforeFirstDeclaratorIsPreservedByTheFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int
                                    /* c */ firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int
                                     /* c */ firstField;
                                     private int
                                     secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix lays out a comment before the first declarator, written at column zero on a later line
    /// than the type, at the column the formatter gives it, and the later field's declarator likewise
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task VerifyFixLaysOutTheCommentBeforeFirstDeclaratorAsTheFormatterLaysItOut()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int
                                /* c */ firstField, {|#0:secondField|};
                                }
                                """;

        const string fixedData = """
                                 internal class TestClass
                                 {
                                     private int
                                     /* c */ firstField;
                                     private int
                                     secondField;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat));
    }

    /// <summary>
    /// Verifies that the fix is not offered when the combined field carries a preprocessor directive, because the
    /// formatter leaves directive-bearing fields intact and the fix would otherwise be a no-op
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFixIsNotOfferedWhenFieldCarriesDirective()
    {
        const string testData = """
                                internal class TestClass
                                {
                                #if DEBUG
                                    private int firstField, secondField;
                                #endif
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH7101DoNotCombineFieldsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<FieldDeclarationSyntax>()
                                                               .Single()
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
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
    /// Verifies the specification example: a comment that trailed a separating comma trails the field that comma now
    /// ends
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixKeepsTheCommentsThatTrailedTheSeparators()
    {
        const string source = """
                              class C
                              {
                                  private int _first, // first
                                              {|RH7101:_second|}; // second
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _first; // first
                                       private int _second; // second
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies the specification example: a region directive in the declaration's trivia withholds the fix, and the
    /// diagnostic is still reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-005")]
    public async Task RegionDirectiveInTheDeclarationWithholdsTheFix()
    {
        const string source = """
                              class C
                              {
                                  #region Fields
                                  private int _a, {|RH7101:_b|};
                                  #endregion
                              }
                              """;

        await VerifyNoCodeFixIsOffered(source);
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
    /// Verifies the specification example: a comment in front of a later declarator gets a line of its own and the
    /// blank line above it that the formatter writes
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixGivesACommentInFrontOfALaterDeclaratorTheBlankLineTheFormatterWrites()
    {
        const string source = """
                              class C
                              {
                                  private int _x, // horizontal
                                              /* vertical */ {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x; // horizontal

                                       /* vertical */
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);

        AssertFormattingChangesNothing(fixedSource);
    }

    /// <summary>
    /// Verifies that a blank line the author wrote in front of a comment in front of a later declarator stays one
    /// blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixKeepsOneBlankLineTheAuthorWroteInFrontOfACommentInFrontOfALaterDeclarator()
    {
        const string source = """
                              class C
                              {
                                  private int _x, // horizontal

                                              /* vertical */ {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x; // horizontal

                                       /* vertical */
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
    }

    /// <summary>
    /// Verifies that documentation comments in front of later declarators get the blank line above them and the shape
    /// the formatter gives them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLaysOutDocumentationCommentsInFrontOfLaterDeclaratorsAsTheFormatterLaysThemOut()
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

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x;

                                       /// <summary>
                                       /// Second field.
                                       /// </summary>
                                       private int _y;

                                       /** Third field. */
                                       private int _z;
                                   }
                                   """;

        await Verify(source, fixedSource);

        AssertFormattingChangesNothing(fixedSource);
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
    /// Verifies that a documentation comment in the gap in front of the declaration goes with the first field onto the
    /// next line, with no blank line above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixMovesADocumentationCommentInTheGapInFrontWithTheFirstField()
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
                                       private int _p;
                                       /** X. */ private int _x;
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
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
    /// Verifies that the fix keeps the document's CRLF line endings and lays out a comment in front of a later
    /// declarator as under LF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixLaysOutACommentInFrontOfALaterDeclaratorUnderBothLineEndings()
    {
        const string source = """
                              class C
                              {
                                  private int _x, // horizontal
                                              /* vertical */ _y;
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x; // horizontal

                                       /* vertical */
                                       private int _y;
                                   }
                                   """;

        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
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
    /// Verifies the specification example with a member in front of the declaration on its line and a comment that spans
    /// lines behind the separating comma: the first field starts the next line at the member column, the comment ends its
    /// line, and the later field starts a line of its own at the member column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixStartsTheFieldBehindACommentThatSpansLinesOnItsOwnLineBehindAMemberInFront()
    {
        const string source = """
                              class C
                              {
                                  private int _p; private int _x, /* a
                                     b */ {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _p;
                                       private int _x; /* a
                                          b */
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
        AssertFormattingChangesNothing(fixedSource);
    }

    /// <summary>
    /// Verifies that the fix ends the first field's line behind a comment that spans lines behind the separating comma, as
    /// the formatter ends it, and starts the later field on its own line at the member column
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7101-004")]
    public async Task FixStartsTheFieldBehindACommentThatSpansLinesBehindTheSeparatorOnItsOwnLine()
    {
        const string source = """
                              class C
                              {
                                  private int _x, /* a
                                     b */ {|RH7101:_y|};
                              }
                              """;

        const string fixedSource = """
                                   class C
                                   {
                                       private int _x; /* a
                                          b */
                                       private int _y;
                                   }
                                   """;

        await Verify(source, fixedSource);
        await AssertSingleFixUnderBothLineEndings(source, fixedSource, "_y");
        AssertFormattingChangesNothing(fixedSource);
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
        TestFileMarkupParser.GetSpans(source, out var plainSource, out ImmutableDictionary<string, ImmutableArray<TextSpan>> _);

        Assert.AreEqual(fixedSource, await ApplyFixOfDeclarationAsync(plainSource, reportedDeclaratorName), "Unexpected fix result under LF line endings.");
        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedSource), await ApplyFixOfDeclarationAsync(NormalizeToCarriageReturnLineFeed(plainSource), reportedDeclaratorName), "Unexpected fix result under CRLF line endings.");
    }

    /// <summary>
    /// Applies the one fix offered for the declaration whose declarator has the given name
    /// </summary>
    /// <param name="source">Source text without markup</param>
    /// <param name="reportedDeclaratorName">Name of the declarator the diagnostic is reported on</param>
    /// <returns>The document's text after the fix</returns>
    private static async Task<string> ApplyFixOfDeclarationAsync(string source, string reportedDeclaratorName)
    {
        var actions = await GetCodeFixActionsAsync(source,
                                                   RH7101DoNotCombineFieldsAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<VariableDeclaratorSyntax>()
                                                               .Single(declarator => declarator.Identifier.ValueText == reportedDeclaratorName)
                                                               .Identifier
                                                               .GetLocation());

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
                                  Diagnostics(RH7101DoNotCombineFieldsAnalyzer.DiagnosticId, AnalyzerResources.RH7101MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}