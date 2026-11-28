using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Full-pipeline tests for directives in a conditional-compilation branch the build does not compile: such a directive,
/// a region directive or any other, is not re-indented and keeps the column it is written in, while a tab in front of it
/// or between its tokens still becomes four spaces and a region directive's text is still written
/// </summary>
[TestClass]
public class InactiveDirectiveColumnTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the spec example: the region directives of an inactive branch between members keep their columns, two and
    /// ten, their text is written as for an active region, and the disabled text between them is left as written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-010")]
    public void SpecExampleInactiveRegionBetweenMembersKeepsItsColumns()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if NEVER
                               #region inner
                                 disabled
                                       #endregion
                             #endif

                                 int _a;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if NEVER
                                  #region Inner
                                    disabled
                                          #endregion // Inner
                                #endif

                                    int _a;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example with tabs: the tabs in front of the inactive region directives become four spaces, the
    /// directives keep the column that replacement gives them, and the tab in front of the disabled text is kept
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-010")]
    public void SpecExampleTabsInFrontOfInactiveRegionAreReplacedAndTabInDisabledTextIsKept()
    {
        // Arrange
        const string input = "class C\n{\n#if NEVER\n\t#region Inner\n\tdisabled\n\t#endregion\n#endif\n\n    int _a;\n}";
        const string expected = "class C\n{\n#if NEVER\n    #region Inner\n\tdisabled\n    #endregion // Inner\n#endif\n\n    int _a;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two tabs in front of an inactive region directive become eight spaces, which is not the member column,
    /// so the directive keeps the column the replacement gives it rather than being re-indented
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-011")]
    public void TabsInFrontOfInactiveRegionGiveAColumnOffTheMemberColumnThatIsKept()
    {
        // Arrange
        const string input = "class C\n{\n#if NEVER\n\t\t#region Inner\n\tdisabled\n\t\t#endregion // Inner\n#endif\n\n    int _a;\n}";
        const string expected = "class C\n{\n#if NEVER\n        #region Inner\n\tdisabled\n        #endregion // Inner\n#endif\n\n    int _a;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: an inactive region inside a method body is not refused, its directives keep their
    /// columns, two and ten, and their text is written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-010")]
    public void SpecExampleInactiveRegionInsideMethodBodyKeepsItsColumns()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                             #if NEVER
                               #region inner
                                     disabled();
                                       #endregion
                             #endif
                                     M();
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                #if NEVER
                                  #region Inner
                                        disabled();
                                          #endregion // Inner
                                #endif
                                        M();
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies one nesting level deeper: inactive region directives in column zero between the members of a type inside
    /// a namespace, whose member column is eight, stay in column zero while their text is written
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-010")]
    public void InactiveRegionInNestedTypeStaysInColumnZero()
    {
        // Arrange
        const string input = """
                             namespace N
                             {
                                 class C
                                 {
                                     int _a;
                             #if NEVER
                             #region inner
                                     int _b;
                             #endregion
                             #endif
                                 }
                             }
                             """;
        const string expected = """
                                namespace N
                                {
                                    class C
                                    {
                                        int _a;
                                #if NEVER
                                #region Inner
                                        int _b;
                                #endregion // Inner
                                #endif
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the spec example: an inactive <c>#pragma</c> in column six and an inactive nested <c>#if</c> and
    /// <c>#endif</c> in column three keep their columns
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-011")]
    public void SpecExampleInactiveNonRegionDirectivesKeepTheirColumns()
    {
        // Arrange
        const string input = """
                             class C
                             {
                             #if NEVER
                                   #pragma warning disable CS0169
                                #if X
                                 int _b;
                                #endif
                             #endif

                                 int _a;
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies the spec example with tabs: the tab in front of an inactive <c>#pragma</c> and the tab between its tokens
    /// each become four spaces, and the directive keeps the column the replacement gives it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-011")]
    public void SpecExampleTabsInFrontOfAndInsideInactivePragmaAreReplacedAndItsColumnIsKept()
    {
        // Arrange
        const string input = "class C\n{\n#if NEVER\n\t#pragma\twarning disable CS0169\n#endif\n\n    int _a;\n}";
        const string expected = "class C\n{\n#if NEVER\n    #pragma    warning disable CS0169\n#endif\n\n    int _a;\n}";

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}