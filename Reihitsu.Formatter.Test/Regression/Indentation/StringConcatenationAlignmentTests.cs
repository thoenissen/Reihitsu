using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Indentation;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/> — string-concatenation alignment
/// </summary>
[TestClass]
public class StringConcatenationAlignmentTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that a multi-line string concatenation inside a method argument
    /// aligns the <c>+</c> operator to the first operand column.
    /// </summary>
    [TestMethod]
    public void MultiLineStringConcatenationInMethodArgumentAlignsToFirstOperand()
    {
        // Arrange
        const string input = """
            using System.Threading.Tasks;

            class C
            {
                /// <inheritdoc/>
                public override Task<string> GetHeader()
                {
                    return Task.FromResult(Context.DisplayName
                                           + " "
                                           + Localizer.GetText("HeaderKey", "Default Header"));
                }
            }

            static class Context
            {
                public static string DisplayName { get; set; }
            }

            static class Localizer
            {
                public static string GetText(string key, string fallback)
                {
                    return fallback;
                }
            }
            """;

        // Act & Assert — already correctly formatted
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a misaligned multi-line string concatenation inside a method argument
    /// is reformatted so the <c>+</c> operator aligns to the first operand column.
    /// </summary>
    [TestMethod]
    public void MisalignedStringConcatenationInMethodArgumentGetsReformatted()
    {
        // Arrange
        const string input = """
            using System.Threading.Tasks;

            class C
            {
                /// <inheritdoc/>
                public override Task<string> GetHeader()
                {
                    return Task.FromResult(Context.DisplayName + " " +
                                           Localizer.GetText("HeaderKey", "Default Header"));
                }
            }

            static class Context
            {
                public static string DisplayName { get; set; }
            }

            static class Localizer
            {
                public static string GetText(string key, string fallback)
                {
                    return fallback;
                }
            }
            """;

        const string expected = """
            using System.Threading.Tasks;

            class C
            {
                /// <inheritdoc/>
                public override Task<string> GetHeader()
                {
                    return Task.FromResult(Context.DisplayName + " "
                                           + Localizer.GetText("HeaderKey", "Default Header"));
                }
            }

            static class Context
            {
                public static string DisplayName { get; set; }
            }

            static class Localizer
            {
                public static string GetText(string key, string fallback)
                {
                    return fallback;
                }
            }
            """;

        // Act & Assert
        AssertRuleResult(input, expected);
    }

    #endregion // Methods
}