using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Regions;

/// <summary>
/// Full-pipeline tests for the form of a <c>#region</c> line with a description — <c>#region</c>, one space, and the trimmed
/// description — for the whitespace around the text of an <c>#endregion</c> whose <c>#region</c> has no description, and
/// for the whitespace between <c>#</c> and the keyword of every paired region line
/// </summary>
[TestClass]
public class RegionLineFormTests : FormatterTestsBase
{
    #region Methods

    #region Region line with a description

    /// <summary>
    /// Verifies the spec example: several spaces in front of an upper-case description become one space, and whitespace behind
    /// a description that already follows one space is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void SeveralSpacesAndTrailingWhitespaceOfRegionLinesAreNormalized()
    {
        // Arrange
        const string input = "class C\n{\n    #region    Bar\n    int a;\n    #endregion    Bar\n\n    #region Baz   \n    int b;\n    #endregion // Baz\n}";
        const string expected = """
                                class C
                                {
                                    #region Bar

                                    int a;

                                    #endregion // Bar

                                    #region Baz

                                    int b;

                                    #endregion // Baz
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a <c>#region</c> line that already has the form is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void RegionLineInCanonicalFormIsLeftUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region Bar

                                 int a;

                                 #endregion // Bar
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: a <c>#region</c> line in a branch the build does not compile gets the same form
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void SeveralSpacesInRegionLineOfDisabledBranchBecomeOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if false
                                 #region    Bar
                                 int a;
                                 #endregion // Bar
                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if false
                                    #region Bar
                                    int a;
                                    #endregion // Bar
                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace behind the description of a <c>#region</c> in a branch the build does not compile is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void TrailingWhitespaceOfRegionLineInDisabledBranchIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n#if false\n    #region Bar   \n    int a;\n    #endregion // Bar\n#endif\n}";
        const string expected = "class C\n{\n#if false\n    #region Bar\n    int a;\n    #endregion // Bar\n#endif\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace behind an upper-case description that follows exactly one space is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void TrailingWhitespaceBehindDescriptionAfterOneSpaceIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    #region Baz   \n\n    int b;\n\n    #endregion // Baz\n}";
        const string expected = "class C\n{\n    #region Baz\n\n    int b;\n\n    #endregion // Baz\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several spaces in front of an upper-case description become one space while a correct <c>#endregion</c>
    /// is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void SeveralSpacesBeforeUpperCaseDescriptionBecomeOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region    Bar

                                 int a;

                                 #endregion // Bar
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Bar

                                    int a;

                                    #endregion // Bar
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several spaces and whitespace behind a lower-case description give the same form as for an upper-case
    /// one, with the first letter upper-cased
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-001")]
    [Requirement("FMT-REGION-007")]
    public void SeveralSpacesAndTrailingWhitespaceAroundLowerCaseDescriptionAreNormalized()
    {
        // Arrange
        const string input = "class C\n{\n    #region    bar   \n\n    int a;\n\n    #endregion\n}";
        const string expected = "class C\n{\n    #region Bar\n\n    int a;\n\n    #endregion // Bar\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several spaces in front of a description that starts with a digit become one space, and the digit is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void SeveralSpacesBeforeDigitLedDescriptionBecomeOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region    1st fields

                                 int a;

                                 #endregion // 1st fields
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region 1st fields

                                    int a;

                                    #endregion // 1st fields
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several spaces in front of a description that starts with an underscore become one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void SeveralSpacesBeforeUnderscoreLedDescriptionBecomeOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region   _fields

                                 int a;

                                 #endregion // _fields
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region _fields

                                    int a;

                                    #endregion // _fields
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace inside a description is kept while the separator in front of it becomes one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void WhitespaceInsideDescriptionIsKept()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region    Fields   and   properties

                                 int a;

                                 #endregion // Fields   and   properties
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Fields   and   properties

                                    int a;

                                    #endregion // Fields   and   properties
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab mixed with spaces between <c>#region</c> and its description becomes one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void TabMixedWithSpacesBeforeDescriptionBecomesOneSpace()
    {
        // Arrange
        const string input = "class C\n{\n    #region \t Bar\n\n    int a;\n\n    #endregion // Bar\n}";
        const string expected = "class C\n{\n    #region Bar\n\n    int a;\n\n    #endregion // Bar\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several spaces in front of the description of a region inside a method body become one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void SeveralSpacesInRegionLineInsideMethodBodyBecomeOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     #region    Setup

                                     var x = 1;

                                     #endregion // Setup
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        #region Setup

                                        var x = 1;

                                        #endregion // Setup
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the <c>#region</c> lines of a nested pair each get the form, and each <c>#endregion</c> repeats its own
    /// description
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void SeveralSpacesInRegionLinesOfNestedPairBecomeOneSpace()
    {
        // Arrange
        const string input = "class C\n{\n    #region    Outer\n\n    #region  Inner  \n\n    int a;\n\n    #endregion\n\n    #endregion\n}";
        const string expected = """
                                class C
                                {
                                    #region Outer

                                    #region Inner

                                    int a;

                                    #endregion // Inner

                                    #endregion // Outer
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a region opened among the members and closed inside a method body gets the form and keeps both directives
    /// in place
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-005")]
    [Requirement("FMT-REGION-007")]
    public void SeveralSpacesInRegionLineOfStraddlingPairBecomeOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region    Members

                                 void M()
                                 {
                                     var x = 1;

                                     #endregion
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Members

                                    void M()
                                    {
                                        var x = 1;

                                        #endregion // Members
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that region-looking text inside a string literal is kept as written while the region directive around it gets
    /// the form
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void RegionLookingTextInsideStringLiteralIsKept()
    {
        // Arrange
        const string input = "class C\n{\n    #region    Bar\n\n    string a = \"#region    Baz   \";\n    string b = @\"\n#endregion    // Stale   \n\";\n\n    #endregion // Bar\n}";
        const string expected = "class C\n{\n    #region Bar\n\n    string a = \"#region    Baz   \";\n    string b = @\"\n#endregion    // Stale   \n\";\n\n    #endregion // Bar\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Region line with a description

    #region Region without a description

    /// <summary>
    /// Verifies the spec example: several spaces in front of the text of an <c>#endregion</c> whose <c>#region</c> has no
    /// description become one space, and whitespace behind that text is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void WhitespaceAroundTextOfEndRegionOfUndescribedRegionIsNormalized()
    {
        // Arrange
        const string input = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion    // Stale   \n}";
        const string expected = """
                                class C
                                {
                                    #region

                                    int _value;

                                    #endregion // Stale
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: text directly behind the <c>#endregion</c> keyword of an undescribed region is kept and gets
    /// one space in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TextDirectlyBehindEndRegionOfUndescribedRegionGetsOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region

                                 int _value;

                                 #endregion//Stale
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region

                                    int _value;

                                    #endregion //Stale
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that several spaces in front of the text of an <c>#endregion</c> of an undescribed region become one space,
    /// with nothing behind the text
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void SeveralSpacesBeforeTextOfEndRegionOfUndescribedRegionBecomeOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region

                                 int _value;

                                 #endregion    // Stale
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region

                                    int _value;

                                    #endregion // Stale
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace behind the text of an <c>#endregion</c> of an undescribed region that already follows one
    /// space is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TrailingWhitespaceBehindTextOfEndRegionOfUndescribedRegionIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion // Stale   \n}";
        const string expected = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion // Stale\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab in front of the text of an <c>#endregion</c> of an undescribed region becomes one space
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TabBeforeTextOfEndRegionOfUndescribedRegionBecomesOneSpace()
    {
        // Arrange
        const string input = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion\t// Stale\n}";
        const string expected = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion // Stale\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a block comment behind the <c>#endregion</c> of an undescribed region is kept as written, with one space
    /// in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void BlockCommentBehindEndRegionOfUndescribedRegionIsKeptWithOneSpace()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region

                                 int _value;

                                 #endregion   /* Stale */
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region

                                    int _value;

                                    #endregion /* Stale */
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a bare <c>#endregion</c> of an undescribed region stays bare
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void BareEndRegionOfUndescribedRegionStaysBare()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region

                                 int _value;

                                 #endregion
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example: a <c>#region</c> without a description and a bare <c>#endregion</c> lose the whitespace
    /// behind their keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TrailingWhitespaceBehindBareRegionAndEndRegionIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    #region   \n\n    int _value;\n\n    #endregion   \n}";
        const string expected = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>#region</c> without a description loses the whitespace behind its keyword when its <c>#endregion</c>
    /// is already bare
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TrailingWhitespaceBehindRegionWithoutDescriptionIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    #region   \n\n    int _value;\n\n    #endregion\n}";
        const string expected = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a bare <c>#endregion</c> of an undescribed region loses the whitespace behind its keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TrailingWhitespaceBehindBareEndRegionIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion   \n}";
        const string expected = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab behind a bare <c>#endregion</c>, which the preparation phase turns into spaces, is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TabBehindBareEndRegionIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion\t\n}";
        const string expected = "class C\n{\n    #region\n\n    int _value;\n\n    #endregion\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example for a branch the build does not compile: the <c>#region</c> without a description loses its
    /// trailing whitespace and the text of the <c>#endregion</c> is normalized
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TrailingWhitespaceBehindRegionWithoutDescriptionInDisabledBranchIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n#if false\n    #region   \n    int a;\n    #endregion    // Stale   \n#endif\n}";
        const string expected = "class C\n{\n#if false\n    #region\n    int a;\n    #endregion // Stale\n#endif\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a bare <c>#endregion</c> in a branch the build does not compile loses the whitespace behind its keyword
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void TrailingWhitespaceBehindBareEndRegionInDisabledBranchIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n#if false\n    #region\n    int a;\n    #endregion   \n#endif\n}";
        const string expected = "class C\n{\n#if false\n    #region\n    int a;\n    #endregion\n#endif\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the text of an <c>#endregion</c> of an undescribed region in a branch the build does not compile gets the
    /// same normalization
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void WhitespaceAroundTextOfEndRegionOfUndescribedRegionInDisabledBranchIsNormalized()
    {
        // Arrange
        const string input = "class C\n{\n#if false\n    #region\n    int a;\n    #endregion    // Stale   \n#endif\n}";
        const string expected = "class C\n{\n#if false\n    #region\n    int a;\n    #endregion // Stale\n#endif\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Region without a description

    #region Whitespace between the number sign and the keyword

    /// <summary>
    /// Verifies the spec example: whitespace between <c>#</c> and the keyword is removed on a <c>#region</c> line whose
    /// description changes and on its <c>#endregion</c>
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    [Requirement("FMT-REGION-002")]
    public void WhitespaceAfterNumberSignOfDescribedRegionPairIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 # region fields

                                 int _a;

                                 #  endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Fields

                                    int _a;

                                    #endregion // Fields
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace between <c>#</c> and the keyword is removed on a described region pair that is canonical
    /// apart from that whitespace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    [Requirement("FMT-REGION-002")]
    public void WhitespaceAfterNumberSignIsRemovedWhenNothingElseChanges()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 # region Fields

                                 int _a;

                                 #   endregion // Fields
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Fields

                                    int _a;

                                    #endregion // Fields
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: a tab between <c>#</c> and <c>region</c> is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    public void TabAfterNumberSignOfRegionLineIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    #\tregion Fields\n\n    int _a;\n\n    #endregion // Fields\n}";
        const string expected = "class C\n{\n    #region Fields\n\n    int _a;\n\n    #endregion // Fields\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a tab between <c>#</c> and <c>endregion</c> of a described region is removed
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-002")]
    public void TabAfterNumberSignOfEndRegionLineIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n    #region Fields\n\n    int _a;\n\n    #\tendregion // Fields\n}";
        const string expected = "class C\n{\n    #region Fields\n\n    int _a;\n\n    #endregion // Fields\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: whitespace between <c>#</c> and the keyword is removed in a branch the build does not
    /// compile
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    [Requirement("FMT-REGION-002")]
    public void WhitespaceAfterNumberSignOfRegionPairInDisabledBranchIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if false
                                 # region fields
                                 int a;
                                 #  endregion
                             #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if false
                                    #region Fields
                                    int a;
                                    #endregion // Fields
                                #endif
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: whitespace between <c>#</c> and the keyword is removed on an undescribed <c>#region</c>
    /// and on its <c>#endregion</c>, whose text keeps one space in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void WhitespaceAfterNumberSignOfUndescribedRegionPairWithEndRegionTextIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 # region

                                 int _value;

                                 #  endregion  note
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region

                                    int _value;

                                    #endregion note
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace between <c>#</c> and the keyword is removed on a bare undescribed region pair
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void WhitespaceAfterNumberSignOfBareRegionPairIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 # region

                                 int _value;

                                 #  endregion
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region

                                    int _value;

                                    #endregion
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace between <c>#</c> and the keyword is removed on an undescribed region pair in a branch the
    /// build does not compile
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-004")]
    public void WhitespaceAfterNumberSignOfUndescribedRegionPairInDisabledBranchIsRemoved()
    {
        // Arrange
        const string input = "class C\n{\n#if false\n    # region\n    int a;\n    #  endregion note\n#endif\n}";
        const string expected = "class C\n{\n#if false\n    #region\n    int a;\n    #endregion note\n#endif\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that whitespace between <c>#</c> and the keyword is removed on nested region pairs
    /// </summary>
    [TestMethod]
    [Requirement("FMT-REGION-007")]
    [Requirement("FMT-REGION-002")]
    public void WhitespaceAfterNumberSignOfNestedRegionPairsIsRemoved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 # region Methods

                                 #  region helpers

                                 void M()
                                 {
                                 }

                                 # endregion

                                 #   endregion // Methods
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Methods

                                    #region Helpers

                                    void M()
                                    {
                                    }

                                    #endregion // Helpers

                                    #endregion // Methods
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Whitespace between the number sign and the keyword

    #endregion // Methods
}