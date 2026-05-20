using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0818EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH0818EmptyRecordsShouldUseSemicolonDeclarationsAnalyzerTests : AnalyzerTestsBase<RH0818EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer, RH0818EmptyRecordsShouldUseSemicolonDeclarationsCodeFixProvider>
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
                     Diagnostics(RH0818EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH0818MessageFormat));
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