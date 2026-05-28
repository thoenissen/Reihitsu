using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5530AccessorAttributesMustFollowPlacementRulesFormatterTests : FormatterTestsBase<RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer>
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
                                 Diagnostics(RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5530MessageFormat));
    }

    #endregion // Tests
}