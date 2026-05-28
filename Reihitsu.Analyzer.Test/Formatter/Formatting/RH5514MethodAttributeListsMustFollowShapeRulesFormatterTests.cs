using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5514MethodAttributeListsMustFollowShapeRulesFormatterTests : FormatterTestsBase<RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer>
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
                                    {|#0:[First, Second]|}
                                    internal void M() { }
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
                                     [First]
                                     [Second]
                                     internal void M()
                                     {
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute;
                                 sealed class SecondAttribute : System.Attribute;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5514MethodAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5514MessageFormat));
    }

    #endregion // Tests
}