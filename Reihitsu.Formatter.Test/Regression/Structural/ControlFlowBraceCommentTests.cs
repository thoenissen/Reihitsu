using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests for <see cref="Reihitsu.Formatter.Pipeline.StructuralTransforms.ControlFlowBraceTransform"/> —
/// comments must survive when missing braces are inserted around control-flow statements
/// </summary>
[TestClass]
public class ControlFlowBraceCommentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a comment above an unbraced if-body statement is preserved when braces are added
    /// </summary>
    [TestMethod]
    public void LeadingCommentAboveUnbracedIfBodyIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool condition)
                                 {
                                     if (condition)
                                         // important
                                         Foo();
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool condition)
                                    {
                                        if (condition)
                                        {
                                            // important
                                            Foo();
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a trailing comment on an unbraced if-body statement is preserved when braces are added
    /// </summary>
    [TestMethod]
    public void TrailingCommentOnUnbracedIfBodyIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool condition)
                                 {
                                     if (condition)
                                         Foo(); // note
                                 }
                             }
                             """;

        const string expected = """
                                class C
                                {
                                    void M(bool condition)
                                    {
                                        if (condition)
                                        {
                                            Foo(); // note
                                        }
                                    }
                                }
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}