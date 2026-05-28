using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineFormatterTests : FormatterTestsBase<RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines after documentation headers
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 /// <summary>
                                 /// Summary.
                                 /// </summary>
                             {|#0:
                             |}    void Method()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /// <summary>
                                     /// Summary.
                                     /// </summary>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH8302ElementDocumentationHeadersMustNotBeFollowedByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH8302MessageFormat));
    }

    #endregion // Tests
}