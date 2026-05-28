using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5531AccessorAttributeListsMustFollowShapeRulesFormatterTests : FormatterTestsBase<RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that single-line accessor attribute lists are merged into one list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterMergesSingleLineAccessorAttributeLists()
    {
        const string input = """
                             sealed class FirstAttribute : System.Attribute;
                             sealed class SecondAttribute : System.Attribute;
                             internal class Example
                             {
                                 internal int Value { [First] {|#0:[Second]|} get; set; }
                             }
                             """;

        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute;
                                 sealed class SecondAttribute : System.Attribute;
                                 internal class Example
                                 {
                                     internal int Value { [First, Second] get; set; }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat));
    }

    #endregion // Tests
}