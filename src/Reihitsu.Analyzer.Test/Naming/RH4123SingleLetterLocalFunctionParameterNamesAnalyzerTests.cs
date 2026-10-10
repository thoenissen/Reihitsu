using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4123SingleLetterLocalFunctionParameterNamesAnalyzer"/>
/// </summary>
[TestClass]
public class RH4123SingleLetterLocalFunctionParameterNamesAnalyzerTests : AnalyzerTestsBase<RH4123SingleLetterLocalFunctionParameterNamesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a single-letter local function parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForLocalFunctionParameter()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public int Process()
                                    {
                                        static int Double(int {|#0:n|}) => n * 2;

                                        return Double(1);
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4123SingleLetterLocalFunctionParameterNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4123MessageFormat));
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
                                    public void Process()
                                    {
                                        static int Double(int value) => value * 2;
                                        static void Ignore(int _)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for method or anonymous function parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForOtherParameterKinds()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(int p)
                                    {
                                        Func<int, int> lambda = n => n;
                                        Func<int, int> anonymousMethod = delegate(int a) { return a; };
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}