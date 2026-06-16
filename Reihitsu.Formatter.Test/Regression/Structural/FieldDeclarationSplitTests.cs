using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for <see cref="Reihitsu.Formatter.Pipeline.StructuralTransforms.FieldDeclarationSplitTransform"/>
/// </summary>
[TestClass]
public class FieldDeclarationSplitTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a combined field declaration without comments is split into separate declarations
    /// </summary>
    [TestMethod]
    public void CombinedFieldsAreSplit()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _a, _b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a;
                                    private int _b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a trailing comment after the separator is preserved on the corresponding declaration
    /// </summary>
    [TestMethod]
    public void TrailingCommentAfterSeparatorIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _a, // first
                                             _b; // second
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a; // first
                                    private int _b; // second
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a standalone comment line before a later declarator is preserved
    /// </summary>
    [TestMethod]
    public void StandaloneCommentBeforeDeclaratorIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 private int _a,
                                             // standalone
                                             _b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    private int _a;

                                    // standalone
                                    private int _b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that static fields declared in an interface are split
    /// </summary>
    [TestMethod]
    public void CombinedStaticInterfaceFieldsAreSplit()
    {
        // Arrange
        const string input = """
                             interface I
                             {
                                 static int _a, _b;
                             }
                             """;
        const string expected = """
                                interface I
                                {
                                    static int _a;
                                    static int _b;
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}