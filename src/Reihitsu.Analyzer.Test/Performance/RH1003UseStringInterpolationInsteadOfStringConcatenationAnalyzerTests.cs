using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Performance;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Performance;

/// <summary>
/// Test methods for <see cref="RH1003UseStringInterpolationInsteadOfStringConcatenationAnalyzer"/>
/// </summary>
[TestClass]
public class RH1003UseStringInterpolationInsteadOfStringConcatenationAnalyzerTests : AnalyzerTestsBase<RH1003UseStringInterpolationInsteadOfStringConcatenationAnalyzer>
{
    #region Methods

    /// <summary>
    /// Verifying that non-constant string concatenation chains are reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNonConstantStringConcatenationChainIsReported()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                internal class RH1003
                                {
                                    public string GetMessage(string userName, int userId, string role)
                                    {
                                        return {|#0:"User: " + userName + ", Id: " + userId + ", Role: " + role|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH1003UseStringInterpolationInsteadOfStringConcatenationAnalyzer.DiagnosticId, AnalyzerResources.RH1003MessageFormat));
    }

    /// <summary>
    /// Verifying that compile-time constant string concatenation chains are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCompileTimeConstantStringConcatenationChainIsIgnored()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                internal class RH1003
                                {
                                    public string GetMessage()
                                    {
                                        const string prefix = "User";
                                        const string suffix = "Name";

                                        return prefix + ": " + suffix;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that two-part string concatenations are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTwoPartStringConcatenationIsIgnored()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                internal class RH1003
                                {
                                    public string GetMessage(string prefix, string name)
                                    {
                                        return prefix + name;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that nested add expressions inside a single concatenation chain only report once
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNestedConcatenationChainOnlyReportsOnce()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                internal class RH1003
                                {
                                    public string GetMessage(string prefix, string name, string suffix)
                                    {
                                        return {|#0:(prefix + name) + suffix|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH1003UseStringInterpolationInsteadOfStringConcatenationAnalyzer.DiagnosticId, AnalyzerResources.RH1003MessageFormat));
    }

    /// <summary>
    /// Verifying that non-string addition expressions are ignored
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNonStringAdditionExpressionIsIgnored()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                internal class RH1003
                                {
                                    public int GetValue(int left, int right)
                                    {
                                        return left + right;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifying that constant nested expressions within a reported chain do not prevent reporting
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStringConcatenationChainWithConstantSubExpressionIsReported()
    {
        const string testData = """
                                namespace Reihitsu.Analyzer.Test.Performance.Resources;

                                internal class RH1003
                                {
                                    public string GetMessage(string name)
                                    {
                                        return {|#0:"Hello " + (1 + 2) + name|};
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH1003UseStringInterpolationInsteadOfStringConcatenationAnalyzer.DiagnosticId, AnalyzerResources.RH1003MessageFormat));
    }

    #endregion // Methods
}