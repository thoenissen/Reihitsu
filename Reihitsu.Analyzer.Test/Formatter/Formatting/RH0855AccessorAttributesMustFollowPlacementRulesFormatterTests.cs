using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0855AccessorAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH0855AccessorAttributesMustFollowPlacementRulesFormatterTests : FormatterTestsBase<RH0855AccessorAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter resolves accessor placement violations
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesAccessorAttributePlacement()
    {
        const string input = """
                             sealed class FirstAttribute : System.Attribute;
                             internal class Example
                             {
                                 internal int Value
                                 {
                                     {|#0:[First]|} get;
                                     set;
                                 }
                             }
                             """;
        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute;
                                 internal class Example
                                 {
                                     internal int Value { [First] get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0855AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH0855MessageFormat));
    }

    #endregion // Tests
}