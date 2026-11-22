using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Regression tests for a preprocessor directive that directly follows disabled text: it moves to column 0 like every
/// other directive (FMT-INDENT-009)
/// </summary>
[TestClass]
public class DirectiveBehindDisabledTextTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the reported example: <c>#endif</c> behind a disabled <c>#else</c> branch in a method body moves to column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-009")]
    public void EndIfBehindDisabledBranchInMethodBodyMovesToColumnZero()
    {
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 0;
                                         #if !NEVER
                                     x = 1;
                                       #else
                                     x = 2;
                                             #endif
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 0;
                                #if !NEVER
                                        x = 1;
                                #else
                                        x = 2;
                                #endif
                                    }
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that <c>#elif</c>, <c>#else</c>, and <c>#endif</c> behind disabled branches at member level move to column 0
    /// </summary>
    [TestMethod]
    [Requirement("FMT-INDENT-009")]
    public void DirectivesBehindDisabledBranchesAtMemberLevelMoveToColumnZero()
    {
        const string input = """
                             class C
                             {
                                     #if NEVER
                                 int a;
                                       #elif !NEVER
                                 int b;
                                    #else
                                 int c;
                                         #endif
                             }
                             """;
        const string expected = """
                                class C
                                {
                                #if NEVER
                                    int a;
                                #elif !NEVER
                                    int b;
                                #else
                                    int c;
                                #endif
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests
}