using System.Text.RegularExpressions;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.SwitchCaseBraces;

/// <summary>
/// Full-pipeline tests for the guard that leaves a switch unbraced when a local declared directly in
/// one section is used in another section of the same switch
/// </summary>
[TestClass]
public class SwitchSectionSharedLocalTests : FormatterTestsBase
{
    #region Tests

    /// <summary>
    /// Verifies the reported shape: a local declared directly in the first section and assigned and
    /// read in the second section leaves the switch unbraced; only the blank line after the
    /// declaration is added
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    [Requirement("FMT-PIPE-003")]
    public void ReportedLocalUsedInAnotherSectionLeavesTheSwitchUnbraced()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;
                                             F(y);
                                             break;

                                         case 2:
                                             y = 2;
                                             F(y);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                int y = 1;

                                                F(y);
                                                break;

                                            case 2:
                                                y = 2;
                                                F(y);
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies the specified example: a local used in another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             F(y);
                                             break;

                                         case 2:
                                             y = 2;
                                             F(y);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a pattern variable declared in the condition of an <c>if</c> statement the section
    /// owns and used in another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void PatternVariableUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, object p)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             if (p is int n)
                                             {
                                                 F(n);
                                             }

                                             break;

                                         case 2:
                                             n = 2;
                                             F(n);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a local function called from a section above its declaration leaves the switch
    /// unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalFunctionCalledAboveItsDeclarationLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             return L();

                                         case 2:
                                             int L()
                                             {
                                                 return 1;
                                             }

                                             G();

