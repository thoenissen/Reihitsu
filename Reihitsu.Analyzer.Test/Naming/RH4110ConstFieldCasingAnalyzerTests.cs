using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4110ConstFieldCasingAnalyzer"/> and <see cref="RH4110ConstFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4110ConstFieldCasingAnalyzerTests : AnalyzerTestsBase<RH4110ConstFieldCasingAnalyzer, RH4110ConstFieldCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for const fields that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForConstFieldAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class RetryPolicy
                                    {
                                        public const int {|#0:maxRetries|} = 3;

                                        public int GetLimit()
                                        {
                                            return maxRetries;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class RetryPolicy
                                     {
                                         public const int MaxRetries = 3;

                                         public int GetLimit()
                                         {
                                             return MaxRetries;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4110ConstFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4110MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase const fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseConstField()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class RetryPolicy
                                    {
                                        private const int MaxRetries = 3;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies multiple const fields can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMultipleConstFields()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class RetryPolicy
                                    {
                                        private const int {|#0:maxRetries|} = 3;
                                        private const int {|#1:retryDelay|} = 5;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4110ConstFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4110MessageFormat, 2));
    }

    #endregion // Tests
}