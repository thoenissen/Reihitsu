using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0339OperatorKeywordMustBeFollowedBySpaceAnalyzer"/> and <see cref="RH0339OperatorKeywordMustBeFollowedBySpaceCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0339OperatorKeywordMustBeFollowedBySpaceAnalyzerTests : AnalyzerTestsBase<RH0339OperatorKeywordMustBeFollowedBySpaceAnalyzer, RH0339OperatorKeywordMustBeFollowedBySpaceCodeFixProvider>
{
    /// <summary>
    /// Verifies that clean code does not produce diagnostics.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass operator +(TestClass left, TestClass right) => left;
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass {|#0:operator|}+(TestClass left, TestClass right) => left;
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public static TestClass operator +(TestClass left, TestClass right) => left;
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0339OperatorKeywordMustBeFollowedBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH0339MessageFormat));
    }
}