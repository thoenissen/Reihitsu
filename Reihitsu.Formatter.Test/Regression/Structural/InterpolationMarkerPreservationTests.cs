using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Regression tests ensuring the formatter never strips the interpolation marker from interpolated strings without
/// holes. The formatter has no semantic model, so it cannot tell whether the converted type is <c>string</c> (safe to
/// rewrite) or <see cref="System.FormattableString"/>/<see cref="System.IFormattable"/> (rewriting produces CS0029 or a
/// silent overload change). Marker removal is therefore owned exclusively by the RH3204 analyzer and its code fix
/// </summary>
[TestClass]
public class InterpolationMarkerPreservationTests : FormatterTestsBase
{
    #region Methods

    /// <summary>
    /// Verifies that the marker on an interpolated string converted to <see cref="System.FormattableString"/> is
    /// preserved, so the formatter no longer produces non-compiling CS0029 output
    /// </summary>
    [TestMethod]
    public void FormattableStringTargetMarkerIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.FormattableString value = $"no holes here";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the marker on an interpolated string converted to <see cref="System.IFormattable"/> is preserved
    /// </summary>
    [TestMethod]
    public void FormattableInterfaceTargetMarkerIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     System.IFormattable value = $"no holes here";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that the marker on an interpolated string bound to a <see cref="System.FormattableString"/> parameter is
    /// preserved, so the formatter no longer silently switches the resolved overload
    /// </summary>
    [TestMethod]
    public void FormattableStringArgumentMarkerIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Consume($"no holes here");
                                 }

                                 void Consume(System.FormattableString value)
                                 {
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a plain interpolated string without holes keeps its marker, because marker removal is now owned by
    /// the RH3204 analyzer and code fix rather than the semantic-model-free formatter
    /// </summary>
    [TestMethod]
    public void PlainInterpolatedStringMarkerIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var message = $"System ready";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    /// <summary>
    /// Verifies that a verbatim interpolated string without holes keeps its marker
    /// </summary>
    [TestMethod]
    public void VerbatimInterpolatedStringMarkerIsPreserved()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var path = $@"C:\Temp";
                                 }
                             }
                             """;

        // Act & Assert
        AssertRuleResult(input);
    }

    #endregion // Methods
}