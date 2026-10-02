using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.FullPipeline;

/// <summary>
/// Tests that members of a class written on one line each start on their own line in the full formatting pipeline
/// </summary>
[TestClass]
public class OneLineClassBodyMembersFullPipelineTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that two constructors with block bodies inside a one-line class body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesConstructorsInOneLineClassBody()
    {
        // Arrange
        const string input = "class AAttribute : System.Attribute { public AAttribute() { } public AAttribute(string s) { } }";
        const string expected = """
                                class AAttribute : System.Attribute
                                {
                                    public AAttribute()
                                    {
                                    }

                                    public AAttribute(string s)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two constructors in a one-line class without a base list are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesConstructorsInOneLineClassBodyWithoutBaseList()
    {
        // Arrange
        const string input = "class C { public C() { } public C(string s) { } }";
        const string expected = """
                                class C
                                {
                                    public C()
                                    {
                                    }

                                    public C(string s)
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two methods with block bodies inside a one-line class body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesMethodsInOneLineClassBody()
    {
        // Arrange
        const string input = "class C { void A() { } void B() { } }";
        const string expected = """
                                class C
                                {
                                    void A()
                                    {
                                    }

                                    void B()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a method followed by a field inside a one-line class body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesMethodAndFieldInOneLineClassBody()
    {
        // Arrange
        const string input = "class C { void A() { } int _f; }";
        const string expected = """
                                class C
                                {
                                    void A()
                                    {
                                    }

                                    int _f;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that two members in a one-line struct body are split onto separate lines
    /// </summary>
    [TestMethod]
    public void SeparatesMethodsInOneLineStructBody()
    {
        // Arrange
        const string input = "struct S { void A() { } void B() { } }";
        const string expected = """
                                struct S
                                {
                                    void A()
                                    {
                                    }

                                    void B()
                                    {
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}