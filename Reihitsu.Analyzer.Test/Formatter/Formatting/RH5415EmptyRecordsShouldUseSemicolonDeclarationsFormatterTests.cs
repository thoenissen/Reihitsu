using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5415EmptyRecordsShouldUseSemicolonDeclarationsFormatterTests : FormatterTestsBase<RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifying that the formatter converts an empty record to semicolon form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEmptyRecord()
    {
        const string input = """
                             internal record {|#0:Example|}
                             {
                             }
                             """;
        const string fixedData = """
                                 internal record Example;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 null,
                                 Diagnostics(RH5415EmptyRecordsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5415MessageFormat));
    }

    /// <summary>
    /// Verifying that the formatter does not delete a comment between the record header and the open brace
    /// </summary>
    [TestMethod]
    public void VerifyFormatterDoesNotRewriteRecordWithLeadingBraceComment()
    {
        const string input = """
                             internal record Example
                             // why this type is empty
                             {
                             }
                             """;
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var actual = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken)
                                      .GetRoot(TestContext.CancellationToken)
                                      .ToFullString();

        Assert.AreEqual(input, actual);
    }

    #endregion // Tests
}