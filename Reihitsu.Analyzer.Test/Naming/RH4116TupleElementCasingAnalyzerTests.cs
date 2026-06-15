using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4116TupleElementCasingAnalyzer"/>
/// </summary>
[TestClass]
public class RH4116TupleElementCasingAnalyzerTests : AnalyzerTestsBase<RH4116TupleElementCasingAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for tuple type elements that are not PascalCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForTupleTypeElementWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int {|#0:firstValue|}, int SecondValue) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4116TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4116MessageFormat));
    }

    /// <summary>
    /// Verifies multiple tuple type elements can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMultipleTupleTypeElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int {|#0:firstValue|}, int {|#1:secondValue|}) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4116TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4116MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase tuple type elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseTupleTypeElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int FirstValue, int SecondValue) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies unnamed tuple elements do not report diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForUnnamedTupleTypeElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int, int) Load()
                                        {
                                            return default;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}