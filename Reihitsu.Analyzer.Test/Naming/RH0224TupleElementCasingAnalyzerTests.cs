using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH0224TupleElementCasingAnalyzer"/> and <see cref="RH0224TupleElementCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0224TupleElementCasingAnalyzerTests : AnalyzerTestsBase<RH0224TupleElementCasingAnalyzer, RH0224TupleElementCasingCodeFixProvider>
{
    #region Members

    /// <summary>
    /// Verifies diagnostics are reported for tuple expression element names that are not PascalCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForTupleExpressionElementWrongCasing()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public (int FirstValue, int SecondValue) Load()
                                        {
                                            return ({|#0:firstValue|}: 1, SecondValue: 2);
                                        }
                                    }
                                }
                                """;

        const string fixedCode = """
                                 namespace Reihitsu.Analyzer.Test.Naming.Resources
                                 {
                                     public class DataLoader
                                     {
                                         public (int FirstValue, int SecondValue) Load()
                                         {
                                             return (FirstValue: 1, SecondValue: 2);
                                         }
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0224TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0224MessageFormat));
    }

    /// <summary>
    /// Verifies multiple tuple expression element names can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticsForMultipleTupleExpressionElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public object Load()
                                        {
                                            return ({|#0:firstValue|}: 1, {|#1:secondValue|}: 2);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH0224TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH0224MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase tuple expression element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForPascalCaseTupleExpressionElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public object Load()
                                        {
                                            return (FirstValue: 1, SecondValue: 2);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies unnamed tuple expression elements do not report diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsForUnnamedTupleExpressionElements()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources
                                {
                                    public class DataLoader
                                    {
                                        public object Load()
                                        {
                                            return (1, 2);
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Members
}