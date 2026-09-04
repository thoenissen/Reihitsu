using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer"/> and <see cref="RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzerTests : BatchCodeFixTestsBase<RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer, RH5307IndexerBracketedArgumentsShouldBeSingleLinedCodeFixProvider>
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
                     Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment written behind the closing bracket does not suppress the diagnostic or
    /// the code fix. The comment sits outside the interior the fix rewrites, so it is not a join
    /// barrier (issue #610)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForMultilineIndexerArgumentsWithTrailingComment()
    {
        const string testData = """
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

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment written between the brackets still suppresses the diagnostic, because
    /// the collapse would have to cross it (issue #610)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForIndexerArgumentsWithInteriorComment()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[] values)
                                    {
                                        return values[ /* note */
                                                      0] + 2;
                                    }
                                }
                                """;

        await Verify(testData);
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

    /// <summary>
    /// Verifies that multiline indexer arguments carrying a documentation comment are ignored, because the
    /// formatter refuses to collapse across it and the fix would otherwise never converge (issue #420)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMultilineIndexerArgumentsWithDocumentationComment()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static int Method(int[] values)
                                    {
                                        return values[
                                            /// <summary>
                                            /// index
                                            /// </summary>
                                            0];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    private static int Method(int[] values1, int[] values2)
                                    {
                                        return {|#0:values1[
                                            0]|} + {|#1:values2[
                                            1]|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     private static int Method(int[] values1, int[] values2)
                                     {
                                         return values1[0] + values2[1];
                                     }
                                 }
                                 """;

        // The two indexer accesses sit side by side in one return statement, so both resolve to the identical
        // fix target: the enclosing return statement. The batch fixer discards the second, overlapping action,
        // and the surviving single fix already collapses both bracketed argument lists to a single line
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer.DiagnosticId, AnalyzerResources.RH5307MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}