using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0368ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0368ChainedStatementBlocksMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH0368ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter removes blank lines before chained statement blocks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method()
                                 {
                                     if (true)
                                     {
                                     }
                             {|#0:
                             |}        else
                                     {
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method()
                                     {
                                         if (true)
                                         {
                                         }
                                         else
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0368ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH0368MessageFormat));
    }

    #endregion // Tests
}