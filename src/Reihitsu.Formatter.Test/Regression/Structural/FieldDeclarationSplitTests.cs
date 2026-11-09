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
    /// Verifies that a directive written before the first declarator still leaves the field declaration intact. The
    /// directive guard inspects the whole declaration, so it covers the newly read slot as well.
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-023")]
    public void DirectiveBeforeFirstDeclaratorLeavesTheFieldIntact()
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
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that disabled text written before the first declarator still leaves the field declaration intact.
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-023")]
    public void DisabledTextBeforeFirstDeclaratorLeavesTheFieldIntact()
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
        AssertRuleResult(input);
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
                                 private int _a = 1, _b = 2 + /* two */ 3, _c;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a = 1;
                                    private int _b = 2 + /* two */ 3;
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

    #endregion // Methods
}