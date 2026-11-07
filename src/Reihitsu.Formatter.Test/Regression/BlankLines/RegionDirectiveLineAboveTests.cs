using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank line directly above an active region directive: only the single line
/// directly above the directive decides, so a conditional directive between an opening brace and the region
/// directive, a comment or directive above a region directive at the start of a file, and a region directive
/// that is not the first one of its kind in its gap all get the blank line
/// </summary>
[TestClass]
public class RegionDirectiveLineAboveTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#if</c> directive directly after the opening brace of a type
    /// and the region directive below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowIfDirectiveAfterTypeBraceGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if !DEBUG
                                 #region A

                                 void M()
                                 {
                                 }

                                 #endregion // A

                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if !DEBUG

                                    #region A

                                    void M()
                                    {
                                    }

                                    #endregion // A

                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line written between an <c>#if</c> directive directly after the opening brace of a type
    /// and the region directive below it is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void BlankLineBetweenIfDirectiveAfterTypeBraceAndRegionIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if !DEBUG

                                 #region A

                                 void M()
                                 {
                                 }

                                 #endregion // A

                             #endif
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#else</c> directive and the region directive below it when
    /// only conditional directives and disabled text stand between the opening brace and the region directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowElseDirectiveAfterTypeBraceGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if DEBUG
                                 int _a;
                             #else
                                 #region A

                                 int _b;

                                 #endregion // A

                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if DEBUG
                                    int _a;
                                #else

                                    #region A

                                    int _b;

                                    #endregion // A

                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#elif</c> directive and the region directive below it when
    /// only conditional directives and disabled text stand between the opening brace and the region directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowElifDirectiveAfterTypeBraceGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if DEBUG
                                 int _a;
                             #elif !DEBUG
                                 #region A

                                 int _b;

                                 #endregion // A

                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if DEBUG
                                    int _a;
                                #elif !DEBUG

                                    #region A

                                    int _b;

                                    #endregion // A

                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#endif</c> directive and the region directive below it when
    /// only conditional directives and disabled text stand between the opening brace and the region directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowEndIfDirectiveAfterTypeBraceGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if DEBUG
                                 int _a;
                             #endif
                                 #region A

                                 int _b;

                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if DEBUG
                                    int _a;
                                #endif

                                    #region A

                                    int _b;

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#if</c> directive directly after the opening brace of a
    /// method body and the region directive below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowIfDirectiveAfterMethodBodyBraceGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                             #if !DEBUG
                                     #region A

                                     M();

                                     #endregion // A

                             #endif
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                #if !DEBUG

                                        #region A

                                        M();

                                        #endregion // A

                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#if</c> directive directly after the opening brace of a
    /// block-scoped namespace and the region directive below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowIfDirectiveAfterNamespaceBraceGetsBlankLine()
    {
        // Arrange
        const string input = """
                             namespace N
                             {
                             #if !DEBUG
                                 #region A

                                 delegate void D();

                                 #endregion // A

                             #endif
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                #if !DEBUG

                                    #region A

                                    delegate void D();

                                    #endregion // A

                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#if</c> directive directly after the opening brace of a
    /// switch block and the region directive below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowIfDirectiveAfterSwitchBlockBraceGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     switch (a)
                                     {
                             #if !DEBUG
                                         #region A

                                         case 1:
                                             break;

                                         #endregion // A

                             #endif
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertBlankLinesAbove(input, ("#if !DEBUG", false), ("#region A", true));
    }

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#if</c> directive directly after the opening brace of an
    /// accessor list and the region directive below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowIfDirectiveAfterAccessorListBraceGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int P
                                 {
                             #if !DEBUG
                                     #region A

                                     get
                                     {
                                         return 0;
                                     }

                                     #endregion // A

                             #endif
                                 }
                             }
                             """;

        // Act & Assert
        AssertBlankLinesAbove(input, ("#if !DEBUG", false), ("#region A", true));
    }

    /// <summary>
    /// Verifies that a blank line is inserted between a comment on the first line of a file and the region directive
    /// below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowHeaderCommentAtFileStartGetsBlankLine()
    {
        // Arrange
        const string input = """
                             // header
                             #region A

                             class C
                             {
                                 int _a;
                             }

                             #endregion // A
                             """;
        const string expected = """
                                // header

                                #region A

                                class C
                                {
                                    int _a;
                                }

                                #endregion // A
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#if</c> directive on the first line of a file and the region
    /// directive below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowIfDirectiveAtFileStartGetsBlankLine()
    {
        // Arrange
        const string input = """
                             #if !DEBUG
                             #region A

                             class C
                             {
                                 int _a;
                             }

                             #endregion // A

                             #endif
                             """;
        const string expected = """
                                #if !DEBUG

                                #region A

                                class C
                                {
                                    int _a;
                                }

                                #endregion // A

                                #endif
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between a <c>#nullable</c> directive on the first line of a file and the
    /// region directive below it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionBelowNullableDirectiveAtFileStartGetsBlankLine()
    {
        // Arrange
        const string input = """
                             #nullable enable
                             #region A

                             class C
                             {
                                 int _a;
                             }

                             #endregion // A
                             """;
        const string expected = """
                                #nullable enable

                                #region A

                                class C
                                {
                                    int _a;
                                }

                                #endregion // A
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region directive on the first line of a file gets no blank line above it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void RegionOnFirstLineOfFileIsUnchanged()
    {
        // Arrange
        const string input = """
                             #region A

                             class C
                             {
                                 int _a;
                             }

                             #endregion // A
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between a comment and a region directive below it when an earlier region
    /// directive stands in the same gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void SecondRegionBelowCommentInSameGapGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _a;

                                 #region A

                                 // c
                                 #region B

                                 int _b;

                                 #endregion // B

                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int _a;

                                    #region A

                                    // c

                                    #region B

                                    int _b;

                                    #endregion // B

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between a <c>#pragma</c> directive and a region directive below it when an
    /// earlier region directive stands in the same gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void SecondRegionBelowPragmaInSameGapGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _a;

                                 #region A

                             #pragma warning disable
                                 #region B

                                 int _b;

                                 #endregion // B

                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int _a;

                                    #region A

                                #pragma warning disable

                                    #region B

                                    int _b;

                                    #endregion // B

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between an <c>#if</c> directive and a region directive below it when an
    /// earlier region directive stands in the same gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void SecondRegionBelowIfDirectiveInSameGapGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _a;

                                 #region A

                             #if !DEBUG
                                 #region B

                                 int _b;

                                 #endregion // B

                             #endif

                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int _a;

                                    #region A

                                #if !DEBUG

                                    #region B

                                    int _b;

                                    #endregion // B

                                #endif

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is still inserted between an <c>#if</c> directive and the first region directive of a gap
    /// behind a member, where no opening brace precedes the gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void FirstRegionBelowIfDirectiveBehindMemberGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int _a;

                             #if !DEBUG
                                 #region A

                                 int _b;

                                 #endregion // A

                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int _a;

                                #if !DEBUG

                                    #region A

                                    int _b;

                                    #endregion // A

                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between a comment and an end-region directive below it when an earlier
    /// end-region directive stands in the same gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void SecondEndRegionBelowCommentInSameGapGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region A

                                 #region B

                                 int _b;

                                 #endregion // B

                                 // c
                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region A

                                    #region B

                                    int _b;

                                    #endregion // B

                                    // c

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line is inserted between a <c>#pragma</c> directive and an end-region directive below it when
    /// an earlier end-region directive stands in the same gap
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-013")]
    [Requirement("SHARED-BLANK-005")]
    public void SecondEndRegionBelowPragmaInSameGapGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region A

                                 #region B

                                 int _b;

                                 #endregion // B

                             #pragma warning disable
                                 #endregion // A
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region A

                                    #region B

                                    int _b;

                                    #endregion // B

                                #pragma warning disable

                                    #endregion // A
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the formatted output keeps a line holding nothing but whitespace directly above a region directive as the
    /// one blank line in front of it, with its whitespace removed. The phase collapses a run of blank lines to one, so this
    /// output cannot show whether the reader took that line for the required blank line;
    /// <c>BlankLineRegionDirectiveRewriterTests</c> pins that
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    [Requirement("SHARED-BLANK-005")]
    [Requirement("FMT-BLANK-013")]
    public void WhitespaceOnlyLineAboveRegionIsKeptAsOneEmptyLine()
    {
        // Arrange
        const string inputTemplate = """
                                     namespace N;

                                     internal class C
                                     {
                                         private int _a;
                                     {WHITESPACE}
                                         #region Methods

                                         public void M()
                                         {
                                         }

                                         #endregion // Methods
                                     }
                                     """;
        const string expected = """
                                namespace N;

                                internal class C
                                {
                                    private int _a;

                                    #region Methods

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;
        var input = inputTemplate.Replace("{WHITESPACE}", "    ");

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the formatted output keeps a line holding nothing but whitespace directly above an end-region directive
    /// as the one blank line in front of it, with its whitespace removed. The phase collapses a run of blank lines to one, so
    /// this output cannot show whether the reader took that line for the required blank line;
    /// <c>BlankLineTriviaBoundaryRewriterTests</c> pins that
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    [Requirement("SHARED-BLANK-005")]
    [Requirement("FMT-BLANK-013")]
    public void WhitespaceOnlyLineAboveEndRegionIsKeptAsOneEmptyLine()
    {
        // Arrange
        const string inputTemplate = """
                                     internal class C
                                     {
                                         #region Methods

                                         public void M()
                                         {
                                         }
                                     {WHITESPACE}
                                         #endregion // Methods

                                         private int _a;
                                     }
                                     """;
        const string expected = """
                                internal class C
                                {
                                    #region Methods

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods

                                    private int _a;
                                }
                                """;
        var input = inputTemplate.Replace("{WHITESPACE}", "    ");

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the formatted output keeps a line holding nothing but whitespace between a comment and the region
    /// directive below it as the one blank line between them, with its whitespace removed. The phase collapses a run of
    /// blank lines to one, so this output cannot show whether the reader took that line for the required blank line;
    /// <c>BlankLineRegionDirectiveRewriterTests</c> pins that
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    [Requirement("SHARED-BLANK-005")]
    [Requirement("FMT-BLANK-013")]
    public void WhitespaceOnlyLineBetweenCommentAndRegionIsKeptAsOneEmptyLine()
    {
        // Arrange
        const string inputTemplate = """
                                     internal class C
                                     {
                                         private int _a;

                                         // Methods
                                     {WHITESPACE}
                                         #region Methods

                                         public void M()
                                         {
                                         }

                                         #endregion // Methods
                                     }
                                     """;
        const string expected = """
                                internal class C
                                {
                                    private int _a;

                                    // Methods

                                    #region Methods

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;
        var input = inputTemplate.Replace("{WHITESPACE}", "    ");

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the formatted output keeps a line holding nothing but whitespace directly above an end-region directive
    /// in front of a closing brace as the one blank line in front of it, with its whitespace removed. The phase collapses a
    /// run of blank lines to one, so this output cannot show which part of the phase decided that line
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    [Requirement("SHARED-BLANK-005")]
    [Requirement("FMT-BLANK-013")]
    public void WhitespaceOnlyLineAboveEndRegionBeforeClosingBraceIsKeptAsOneEmptyLine()
    {
        // Arrange
        const string inputTemplate = """
                                     internal class C
                                     {
                                         #region Methods

                                         public void M()
                                         {
                                         }
                                     {WHITESPACE}
                                         #endregion // Methods
                                     }
                                     """;
        const string expected = """
                                internal class C
                                {
                                    #region Methods

                                    public void M()
                                    {
                                    }

                                    #endregion // Methods
                                }
                                """;
        var input = inputTemplate.Replace("{WHITESPACE}", "    ");

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}