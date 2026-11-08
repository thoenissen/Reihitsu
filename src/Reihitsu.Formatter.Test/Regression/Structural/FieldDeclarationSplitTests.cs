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
    /// Verifies that a trailing comment after the separator is preserved on the corresponding declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void TrailingCommentAfterSeparatorIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _a, // first
                                             _b; // second
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a; // first
                                    private int _b; // second
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment that trailed a separating comma on the declarators' line stays behind the field that comma
    /// now ends, with one space in front of it, and that the field behind it starts a line of its own
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    [Requirement("FMT-STRUCT-030")]
    public void TrailingBlockCommentAfterSeparatorOnTheDeclaratorsLineIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _a,/* first */ _b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a; /* first */
                                    private int _b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment that spans lines behind a separating comma ends the line of the field that comma now
    /// ends, and that the field behind it starts its own line at the member column
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void BlockCommentThatSpansLinesBehindASeparatorEndsTheLineOfTheField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x, /* a
                                    b */ _y;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x; /* a
                                       b */
                                    private int _y;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment that spans lines behind a separating comma ends the line of the field that comma now
    /// ends when the later declarator stands on the line behind the comment
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void BlockCommentThatSpansLinesBehindASeparatorWithTheDeclaratorOnTheNextLineEndsTheLineOfTheField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x, /* a
                                    b */
                                     _y;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x; /* a
                                       b */
                                    private int _y;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the line of the field ends behind the last comment of a separating comma when that comment spans lines
    /// and an earlier one starts and ends on one line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void LastOfTwoCommentsBehindASeparatorThatSpansLinesEndsTheLineOfTheField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x, /* a */ /* b
                                    c */ _y;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x; /* a */ /* b
                                       c */
                                    private int _y;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the line of the field ends behind the last comment of a separating comma when that comment starts and
    /// ends on one line and an earlier one spans lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void OneLineCommentBehindACommentThatSpansLinesBehindASeparatorEndsTheLineOfTheField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x, /* a
                                    b */ /* c */ _y;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x; /* a
                                       b */ /* c */
                                    private int _y;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single-line comment behind a comment that spans lines behind a separating comma ends the line of the
    /// field, and that the field behind it starts its own line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void SingleLineCommentBehindACommentThatSpansLinesBehindASeparatorEndsTheLineOfTheField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x, /* a
                                    b */ // c
                                     _y;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x; /* a
                                       b */ // c
                                    private int _y;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that only the field behind a comment that spans lines starts a new line behind it, and that a later
    /// declarator without a comment is split as any other
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void CommentThatSpansLinesBehindTheFirstOfThreeDeclaratorsEndsTheLineOfTheFirstField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x, /* a */ /* b
                                    c */ _y, _z;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x; /* a */ /* b
                                       c */
                                    private int _y;
                                    private int _z;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the FMT-STRUCT-026 example: each field behind a comment that spans lines starts its own line, and the
    /// comments' later lines keep the text the author wrote, also when a one-line comment follows on the last line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-026")]
    public void EveryFieldBehindACommentThatSpansLinesStartsItsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _p, /* a */ /* b
                                    c */ _q, /* d
                                    e */ /* f */ _r;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _p; /* a */ /* b
                                       c */
                                    private int _q; /* d
                                       e */ /* f */
                                    private int _r;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a standalone comment line before a later declarator is preserved
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void StandaloneCommentBeforeDeclaratorIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _a,
                                             // standalone
                                             _b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a;

                                    // standalone
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
    /// Verifies that a documentation comment preceding a later declarator is preserved and documents the field it
    /// precedes. Before the fix the split rebuilt the generated field's trivia from a comment set that excluded
    /// documentation comments, which deleted it outright.
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentationCommentBeforeDeclaratorIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first,
                                             /// <summary>Second.</summary>
                                             _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first;

                                    /// <summary>
                                    /// Second.
                                    /// </summary>
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment preceding a later declarator is preserved. This form carries
    /// no line break of its own, so the split has to supply one.
    /// </summary>
    [TestMethod]
    public void MultiLineDocumentationCommentBeforeDeclaratorIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first,
                                             /** Second. */
                                             _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first;

                                    /** Second. */
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written after the separator documents the following field. Roslyn files
    /// it as leading trivia of the next declarator, unlike the line comment in
    /// <see cref="TrailingCommentAfterSeparatorIsPreserved"/>, which stays trailing on the first field.
    /// </summary>
    [TestMethod]
    public void DocumentationCommentAfterSeparatorDocumentsFollowingField()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first, /// <summary>Second.</summary>
                                             _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first;

                                    /// <summary>
                                    /// Second.
                                    /// </summary>
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment written before the separator stays before the semicolon the separator becomes.
    /// The separator terminates the first declarator exactly as the semicolon terminates the last, so the split
    /// preserves the comment's position instead of relocating it across the generated semicolon.
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void BlockCommentBeforeSeparatorStaysBeforeTheGeneratedSemicolon()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first /* first */,
                                             _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first /* first */;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration is not split also when a later declarator carries a
    /// documentation comment of its own. The requirement decides only that the declaration stays one declaration, so
    /// the test asserts that and a stable second pass, not the layout of the comment in front of the later declarator
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedDeclarationWithADocumentedLaterDeclaratorIsNotSplit()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 /// <summary>First.</summary>
                                 private int _first,
                                             /// <summary>Second.</summary>
                                             _second;
                             }
                             """;

        // Act & Assert
        AssertDeclarationIsNotSplit(input, "_first");
    }

    /// <summary>
    /// Verifies that a documentation comment preceding the second of three declarators is preserved on that field
    /// only, leaving the third field undocumented.
    /// </summary>
    [TestMethod]
    public void DocumentationCommentBeforeSecondOfThreeDeclaratorsIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first,
                                             /// <summary>Second.</summary>
                                             _second,
                                             _third;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first;

                                    /// <summary>
                                    /// Second.
                                    /// </summary>
                                    private int _second;
                                    private int _third;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment and a documentation comment preceding the same declarator are both preserved in
    /// source order.
    /// </summary>
    [TestMethod]
    public void LineAndDocumentationCommentBeforeDeclaratorArePreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first,
                                             // note
                                             /// <summary>Second.</summary>
                                             _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first;

                                    // note
                                    /// <summary>
                                    /// Second.
                                    /// </summary>
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment preceding a later declarator is preserved at the nested member
    /// indentation.
    /// </summary>
    [TestMethod]
    public void DocumentationCommentBeforeDeclaratorInNestedTypeIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class Outer
                             {
                                 internal class Inner
                                 {
                                     private int _first,
                                                 /// <summary>Second.</summary>
                                                 _second;
                                 }
                             }
                             """;
        const string expected = """
                                internal class Outer
                                {
                                    internal class Inner
                                    {
                                        private int _first;

                                        /// <summary>
                                        /// Second.
                                        /// </summary>
                                        private int _second;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment preceding a later declarator is preserved in an interface.
    /// </summary>
    [TestMethod]
    public void DocumentationCommentBeforeDeclaratorInInterfaceIsPreserved()
    {
        // Arrange
        const string input = """
                             internal interface ITest
                             {
                                 static int _first,
                                            /// <summary>Second.</summary>
                                            _second;
                             }
                             """;
        const string expected = """
                                internal interface ITest
                                {
                                    static int _first;

                                    /// <summary>
                                    /// Second.
                                    /// </summary>
                                    static int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written before the semicolon of a combined field declaration appears
    /// exactly once in the split output, in the position the author wrote it.
    /// </summary>
    [TestMethod]
    public void DocumentationCommentBeforeSemicolonIsNotDuplicated()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first, _second /** Trailing note. */;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first;
                                    private int _second /** Trailing note. */;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single line documentation comment written before the semicolon appears exactly once
    /// </summary>
    [TestMethod]
    public void SingleLineDocumentationCommentBeforeSemicolonIsNotDuplicated()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first, _second
                                 /// <summary>Note.</summary>
                                 ;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first;
                                    private int _second

                                    /// <summary>
                                    /// Note.
                                    /// </summary>
                                    ;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment written on its own line before the semicolon appears exactly once. Roslyn files
    /// it in the semicolon's leading trivia once a line break separates it from the declarator, which is the same
    /// slot the documentation comment of <see cref="DocumentationCommentBeforeSemicolonIsNotDuplicated"/> lands in
    /// </summary>
    [TestMethod]
    public void OwnLineCommentBeforeSemicolonIsNotDuplicated()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first, _second
                                 // note
                                 ;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first;
                                    private int _second

                                    // note
                                    ;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment written on its own line before the semicolon appears exactly once
    /// </summary>
    [TestMethod]
    public void OwnLineBlockCommentBeforeSemicolonIsNotDuplicated()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first, _second
                                 /* note */
                                 ;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first;
                                    private int _second

                                    /* note */
                                    ;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment written before the separator is preserved on the field the author
    /// wrote it on, instead of being dropped with the separator's leading trivia.
    /// </summary>
    [TestMethod]
    public void DocumentationCommentBeforeSeparatorIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first /** First field. */,
                                             _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first /** First field. */;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single line documentation comment written before the separator is preserved.
    /// </summary>
    [TestMethod]
    public void SingleLineDocumentationCommentBeforeSeparatorIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first
                                 /// <summary>First.</summary>
                                 , _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first

                                    /// <summary>
                                    /// First.
                                    /// </summary>
                                    ;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every declarator keeps its own comment before its own generated semicolon when three
    /// declarators each carry one, which is the boundary between the last declarator and the ones before it
    /// </summary>
    [TestMethod]
    public void CommentsBeforeEveryTerminatorStayOnTheirOwnGeneratedField()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first /* a */, _second /* b */, _third /* c */;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first /* a */;
                                    private int _second /* b */;
                                    private int _third /* c */;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment before the separator and a comment after it are split around the generated
    /// semicolon rather than both landing on the same side of it
    /// </summary>
    [TestMethod]
    public void CommentsAroundTheSeparatorAreSplitAroundTheGeneratedSemicolon()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first /* before */, // after
                                             _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first /* before */; // after
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a declarator comment and a separator comment written on the same declarator keep their source
    /// order. Roslyn files the block comment in the declarator's trailing trivia and the documentation comment in
    /// the separator's leading trivia, so the two halves have to be concatenated in that order
    /// </summary>
    [TestMethod]
    public void DeclaratorAndSeparatorCommentsKeepTheirSourceOrder()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int _first /* x */ /** y */, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int _first /* x */ /** y */;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written before the first declarator is preserved where the author wrote it. It sits
    /// in the first declarator's leading trivia, which the split used to strip without any call site reading it.
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentBeforeFirstDeclaratorIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                 /* c */ _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    /* c */ _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a line comment written before the first declarator is preserved. It ends its own line, so the
    /// declarator stays on the following line rather than being commented out.
    /// </summary>
    [TestMethod]
    public void LineCommentBeforeFirstDeclaratorIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                 // c
                                 _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int

                                    // c
                                    _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single line documentation comment written before the first declarator is preserved and
    /// normalized to its multi line form.
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void SingleLineDocumentationCommentBeforeFirstDeclaratorIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                 /// <summary>First.</summary>
                                 _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int

                                    /// <summary>
                                    /// First.
                                    /// </summary>
                                    _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited documentation comment written before the first declarator is preserved. It carries
    /// no line break of its own, so the declarator stays on its line.
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DelimitedDocumentationCommentBeforeFirstDeclaratorIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                 /** c */ _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int

                                    /** c */ _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two comments written before the first declarator are both preserved in their source order.
    /// </summary>
    [TestMethod]
    public void TwoCommentsBeforeFirstDeclaratorArePreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                 /* a */ /* b */ _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    /* a */ /* b */ _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written before the first of three declarators lands on the first generated field only.
    /// </summary>
    [TestMethod]
    public void CommentBeforeFirstOfThreeDeclaratorsLandsOnTheFirstFieldOnly()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                 /* c */ _first, _second, _third;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    /* c */ _first;
                                    private int
                                    _second;
                                    private int
                                    _third;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration with a comment on its own line in front of the first
    /// declarator is not split: the documentation comment keeps the declaration whole although the comment in front
    /// of the first name alone would not
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentedDeclarationWithACommentBeforeTheFirstDeclaratorIsNotSplit()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 /// <summary>
                                 /// Fields.
                                 /// </summary>
                                 private int
                                 /* c */ _first, _second;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the attribute list is still duplicated onto every generated field while a comment written
    /// before the first declarator lands on the first field only.
    /// </summary>
    [TestMethod]
    public void AttributeListAndCommentBeforeFirstDeclaratorAreBothPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 [System.Obsolete]
                                 private int
                                 /* c */ _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    [System.Obsolete]
                                    private int
                                    /* c */ _first;
                                    [System.Obsolete]
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written before the first declarator is preserved when the declarators carry
    /// initializers.
    /// </summary>
    [TestMethod]
    public void CommentBeforeFirstDeclaratorWithInitializersIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                 /* c */ _first = 1, _second = 2;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    /* c */ _first = 1;
                                    private int
                                    _second = 2;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written before the first declarator and a comment written after its separator are
    /// both preserved on the first generated field, on either side of the generated semicolon.
    /// </summary>
    [TestMethod]
    public void CommentsBeforeAndAfterTheFirstDeclaratorAreBothPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                                 /* a */ _first, // b
                                         _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    /* a */ _first; // b
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written before the first declarator of a struct field is preserved.
    /// </summary>
    [TestMethod]
    public void CommentBeforeFirstDeclaratorInStructIsPreserved()
    {
        // Arrange
        const string input = """
                             internal struct TestStruct
                             {
                                 private int
                                 /* c */ _first, _second;
                             }
                             """;
        const string expected = """
                                internal struct TestStruct
                                {
                                    private int
                                    /* c */ _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written before the first declarator of a record struct field is preserved.
    /// </summary>
    [TestMethod]
    public void CommentBeforeFirstDeclaratorInRecordStructIsPreserved()
    {
        // Arrange
        const string input = """
                             internal record struct TestRecord
                             {
                                 private int
                                 /* c */ _first, _second;
                             }
                             """;
        const string expected = """
                                internal record struct TestRecord
                                {
                                    private int
                                    /* c */ _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written before the first declarator of a static interface field is preserved.
    /// </summary>
    [TestMethod]
    public void CommentBeforeFirstDeclaratorInInterfaceIsPreserved()
    {
        // Arrange
        const string input = """
                             internal interface ITestInterface
                             {
                                 static int
                                 /* c */ _first, _second;
                             }
                             """;
        const string expected = """
                                internal interface ITestInterface
                                {
                                    static int
                                    /* c */ _first;
                                    static int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment written before the first declarator of a nested type's field is preserved at the
    /// nested indentation.
    /// </summary>
    [TestMethod]
    public void CommentBeforeFirstDeclaratorInNestedTypeIsPreserved()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private class Inner
                                 {
                                     private int
                                     /* c */ _first, _second;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private class Inner
                                    {
                                        private int
                                        /* c */ _first;
                                        private int
                                        _second;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a misindented comment before the first declarator is re-anchored to the member indentation.
    /// The split preserves the author's whitespace and the indentation phase owns the column, so the output does not
    /// depend on the column the comment was written at.
    /// </summary>
    [TestMethod]
    public void MisindentedCommentBeforeFirstDeclaratorIsNormalizedToMemberIndentation()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int
                             /* c */ _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    private int
                                    /* c */ _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
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
    /// Verifies that a combined field declaration with a comment behind the type, on the type's line, is left as one
    /// declaration instead of copying the comment onto every field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentBehindTheTypeKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int /* c */ _first, _second;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
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
    /// Verifies that a comment before the declarator of a single declarator field is untouched. The split does not
    /// run at all, so the slot is never rebuilt.
    /// </summary>
    [TestMethod]
    public void CommentBeforeDeclaratorOfSingleDeclaratorFieldIsUnchanged()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 private int

                                 /* c */ _first;
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
    /// Verifies that a combined field declaration with a comment between a modifier and the type is left as one
    /// declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentBetweenAModifierAndTheTypeKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private /* c */ int _a, _b;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combined field declaration with a comment inside its attribute list is left as one declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentInsideAnAttributeListKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 [System.Obsolete /* why */]
                                 private int _c, _d;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combined field declaration with a comment behind its attribute list, on the attribute list's
    /// line, is left as one declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentBehindAnAttributeListOnItsLineKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 [System.Obsolete] /* c */
                                 private int _s, _t;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combined field declaration with a comment on its own line between its attribute list and its
    /// modifiers is left as one declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentOnItsOwnLineBetweenTheAttributeListAndTheModifiersKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 [System.Obsolete]

                                 // c
                                 private int _e, _f;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combined field declaration with a comment between two of its modifiers is left as one
    /// declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentBetweenTwoModifiersKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 public /* c */ static int _q, _r;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combined field declaration with a comment inside its qualified type is left as one declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentInsideTheTypeKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private System. /* c */Int32 _o, _p;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a line comment behind the type, on the type's line, keeps the declaration whole also when the
    /// first declarator starts the next line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void LineCommentBehindTheTypeWithTheDeclaratorsOnTheNextLineKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int // c
                                 _e, _f;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a documentation-style comment between the attribute list and the modifiers, which is no
    /// documentation comment of the declaration, keeps the declaration whole as any comment in that slot does
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentationCommentBehindAnAttributeListKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 [System.Obsolete]

                                 /// <summary>
                                 /// m
                                 /// </summary>
                                 public int U, V;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that in the documentation mode None a documentation-style comment between the attribute list and the
    /// modifiers is an ordinary comment in the part every field repeats and keeps the declaration whole
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void DocumentationStyleCommentBehindAnAttributeListKeepsTheDeclarationUnsplitInDocumentationModeNone()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 [System.Obsolete]

                                 /// m
                                 public int U, V;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies that a combined field declaration of a nested type with a comment behind the type is left as one
    /// declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentBehindTheTypeInANestedTypeKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private class Inner
                                 {
                                     private int /* c */ _k, _l;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combined static field declaration of an interface with a comment behind the type is left as
    /// one declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentBehindTheTypeInAnInterfaceKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 static int /* c */ _k, _l;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a combined field declaration of a record struct with a comment between a modifier and the type
    /// is left as one declaration
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentBetweenAModifierAndTheTypeInARecordStructKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             record struct R
                             {
                                 private /* c */ int _m, _n;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment in the part one declaration repeats keeps only that declaration whole, while an
    /// undocumented declaration without such a comment beside it is split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentBehindTheTypeOfOneDeclarationLeavesAnotherDeclarationSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int /* c */ _first, _second;
                                 private int _a, _b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int /* c */ _first, _second;
                                    private int _a;
                                    private int _b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: when the documentation mode is Parse, neither a documented declaration nor
    /// one with a comment in the part every field repeats is split; the declarators behind the line comment on the
    /// type's line keep their line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void SpecExampleDeclarationsWithDocumentationOrACommentInTheRepeatedPartAreNotSplit()
    {
        // Arrange
        const string input = """
                             using System;

                             class C
                             {
                                 /// <summary>
                                 /// The coordinates
                                 /// </summary>
                                 public int X, Y;

                                 /** <summary>The sizes</summary> */
                                 public int Width, Height;

                                 private int /* c */ _first, _second;

                                 private /* c */ int _a, _b;

                                 [Obsolete /* why */]
                                 private int _c, _d;

                                 private int // c
                                     _e, _f;
                             }
                             """;
        const string expected = """
                                using System;

                                class C
                                {
                                    /// <summary>
                                    /// The coordinates
                                    /// </summary>
                                    public int X, Y;

                                    /** <summary>The sizes</summary> */
                                    public int Width, Height;

                                    private int /* c */ _first, _second;

                                    private /* c */ int _a, _b;

                                    [Obsolete /* why */]
                                    private int _c, _d;

                                    private int // c
                                    _e, _f;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example for the other side of each condition: an ordinary comment in front of the
    /// declaration and a comment in the first declarator's initializer do not keep a declaration from being split
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void SpecExampleOrdinaryCommentInFrontAndCommentInsideTheFirstDeclaratorDoNotKeepTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 // The coordinates
                                 public int X, Y;

                                 private int _first = /* c */ 1, _second;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // The coordinates
                                    public int X;
                                    public int Y;

                                    private int _first = /* c */ 1;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specification example: a comment on its own line in front of the first declarator's name belongs
    /// to the first declarator, so the declaration is split and the comment stays in front of the first field only
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    [Requirement("FMT-STRUCT-022")]
    public void SpecExampleCommentOnItsOwnLineInFrontOfTheFirstNameStaysWithTheFirstField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int
                                     /* c */ _first, _second;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int
                                    /* c */ _first;
                                    private int
                                    _second;
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
    /// Verifies the specification example: the ordinary comment in front of the declaration stays on the first field,
    /// the comment behind the separating comma trails the first field, and the comment in front of the later
    /// declarator gets a line of its own in front of that field
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-022")]
    public void SpecExampleCommentsKeepDescribingTheDeclaratorTheyWereWrittenNextTo()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 // The coordinates
                                 private int _x, // horizontal

                                             /* vertical */ _y;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    // The coordinates
                                    private int _x; // horizontal

                                    /* vertical */
                                    private int _y;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a delimited comment that spans lines between a modifier and the type keeps the declaration whole. The
    /// requirement decides only that the declaration stays one declaration, so the test asserts that and a stable second pass
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void CommentThatSpansLinesBetweenAModifierAndTheTypeKeepsTheDeclarationUnsplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private /* a
                                    b */ int _a, _b;
                             }
                             """;

        // Act & Assert
        AssertDeclarationIsNotSplit(input, "_a");
    }

    /// <summary>
    /// Verifies the specification example: when the documentation mode is Parse, a <c>/** */</c> or <c>///</c> comment behind
    /// the type on the type's line belongs to the first declarator, so the declaration is split and the comment stays in the
    /// first field only, also when the first declarator starts the next line
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    [Requirement("FMT-STRUCT-022")]
    public void SpecExampleDocumentationStyleCommentBehindTheTypeStaysWithTheFirstField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int /** c */ _first, _second;

                                 private int /// <summary>c</summary>
                                     _g, _h;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int

                                    /** c */ _first;
                                    private int _second;

                                    private int

                                    /// <summary>
                                    /// c
                                    /// </summary>
                                    _g;
                                    private int _h;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));
    }

    /// <summary>
    /// Verifies the specification example: in the documentation mode None, a <c>/** */</c> or <c>///</c> comment behind the
    /// type on the type's line is an ordinary comment in the part every field repeats, and neither declaration is split. The
    /// requirement decides only the split; the input writes the declarators on the next line at the column the formatter
    /// gives a declaration that stays whole, so the rest of the layout is a fixed point
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    public void SpecExampleDocumentationStyleCommentBehindTheTypeKeepsTheDeclarationUnsplitInDocumentationModeNone()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int /** c */ _first, _second;

                                 private int /// <summary>c</summary>
                                 _g, _h;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, parseOptions: CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.None));
    }

    /// <summary>
    /// Verifies that when the documentation mode is Parse, a <c>/** */</c> comment behind the type on the type's line, with the
    /// first declarator on the next line, belongs to the first declarator: the declaration is split and the comment stays in the
    /// first field only
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    [Requirement("FMT-STRUCT-022")]
    public void DelimitedDocumentationStyleCommentBehindTheTypeWithTheDeclaratorsOnTheNextLineStaysWithTheFirstField()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int /** c */
                                     _first, _second;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int

                                    /** c */
                                    _first;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse));
    }

    /// <summary>
    /// Verifies that when the documentation mode is Diagnose, a <c>///</c> comment behind a generic type on the type's line
    /// belongs to the first declarator: the declaration is split, the type is repeated without the comment, and the comment
    /// stays in the first field only
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-030")]
    [Requirement("FMT-STRUCT-022")]
    public void SingleLineDocumentationStyleCommentBehindAGenericTypeStaysWithTheFirstFieldInDocumentationModeDiagnose()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private System.Collections.Generic.List<int> /// c
                                     _first, _second;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private System.Collections.Generic.List<int>

                                    /// c
                                    _first;
                                    private System.Collections.Generic.List<int> _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected, CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Diagnose));
    }

    /// <summary>
    /// Formats the input under LF and CRLF line endings, verifies that a second pass is stable, and that the field declaration that
    /// declares the named variable keeps every declarator it has in the input
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <param name="variableName">A variable the declaration declares</param>
    private void AssertDeclarationIsNotSplit(string input, string variableName)
    {
        AssertSecondPassIsStable(input);

        foreach (var endOfLine in _lineEndings)
        {
            var tree = CSharpSyntaxTree.ParseText(NormalizeLineEndings(input, endOfLine), cancellationToken: TestContext.CancellationToken);
            var formatted = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

            Assert.AreEqual(GetDeclaratorNames(tree.GetRoot(TestContext.CancellationToken), variableName),
                            GetDeclaratorNames(formatted, variableName),
                            $"The declaration must stay one declaration under {DescribeLineEnding(endOfLine)} line endings.");
        }

        static string GetDeclaratorNames(SyntaxNode root, string declaredName)
        {
            return string.Join(", ",
                               root.DescendantNodes()
                                   .OfType<FieldDeclarationSyntax>()
                                   .Single(field => field.Declaration.Variables.Any(variable => variable.Identifier.ValueText == declaredName))
                                   .Declaration
                                   .Variables
                                   .Select(variable => variable.Identifier.ValueText));
        }
    }

    #endregion // Methods
}