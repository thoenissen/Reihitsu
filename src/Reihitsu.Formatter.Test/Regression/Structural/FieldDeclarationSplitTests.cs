using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for <see cref="Pipeline.StructuralTransforms.Rewriter.FieldDeclarationSplitTransform"/>
/// </summary>
[TestClass]
public class FieldDeclarationSplitTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a combined field declaration without comments is split into separate declarations
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void CombinedFieldsAreSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _a, _b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a;
                                    private int _b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a combined field declaration with a single-line documentation comment is not split, so the
    /// comment keeps documenting every field; only the comment is laid out
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedCombinedFieldsAreNotSplit()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 /// <summary>Two fields.</summary>
                                 private int _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Two fields.
                                    /// </summary>
                                    private int _first, _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in a nested type is not split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedCombinedFieldsInNestedTypeAreNotSplit()
    {
        // Arrange
        const string input = """
                             internal class Outer
                             {
                                 internal class Inner
                                 {
                                     /// <summary>
                                     /// Two fields.
                                     /// </summary>
                                     private int _first, _second;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration with three declarators is not split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedCombinedFieldsWithThreeDeclaratorsAreNotSplit()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 /// <summary>
                                 /// Three fields.
                                 /// </summary>
                                 private int _first, _second, _third;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combined field declaration whose documentation comment stands in front of its attribute list
    /// is not split: the comment is in the declaration's leading trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentationCommentInFrontOfTheAttributeListKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 /// <summary>
                                 /// Two fields.
                                 /// </summary>
                                 [System.Obsolete]
                                 private int _first, _second;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combined field declaration preceded by an inline block comment is split at the indentation of
    /// the line the field starts on, instead of accumulating the whitespace on both sides of the comment.
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void InlineBlockCommentCombinedFieldsAreSplitAtMemberIndentation()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 /* note */ private int _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /* note */ private int _first;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a combined field declaration preceded by a block comment on its own line keeps its current,
    /// already correct indentation. This is the "field starts its own line" side of the boundary that
    /// <see cref="InlineBlockCommentCombinedFieldsAreSplitAtMemberIndentation"/> exercises from the other side
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void OwnLineBlockCommentCombinedFieldsAreSplitAtMemberIndentation()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 /* note */
                                 private int _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /* note */
                                    private int _first;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a combined field declaration documented with a delimited documentation comment on its own line
    /// is not split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DelimitedDocumentationCommentKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 /** Two fields. */
                                 private int _first, _second;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in a struct is not split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedCombinedStructFieldsAreNotSplit()
    {
        // Arrange
        const string input = """
                             internal struct TestStruct
                             {
                                 /// <summary>
                                 /// Two fields.
                                 /// </summary>
                                 private int _first, _second;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in a record is not split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedCombinedRecordFieldsAreNotSplit()
    {
        // Arrange
        const string input = """
                             internal record TestRecord
                             {
                                 /// <summary>
                                 /// Two fields.
                                 /// </summary>
                                 private int _first, _second;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in a record struct is not split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedCombinedRecordStructFieldsAreNotSplit()
    {
        // Arrange
        const string input = """
                             internal record struct TestRecordStruct
                             {
                                 /// <summary>
                                 /// Two fields.
                                 /// </summary>
                                 private int _first, _second;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in a interface is not split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedCombinedInterfaceFieldsAreNotSplit()
    {
        // Arrange
        const string input = """
                             internal interface ITest
                             {
                                 /// <summary>
                                 /// Two fields.
                                 /// </summary>
                                 static int _first, _second;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the pipeline output for a first declarator whose slot holds no comment is unchanged by the
    /// split. The indentation phase re-anchors the declarator either way, so this asserts that reading the slot
    /// caused no regression here rather than pinning the boundary itself - that side is only observable on the code
    /// fix surface, where no later phase runs.
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void WhitespaceOnlyTriviaBeforeFirstDeclaratorIsUnchanged()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                 _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a line break behind the type and a line break between two modifiers are
    /// repeated in every field the split creates, and a line break behind a <c>,</c> stands in no field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void LineBreaksInsideAndBehindTheHeadAreRepeatedInEveryField()
    {
        // Arrange
        const string input = """
                             using System;
                             using System.Collections.Generic;

                             class C
                             {
                                 private Dictionary<string, List<int>>
                                     _map1, _map2 = new();

                                 private static
                                     readonly int _g = 1, _h;

                                 [NonSerialized]
                                 private int _x,
                                     _y;
                             }
                             """;
        const string expected = """
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break behind the type of a field with several declarators, with the first declarator on a
    /// deeper column, is repeated in every field the split creates
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void LineBreakBehindTheTypeIsRepeatedInEveryField()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                     _g, _h;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    _g;
                                    private int
                                    _h;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break behind a generic type is repeated in every field the split creates, and that the
    /// initializer of the later declarator stays with its field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void LineBreakBehindAGenericTypeIsRepeatedInEveryField()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private Dictionary<string, List<int>>
                                     _map1, _map2 = new();
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private Dictionary<string, List<int>>
                                    _map1;
                                    private Dictionary<string, List<int>>
                                    _map2 = new();
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break behind the type is repeated in every field when several declarators carry
    /// initializers, and that each field keeps its own initializer
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void LineBreakBehindTheTypeIsRepeatedForDeclaratorsWithInitializers()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                     _g = 1, _h = 2, _i;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    _g = 1;
                                    private int
                                    _h = 2;
                                    private int
                                    _i;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break between two modifiers is repeated in every field the split creates
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void LineBreakBetweenTwoModifiersIsRepeatedInEveryField()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private static
                                     readonly int _g = 1, _h;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private static
                                    readonly int _g = 1;
                                    private static
                                    readonly int _h;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break between the last modifier and the type is repeated in every field the split creates
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void LineBreakBetweenTheLastModifierAndTheTypeIsRepeatedInEveryField()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private static readonly
                                     int _g, _h;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private static readonly
                                    int _g;
                                    private static readonly
                                    int _h;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line break behind a <c>,</c> between two declarators stands in no field the split creates, while
    /// the attribute list on its own line is repeated in every field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void LineBreakBehindACommaStandsInNoField()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 [NonSerialized]
                                 private int _x,
                                     _y;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    [NonSerialized]
                                    private int _x;
                                    [NonSerialized]
                                    private int _y;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in a declaration with a line break behind the type and a line break behind a <c>,</c>, every field
    /// repeats the line break behind the type and none takes the line break behind the <c>,</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void LineBreakBehindTheTypeIsRepeatedAndLineBreakBehindACommaIsNot()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                     _a,
                                     _b;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    _a;
                                    private int
                                    _b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a field with a single declarator and a line break behind its type, which is not split, is laid out
    /// as each field the split creates from a declaration with several declarators
    /// </summary>
    [TestMethod]
    public void SingleDeclaratorWithLineBreakBehindTheTypeIsLaidOutAsASplitField()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                     _x;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    _x;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a conditional group between the type and the first declarator, whose comment is disabled text, is a shape
    /// the formatter refuses: the input is returned unchanged, and the <c>#if DEBUG</c> directive is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    public void DirectiveBeforeFirstDeclaratorIsUnsupported()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                             #if DEBUG
                                 /* c */
                             #endif
                                 _first, _second;
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, input, "#if DEBUG");
    }

    /// <summary>
    /// Verifies that a fully inactive group between the type and the first declarator is a shape the formatter refuses: the
    /// input is returned unchanged, and the <c>#if false</c> directive is reported
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-010")]
    public void DisabledTextBeforeFirstDeclaratorIsUnsupported()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                             #if false
                                 /* c */
                             #endif
                                 _first, _second;
                             }
                             """;

        // Act & Assert
        AssertUnsupportedFor(FieldDeclarationDirectiveReason, input, "#if false");
    }

    /// <summary>
    /// Verifies that static fields declared in an interface are split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-021")]
    public void CombinedStaticInterfaceFieldsAreSplit()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 static int _a, _b;
                             }
                             """;
        const string expected = """
                                interface I
                                {
                                    static int _a;
                                    static int _b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: in the documentation mode None the documentation-style comments in front
    /// of the declarations are ordinary comments, and the declarations are split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void SpecExampleDocumentationStyleCommentsAreOrdinaryCommentsInDocumentationModeNone()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>The coordinates</summary>
                                 public int X, Y;

                                 /** <summary>The sizes</summary> */
                                 public int Width, Height;
                             }
                             """;
        const string expected = """
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

        // Act & Assert
        AssertRuleResult(input, expected, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies that a declaration documented with a single-line documentation comment and one documented with a
    /// delimited documentation comment are not split, while an undocumented declaration beside them is
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedDeclarationsAreNotSplitWhileAnUndocumentedDeclarationIs()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>The coordinates</summary>
                                 public int X, Y;

                                 /** <summary>The sizes</summary> */
                                 public int Width, Height;

                                 public int A, B;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    /// <summary>
                                    /// The coordinates
                                    /// </summary>
                                    public int X, Y;

                                    /** <summary>The sizes</summary> */
                                    public int Width, Height;

                                    public int A;
                                    public int B;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration is not split when the documentation mode is Diagnose
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedCombinedFieldsAreNotSplitInDocumentationModeDiagnose()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// The coordinates
                                 /// </summary>
                                 public int X, Y;

                                 /** <summary>The sizes</summary> */
                                 public int Width, Height;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Diagnose));
    }

    /// <summary>
    /// Verifies that a single-line documentation comment written behind the code of the member in front, which is
    /// leading trivia of the declaration, keeps the declaration whole while the comment is moved onto its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentationCommentWrittenBehindTheCodeOfTheMemberInFrontKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _p; /// <summary>X.</summary>
                                 public int X, Y;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _p;

                                    /// <summary>
                                    /// X.
                                    /// </summary>
                                    public int X, Y;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written behind the code of the member in front, on the
    /// declaration's line, keeps the declaration whole while the comment and the declaration start a line of their
    /// own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DelimitedDocumentationCommentWrittenBehindTheCodeOfTheMemberInFrontKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _r; /** W. */ public int W, H;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _r;

                                    /** W. */ public int W, H;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: the declaration's leading trivia stays on the first field, a comment inside an
    /// initializer value stays in that value, and the comment behind the semicolon stays behind the last field. The space
    /// between the comment and the argument is the one the formatter writes behind a comment that <c>(</c> carries
    /// (FMT-SPACE-013)
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void SpecExampleSplitFieldsKeepTheirLeadingTriviaTheirValueCommentsAndTheCommentBehindTheSemicolon()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 // The coordinates
                                 private int _x = Offset(/* horizontal */ 1), _y; // vertical
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // The coordinates
                                    private int _x = Offset(/* horizontal */1);
                                    private int _y; // vertical
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment inside the initializer value of a later declarator stays in that value in the declarator's
    /// field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void CommentInsideTheValueOfALaterDeclaratorStaysInThatValue()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _a = 1, _b = 2 /* two */ + 3, _c;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a = 1;
                                    private int _b = 2 /* two */ + 3;
                                    private int _c;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: when the documentation mode is <c>Parse</c>, neither documented declaration is
    /// split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void SpecExampleDocumentedDeclarationsAreNotSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 /// <summary>
                                 /// The coordinates
                                 /// </summary>
                                 public int X, Y;

                                 /** <summary>The sizes</summary> */
                                 public int Width, Height;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the specification example of the other side of the condition: an ordinary comment in front of the
    /// declaration and a comment inside the first declarator's value do not keep a declaration from being split. The space
    /// between the comment and the argument is the one the formatter writes behind a comment that <c>(</c> carries
    /// (FMT-SPACE-013)
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void SpecExampleOrdinaryCommentInFrontAndCommentInsideTheFirstValueDoNotKeepTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 // The coordinates
                                 public int X, Y;

                                 private int _first = Offset(/* c */ 1), _second;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // The coordinates
                                    public int X;
                                    public int Y;

                                    private int _first = Offset(/* c */1);
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a region in front of the declaration and a conditional group that holds the whole
    /// declaration stay in front of the first field, and both declarations are split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void SpecExampleRegionAndGroupAroundTheDeclarationAreSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Fields

                                 private int _x, _y;

                                 #endregion // Fields

                             #if !RELEASE
                                 private int _p, _q = 1;
                             #endif
                             }
                             """;
        const string expected = """
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

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: in a build that defines neither <c>DEBUG</c> nor <c>NETSTANDARD</c>, the declarations
    /// behind an inactive attribute list and behind an inactive modifier stay whole, while the declaration behind the end of a
    /// group the file's own <c>#define</c> makes active is split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void SpecExampleDisabledTextInTheLeadingTriviaKeepsTheDeclarationWhole()
    {
        // Arrange
        const string input = """
                             #define D
                             using System.Diagnostics;

                             class C
                             {
                             #if DEBUG
                                 [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                             #endif
                                 private int _a, _b;

                             #if NETSTANDARD
                                 static
                             #endif
                                 private int _c, _d;

                             #if D
                                 private int _x;
                             #endif
                                 private int _m, _n;
                             }
                             """;
        const string expected = """
                                #define D
                                using System.Diagnostics;

                                class C
                                {
                                #if DEBUG
                                    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                                #endif
                                    private int _a, _b;

                                #if NETSTANDARD
                                    static
                                #endif
                                    private int _c, _d;

                                #if D
                                    private int _x;
                                #endif
                                    private int _m;
                                    private int _n;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that disabled text in the declaration's leading trivia keeps it whole whatever the disabled text holds: an
    /// attribute list in each of two inactive branches, a whole member of its own, the declaration's own branch for another build
    /// with the declaration in the active <c>#else</c>, and an attribute list in a fully inactive group
    /// </summary>
    /// <param name="input">The formatted source text, which the formatter leaves unchanged</param>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    [DataRow("using System;\n\nclass C\n{\n#if DEBUG\n    [Obsolete]\n#elif TRACE\n    [NonSerialized]\n#endif\n    private int _a, _b;\n}", DisplayName = "Attribute lists in an if and an elif branch")]
    [DataRow("class C\n{\n#if DEBUG\n    private int _dbg;\n#endif\n    private int _a, _b;\n}", DisplayName = "Inactive group that holds a member of its own")]
    [DataRow("class C\n{\n#if X\n    private long _a, _b;\n#else\n    private int _a, _b;\n#endif\n}", DisplayName = "Declaration in the active else branch")]
    [DataRow("using System;\n\nclass C\n{\n#if false\n    [Obsolete]\n#endif\n    private int _a, _b;\n}", DisplayName = "Attribute list in a fully inactive group")]
    [DataRow("class C\n{\n    #region Fields\n\n#if DEBUG\n    private int _dbg;\n#endif\n    private int _a, _b;\n\n    #endregion // Fields\n}", DisplayName = "Region and inactive group in front of the declaration")]
    public void DisabledTextInTheLeadingTriviaKeepsTheDeclarationWhole(string input)
    {
        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a pragma directive in front of the declaration stays in front of the first field, the pragma directive behind
    /// the semicolon stays behind the last field, and the declaration is split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void PragmaDirectiveInFrontOfTheDeclarationStaysOnTheFirstField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #pragma warning disable CS0169
                                 private int _a, _b;
                             #pragma warning restore CS0169
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #pragma warning disable CS0169
                                    private int _a;
                                    private int _b;
                                #pragma warning restore CS0169
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a declaration in the active branch of a group whose disabled <c>#else</c> branch follows it is split: its
    /// leading trivia holds only the <c>#if</c> directive, and the disabled branch belongs to the token behind its semicolon
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void DeclarationInTheActiveBranchInFrontOfADisabledElseBranchIsSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if !X
                                 private int _a, _b;
                             #else
                                 private long _a, _b;
                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if !X
                                    private int _a;
                                    private int _b;
                                #else
                                    private long _a, _b;
                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region in front of an attributed declaration stays in front of the first field's attribute list, and every
    /// field repeats the attribute list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void RegionInFrontOfAnAttributedDeclarationStaysInFrontOfTheFirstField()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 #region Fields

                                 [NonSerialized]
                                 private static readonly string _a = "a", _b = "b";

                                 #endregion // Fields
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    #region Fields

                                    [NonSerialized]
                                    private static readonly string _a = "a";
                                    [NonSerialized]
                                    private static readonly string _b = "b";

                                    #endregion // Fields
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region and an ordinary comment in front of the declaration both stay in front of the first field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void RegionAndCommentInFrontOfTheDeclarationStayInFrontOfTheFirstField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Fields

                                 // The coordinates
                                 private int _x, _y;

                                 #endregion // Fields
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Fields

                                    // The coordinates
                                    private int _x;
                                    private int _y;

                                    #endregion // Fields
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}