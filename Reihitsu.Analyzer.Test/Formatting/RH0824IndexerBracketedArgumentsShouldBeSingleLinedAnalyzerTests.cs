using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0824IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer"/> and <see cref="RH0824IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0824IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests : AnalyzerTestsBase<RH0824IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer, RH0824IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that multiline indexer bracketed arguments are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForMultilineIndexerArguments()
    {
        const string testData = """
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

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH0824IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH0824MessageFormat));
    }

    /// <summary>
    /// Verifies that single-line indexer bracketed arguments remain valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineIndexerArguments()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[,] matrix)
                                    {
                                        return matrix[1, 2];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiline indexer arguments with comments are ignored because they are not safely fixable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMultilineIndexerArgumentsWithComments()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[,] matrix)
                                    {
                                        return matrix[
                                            1, // keep
                                            2
                                        ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiline indexer arguments with directives are ignored because they are not safely fixable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMultilineIndexerArgumentsWithDirectives()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[,] matrix)
                                    {
                                        return matrix[
                                                       #if DEBUG
                                                           1,
                                                       #else
                                                           2,
                                                       #endif
                                                           3
                                                   ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that list patterns are not analyzed by this rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForListPattern()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static bool Method(int[] values)
                                    {
                                        return values is [
                                            1,
                                            2
                                        ];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}