using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5104CommentsMustBeOnTheirOwnLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5104CommentsMustBeOnTheirOwnLineFormatterTests : FormatterTestsBase<RH5104CommentsMustBeOnTheirOwnLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter moves inline comments onto their own line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 void Method(bool left, bool right)
                                 {
                                     if (left == right) {|#0:// Compare the values.|}
                                     {
                                         return;
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     void Method(bool left, bool right)
                                     {
                                         // Compare the values.
                                         if (left == right)
                                         {
                                             return;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5104CommentsMustBeOnTheirOwnLineAnalyzer.DiagnosticId, AnalyzerResources.RH5104MessageFormat));
    }

    #endregion // Tests
}