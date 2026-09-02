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

        await VerifyFormatter(input,
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

        await VerifyFormatter(input,
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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves a multi-line accessor-attributed auto-property exactly as written,
    /// because an accessor carrying its own attribute list is no longer simple and RH5408 must not force it
    /// onto one line (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesAccessorAttributedAutoPropertyAsWritten()
    {
        const string input = """
                             sealed class TestAttribute : System.Attribute;

                             internal class Example
                             {
                                 internal int Value
                                 {
                                     [Test]
                                     get;
                                     [Test]
                                     set;
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter leaves the issue's exact reported shape — a property-level attribute
    /// combined with accessor-level attributes, written multi-line — exactly as written under LF and CRLF, and
    /// stable on a second pass. This is the shape the shipped code fix could not converge on before the fix
    /// (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterLeavesPropertyAndAccessorAttributedAutoPropertyAsWritten()
    {
        const string input = """
                             sealed class TestAttribute : System.Attribute;

                             internal class Example
                             {
                                 [Test]
                                 internal int Value
                                 {
                                     [Test]
                                     get;
                                     [Test]
                                     set;
                                 }
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter never routes a single-line accessor-attributed auto-property into the
    /// Allman brace-normalization branch. A guard that merely refuses to collapse (rather than bypassing both
    /// branches) would force this already-correct single-line declaration apart — precisely the forced
    /// multi-line expansion this fix must not reintroduce (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsSingleLineAccessorAttributedAutoPropertyStable()
    {
        const string input = """
                             sealed class TestAttribute : System.Attribute;

                             internal class Example
                             {
                                 internal int Value { [Test] get; [Test] set; }
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter keeps a single-line auto-property stable when it combines a property-level
    /// attribute on its own line with accessor-level attributes inline. Before the span repair, the shared
    /// single-line predicate wrongly counted the property attribute's own line and treated the declaration as
    /// multi-line, which made RH5530 tear the accessor attributes apart on a document RH5408 never touched
    /// (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsSinglePropertyAndAccessorAttributedAutoPropertyStable()
    {
        const string input = """
                             sealed class TestAttribute : System.Attribute;

                             internal class Example
                             {
                                 [Test]
                                 internal int Value { [Test] get; [Test] set; }
                             }
                             """;

        await VerifyFormatter(input);
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

        await VerifyFormatter(input,
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

        await VerifyFormatter(input);
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

        await VerifyFormatter(input,
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

        await VerifyFormatter(input,
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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5408SimpleAutoPropertiesShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5408MessageFormat));
    }

    #endregion // Tests
}