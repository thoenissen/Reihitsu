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
    /// Verifies that a standalone comment line before a later declarator is preserved
    /// </summary>
    [TestMethod]
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
    /// Verifies that a documented combined field declaration is split at the member indentation on the first pass.
    /// A documentation comment carries its line break inside its own structure, so the indentation anchor must not
    /// look for a top-level end-of-line trivia after it (issue #592)
    /// </summary>
    [TestMethod]
    public void DocumentedCombinedFieldsAreSplitAtMemberIndentation()
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
                                    private int _first;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in a nested type is split at the nested member
    /// indentation rather than at a multiple of it (issue #592)
    /// </summary>
    [TestMethod]
    public void DocumentedCombinedFieldsInNestedTypeAreSplitAtMemberIndentation()
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
        const string expected = """
                                internal class Outer
                                {
                                    internal class Inner
                                    {
                                        /// <summary>
                                        /// Two fields.
                                        /// </summary>
                                        private int _first;
                                        private int _second;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that every field generated from a documented declaration with more than two declarators is placed
    /// at the member indentation (issue #592)
    /// </summary>
    [TestMethod]
    public void DocumentedCombinedFieldsWithThreeDeclaratorsAreSplitAtMemberIndentation()
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
        const string expected = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Three fields.
                                    /// </summary>
                                    private int _first;
                                    private int _second;
                                    private int _third;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration carrying an attribute list is split at the member
    /// indentation, so the generated attribute lists line up with the generated fields (issue #592)
    /// </summary>
    [TestMethod]
    public void DocumentedCombinedFieldsWithAttributeAreSplitAtMemberIndentation()
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
        const string expected = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// Two fields.
                                    /// </summary>
                                    [System.Obsolete]
                                    private int _first;
                                    [System.Obsolete]
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a combined field declaration preceded by an inline block comment is split at the indentation of
    /// the line the field starts on, instead of accumulating the whitespace on both sides of the comment (issue #592)
    /// </summary>
    [TestMethod]
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
    /// Verifies that a combined field declaration documented with a delimited documentation comment keeps its
    /// current, already correct indentation. Unlike a single-line documentation comment this form does not carry its
    /// line break internally, so it is the side of the boundary that a fix widened to "documentation comments" would
    /// break (issue #592)
    /// </summary>
    [TestMethod]
    public void MultiLineDocumentationCommentCombinedFieldsAreSplitAtMemberIndentation()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 /** Two fields. */
                                 private int _first, _second;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    /** Two fields. */
                                    private int _first;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in a struct is split at the member indentation (issue #592)
    /// </summary>
    [TestMethod]
    public void DocumentedCombinedStructFieldsAreSplitAtMemberIndentation()
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
        const string expected = """
                                internal struct TestStruct
                                {
                                    /// <summary>
                                    /// Two fields.
                                    /// </summary>
                                    private int _first;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in a record is split at the member indentation (issue #592)
    /// </summary>
    [TestMethod]
    public void DocumentedCombinedRecordFieldsAreSplitAtMemberIndentation()
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
        const string expected = """
                                internal record TestRecord
                                {
                                    /// <summary>
                                    /// Two fields.
                                    /// </summary>
                                    private int _first;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in a record struct is split at the member indentation (issue #592)
    /// </summary>
    [TestMethod]
    public void DocumentedCombinedRecordStructFieldsAreSplitAtMemberIndentation()
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
        const string expected = """
                                internal record struct TestRecordStruct
                                {
                                    /// <summary>
                                    /// Two fields.
                                    /// </summary>
                                    private int _first;
                                    private int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documented combined field declaration in an interface is split at the member indentation (issue #592)
    /// </summary>
    [TestMethod]
    public void DocumentedCombinedInterfaceFieldsAreSplitAtMemberIndentation()
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
        const string expected = """
                                internal interface ITest
                                {
                                    /// <summary>
                                    /// Two fields.
                                    /// </summary>
                                    static int _first;
                                    static int _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a documentation comment preceding a later declarator is preserved and documents the field it
    /// precedes. Before the fix the split rebuilt the generated field's trivia from a comment set that excluded
    /// documentation comments, which deleted it outright (issue #592)
    /// </summary>
    [TestMethod]
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
    /// no line break of its own, so the split has to supply one (issue #592)
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
    /// <see cref="TrailingCommentAfterSeparatorIsPreserved"/>, which stays trailing on the first field (issue #592)
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
    /// preserves the comment's position instead of relocating it across the generated semicolon (issue #625)
    /// </summary>
    [TestMethod]
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
    /// Verifies that a documentation comment on the declaration and a documentation comment on a later declarator are
    /// both preserved, each documenting its own generated field (issue #592)
    /// </summary>
    [TestMethod]
    public void FieldAndDeclaratorDocumentationCommentsAreBothPreserved()
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
        const string expected = """
                                internal class TestClass
                                {
                                    /// <summary>
                                    /// First.
                                    /// </summary>
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
    /// Verifies that a documentation comment preceding the second of three declarators is preserved on that field
    /// only, leaving the third field undocumented (issue #592)
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
    /// source order (issue #592)
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
    /// indentation (issue #592)
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
    /// Verifies that a documentation comment preceding a later declarator is preserved in an interface (issue #592)
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
    /// exactly once in the split output, in the position the author wrote it (issue #625)
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
    /// wrote it on, instead of being dropped with the separator's leading trivia (issue #624)
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
    /// Verifies that a single line documentation comment written before the separator is preserved (issue #624)
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
    /// Verifies that a comment written before the first declarator is still dropped. It sits in the first
    /// declarator's leading trivia, which has no terminator in front of it, so the terminator rule does not reach
    /// it. This records the remaining gap deliberately rather than widening the split silently
    /// </summary>
    [TestMethod]
    public void CommentBeforeFirstDeclaratorIsStillDropped()
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
                                    _first;
                                    private int
                                    _second;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that static fields declared in an interface are split
    /// </summary>
    [TestMethod]
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

    #endregion // Methods
}