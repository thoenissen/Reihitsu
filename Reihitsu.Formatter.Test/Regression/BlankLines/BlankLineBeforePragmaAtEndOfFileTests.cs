using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.BlankLines;

/// <summary>
/// Reproduction tests for the chat-reported additional scenario tied to issue #769: a blank line
/// preceding a trailing <c>#pragma</c> directive being removed. Covers both the end-of-file position
/// (the same position the issue's own scenario reports) and a position clearly not at end of file, to
/// tell apart the reported <c>CleanupPhase</c> end-of-file mechanism from a distinct blank-line-collapsing
/// defect elsewhere
/// </summary>
[TestClass]
public class BlankLineBeforePragmaAtEndOfFileTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a blank line separating the outermost closing brace from a trailing <c>#pragma</c>
    /// directive at end of file is preserved rather than removed, and that the result is a fixed point
    /// under a second formatting pass — before the fix this shape degraded into invalid C# on pass two
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeTrailingPragmaAtEndOfFileIsPreserved()
    {
        // Arrange
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

        // Act & Assert — passing input as expected also exercises the second-pass idempotency check
        AssertRuleResult(input, input);
    }

    /// <summary>
    /// Verifies that a blank line separating a top-level type's closing brace (no enclosing namespace)
    /// from a trailing <c>#pragma</c> directive at end of file is preserved rather than removed, and
    /// that the result is a fixed point under a second formatting pass
    /// </summary>
    [TestMethod]
    public void BlankLineBeforeTrailingPragmaAfterTopLevelClassAtEndOfFileIsPreserved()
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

        // Act & Assert — passing input as expected also exercises the second-pass idempotency check
        AssertRuleResult(input, input);
    }

    /// <summary>
    /// Verifies that two or more blank lines before a trailing <c>#pragma</c> directive at end of file
    /// collapse to exactly one (owned by <c>BlankLineCollapser</c>, not by <c>CleanupPhase</c>) and that
    /// no further newline is removed — before the fix this shape was non-convergent: the collapse and
    /// the end-of-file strip combined to merge the directive onto the closing brace by the second pass
    /// </summary>
    [TestMethod]
    public void MultipleBlankLinesBeforeTrailingPragmaAtEndOfFileCollapseToOne()
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
        const string expected = """
                                public class Foo
                                {
                                    public void Bar()
                                    {
                                    }
                                }

                                #pragma warning restore CS1591
                                """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies that a blank line before a <c>#pragma</c> directive between two type members — clearly
    /// not at end of file, with further code following the directive — is preserved
    /// </summary>
    [TestMethod]
    public void BlankLineBeforePragmaBetweenMembersNotAtEndOfFileIsPreserved()
    {
        // Arrange
        const string input = """
                             public class Foo
                             {
                                 public void Bar()
                                 {
                                 }

                             #pragma warning restore CS1591

                                 public void Baz()
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a blank line before a <c>#pragma</c> directive at the top level, between two type
    /// declarations — clearly not at end of file, with further code following the directive — is
    /// preserved
    /// </summary>
    [TestMethod]
    public void BlankLineBeforePragmaBetweenTopLevelTypesNotAtEndOfFileIsPreserved()
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

                             public class Baz
                             {
                                 public void Qux()
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}