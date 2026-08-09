using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5408SimpleAutoPropertiesShouldBeSingleLinedFormatterTests : FormatterTestsBase<RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter collapses a multi-line get/set auto-property to one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesGetSetAutoProperty()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value
                                 {
                                     get;
                                     set;
                                 }|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value { get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses a multi-line get-only auto-property to one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesGetOnlyAutoProperty()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value
                                 {
                                     get;
                                 }|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value { get; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses a multi-line property-attributed auto-property to one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesPropertyAttributedAutoProperty()
    {
        const string input = """
                             sealed class TestAttribute : System.Attribute;

                             internal class Example
                             {
                                 [Test]
                                 {|#0:internal int Value
                                 {
                                     get;
                                     set;
                                 }|}
                             }
                             """;
        const string fixedData = """
                                 sealed class TestAttribute : System.Attribute;

                                 internal class Example
                                 {
                                     [Test]
                                     internal int Value { get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses a multi-line accessor-attributed auto-property to one line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesAccessorAttributedAutoProperty()
    {
        const string input = """
                             sealed class TestAttribute : System.Attribute;

                             internal class Example
                             {
                                 {|#0:internal int Value
                                 {
                                     [Test]
                                     get;
                                     [Test]
                                     set;
                                 }|}
                             }
                             """;
        const string fixedData = """
                                 sealed class TestAttribute : System.Attribute;

                                 internal class Example
                                 {
                                     internal int Value { [Test] get; [Test] set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses a multi-line auto-property whose accessor list is followed by a
    /// trailing comment, so analyzer and formatter agree on that shape (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesAutoPropertyWithTrailingComment()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value
                                 {
                                     get;
                                     set;
                                 }|} // explanation
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value { get; set; } // explanation
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves a single-line auto-property with a trailing comment untouched, so
    /// analyzer-clean code stays stable (issue #604)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsSingleLineAutoPropertyWithTrailingComment()
    {
        const string input = """
                             internal class Example
                             {
                                 internal int Value { get; set; } // explanation
                             }
                             """;

        await VerifyFormatterStability(input);
    }

    /// <summary>
    /// Verifies that the formatter joins a terminating semicolon that sits on its own line, so the diagnostic the
    /// analyzer reports for that shape can be cleared (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesOwnLineDeclarationSemicolonAutoProperty()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value { get; set; } = 1
                                     ;|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value { get; set; } = 1;
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses the accessor list and joins the terminating semicolon in one pass, so
    /// the two line-break subphases cooperate on the same declaration (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMultiLineAccessorListWithOwnLineDeclarationSemicolon()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value
                                 {
                                     get;
                                     set;
                                 } = 1
                                     ;|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value { get; set; } = 1;
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses an auto-property carrying a comment between the initializer value and a
    /// semicolon on the same line, so the shape RH5408 newly reports is also corrected by the CLI, and that a second
    /// pass over the result changes nothing under both line endings (issue #650)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesAutoPropertyWithTrailingInitializerComment()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int Value
                                 {
                                     get;
                                     set;
                                 } = 1 /* note */;|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int Value { get; set; } = 1 /* note */;
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    #endregion // Tests
}