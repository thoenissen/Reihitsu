using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0824IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0824IndexerBracketedArgumentsShouldBeSingleLinedFormatterTests : FormatterTestsBase<RH0824IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter collapses multiline indexer arguments to a single line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMultilineIndexerArguments()
    {
        const string input = """
                             internal class Example
                             {
                                 private static int Method(int[,] matrix)
                                 {
                                     return {|#0:matrix[
                                         1,
                                         2]|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static int Method(int[,] matrix)
                                     {
                                         return matrix[1, 2];
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0824IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH0824MessageFormat));
    }

    #endregion // Tests
}