using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Tests of the formatter-parity helpers of <see cref="FormatterTestsBase{TAnalyzer}"/>: a helper that expects the formatter
/// to keep its input fails when the formatter refuses that input as unsupported, because a refused input comes back unchanged
/// and would otherwise pass. <see cref="RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer"/> stands in for any analyzer;
/// it reports nothing on the fixtures
/// </summary>
[TestClass]
public class FormatterTestsBaseTests : FormatterTestsBase<RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer>
{
    #region Constants

    /// <summary>
    /// The reason the formatter gives for a comment inside a variable declaration
    /// </summary>
    private const string VariableDeclarationCommentReason = "a comment between a variable declaration's own tokens";

    /// <summary>
    /// Compiling source the formatter refuses as unsupported, for a comment inside a field declaration, and that the
    /// analyzer does not report
    /// </summary>
    private const string RefusedSource = """
                                         internal class TestClass
                                         {
                                             private int /* comment */ _value;
                                         }
                                         """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase{TAnalyzer}.VerifyFormatter(string)"/> fails, naming the refusal, when the
    /// formatter refuses analyzer-clean source as unsupported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFailsWhenTheFormatterRefusesTheSource()
    {
        var exception = await Assert.ThrowsExactlyAsync<AssertFailedException>(() => VerifyFormatter(RefusedSource));

        Assert.Contains(VariableDeclarationCommentReason, exception.Message);
    }

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase{TAnalyzer}.VerifyFormatterKeepsReportedShape(string)"/> fails, naming the
    /// refusal, when the formatter refuses the source as unsupported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsReportedShapeFailsWhenTheFormatterRefusesTheSource()
    {
        var exception = await Assert.ThrowsExactlyAsync<AssertFailedException>(() => VerifyFormatterKeepsReportedShape(RefusedSource));

        Assert.Contains(VariableDeclarationCommentReason, exception.Message);
    }

    /// <summary>
    /// Verifies that <see cref="FormatterTestsBase{TAnalyzer}.VerifyFormatter(string)"/> still passes for the same source
    /// without the comment, which the formatter does not refuse and keeps as written
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterPassesWhenTheFormatterKeepsTheSource()
    {
        await VerifyFormatter(RefusedSource.Replace("/* comment */ ", string.Empty));
    }

    #endregion // Tests
}