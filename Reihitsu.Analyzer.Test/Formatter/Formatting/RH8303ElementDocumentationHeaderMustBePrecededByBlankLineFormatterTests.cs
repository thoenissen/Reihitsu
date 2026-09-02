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

    #endregion // Tests
}