using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0377OpeningParenthesisMustBeOnDeclarationLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0377OpeningParenthesisMustBeOnDeclarationLineFormatterTests : FormatterTestsBase<RH0377OpeningParenthesisMustBeOnDeclarationLineAnalyzer>
{
    #region Members

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
                                 Diagnostics(RH0377OpeningParenthesisMustBeOnDeclarationLineAnalyzer.DiagnosticId, AnalyzerResources.RH0377MessageFormat));
    }

    #endregion // Members
}