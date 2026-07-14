using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsFormatterTests : FormatterTestsBase<RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifying that the formatter converts an empty record struct to semicolon form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEmptyRecordStruct()
    {
        const string input = """
                             internal record struct {|#0:Example|}
                             {
                             }
                             """;
        const string fixedData = """
                                 internal record struct Example;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 null,
                                 Diagnostics(RH5416EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5416MessageFormat));
    }

    /// <summary>
    /// Verifying that the formatter does not rewrite directive-containing record structs
    /// </summary>
    [TestMethod]
    public void VerifyFormatterDoesNotRewriteDirectiveContainingRecordStruct()
    {
        const string input = """
                             internal record struct Example
                             {
                             #if DEBUG
                             #endif
                             }
                             """;
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var actual = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken)
                                      .GetRoot(TestContext.CancellationToken)
                                      .ToFullString();

        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifying that the formatter does not delete a comment between the record struct header and the open brace
    /// </summary>
    [TestMethod]
    public void VerifyFormatterDoesNotRewriteRecordStructWithLeadingBraceComment()
    {
        const string input = """
                             internal record struct Example
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

    /// <summary>
    /// Verifying that the formatter does not rewrite record structs for unsupported language versions
    /// </summary>
    [TestMethod]
    public void VerifyFormatterDoesNotRewriteRecordStructForUnsupportedLanguageVersion()
    {
        const string input = """
                             internal record struct Example
                             {
                             }
                             """;
        var tree = CSharpSyntaxTree.ParseText(input, new CSharpParseOptions(LanguageVersion.CSharp9), cancellationToken: TestContext.CancellationToken);
        var actual = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken)
                                      .GetRoot(TestContext.CancellationToken)
                                      .ToFullString();

        Assert.AreEqual(input, actual);
    }

    #endregion // Tests
}