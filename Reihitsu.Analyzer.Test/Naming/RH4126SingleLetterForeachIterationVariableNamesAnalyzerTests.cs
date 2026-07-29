using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4126SingleLetterForeachIterationVariableNamesAnalyzer"/>
/// </summary>
[TestClass]
public class RH4126SingleLetterForeachIterationVariableNamesAnalyzerTests : AnalyzerTestsBase<RH4126SingleLetterForeachIterationVariableNamesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a single-letter foreach iteration variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForForeachIterationVariable()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(int[] items)
                                    {
                                        foreach (var {|#0:i|} in items)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4126SingleLetterForeachIterationVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4126MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for deconstructed foreach iteration variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForDeconstructedForeachIterationVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process((int Left, int Top)[] items)
                                    {
                                        foreach (var ({|#0:x|}, {|#1:y|}) in items)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4126SingleLetterForeachIterationVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4126MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a descriptive name or the discard identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDescriptiveNameAndDiscardIdentifier()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(int[] items)
                                    {
                                        foreach (var item in items)
                                        {
                                        }

                                        foreach (var _ in items)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for regular local variables
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForRegularLocalVariables()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        var i = 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies local designations in a foreach body are left to the local-variable rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDesignationsInForeachBody()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(object[] items)
                                    {
                                        foreach (var item in items)
                                        {
                                            if (item is int x)
                                            {
                                            }
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}