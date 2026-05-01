using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH0322SingleLineCommentsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter inserts a blank line before the first comment in a comment block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesCommentBlockViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     var value = 1;
                                     // First comment
                                     // Follow-up comment
                                     Consume(value);
                                 }

                                 private void Consume(int value)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var value = 1;

                                         // First comment
                                         // Follow-up comment
                                         Consume(value);
                                     }

                                     private void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0322SingleLineCommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 25, AnalyzerResources.RH0322MessageFormat));
    }

    #endregion // Members
}