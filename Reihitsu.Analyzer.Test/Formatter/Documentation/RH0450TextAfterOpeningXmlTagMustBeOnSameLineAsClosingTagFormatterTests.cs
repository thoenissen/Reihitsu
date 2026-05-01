using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Documentation;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Documentation;

/// <summary>
/// Formatter validation tests for <see cref="RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer"/>
/// </summary>
[TestClass]
public class RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagFormatterTests : FormatterTestsBase<RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter moves multi-line XML element text below the opening tag
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 /// <remarks>First line
                                 /// Second line
                                 /// </remarks>
                                 void Method()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     /// <remarks>
                                     /// First line
                                     /// Second line
                                     /// </remarks>
                                     void Method()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0450TextAfterOpeningXmlTagMustBeOnSameLineAsClosingTagAnalyzer.DiagnosticId, 3, 9, 5, 19, AnalyzerResources.RH0450MessageFormat));
    }

    #endregion // Members
}