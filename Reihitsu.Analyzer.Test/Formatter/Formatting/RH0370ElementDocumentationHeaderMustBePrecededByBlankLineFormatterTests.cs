using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0370ElementDocumentationHeaderMustBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer>
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

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0370ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0370MessageFormat));
    }

    #endregion // Tests
}