                                             return L();
                                     }

                                     return 0;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a local function declared in one section and called from a section below it
    /// leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalFunctionCalledBelowItsDeclarationLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int L()
                                             {
                                                 return 1;
                                             }

                                             A();

                                             return L();

                                         case 2:
                                             return L();
                                     }

                                     return 0;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a constant used as the label of another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void ConstantUsedAsAnotherSectionLabelLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             const int K = 2;

                                             F(K);
                                             break;

                                         case K:
                                             F(x);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a local function called in the <c>when</c> clause of another section leaves the
    /// switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalFunctionCalledInAnotherSectionWhenClauseLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int L()
                                             {
                                                 return 1;
                                             }

                                             F(L());
                                             break;

                                         case int v when L() > 0:
                                             F(v);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>out var</c> declared in a <c>return</c> statement the section owns and
    /// used in another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void OutVariableOfReturnStatementUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();

                                             return F(out var o);

                                         case 2:
                                             o = 2;

                                             return o;
                                     }

                                     return 0;
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>out var</c> declared in an expression statement the section owns and used in
    /// another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void OutVariableOfExpressionStatementUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             F(out var o);
                                             G(o);
                                             break;

                                         case 2:
                                             o = 2;
                                             G(o);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a deconstruction declaration whose variable is used in another section leaves
    /// the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void DeconstructionVariableUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             var (a, b) = P();

                                             F(a, b);
                                             break;

                                         case 2:
                                             a = 2;
                                             F(a);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a local declared behind a label and used in another section leaves the switch
    /// unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LabeledLocalDeclarationUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             start:
                                             int y = 1;

                                             F(y);
                                             break;

                                         case 2:
                                             y = 2;
                                             F(y);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>out var</c> declared in a <c>throw</c> statement the section owns and used in
    /// another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void OutVariableOfThrowStatementUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();

                                             throw Make(out var o);

                                         case 2:
                                             o = 2;
                                             F(o);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>out var</c> declared in a <c>yield return</c> statement the section owns and used
    /// in another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void OutVariableOfYieldReturnStatementUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 IEnumerable<int> M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();

                                             yield return F(out var o);
                                             break;

                                         case 2:
                                             o = 2;

                                             yield return o;
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>out var</c> declared in the expression of a <c>lock</c> statement the section owns
    /// and used in another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void OutVariableOfLockExpressionUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             lock (Get(out var o))
                                             {
                                                 F(o);
                                             }

                                             break;

                                         case 2:
                                             o = 2;
                                             F(o);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>out var</c> declared in the governing expression of a nested switch and used in
    /// another section of the outer switch leaves the outer switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void OutVariableOfNestedSwitchGoverningExpressionUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             switch (Get(out var o))
                                             {
                                                 case 1:
                                                     F(o);
                                                     break;
                                             }

                                             break;

                                         case 2:
                                             o = 2;
                                             F(o);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a pattern variable declared in an expression statement the section owns and used in
    /// another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void PatternVariableOfExpressionStatementUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, object p)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             F(p is int n);
                                             G(n);
                                             break;

                                         case 2:
                                             n = 2;
                                             G(n);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>out var</c> declared in an expression statement behind a label and used in another
    /// section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LabeledOutVariableUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             start:
                                             F(out var y);
                                             G(y);
                                             break;

                                         case 2:
                                             y = 2;
                                             G(y);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a use inside a lambda of another section counts
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalUsedInLambdaOfAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             F(y);
                                             break;

                                         case 2:
                                             y = 2;
                                             Run(() => F(y));
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a use inside the existing block of another section still counts, so the switch
    /// stays unchanged although the existing block would otherwise brace it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalUsedInExistingBlockOfAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;
                                             break;

                                         case 2:
                                             {
                                                 y = 2;
                                                 F(y);
                                             }
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that an <c>out var</c> declared in the initializer of a local declaration the section owns and
    /// used in another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void OutVariableOfLocalDeclarationInitializerUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             var b = F(out var o);

                                             G(b);
                                             break;

                                         case 2:
                                             o = 2;
                                             G(o);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a pattern variable declared in the initializer of a local declaration the section owns and
    /// used in another section leaves the switch unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void PatternVariableOfLocalDeclarationInitializerUsedInAnotherSectionLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, object p)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             var b = p is int n;

                                             G(b);
                                             break;

                                         case 2:
                                             n = 2;
                                             G(n);
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a lambda parameter of another section that shadows the local is not a use of the local, so
    /// a local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LambdaParameterShadowingTheLocalInAnotherSectionDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             Func<int, int> f = y => y + 1;

                                             Use(f(2));
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    Func<int, int> f = y => y + 1;

                                                    Use(f(2));
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a local-function parameter of another section that shadows the local is not a use of the
    /// local, so a local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalFunctionParameterShadowingTheLocalInAnotherSectionDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             int L(int y)
                                             {
                                                 return y + 1;
                                             }

                                             Use(L(2));
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    int L(int y)
                                                    {
                                                        return y + 1;
                                                    }

                                                    Use(L(2));
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a pattern variable declared in a query clause, which C# scopes to that clause, does not
    /// block the braces when another section uses a field of the same name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void PatternVariableInQueryClauseDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int n;

                                 void M(int x, object[] xs)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             var q = from a in xs where a is int n select n;

                                             Use(q);
                                             break;

                                         case 2:
                                             Use(n);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int n;

                                    void M(int x, object[] xs)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    var q = from a in xs where a is int n select n;

                                                    Use(q);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    Use(n);
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a name in the attribute list of a local function whose parameter has the same name is a
    /// use of the shared local, not of the parameter, so the switch is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    [Requirement("FMT-PIPE-003")]
    public void AttributeArgumentOfLocalFunctionUsesTheLocalAndLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             const int y = 2;

                                             Use(y);
                                             break;

                                         case 2:
                                             [My(y)]
                                             int L(int y)
                                             {
                                                 return y;
                                             }

                                             Use(L(1));
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a name in a parameter default value of a local function that has a parameter of the same
    /// name is a use of the shared local, so the switch is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    [Requirement("FMT-PIPE-003")]
    public void DefaultValueOfLocalFunctionParameterUsesTheLocalAndLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             const int y = 2;

                                             Use(y);
                                             break;

                                         case 2:
                                             int L(int y, int b = y)
                                             {
                                                 return b;
                                             }

                                             Use(L(1));
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a name in the attribute list of a lambda whose parameter has the same name is a use of the
    /// shared local, so no section of the switch is braced. The layout of a lambda's attribute list is not
    /// specified, so only the sections are asserted
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    [Requirement("FMT-PIPE-003")]
    public void AttributeArgumentOfLambdaUsesTheLocalAndLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             const int y = 2;

                                             Use(y);
                                             break;

                                         case 2:
                                             Func<int, int> f = [My(y)] (int y) => y;

                                             Use(f(1));
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertSwitchSectionsStayUnbraced(input);
    }

    /// <summary>
    /// Verifies that a name in a parameter default value of a lambda that has a parameter of the same name is a
    /// use of the shared local, so the switch is left unchanged
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    [Requirement("FMT-PIPE-003")]
    public void DefaultValueOfLambdaParameterUsesTheLocalAndLeavesTheSwitchUnchanged()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             const int y = 2;

                                             Use(y);
                                             break;

                                         case 2:
                                             var f = (int y, int b = y) => b;

                                             Use(f(1, 2));
                                             break;
                                     }
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a <c>foreach</c> variable in a lambda in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void ForeachVariableInLambdaShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, int[] ys)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             Action a = () =>
                                                        {
                                                            foreach (var y in ys)
                                                            {
                                                                Use(y);
                                                            }
                                                        };

                                             a();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x, int[] ys)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    Action a = () =>
                                                               {
                                                                   foreach (var y in ys)
                                                                   {
                                                                       Use(y);
                                                                   }
                                                               };

                                                    a();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>foreach</c> variable in a local function in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void ForeachVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, int[] ys)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             void L()
                                             {
                                                 foreach (var y in ys)
                                                 {
                                                     Use(y);
                                                 }
                                             }

                                             L();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x, int[] ys)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    void L()
                                                    {
                                                        foreach (var y in ys)
                                                        {
                                                            Use(y);
                                                        }
                                                    }

                                                    L();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>for</c> variable in a local function in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void ForVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             void L()
                                             {
                                                 for (var y = 0; y < 2; y++)
                                                 {
                                                     Use(y);
                                                 }
                                             }

                                             L();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    void L()
                                                    {
                                                        for (var y = 0; y < 2; y++)
                                                        {
                                                            Use(y);
                                                        }
                                                    }

                                                    L();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>using</c> variable in a local function in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void UsingVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             void L()
                                             {
                                                 using (var y = Open())
                                                 {
                                                     Use(y);
                                                 }
                                             }

                                             L();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    void L()
                                                    {
                                                        using (var y = Open())
                                                        {
                                                            Use(y);
                                                        }
                                                    }

                                                    L();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>fixed</c> variable in a local function in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void FixedVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, int[] ys)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             unsafe void L()
                                             {
                                                 fixed (int* y = ys)
                                                 {
                                                     Use(*y);
                                                 }
                                             }

                                             L();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x, int[] ys)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    unsafe void L()
                                                    {
                                                        fixed (int* y = ys)
                                                        {
                                                            Use(*y);
                                                        }
                                                    }

                                                    L();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a <c>catch</c> variable in a local function in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void CatchVariableInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             void L()
                                             {
                                                 try
                                                 {
                                                     A();
                                                 }
                                                 catch (Exception y)
                                                 {
                                                     Use(y);
                                                 }
                                             }

                                             L();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    void L()
                                                    {
                                                        try
                                                        {
                                                            A();
                                                        }
                                                        catch (Exception y)
                                                        {
                                                            Use(y);
                                                        }
                                                    }

                                                    L();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a pattern variable in a <c>while</c> condition in a local function in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void WhileConditionDesignationInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             void L(object o)
                                             {
                                                 while (o is int y && y > 0)
                                                 {
                                                     o = Next(y);
                                                 }
                                             }

                                             L(1);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    void L(object o)
                                                    {
                                                        while (o is int y && y > 0)
                                                        {
                                                            o = Next(y);
                                                        }
                                                    }

                                                    L(1);
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that an <c>out var</c> in a <c>do</c> condition in a local function in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void DoConditionDesignationInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             void L()
                                             {
                                                 do
                                                 {
                                                     A();
                                                 }
                                                 while (F(out var y) && y > 0);
                                             }

                                             L();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    void L()
                                                    {
                                                        do
                                                        {
                                                            A();
                                                        }
                                                        while (F(out var y) && y > 0);
                                                    }

                                                    L();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a pattern variable of a switch-expression arm in a lambda in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void SwitchExpressionArmDesignationInLambdaShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             Func<object, int> f = o => o switch
                                                                        {
                                                                            int y => y,
                                                                            _ => 0
                                                                        };

                                             Use(f(1));
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    Func<object, int> f = o => o switch
                                                                               {
                                                                                   int y => y,
                                                                                   _ => 0
                                                                               };

                                                    Use(f(1));
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a local of a nested switch section in a local function in another section that shadows the local is not a use of the local, so a
    /// local used only in its own section does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void NestedSwitchSectionLocalInLocalFunctionShadowingTheLocalDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             Use(y);
                                             break;

                                         case 2:
                                             void L(int z)
                                             {
                                                 switch (z)
                                                 {
                                                     case 1:
                                                         int y = 2;
                                                         break;
                                                 }
                                             }

                                             L(1);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    Use(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    void L(int z)
                                                    {
                                                        switch (z)
                                                        {
                                                            case 1:
                                                                int y = 2;
                                                                break;
                                                        }
                                                    }

                                                    L(1);
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a local used only in the section that declares it does not block the braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalUsedOnlyInItsOwnSectionDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             F(y);
                                             break;

                                         case 2:
                                             F(2);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    F(y);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    F(2);
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that locals declared in a nested block and in a <c>foreach</c> header do not block the
    /// braces, even when another section uses a field of the same name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalsInNestedBlockAndForeachHeaderDoNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int y;

                                 void M(int x, int[] ys)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             if (x > 0)
                                             {
                                                 int y = 1;

                                                 F(y);
                                             }

                                             foreach (var y in ys)
                                             {
                                                 F(y);
                                             }

                                             break;

                                         case 2:
                                             F(y);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int y;

                                    void M(int x, int[] ys)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    if (x > 0)
                                                    {
                                                        int y = 1;

                                                        F(y);
                                                    }

                                                    foreach (var y in ys)
                                                    {
                                                        F(y);
                                                    }
                                                }
                                                break;

                                            case 2:
                                                {
                                                    F(y);
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a local declared inside a lambda body of a section does not block the braces when
    /// another section uses a field of the same name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalInLambdaBodyDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int y;

                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             Run(() =>
                                                 {
                                                     int y = 1;

                                                     F(y);
                                                 });
                                             break;

                                         case 2:
                                             F(y);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int y;

                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    Run(() =>
                                                        {
                                                            int y = 1;

                                                            F(y);
                                                        });
                                                }
                                                break;

                                            case 2:
                                                {
                                                    F(y);
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a pattern variable declared in an <c>else if</c> condition does not block the
    /// braces when another section uses a field of the same name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void PatternVariableInElseIfConditionDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int n;

                                 void M(int x, object p)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             if (x > 0)
                                             {
                                                 A();
                                             }
                                             else if (p is int n)
                                             {
                                                 F(n);
                                             }

                                             break;

                                         case 2:
                                             F(n);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int n;

                                    void M(int x, object p)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    if (x > 0)
                                                    {
                                                        A();
                                                    }
                                                    else if (p is int n)
                                                    {
                                                        F(n);
                                                    }
                                                }
                                                break;

                                            case 2:
                                                {
                                                    F(n);
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a local declared in a section of a nested switch does not block the braces of the
    /// outer switch when another outer section uses a field of the same name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalInNestedSwitchSectionDoesNotBlockTheOuterBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int y;

                                 void M(int x, int z)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             switch (z)
                                             {
                                                 case 1:
                                                     int y = 1;
                                                     break;

                                                 case 2:
                                                     B();
                                                     break;
                                             }

                                             break;

                                         case 2:
                                             F(y);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    int y;

                                    void M(int x, int z)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    switch (z)
                                                    {
                                                        case 1:
                                                            int y = 1;
                                                            break;

                                                        case 2:
                                                            B();
                                                            break;
                                                    }
                                                }
                                                break;

                                            case 2:
                                                {
                                                    F(y);
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a local of an outer section used only inside an inner switch of the same outer
    /// section does not block the outer braces
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void LocalUsedOnlyInInnerSwitchOfItsOwnSectionDoesNotBlockTheBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, int z)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             switch (z)
                                             {
                                                 case 1:
                                                     F(y);
                                                     break;

                                                 case 2:
                                                     G(y);
                                                     break;
                                             }

                                             break;

                                         case 2:
                                             B();
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x, int z)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int y = 1;

                                                    switch (z)
                                                    {
                                                        case 1:
                                                            F(y);
                                                            break;

                                                        case 2:
                                                            G(y);
                                                            break;
                                                    }
                                                }
                                                break;

                                            case 2:
                                                {
                                                    B();
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the guard on an outer switch does not stop an inner switch from being braced by
    /// its own decision
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void GuardedOuterSwitchKeepsInnerSwitchDecision()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x, int z)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             switch (z)
                                             {
                                                 case 1:
                                                     A();
                                                     B();
                                                     break;

                                                 case 2:
                                                     C();
                                                     break;
                                             }

                                             break;

                                         case 2:
                                             F(y);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x, int z)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                int y = 1;

                                                switch (z)
                                                {
                                                    case 1:
                                                        {
                                                            A();
                                                            B();
                                                        }
                                                        break;

                                                    case 2:
                                                        {
                                                            C();
                                                        }
                                                        break;
                                                }

                                                break;

                                            case 2:
                                                F(y);
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that the guard decides each switch on its own when one method holds a guarded and an
    /// unguarded switch that declare locals of the same name
    /// </summary>
    [TestMethod]
    [Requirement("FMT-SWITCH-014")]
    public void TwoSwitchesInOneMethodAreDecidedSeparately()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             int y = 1;

                                             F(y);
                                             break;

                                         case 2:
                                             y = 2;
                                             F(y);
                                             break;
                                     }

                                     switch (x)
                                     {
                                         case 1:
                                             int w = 1;

                                             F(w);
                                             break;

                                         case 2:
                                             F(2);
                                             break;
                                     }
                                 }
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    void M(int x)
                                    {
                                        switch (x)
                                        {
                                            case 1:
                                                int y = 1;

                                                F(y);
                                                break;

                                            case 2:
                                                y = 2;
                                                F(y);
                                                break;
                                        }

                                        switch (x)
                                        {
                                            case 1:
                                                {
                                                    int w = 1;

                                                    F(w);
                                                }
                                                break;

                                            case 2:
                                                {
                                                    F(2);
                                                }
                                                break;
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Formats the input under LF and CRLF and asserts that no section of the switch is braced and that a second
    /// pass changes nothing, without asserting the layout of anything else
    /// </summary>
    /// <param name="input">The source text to format</param>
    private static void AssertSwitchSectionsStayUnbraced(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var endingName = DescribeLineEnding(endOfLine);
            var formatted = Format(NormalizeLineEndings(input, endOfLine));

            Assert.AreEqual(formatted, Format(formatted), $"Formatter is not idempotent under {endingName} line endings.");
            AssertUsesLineEnding(formatted, endOfLine);
            Assert.DoesNotMatchRegex(new Regex(@"case [^\r\n]*:\r?\n[ ]*\{", RegexOptions.None, Regex.InfiniteMatchTimeout),
                                     formatted,
                                     $"No switch section may be braced under {endingName} line endings.");
        }
    }

    /// <summary>
    /// Formats the source text with the whole formatter
    /// </summary>
    /// <param name="input">The source text to format</param>
    /// <returns>The formatted source text</returns>
    private static string Format(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: CancellationToken.None);

        return ReihitsuFormatter.FormatSyntaxTree(tree, CancellationToken.None).GetRoot(CancellationToken.None).ToFullString();
    }

    #endregion // Methods
}