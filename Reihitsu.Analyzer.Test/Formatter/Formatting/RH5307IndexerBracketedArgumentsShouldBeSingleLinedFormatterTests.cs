using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5307IndexerBracketedArgumentsShouldBeSingleLinedFormatterTests : FormatterTestsBase<RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer>
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
                                 Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat));
    }

    #endregion // Tests
}