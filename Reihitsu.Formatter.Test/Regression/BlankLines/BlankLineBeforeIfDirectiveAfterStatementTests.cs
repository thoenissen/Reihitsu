using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — the number of blank lines
/// separating a statement from a following preprocessor directive or disabled text (issue #695)
/// </summary>
[TestClass]
public class BlankLineBeforeIfDirectiveAfterStatementTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a single blank line already present between a statement and a following
    /// <c>#if</c> directive is preserved rather than doubled (issue #695)
    /// </summary>
    [TestMethod]
    public void SingleBlankLineBeforeIfDirectiveIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #if DEBUG
                                     bar = "foo2";
                             #endif

                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that two blank lines before an <c>#if</c> directive collapse to one when a single
    /// blank line already follows the directive block (issue #695)
    /// </summary>
    [TestMethod]
    public void ExcessBlankLinesBeforeIfDirectiveCollapseToOneWhenOneBlankLineFollows()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";


                             #if DEBUG
                                     bar = "foo2";
                             #endif

                                     bar = "foo2";
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";

                                #if DEBUG
                                        bar = "foo2";
                                #endif

                                        bar = "foo2";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two blank lines after an <c>#if</c> directive block collapse to one when a
    /// single blank line already precedes the directive (issue #695)
    /// </summary>
    [TestMethod]
    public void ExcessBlankLinesAfterIfDirectiveCollapseToOneWhenOneBlankLinePrecedes()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #if DEBUG
                                     bar = "foo2";
                             #endif


                                     bar = "foo2";
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";

                                #if DEBUG
                                        bar = "foo2";
                                #endif

                                        bar = "foo2";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that excess blank lines on both sides of an <c>#if</c> directive block each
    /// independently collapse to one (issue #695)
    /// </summary>
    [TestMethod]
    public void ExcessBlankLinesOnBothSidesOfIfDirectiveCollapseToOne()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";


                             #if DEBUG
                                     bar = "foo2";
                             #endif


                                     bar = "foo2";
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";

                                #if DEBUG
                                        bar = "foo2";
                                #endif

                                        bar = "foo2";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that no blank line before an <c>#if</c> directive is left alone when exactly one
    /// blank line follows the directive block (issue #695)
    /// </summary>
    [TestMethod]
    public void NoBlankLineBeforeIfDirectiveIsPreservedWhenOneBlankLineFollows()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";
                             #if DEBUG
                                     bar = "foo2";
                             #endif

                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that one blank line before an <c>#if</c> directive is left alone when no blank line
    /// follows the directive block (issue #695)
    /// </summary>
    [TestMethod]
    public void OneBlankLineBeforeIfDirectiveIsPreservedWhenNoneFollows()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #if DEBUG
                                     bar = "foo2";
                             #endif
                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a directive block with no blank line on either side is left alone (issue #695)
    /// </summary>
    [TestMethod]
    public void NoBlankLinesAroundIfDirectiveArePreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";
                             #if DEBUG
                                     bar = "foo2";
                             #endif
                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that excess blank lines before an <c>#if</c> directive still collapse to one when no
    /// blank line follows the directive block — already correct before issue #695's fix, and must not
    /// regress
    /// </summary>
    [TestMethod]
    public void ExcessBlankLinesBeforeIfDirectiveCollapseToOneWhenNoneFollow()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";


                             #if DEBUG
                                     bar = "foo2";
                             #endif
                                     bar = "foo2";
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";

                                #if DEBUG
                                        bar = "foo2";
                                #endif
                                        bar = "foo2";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that excess blank lines after an <c>#if</c> directive block still collapse to one
    /// when no blank line precedes the directive — already correct before issue #695's fix, and must
    /// not regress
    /// </summary>
    [TestMethod]
    public void ExcessBlankLinesAfterIfDirectiveCollapseToOneWhenNonePrecede()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";
                             #if DEBUG
                                     bar = "foo2";
                             #endif


                                     bar = "foo2";
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";
                                #if DEBUG
                                        bar = "foo2";
                                #endif

                                        bar = "foo2";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single blank line before an <c>#if</c> directive inside a switch section is
    /// preserved rather than doubled (issue #695)
    /// </summary>
    [TestMethod]
    public void SingleBlankLineBeforeIfDirectiveIsPreservedInSwitchSection()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             var bar = "foo1";

                             #if DEBUG
                                             bar = "foo2";
                             #endif

                                             bar = "foo2";
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that excess blank lines before an <c>#if</c> directive inside a switch section
    /// collapse to one (issue #695)
    /// </summary>
    [TestMethod]
    public void ExcessBlankLinesBeforeIfDirectiveCollapseToOneInSwitchSection()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start(int value)
                                 {
                                     switch (value)
                                     {
                                         case 1:
                                             var bar = "foo1";


                             #if DEBUG
                                             bar = "foo2";
                             #endif

                                             bar = "foo2";
                                             break;
                                     }
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start(int value)
                                    {
                                        switch (value)
                                        {
                                            case 1:
                                                var bar = "foo1";

                                #if DEBUG
                                                bar = "foo2";
                                #endif

                                                bar = "foo2";
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a single blank line before a <c>#pragma</c> directive is preserved rather than
    /// doubled — the mechanism is directive-kind agnostic, not specific to <c>#if</c> (issue #695)
    /// </summary>
    [TestMethod]
    public void SingleBlankLineBeforePragmaDirectiveIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #pragma warning disable CS0168

                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single blank line before a <c>#nullable</c> directive is preserved rather than
    /// doubled (issue #695)
    /// </summary>
    [TestMethod]
    public void SingleBlankLineBeforeNullableDirectiveIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #nullable enable

                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single blank line before a <c>#line</c> directive is preserved rather than
    /// doubled (issue #695)
    /// </summary>
    [TestMethod]
    public void SingleBlankLineBeforeLineDirectiveIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #line 42

                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a single blank line before a <c>#warning</c> directive is preserved rather than
    /// doubled (issue #695)
    /// </summary>
    [TestMethod]
    public void SingleBlankLineBeforeWarningDirectiveIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #warning Remove this before shipping

                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>#region</c>/<c>#endregion</c> block between two statements still receives
    /// its own required blank lines without any run holding two consecutive blank lines (issue #695)
    /// </summary>
    [TestMethod]
    public void RegionDirectiveBetweenStatementsHoldsNoDoubledBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                                     #region Detail
                                     bar = "foo2";
                                     #endregion

                                     bar = "foo2";
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";

                                        #region Detail

                                        bar = "foo2";

                                        #endregion // Detail

                                        bar = "foo2";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>#if</c> directive whose active branch supplies a real statement is
    /// unaffected — the active statement splits the gap into two one-blank-line gaps, so the doubling
    /// mechanism never applies (issue #695)
    /// </summary>
    [TestMethod]
    public void ActiveIfBranchStatementSeparatesGapAndIsUnaffected()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #if true
                                     bar = "foo2";
                             #endif

                                     bar = "foo3";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>#if</c> directive whose symbol is defined is unaffected in the same way as
    /// a literal <c>#if true</c> — the active branch again supplies a real statement (issue #695)
    /// </summary>
    [TestMethod]
    public void DefinedSymbolIfBranchStatementSeparatesGapAndIsUnaffected()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #if DEBUG
                                     bar = "foo2";
                             #endif

                                     bar = "foo3";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input, expected: null, parseOptions: CSharpParseOptions.Default.WithPreprocessorSymbols("DEBUG"));
    }

    /// <summary>
    /// Verifies that a directive block between two type members is unaffected — the changed rewriter
    /// only visits block and switch-section statement lists, not a member list (issue #695)
    /// </summary>
    [TestMethod]
    public void DirectiveBlockBetweenTypeMembersIsUnaffected()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 private int _a;

                             #if DEBUG
                                 private int _b;
                             #endif

                                 private int _c;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a directive block between top-level statements is unaffected — top-level
    /// statements live directly on the compilation unit, not inside a block (issue #695)
    /// </summary>
    [TestMethod]
    public void DirectiveBlockBetweenTopLevelStatementsIsUnaffected()
    {
        // Arrange
        const string input = """
                             var bar = "foo1";

                             #if DEBUG
                             bar = "foo2";
                             #endif

                             bar = "foo2";
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an empty statement adjacent to a directive-carrying gap keeps the pair exempt
    /// from the excess-blank-line correction, unaffected by issue #695's fix
    /// </summary>
    [TestMethod]
    public void EmptyStatementSiblingWithDirectiveGapIsUnaffected()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";
                                     ;

                             #if DEBUG
                                     bar = "foo2";
                             #endif

                                     bar = "foo2";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a directive block that is the last content of a block is
    /// preserved, matching the "blank lines are preserved unless there's a reason to change them"
    /// policy applied everywhere else in the formatter (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveWithNoFollowingStatementKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #if DEBUG
                                     bar = "foo2";
                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a directive that is the last content ahead of an opening
    /// brace is preserved (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeOpenBraceKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()

                             #pragma warning disable CA1801
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a directive that is the last content ahead of an <c>else</c>
    /// keyword is preserved (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeElseKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start(bool flag)
                                 {
                                     if (flag)
                                     {
                                     }

                             #pragma warning disable CA1801
                                     else
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a directive that is the last content ahead of a <c>catch</c>
    /// keyword is preserved (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeCatchKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     try
                                     {
                                     }

                             #pragma warning disable CA1801
                                     catch
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a directive that is the last content ahead of a
    /// <c>finally</c> keyword is preserved (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeFinallyKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     try
                                     {
                                     }

                             #pragma warning disable CA1801
                                     finally
                                     {
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a directive that is the last content ahead of a do-<c>while</c>
    /// footer is preserved (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeDoWhileFooterKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     do
                                     {
                                     }

                             #pragma warning disable CA1801
                                     while (true);
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before disabled text (an inactive <c>#if false</c> branch) that is
    /// the last content of a block is preserved (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDisabledTextWithNoFollowingStatementKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #if false
                                     bar = "foo2";
                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a <c>#nullable</c> directive that is the last content of a
    /// block is preserved (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingNullableDirectiveWithNoFollowingStatementKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #nullable disable
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that two blank lines before a trailing directive collapse to one rather than to zero
    /// (issue #711)
    /// </summary>
    [TestMethod]
    public void ExcessBlankLinesBeforeTrailingDirectiveCollapseToOne()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";


                             #if DEBUG
                                     bar = "foo2";
                             #endif
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";

                                #if DEBUG
                                        bar = "foo2";
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line directly above a closing brace, with no directive interposed, is
    /// still removed — the guard's non-exempt boundary, matching RH5024 (issue #711)
    /// </summary>
    [TestMethod]
    public void BlankLineDirectlyAboveClosingBraceWithNoDirectiveIsStillRemoved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line directly after an opening brace, ahead of a directive block that is
    /// the block's only content, is still removed — RH5022's region, untouched by this fix (issue #711)
    /// </summary>
    [TestMethod]
    public void BlankLineAfterOpenBraceBeforeDirectiveBlockIsStillRemoved()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {

                             #if DEBUG
                                     var bar = "foo1";
                             #endif
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                #if DEBUG
                                        var bar = "foo1";
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line at the very start of the file, ahead of a directive, is still
    /// removed — RH5028's region, untouched by this fix (issue #711)
    /// </summary>
    [TestMethod]
    public void BlankLineAtFileStartBeforeDirectiveIsStillRemoved()
    {
        // Arrange
        const string input = """

                             #pragma warning disable CA1801
                             var bar = "foo1";
                             """;

        const string expected = """
                                #pragma warning disable CA1801
                                var bar = "foo1";
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that when a blank line precedes a trailing directive and another blank line follows it
    /// directly above the closing brace, the first is preserved and the second is removed — the two
    /// owners of this fix now agree with each other and with RH5024 (issue #711)
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeAndAfterTrailingDirectiveKeepsOnlyTheFirst()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";

                             #if DEBUG
                                     bar = "foo2";
                             #endif

                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";

                                #if DEBUG
                                        bar = "foo2";
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that when no blank line precedes a trailing directive but one follows it directly
    /// above the closing brace, neither blank line is written — no unauthored insertion, and the
    /// RH5024-flagged blank line is removed (issue #711)
    /// </summary>
    [TestMethod]
    public void BlankLineOnlyAfterTrailingDirectiveWritesNeitherBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var bar = "foo1";
                             #if DEBUG
                                     bar = "foo2";
                             #endif

                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var bar = "foo1";
                                #if DEBUG
                                        bar = "foo2";
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line before a trailing directive ahead of a type declaration's closing
    /// brace is preserved — the same guard reached through a different owning rewriter (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeTypeDeclarationClosingBraceKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 private int _a;

                             #pragma warning restore CA1802
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a trailing directive ahead of an enum's closing brace is
    /// preserved — the trailing comma on the last member is a separate, pre-existing structural
    /// decision this fix does not own (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeEnumClosingBraceKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public enum Color
                             {
                                 Red,
                                 Green,

                             #pragma warning restore CA1008
                             }
                             """;

        const string expected = """
                                public enum Color
                                {
                                    Red,
                                    Green

                                #pragma warning restore CA1008
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line before a trailing directive ahead of a property accessor list's
    /// closing brace is preserved (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforePropertyAccessorListClosingBraceKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public int Value
                                 {
                                     get
                                     {
                                         return _value;
                                     }

                             #pragma warning restore CA1822
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a trailing directive ahead of an object initializer's closing
    /// brace is preserved (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeInitializerClosingBraceKeepsItsBlankLine()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     var data = new Data
                                     {
                                         Value = 1

                             #pragma warning restore CA1861
                                     };
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        var data = new Data
                                                   {
                                                       Value = 1

                                #pragma warning restore CA1861
                                                   };
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line before a trailing directive ahead of a closing bracket is left
    /// unchanged — a closing bracket is not one of this fix's anchor tokens (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeCloseBracketIsUnaffected()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     int[] values =
                                     [
                                         1,
                                         2

                             #pragma warning restore CA1861
                                     ];
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        int[] values = [
                                                           1,
                                                           2

                                #pragma warning restore CA1861
                                                       ];
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line before a trailing directive ahead of a closing parenthesis is left
    /// unchanged — a closing parenthesis is not one of this fix's anchor tokens (issue #711)
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveBeforeCloseParenIsUnaffected()
    {
        // Arrange
        const string input = """
                             public class Implementation
                             {
                                 public void Start()
                                 {
                                     Method(
                                         1,
                                         2

                             #pragma warning restore CA1861
                                     );
                                 }

                                 private static void Method(int a, int b)
                                 {
                                 }
                             }
                             """;

        const string expected = """
                                public class Implementation
                                {
                                    public void Start()
                                    {
                                        Method(1,
                                               2

                                #pragma warning restore CA1861
                                               );
                                    }

                                    private static void Method(int a, int b)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}