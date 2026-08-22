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
    /// Verifies that a directive block that is the last content of a block still loses the blank line
    /// above it — a separate, pre-existing behavior of <see cref="Pipeline.BlankLines.Rewriter.BlankLineTokenCleanupRewriter"/>
    /// that issue #695 does not own or change, frozen here so a future change cannot alter it silently
    /// </summary>
    [TestMethod]
    public void TrailingDirectiveWithNoFollowingStatementStillLosesItsBlankLine()
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

    #endregion // Methods
}