using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0326ExpressionStyleConstructorsShouldNotBeUsedFormatterTests : FormatterTestsBase<RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer>
{
    #region Members

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
                                 ExpectedDiagnostic(RH0326ExpressionStyleConstructorsShouldNotBeUsedAnalyzer.DiagnosticId, 3, 5, 3, 38, AnalyzerResources.RH0326MessageFormat));
    }

    #endregion // Members
}