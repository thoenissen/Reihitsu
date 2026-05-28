using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5509EnumAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5509EnumAttributesMustFollowPlacementRulesFormatterTests : FormatterTestsBase<RH5509EnumAttributesMustFollowPlacementRulesAnalyzer>
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
                                {|#0:[First]|} internal enum Example { Value }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                
                             """;
        const string fixedData = """
                                 [First]
                                 internal enum Example
                                 {
                                     Value
                                 }
                                 sealed class FirstAttribute : System.Attribute;
                                 sealed class SecondAttribute : System.Attribute;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5509EnumAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5509MessageFormat));
    }

    #endregion // Tests
}