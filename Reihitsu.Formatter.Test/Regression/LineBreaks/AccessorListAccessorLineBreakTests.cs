using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.LineBreaks;

/// <summary>
/// Every accessor of an accessor list that the line-break phase lays out over several lines starts its own line,
/// regardless of whether the input placed several accessors on one line. Comments and directives keep their
/// position relative to the code, and gaps that already contain a line break are left alone
/// </summary>
[TestClass]
public class AccessorListAccessorLineBreakTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that two expression-bodied accessors written on one line each start their own line once the
    /// accessor list is laid out over several lines
    /// </summary>
    [TestMethod]
    public void ExpressionBodiedAccessorsInSingleLineListStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get => _x; set => _x = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that block accessors converted to expression bodies in the same pass each start their own line
    /// </summary>
    [TestMethod]
    public void ConvertedBlockAccessorsStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get { return _x; } set { _x = value; } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block accessor following another block accessor on the same line starts its own line,
    /// without a blank line between the closing brace and the next accessor
    /// </summary>
    [TestMethod]
    public void MultiStatementBlockAccessorsStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get { var a = _x; return a; } set { _x = value; Changed(); } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get
                                        {
                                            var a = _x;

                                            return a;
                                        }
                                        set
                                        {
                                            _x = value;
                                            Changed();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an accessor written directly after the previous accessor's closing brace starts its own line
    /// </summary>
    [TestMethod]
    public void AccessorFollowingClosingBraceStartsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get
                                     {
                                         var a = _x;

                                         return a;
                                     } set
                                     {
                                         _x = value;
                                         Changed();
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get
                                        {
                                            var a = _x;

                                            return a;
                                        }
                                        set
                                        {
                                            _x = value;
                                            Changed();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block accessor following an expression-bodied accessor on the same line starts its own line
    /// </summary>
    [TestMethod]
    public void BlockAccessorFollowingExpressionBodiedAccessorStartsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get => _x; set { _x = value; Changed(); } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        set
                                        {
                                            _x = value;
                                            Changed();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that accessors sharing a line inside an accessor list that already spans several lines each start
    /// their own line
    /// </summary>
    [TestMethod]
    public void AccessorsSharingLineInMultiLineListStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get => _x; set => _x = value;
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that accessors sharing a line start their own lines when the opening brace was written on the
    /// signature line
    /// </summary>
    [TestMethod]
    public void AccessorsSharingLineAfterOpeningBraceOnSignatureLineStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X {
                                     get => _x; set => _x = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the accessors of an indexer written on one line each start their own line
    /// </summary>
    [TestMethod]
    public void IndexerAccessorsStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _a = new int[1];

                                 public int this[int i] { get => _a[i]; set => _a[i] = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private readonly int[] _a = new int[1];

                                    public int this[int i]
                                    {
                                        get => _a[i];
                                        set => _a[i] = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that indexer block accessors converted to expression bodies in the same pass each start their own line
    /// </summary>
    [TestMethod]
    public void ConvertedIndexerBlockAccessorsStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private readonly int[] _a = new int[1];

                                 public int this[int i] { get { return _a[i]; } set { _a[i] = value; } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private readonly int[] _a = new int[1];

                                    public int this[int i]
                                    {
                                        get => _a[i];
                                        set => _a[i] = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that expression-bodied event accessors written on one line each start their own line
    /// </summary>
    [TestMethod]
    public void EventAccessorsStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private System.EventHandler _e;

                                 public event System.EventHandler Changed { add => _e += value; remove => _e -= value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private System.EventHandler _e;

                                    public event System.EventHandler Changed
                                    {
                                        add => _e += value;
                                        remove => _e -= value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an event block accessor following another block accessor on the same line starts its own line
    /// </summary>
    [TestMethod]
    public void EventBlockAccessorsStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private System.EventHandler _e;

                                 public event System.EventHandler Changed { add { _e += value; } remove { _e -= value; } }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private System.EventHandler _e;

                                    public event System.EventHandler Changed
                                    {
                                        add
                                        {
                                            _e += value;
                                        }
                                        remove
                                        {
                                            _e -= value;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an accessor modifier moves together with its accessor, so the line starts at the modifier
    /// </summary>
    [TestMethod]
    public void AccessorModifierStartsTheAccessorLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get => _x; private set => _x = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        private set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an init accessor following another accessor on the same line starts its own line
    /// </summary>
    [TestMethod]
    public void InitAccessorStartsOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get => _x; init => _x = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        init => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the attribute list of the second accessor moves together with its accessor, instead of being left
    /// behind on the previous accessor's line
    /// </summary>
    [TestMethod]
    public void AttributeOnSecondAccessorStartsTheAccessorLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get => _x; [System.Obsolete] set => _x = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        [System.Obsolete]
                                        set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an attribute on the first accessor does not keep the following accessor on the first accessor's line
    /// </summary>
    [TestMethod]
    public void AttributeOnFirstAccessorKeepsFollowingAccessorOnOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { [System.Obsolete] get => _x; set => _x = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        [System.Obsolete]
                                        get => _x;
                                        set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment between two accessors on one line stays on the previous accessor's line, while the
    /// following accessor starts its own line
    /// </summary>
    [TestMethod]
    public void SameLineBlockCommentStaysWithPreviousAccessor()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get => _x; /* c */ set => _x = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x; /* c */
                                        set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment inside the following accessor stays in place when that accessor moves to its own line
    /// </summary>
    [TestMethod]
    public void CommentInsideFollowingAccessorIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get => _x; set /* c */ => _x = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        set /* c */ => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a comment trailing the last accessor stays on that accessor's line
    /// </summary>
    [TestMethod]
    public void CommentTrailingLastAccessorStaysOnItsLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get => _x; set => _x = value; /* c */ }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x;
                                        set => _x = value; /* c */
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an accessor following a multi-line expression-bodied accessor starts its own line, while the
    /// continuation line keeps its alignment
    /// </summary>
    [TestMethod]
    public void MultiLineAccessorIsFollowedByAccessorOnOwnLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X { get => _x
                                     + 1; set => _x = value; }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                        get => _x
                                               + 1;
                                        set => _x = value;
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that accessors sharing a line inside an active conditional branch each start their own line, while the
    /// directives stay in place
    /// </summary>
    [TestMethod]
    public void AccessorsSharingLineInActiveConditionalBranchStartOwnLines()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                 #if true
                                     get => _x; set => _x = value;
                                 #endif
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _x;

                                    public int X
                                    {
                                    #if true
                                        get => _x;
                                        set => _x = value;
                                    #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that accessors already separated by a single-line comment remain unchanged
    /// </summary>
    [TestMethod]
    public void AccessorsSeparatedByLineCommentRemainUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get => _x; // getter
                                     set => _x = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an accessor following a multi-line block comment that ends mid-line remains unchanged, because the
    /// gap already contains a line break
    /// </summary>
    [TestMethod]
    public void AccessorAfterMultiLineCommentEndingMidLineRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get => _x; /* first
                                     second */ set => _x = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a laid-out accessor list with a single accessor remains unchanged
    /// </summary>
    [TestMethod]
    public void SingleAccessorListRemainsUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get => _x;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that accessors already on separate lines keep the blank line between them
    /// </summary>
    [TestMethod]
    public void AccessorsSeparatedByBlankLineRemainUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _x;

                                 public int X
                                 {
                                     get => _x;

                                     set => _x = value;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}