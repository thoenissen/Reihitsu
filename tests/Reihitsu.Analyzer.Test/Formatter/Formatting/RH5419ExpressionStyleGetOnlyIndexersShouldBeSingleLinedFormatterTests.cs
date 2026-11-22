using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer"/>
/// </summary>
[TestClass]
public class RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedFormatterTests : FormatterTestsBase<RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter joins an arrow below the closing bracket, and an expression below the arrow, onto the
    /// line of the closing bracket, which clears the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyFormatterFixesAWrappedArrowOrExpression()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:internal int this[int i]
                                     => i;|}

                                 {|#1:internal int this[long i] =>
                                     1;|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int this[int i] => i;

                                     internal int this[long i] => 1;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer.DiagnosticId, MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the formatter joins the <c>[</c> onto <c>this</c> and the arrow onto the closing bracket's line, which
    /// clears the diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyFormatterFixesAnArrowBehindALineBreakBetweenThisAndTheOpeningBracket()
    {
        const string input = """
                             internal class Example
                             {
                                 {|#0:int this
                                     [short i]
                                     => i;|}
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     int this[short i] => i;
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzer.DiagnosticId, MessageFormat));
    }

    /// <summary>
    /// Verifies that indexers the rule does not report stay unchanged and unreported after formatting: one on one line, and
    /// one whose expression starts on the closing bracket's line and continues below
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    public async Task VerifyFormatterKeepsUnreportedIndexers()
    {
        const string input = """
                             internal class Example
                             {
                                 int this[uint a, uint b] => 1;

                                 int this[char a, char b] => a
                                                             + b;
                             }
                             """;

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that the expression-bodied indexer the formatter produces from a get-only accessor list is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterProducesAnUnreportedIndexerFromAGetOnlyAccessorList()
    {
        const string input = """
                             internal class Example
                             {
                                 internal int this[int i]
                                 {
                                     get
                                     {
                                         return 42;
                                     }
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal int this[int i] => 42;
                                 }
                                 """;

        await VerifyFormatter(input, fixedData);
    }

    /// <summary>
    /// Verifies that the formatter keeps a parameter list on several lines, which the rule reports: with and without a
    /// comment inside the list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5419-001")]
    [Requirement("SHARED-DIAG-010")]
    public async Task VerifyFormatterKeepsAReportedParameterListOnSeveralLines()
    {
        const string input = """
                             internal class Example
                             {
                                 {|RH5419:int this[byte a,
                                          byte b] => a;|}

                                 {|RH5419:int this[short a, // note
                                          short b] => a;|}
                             }
                             """;

        await VerifyFormatterKeepsReportedShape(input);
    }

    #endregion // Tests
}