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

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses multiline indexer arguments that carry a comment behind
    /// the closing bracket, and that analyzer and formatter agree on that decision under both line
    /// endings (issue #610)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMultilineIndexerArgumentsWithTrailingComment()
    {
        const string input = """
                             internal class Example
                             {
                                 private static int Method(int[] values)
                                 {
                                     return {|#0:values[
                                         0]|} /* note */ + 2;
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static int Method(int[] values)
                                     {
                                         return values[0] /* note */ + 2;
                                     }
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves an indexer argument list alone when a comment sits between
    /// the brackets, because that gap is crossed by the collapse (issue #610)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsIndexerArgumentsWithInteriorComment()
    {
        const string input = """
                             internal class Example
                             {
                                 private static int Method(int[] values)
                                 {
                                     return values[ /* note */
                                                   0] + 2;
                                 }
                             }
                             """;

        await VerifyFormatterStability(input);
    }

    #endregion // Tests
}