using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5528ReturnValueAttributeListsMustFollowShapeRulesFormatterTests : FormatterTestsBase<RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter fixes the rule violation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesRuleViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:[return: First, Second]|}
                                 internal int M()
                                 {
                                     return 0;
                                 }
                             }
                             sealed class FirstAttribute : System.Attribute
                             {
                             }
                             sealed class SecondAttribute : System.Attribute
                             {
                             }
                             """;

        const string fixedData = """
                                 internal class Example
                                 {
                                     [return: First]
                                     [return: Second]
                                     internal int M()
                                     {
                                         return 0;
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute;
                                 sealed class SecondAttribute : System.Attribute;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5528ReturnValueAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5528MessageFormat));
    }

    #endregion // Tests
}