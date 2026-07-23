using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4117DeconstructionVariableCasingAnalyzer"/> and <see cref="RH4117DeconstructionVariableCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH4117DeconstructionVariableCasingAnalyzerTests : AnalyzerTestsBase<RH4117DeconstructionVariableCasingAnalyzer, RH4117DeconstructionVariableCasingCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for deconstruction variables that are not camelCase and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForDeconstructionVariableAndReferenceAreFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            var ({|#0:ResultCount|}, retryCount) = (42, 2);

                                            return ResultCount + retryCount;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load()
                                         {
                                             var (resultCount, retryCount) = (42, 2);

                                             return resultCount + retryCount;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4117DeconstructionVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4117MessageFormat));
    }

    /// <summary>
    /// Verifies multiple deconstruction variables can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMultipleDeconstructionVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            var ({|#0:ResultCount|}, {|#1:RetryCount|}) = (42, 2);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4117DeconstructionVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4117MessageFormat, 2));
    }

    /// <summary>
    /// Verifies diagnostics are reported for variables in nested deconstructions and that references are renamed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForNestedDeconstructionVariableAndReferenceIsFixed()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public int Load()
                                        {
                                            var (outerValue, ({|#0:ResultCount|}, innerValue)) = (1, (2, 3));

                                            return outerValue + ResultCount + innerValue;
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public int Load()
                                         {
                                             var (outerValue, (resultCount, innerValue)) = (1, (2, 3));

                                             return outerValue + resultCount + innerValue;
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH4117DeconstructionVariableCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4117MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for camelCase deconstruction variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForCamelCaseDeconstructionVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public void Load()
                                        {
                                            var (resultCount, retryCount) = (42, 2);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}