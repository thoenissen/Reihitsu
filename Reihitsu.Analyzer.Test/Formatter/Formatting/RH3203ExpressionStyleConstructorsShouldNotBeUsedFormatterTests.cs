using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH3203ExpressionStyleConstructorsShouldNotBeUsedFormatterTests : FormatterTestsBase<RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter converts expression-bodied constructors into block-bodied constructors
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal Example() => Value = 42;

                                 internal int Value { get; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal Example()
                                     {
                                         Value = 42;
                                     }

                                     internal int Value { get; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH3203ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, 3, 5, 3, 38, AnalyzerResources.RH3203MessageFormat));
    }

    #endregion // Tests
}