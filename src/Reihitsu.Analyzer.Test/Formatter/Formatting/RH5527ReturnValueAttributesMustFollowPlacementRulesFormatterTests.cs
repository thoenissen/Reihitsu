using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[TestClass]
public class RH5527ReturnValueAttributesMustFollowPlacementRulesFormatterTests : FormatterTestsBase<RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer>
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

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5527MessageFormat));
    }

    /// <summary>
    /// Verifies that analyzer-clean single-line property accessor with return specifier stays untouched
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsReturnSpecifierOnSingleLinePropertyAccessor()
    {
        const string input = """
                             internal class Example
                             {
                                 public int Value { [return: First] get; set; }
                             }
                             sealed class FirstAttribute : System.Attribute;
                             sealed class SecondAttribute : System.Attribute;
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the formatter expands a single-line property whose accessor has an expression body and moves the
    /// return specifier onto its own line, which is why the analyzer keeps reporting it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesReturnSpecifierOnSingleLinePropertyAccessorWithExpressionBody()
    {
        const string input = """
                             internal class Example
                             {
                                 public int Value { {|#0:[return: First]|} get => 1; }
                             }
                             sealed class FirstAttribute : System.Attribute;
                             sealed class SecondAttribute : System.Attribute;
                             """;

        const string fixedData = """
                                 internal class Example
                                 {
                                     public int Value
                                     {
                                         [return: First]
                                         get => 1;
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute;
                                 sealed class SecondAttribute : System.Attribute;
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5527MessageFormat));
    }

    #endregion // Tests
}