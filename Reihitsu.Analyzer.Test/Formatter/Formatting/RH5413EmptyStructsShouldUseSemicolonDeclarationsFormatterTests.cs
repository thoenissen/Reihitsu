using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5413EmptyStructsShouldUseSemicolonDeclarationsFormatterTests : FormatterTestsBase<RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifying that the formatter converts an empty struct to semicolon form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEmptyStruct()
    {
        const string input = """
                             internal struct {|#0:Example|}
                             {
                             }
                             """;
        const string fixedData = """
                                 internal struct Example;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 null,
                                 Diagnostics(RH5413EmptyStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5413MessageFormat));
    }

    /// <summary>
    /// Verifying that the formatter does not delete a comment between the struct header and the open brace
    /// </summary>
    [TestMethod]
    public void VerifyFormatterDoesNotRewriteStructWithLeadingBraceComment()
    {
        const string input = """
                             internal struct Example
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