using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5412EmptyClassesShouldUseSemicolonDeclarationsFormatterTests : FormatterTestsBase<RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Properties

    /// <summary>
    /// Test context
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifying that the formatter converts an empty class to semicolon form
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEmptyClass()
    {
        const string input = """
                             internal class {|#0:Example|}
                             {
                             }
                             """;
        const string fixedData = """
                                 internal class Example;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 null,
                                 Diagnostics(RH5412EmptyClassesShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5412MessageFormat));
    }

    /// <summary>
    /// Verifying that the formatter does not rewrite commented classes
    /// </summary>
    [TestMethod]
    public void VerifyFormatterDoesNotRewriteCommentedClass()
    {
        const string input = """
                             internal class Example
                             {
                                 // Comment
                             }
                             """;
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var actual = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken)
                                      .GetRoot(TestContext.CancellationToken)
                                      .ToFullString();

        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifying that the formatter does not delete a comment between the class header and the open brace
    /// </summary>
    [TestMethod]
    public void VerifyFormatterDoesNotRewriteClassWithLeadingBraceComment()
    {
        const string input = """
                             internal class Example
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
    /// Verifying that the formatter does not rewrite classes for unsupported language versions
    /// </summary>
    [TestMethod]
    public void VerifyFormatterDoesNotRewriteClassForUnsupportedLanguageVersion()
    {
        const string input = """
                             internal class Example
                             {
                             }
                             """;
        var tree = CSharpSyntaxTree.ParseText(input, new CSharpParseOptions(LanguageVersion.CSharp11), cancellationToken: TestContext.CancellationToken);
        var actual = ReihitsuFormatter.FormatSyntaxTree(tree, TestContext.CancellationToken)
                                      .GetRoot(TestContext.CancellationToken)
                                      .ToFullString();

        Assert.AreEqual(input, actual);
    }

    #endregion // Tests
}