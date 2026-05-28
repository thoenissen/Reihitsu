using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5506ClassAttributeListsMustFollowShapeRulesFormatterTests : FormatterTestsBase<RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer>
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
                                {|#0:[First, Second]|}
                                internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                
                             """;
        const string fixedData = """
                                 [First]
                                 [Second]
                                 internal class Example;
                                 sealed class FirstAttribute : System.Attribute;
                                 sealed class SecondAttribute : System.Attribute;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5506ClassAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5506MessageFormat));
    }

    #endregion // Tests
}