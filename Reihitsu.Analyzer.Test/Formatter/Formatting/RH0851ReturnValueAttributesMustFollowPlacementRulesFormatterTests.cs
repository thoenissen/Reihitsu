using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0851ReturnValueAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH0851ReturnValueAttributesMustFollowPlacementRulesFormatterTests : FormatterTestsBase<RH0851ReturnValueAttributesMustFollowPlacementRulesAnalyzer>
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
                                 {|#0:[return: First]|} internal int M()
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
                                 Diagnostics(RH0851ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH0851MessageFormat));
    }

    #endregion // Tests
}