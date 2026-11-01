using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Full-pipeline tests for the blank lines of a gap before a closing brace that a non-region directive or disabled text
/// starts and a region directive ends
/// </summary>
[TestClass]
public class DirectiveBeforeEndRegionBlankLineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies the spec example: no blank line is inserted in front of the first directive of a gap that a region
    /// directive ends; the blank line between the directive and the region directive is kept in a type body and inserted
    /// in a method body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void InsertsNoBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGap()
    {
        const string input = """
                             class C
                             {
                                 #region Fields

                             #pragma warning disable CS0169
                                 int x;
                             #pragma warning restore CS0169

                                 #endregion // Fields
                             }

                             class D
                             {
                                 void M(int a)
                                 {
                                     #region R

                                     a++;
                             #pragma warning disable CS0168
                                     #endregion // R
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Fields

                                #pragma warning disable CS0169
                                    int x;
                                #pragma warning restore CS0169

                                    #endregion // Fields
                                }

                                class D
                                {
                                    void M(int a)
                                    {
                                        #region R

                                        a++;
                                #pragma warning disable CS0168

                                        #endregion // R
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in a type body the region's blank line is inserted between a directive and the <c>#endregion</c>
    /// directly below it, and none in front of the directive
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void InsertsRegionBlankLineButNoneInFrontOfDirectiveInTypeBody()
    {
        const string input = """
                             class C
                             {
                                 #region Fields

                                 int x;
                             #pragma warning disable CS0169
                                 #endregion // Fields
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region Fields

                                    int x;
                                #pragma warning disable CS0169

                                    #endregion // Fields
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that in a method body a directive followed by the author's blank line and an <c>#endregion</c> stays
    /// directly below the statement in front of it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void KeepsDirectiveDirectlyBelowStatementWhenRegionDirectiveEndsTheGapInMethodBody()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     #region R

                                     a++;
                             #pragma warning disable CS0168

                                     #endregion // R
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that no blank line is inserted in front of an <c>#if</c> whose disabled branch and <c>#endif</c> stand
    /// in front of an <c>#endregion</c> at the end of a method body
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void InsertsNoBlankLineInFrontOfConditionalBlockWhenRegionDirectiveEndsTheGap()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     #region R

                                     a++;
                             #if NEVER
                                     a--;
                             #endif

                                     #endregion // R
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the author's blank line in front of the first directive of a gap that a region directive ends is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void KeepsAuthorBlankLineInFrontOfDirectiveWhenRegionDirectiveEndsTheGap()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     #region R

                                     a++;

                             #pragma warning disable CS0168

                                     #endregion // R
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the blank line below an <c>#endregion</c> is inserted in front of a directive that stands directly
    /// below it before a closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void InsertsBlankLineBelowEndRegionInFrontOfDirectiveBeforeClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     #region R

                                     a++;

                                     #endregion // R
                             #pragma warning disable CS0168
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        #region R

                                        a++;

                                        #endregion // R

                                #pragma warning disable CS0168
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the blank line below an <c>#endregion</c> in front of a directive before a closing brace is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BLANK-009")]
    public void KeepsBlankLineBelowEndRegionInFrontOfDirectiveBeforeClosingBrace()
    {
        const string input = """
                             class C
                             {
                                 void M(int a)
                                 {
                                     #region R

                                     a++;

                                     #endregion // R

                             #pragma warning disable CS0168
                                 }
                             }
                             """;

        AssertRuleResult(input);
    }

    #endregion // Methods
}