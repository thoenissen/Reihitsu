using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH6009OpeningBracesMustBeSpacedCorrectlyFormatterTests : FormatterTestsBase<RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the targeted violation and clears the analyzer diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Property{|#0:{|} get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Property { get; set; }
                                 }
                                 """;

        await VerifyFormatter(testData,
                              fixedData,
                              Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that the opening brace of an array initializer in an interpolation hole is reported, and that the
    /// formatter writes a space in front of it and clears the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    public async Task VerifyFormatterFixesArrayInitializerBraceInAHole()
    {
        const string testData = """
                                class C
                                {
                                    string s = $"{new int[]{|#0:{|} 1 }.Length}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = $"{new int[] { 1 }.Length}";
                                 }
                                 """;

        await VerifyFormatter(testData, fixedData, Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, MessageFormat));
    }

    #endregion // Tests
}