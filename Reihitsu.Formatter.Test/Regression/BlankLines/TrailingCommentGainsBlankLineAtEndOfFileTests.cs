using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Regression tests for the accepted end-to-end side effect of the issue #769 fix (decision D1, chosen
/// by the user as "Option A"): once <c>CleanupPhase</c> stops silently deleting the end-of-file newline
/// that <c>BlankLinePhase</c> already inserts before a trailing comment or <c>#endregion</c>, that
/// existing insertion policy becomes visible for the first time. A trailing comment/region that used to
/// sit directly against the closing brace, with no blank line, now gets exactly one
/// </summary>
[TestClass]
public class TrailingCommentGainsBlankLineAtEndOfFileTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a trailing single-line comment at end of file, with no blank line before it, gains
    /// exactly one blank line
    /// </summary>
    [TestMethod]
    public void TrailingCommentWithNoBlankLineGainsOneAtEndOfFile()
    {
        // Arrange
        const string input = """
                             public class Foo
                             {
                                 public void Bar()
                                 {
                                 }
                             }
                             // trailing comment
                             """;
        const string expected = """
                                public class Foo
                                {
                                    public void Bar()
                                    {
                                    }
                                }

                                // trailing comment
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a trailing single-line comment at end of file that already has a blank line before
    /// it keeps exactly one — the newly visible insertion must not double an already-present blank line
    /// </summary>
    [TestMethod]
    public void TrailingCommentWithExistingBlankLineStaysAtOneAtEndOfFile()
    {
        // Arrange
        const string input = """
                             public class Foo
                             {
                                 public void Bar()
                                 {
                                 }
                             }

                             // trailing comment
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a trailing <c>#region</c>/<c>#endregion</c> pair at end of file, with no blank line
    /// before it, gains exactly one blank line before <c>#region</c> — the same previously masked
    /// <c>BlankLinePhase</c> insertion that applies to comments also applies to regions
    /// </summary>
    [TestMethod]
    public void TrailingRegionWithNoBlankLineGainsOneAtEndOfFile()
    {
        // Arrange
        const string input = """
                             public class Foo
                             {
                                 public void Bar()
                                 {
                                 }
                             }
                             #region Footer
                             #endregion
                             """;
        const string expected = """
                                public class Foo
                                {
                                    public void Bar()
                                    {
                                    }
                                }

                                #region Footer

                                #endregion // Footer
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}