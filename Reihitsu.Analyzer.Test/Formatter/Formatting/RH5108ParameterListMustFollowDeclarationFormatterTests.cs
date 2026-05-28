using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5108ParameterListMustFollowDeclarationAnalyzer"/>
/// </summary>
[TestClass]
public class RH5108ParameterListMustFollowDeclarationFormatterTests : FormatterTestsBase<RH5108ParameterListMustFollowDeclarationAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter keeps a method parameter list on the declaration line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method(
                                     {|#0:int|} value)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method(int value)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
    }

    #endregion // Tests
}