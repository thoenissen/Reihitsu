using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4127SingleLetterCatchExceptionVariableNamesAnalyzer"/>
/// </summary>
[TestClass]
public class RH4127SingleLetterCatchExceptionVariableNamesAnalyzerTests : AnalyzerTestsBase<RH4127SingleLetterCatchExceptionVariableNamesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a single-letter catch exception variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForCatchExceptionVariable()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        try
                                        {
                                        }
                                        catch (Exception {|#0:e|})
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4127SingleLetterCatchExceptionVariableNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4127MessageFormat));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for a descriptive name, discard identifier, or omitted variable
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForAllowedCatchDeclarations()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        try
                                        {
                                        }
                                        catch (ArgumentException exception)
                                        {
                                        }
                                        catch (InvalidOperationException _)
                                        {
                                        }
                                        catch (Exception)
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
                                        var e = 1;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}