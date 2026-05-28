using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineFormatterTests : FormatterTestsBase<RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer>
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
                                 Diagnostics(RH5026ChainedStatementBlocksMustNotBePrecededByBlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5026MessageFormat));
    }

    #endregion // Tests
}