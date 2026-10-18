using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4118TupleElementCasingAnalyzer"/>
/// </summary>
[TestClass]
public class RH4118TupleElementCasingAnalyzerTests : AnalyzerTestsBase<RH4118TupleElementCasingAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies diagnostics are reported for tuple expression element names that are not PascalCase
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4118-001")]
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

        await Verify(testCode, Diagnostics(RH4118TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4118MessageFormat));
    }

    /// <summary>
    /// Verifies multiple tuple expression element names can produce multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4118-001")]
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

        await Verify(testCode, Diagnostics(RH4118TupleElementCasingAnalyzer.DiagnosticId, AnalyzerResources.RH4118MessageFormat, 2));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for PascalCase tuple expression element names
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4118-001")]
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
    [Requirement("RH4118-001")]
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

    /// <summary>
    /// Verifies names written in a tuple expression are reported, including those of a nested tuple expression, while unnamed elements and names inferred from a variable are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4118-001")]
    public async Task WrittenTupleExpressionElementNamesAreChecked()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int count)
                                    {
                                        var a = ({|RH4118:count|}: 1, Total: 2, 3);
                                        var b = (count, Total: count);
                                        var c = ({|RH4118:outer|}: ({|RH4118:inner|}: 1, Other: 2), 3);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies a name written as a verbatim identifier in a tuple expression is reported, with the verbatim prefix inside the location
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4118-001")]
    public async Task TupleExpressionElementNamedWithVerbatimIdentifierIsReportedIncludingThePrefix()
    {
        const string testCode = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = ({|RH4118:@value|}: 1, Total: 2);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}