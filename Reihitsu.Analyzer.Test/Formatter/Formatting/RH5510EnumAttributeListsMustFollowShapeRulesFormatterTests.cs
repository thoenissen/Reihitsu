using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5510EnumAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5510EnumAttributeListsMustFollowShapeRulesFormatterTests : FormatterTestsBase<RH5510EnumAttributeListsMustFollowShapeRulesAnalyzer>
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
                                internal enum Example { Value }
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
                                 internal enum Example
                                 {
                                     Value
                                 }
                                 sealed class FirstAttribute : System.Attribute;
                                 sealed class SecondAttribute : System.Attribute;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5510EnumAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5510MessageFormat));
    }

    #endregion // Tests
}