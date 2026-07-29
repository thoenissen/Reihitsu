using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4125SingleLetterLocalVariableNamesAnalyzer"/>
/// </summary>
[TestClass]
public class RH4125SingleLetterLocalVariableNamesAnalyzerTests : AnalyzerTestsBase<RH4125SingleLetterLocalVariableNamesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for local variable declarators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForLocalVariableDeclarators()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        var {|#0:x|} = 1;

                                        for (var {|#1:i|} = 0; i < x; i++)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4125SingleLetterLocalVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4125MessageFormat, 2));
    }

    /// <summary>
    /// Verifies diagnostics are reported for out, pattern, and deconstruction variable designations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForVariableDesignations()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(object value)
                                    {
                                        _ = int.TryParse("1", out var {|#0:x|});

                                        if (value is int {|#1:y|})
                                        {
                                        }

                                        var ({|#2:l|}, {|#3:t|}) = GetCoordinates();
                                    }

                                    private static (int Left, int Top) GetCoordinates()
                                    {
                                        return (1, 2);
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4125SingleLetterLocalVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4125MessageFormat, 4));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for descriptive local names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDescriptiveLocalNames()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(object value)
                                    {
                                        var count = 1;
                                        _ = int.TryParse("1", out var result);

                                        if (value is int number)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for discard forms
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForDiscards()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        var _ = 1;
                                        int.TryParse("1", out _);
                                        var (_, value) = GetCoordinates();
                                    }

                                    private static (int Left, int Top) GetCoordinates()
                                    {
                                        return (1, 2);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for fields
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForFields()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    private int x;
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}