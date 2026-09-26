using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH8303ElementDocumentationHeaderMustBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before documentation headers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void First()
                                 {
                                 }
                                 {|#0:///|} <summary>
                                 /// Summary.
                                 /// </summary>
                                 void Second()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void First()
                                     {
                                     }
                                 
                                     /// <summary>
                                     /// Summary.
                                     /// </summary>
                                     void Second()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH8303MessageFormat));
    }

    /// <summary>
    /// Verifies formatter parity and second-pass stability for delimited documentation headers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesDelimitedDocumentationAndIsIdempotent()
    {
        const string input = """
                             internal class Example
                             {
                                 void First()
                                 {
                                 }
                                 {|#0:/**|} <summary>Second.</summary> */
                                 void Second()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void First()
                                     {
                                     }

                                     /** <summary>Second.</summary> */
                                     void Second()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH8303MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter inserts the blank line above a documentation header below a multi-line block comment
    /// behind the previous member, whose inner line break the analyzer does not count as a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterSeparatesDocumentationBelowMultiLineTrailingBlockComment()
    {
        const string input = """
                             internal class Example
                             {
                                 private string _d; /* a
                                 b */
                                 {|#0:///|} <summary>
                                 /// Summary.
                                 /// </summary>
                                 public string Description { get; set; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private string _d; /* a
                                     b */

                                     /// <summary>
                                     /// Summary.
                                     /// </summary>
                                     public string Description { get; set; }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH8303MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter inserts the blank line above a delimited documentation header below a multi-line
    /// block comment behind the previous member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterSeparatesDelimitedDocumentationBelowMultiLineTrailingBlockComment()
    {
        const string input = """
                             internal class Example
                             {
                                 private string _d; /* a
                                 b */
                                 {|#0:/**|} <summary>Description.</summary> */
                                 public string Description { get; set; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private string _d; /* a
                                     b */

                                     /** <summary>Description.</summary> */
                                     public string Description { get; set; }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH8303MessageFormat));
    }

    #endregion // Tests
}