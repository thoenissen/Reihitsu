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
public class RH4110ConstFieldCasingAnalyzerTests : BatchCodeFixTestsBase<RH4110ConstFieldCasingAnalyzer, RH4110ConstFieldCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for const fields that are not PascalCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4110-001")]
    [Requirement("RH4110-002")]
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
    [Requirement("RH4110-001")]
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
    [Requirement("RH4110-001")]
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

    /// <summary>
    /// Verifies that every declarator of public, private, and internal constants is checked, while fields that are not constants and local constants are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4110-001")]
    public async Task ConstantsOfEveryAccessibilityAreChecked()
    {
        const string testCode = """
                                public class C
                                {
                                    public const int MaxRetries = 3;
                                    public const int {|RH4110:maxItems|} = 10;
                                    private const int {|RH4110:retryDelay|} = 5, {|RH4110:timeout_ms|} = 100;
                                    internal const int {|RH4110:MAX_PAGES|} = 4;
                                    private int count;

                                    public void M()
                                    {
                                        const int localLimit = 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the fix renames constants to PascalCase together with the use in the initializer of another constant
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4110-002")]
    public async Task FixRenamesConstantAndEveryReferenceInAnotherInitializer()
    {
        const string testCode = """
                                public class Limits
                                {
                                    private const int {|RH4110:maxItems|} = 10;
                                    private const int {|RH4110:maxPages|} = maxItems / 2;
                                }
                                """;

        const string fixedCode = """
                                 public class Limits
                                 {
                                     private const int MaxItems = 10;
                                     private const int MaxPages = MaxItems / 2;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the fix joins shouted and lowercase words separated by underscores and uppercases the first letter of a single word
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-CASING-006")]
    [Requirement("RH4110-002")]
    public async Task FixDerivesPascalCaseFromTheWordsOfTheOldName()
    {
        const string testCode = """
                                public class C
                                {
                                    private const int {|RH4110:MAX_RETRIES|} = 3;
                                    private const int {|RH4110:retry_delay|} = 5;
                                    private const int {|RH4110:timeout|} = 7;

                                    public int Sum => MAX_RETRIES + retry_delay + timeout;
                                }
                                """;

        const string fixedCode = """
                                 public class C
                                 {
                                     private const int MaxRetries = 3;
                                     private const int RetryDelay = 5;
                                     private const int Timeout = 7;

                                     public int Sum => MaxRetries + RetryDelay + Timeout;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class Limits
                                    {
                                        private const int {|#0:maxItems|} = 10;

                                        private const int {|#1:maxPages|} = maxItems / 2;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class Limits
                                     {
                                         private const int MaxItems = 10;

                                         private const int MaxPages = MaxItems / 2;
                                     }
                                 }
                                 """;

        // The second constant is initialized from the first, so one rename has to reach the other
        // declaration's initializer
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH4110ConstFieldCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4110MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}