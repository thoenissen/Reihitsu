using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsFormatterTests : FormatterTestsBase<RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter inserts braces for multi-line child statements
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
                                         {|#0:Other(1,
                                               2);|}
                                 }
                             
                                    void Other(int value1,
                                               int value2)
                                    {
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
                                             Other(1,
                                                   2);
                                         }
                                     }
                                 
                                     void Other(int value1,
                                                int value2)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, AnalyzerResources.RH0373MessageFormat));
    }

    #endregion // Members
}