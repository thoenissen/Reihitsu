using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzerTests : AnalyzerTestsBase<RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer, RH5415EmptyRecordsShouldUseSemicolonDeclarationsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying that an empty record is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyEmptyRecordIsDetectedAndFixed()
    {
        const string testData = """
                                internal record {|#0:Example|}
                                {
                                }
                                """;
        const string fixedData = """
                                 internal record Example;
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5415MessageFormat));
    }

    /// <summary>
    /// Verifying that semicolon record declarations are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySemicolonRecordIsNotFlagged()
    {
        const string testData = """
                                internal record Example;
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}