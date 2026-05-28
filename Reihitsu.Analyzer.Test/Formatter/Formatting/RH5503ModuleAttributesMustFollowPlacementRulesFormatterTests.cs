using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5503ModuleAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5503ModuleAttributesMustFollowPlacementRulesFormatterTests : FormatterTestsBase<RH5503ModuleAttributesMustFollowPlacementRulesAnalyzer>
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
                                {|#0:[module: First]|} internal class Example { }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                
                             """;
        const string fixedData = """
                                 [module: First]
                                 internal class Example;
                                 sealed class FirstAttribute : System.Attribute;
                                 sealed class SecondAttribute : System.Attribute;
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5503ModuleAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5503MessageFormat));
    }

    #endregion // Tests
}