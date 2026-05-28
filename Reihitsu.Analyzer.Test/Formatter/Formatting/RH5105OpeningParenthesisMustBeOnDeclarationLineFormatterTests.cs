using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5105OpeningParenthesisMustBeOnDeclarationLineFormatterTests : FormatterTestsBase<RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter moves the opening parenthesis onto the declaration line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method
                                 {|#0:(|}int value)
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
                                 Diagnostics(RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH5105MessageFormat));
    }

    #endregion // Tests
}