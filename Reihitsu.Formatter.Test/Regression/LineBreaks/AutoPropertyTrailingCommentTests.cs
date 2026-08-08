using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Regression tests for issue #604: a comment that trails the accessor list of an auto-property sits
/// outside that list and is never rewritten by the single-line collapse, so it must not force the
/// accessor list apart. Trivia the collapse would cross - inside the accessor list, or in the gap
/// between the property signature and the opening brace - remains a join barrier
/// </summary>
[TestClass]
public class AutoPropertyTrailingCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a single-line comment trailing the accessor list does not split a single-line
    /// get/set auto-property
    /// </summary>
    [TestMethod]
    public void CommentAfterAutoAccessorListDoesNotSplitTheProperty()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } // explanation
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a block comment trailing the accessor list does not split a single-line
    /// get/set auto-property
    /// </summary>
    [TestMethod]
    public void BlockCommentAfterAutoAccessorListDoesNotSplitTheProperty()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /* explanation */
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment trailing the accessor list does not split a single-line get-only
    /// auto-property
    /// </summary>
    [TestMethod]
    public void CommentAfterGetOnlyAutoAccessorListDoesNotSplitTheProperty()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; } // explanation
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment trailing the accessor list does not split a single-line auto-property
    /// whose accessors carry attributes
    /// </summary>
    [TestMethod]
    public void CommentAfterAttributedAutoAccessorListDoesNotSplitTheProperty()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public required int Value { [Test] get; [Test] init; } // explanation
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment between the accessor list and the property initializer does not split
    /// a single-line auto-property
    /// </summary>
    [TestMethod]
    public void CommentBetweenAccessorListAndInitializerDoesNotSplitTheProperty()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /* note */ = 1;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line get/set auto-property collapses even when a comment trails its
    /// accessor list
    /// </summary>
    [TestMethod]
    public void MultiLineAutoPropertyWithTrailingCommentCollapses()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                                     get; set;
                                 } // explanation
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; } // explanation
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line get-only auto-property collapses even when a comment trails its
    /// accessor list
    /// </summary>
    [TestMethod]
    public void MultiLineGetOnlyAutoPropertyWithTrailingCommentCollapses()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                                     get;
                                 } // explanation
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; } // explanation
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a multi-line get/set auto-property collapses even when a block comment trails
    /// its accessor list
    /// </summary>
    [TestMethod]
    public void MultiLineAutoPropertyWithTrailingBlockCommentCollapses()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                                     get; set;
                                 } /* explanation */
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; } /* explanation */
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment inside the accessor list still prevents the single-line collapse
    /// </summary>
    [TestMethod]
    public void CommentInsideAccessorListStillSplitsTheProperty()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; /* inner */ set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value
                                    {
                                        get; /* inner */ set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a directive inside the accessor list still prevents the single-line collapse.
    /// The test deliberately uses <c>#if</c> rather than <c>#region</c>, because region directives
    /// receive their own normalization before the line-break phase runs
    /// </summary>
    [TestMethod]
    public void DirectiveInsideAccessorListStillSplitsTheProperty()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                             #if FEATURE
                                     get;
                             #endif
                                     set;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a comment in the gap between the property signature and the accessor-list
    /// opening brace still prevents the collapse and is not crossed
    /// </summary>
    [TestMethod]
    public void CommentInSignatureGapIsNotCrossed()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value // note
                                 { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value // note
                                    {
                                        get; set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment on its own line before the accessor-list opening brace still prevents
    /// the collapse and is not crossed
    /// </summary>
    [TestMethod]
    public void CommentOnOwnLineBeforeAccessorListIsNotCrossed()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 // note
                                 { get; set; }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value
                                    // note
                                    {
                                        get; set;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a property whose signature spans several lines keeps its existing layout even
    /// when a comment trails the accessor list, because the collapse is refused for the signature
    /// rather than for the trivia
    /// </summary>
    [TestMethod]
    public void MultiLineSignatureWithTrailingCommentRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public System.Collections.Generic.Dictionary<string,
                                 int> Value { get; set; } // explanation
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public System.Collections.Generic.Dictionary<string,
                                    int> Value
                                    {
                                        get; set;
                                    } // explanation
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment between the accessor list and an initializer on the following line is
    /// not crossed, so the declaration keeps its existing layout
    /// </summary>
    [TestMethod]
    public void CommentBeforeOwnLineInitializerIsNotCrossed()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } // note
                                 = 1;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a multi-line block comment between the accessor list and the initializer is not
    /// crossed, so the declaration keeps its existing layout
    /// </summary>
    [TestMethod]
    public void BlockCommentBeforeInitializerIsNotCrossed()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; } /* note
                                    more */ = 2;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an initializer on its own line is joined onto the property line when no trivia
    /// sits in between, which is the boundary that keeps the initializer-gap guard from over-reaching
    /// </summary>
    [TestMethod]
    public void OwnLineInitializerWithoutTriviaIsJoined()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value { get; set; }
                                 = 1;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; } = 1;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment sitting on one line between the accessor list and the initializer is
    /// carried along when the accessor list collapses, because that gap is never crossed
    /// </summary>
    [TestMethod]
    public void SameLineCommentBeforeInitializerCollapsesWithTheProperty()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                                     get; set;
                                 } /* note */ = 1;
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; set; } /* note */ = 1;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an accessor modifier is collapsed together with its keyword, so a single pass
    /// produces the final spacing instead of leaving the modifier's original indentation behind
    /// </summary>
    [TestMethod]
    public void AccessorModifierWithTrailingCommentCollapsesInOnePass()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                                     get;
                                     private set;
                                 } // explanation
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; private set; } // explanation
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an accessor modifier is collapsed in one pass even without a trailing comment,
    /// which is the shape that reaches the collapse independently of this issue
    /// </summary>
    [TestMethod]
    public void AccessorModifierCollapsesInOnePass()
    {
        // Arrange
        const string input = """
                             internal class TestClass
                             {
                                 public int Value
                                 {
                                     get;
                                     private set;
                                 }
                             }
                             """;
        const string expected = """
                                internal class TestClass
                                {
                                    public int Value { get; private set; }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an indexer with a multi-line auto-accessor list and a trailing comment keeps its
    /// existing layout, so the property fix does not leak into the indexer path
    /// </summary>
    [TestMethod]
    public void MultiLineAutoAccessorIndexerWithTrailingCommentRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 int this[int index]
                                 {
                                     get; set;
                                 } // trailing
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}