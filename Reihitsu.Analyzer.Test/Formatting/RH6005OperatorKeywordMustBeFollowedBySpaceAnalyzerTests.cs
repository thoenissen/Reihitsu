using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer"/> and <see cref="RH6005OperatorKeywordMustBeFollowedBySpaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzerTests : AnalyzerTestsBase<RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer, RH6005OperatorKeywordMustBeFollowedBySpaceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testData, fixedData, Diagnostics(RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer.DiagnosticId, AnalyzerResources.RH6005MessageFormat));
    }

    #endregion // Tests
}