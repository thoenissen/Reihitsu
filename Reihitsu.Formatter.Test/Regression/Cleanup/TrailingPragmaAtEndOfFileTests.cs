using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Cleanup;

/// <summary>
/// Full-pipeline reproduction tests for issue #769 — a trailing <c>#pragma</c> directive at end of
/// file is merged onto the preceding closing brace line, producing invalid C#
/// </summary>
[TestClass]
public class TrailingPragmaAtEndOfFileTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a trailing <c>#pragma</c> directive following the outermost closing brace at end
    /// of file stays on its own line, using the issue's own reported scenario (issue #769)
    /// </summary>
    [TestMethod]
    public void TrailingPragmaAfterNamespaceClosingBraceStaysOnOwnLine()
    {
        // Arrange — the issue's own reported scenario, with a member so structural simplification of
        // the empty class body does not interfere with the assertion
        const string input = """
                             namespace Test
                             {
                                 public class Foo
                                 {
                                     public void Bar()
                                     {
                                     }
                                 }
                             }
                             #pragma warning restore CS1591
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a trailing <c>#pragma</c> directive following a top-level type's closing brace at
    /// end of file (no enclosing namespace) stays on its own line (issue #769)
    /// </summary>
    [TestMethod]
    public void TrailingPragmaAfterTopLevelClassClosingBraceStaysOnOwnLine()
    {
        // Arrange
        const string input = """
                             public class Foo
                             {
                                 public void Bar()
                                 {
                                 }
                             }
                             #pragma warning restore CS1591
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}