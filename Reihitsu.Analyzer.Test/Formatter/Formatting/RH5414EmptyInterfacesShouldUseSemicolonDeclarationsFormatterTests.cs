using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5414EmptyInterfacesShouldUseSemicolonDeclarationsFormatterTests : FormatterTestsBase<RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifying that the formatter converts an empty interface to semicolon form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEmptyInterface()
    {
        const string input = """
                             internal interface {|#0:IExample|}
                             {
                             }
                             """;
        const string fixedData = """
                                 internal interface IExample;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 null,
                                 Diagnostics(RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5414MessageFormat));
    }

    /// <summary>
    /// Verifying that the formatter does not delete a comment between the interface header and the open brace
    /// </summary>
    [TestMethod]
    public void VerifyFormatterDoesNotRewriteInterfaceWithLeadingBraceComment()
    {
        const string input = """
                             internal interface IExample
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