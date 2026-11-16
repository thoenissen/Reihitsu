using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer"/>
/// </summary>
[TestClass]
public class RH5418EmptyUnionsShouldUseSemicolonDeclarationsFormatterTests : FormatterTestsBase<RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter converts an empty union, top-level and nested, to the semicolon declaration, so the output
    /// carries no diagnostic, and that a second pass leaves it unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-STRUCT-017")]
    [Requirement("RH5418-001")]
    public async Task VerifyFormatterFixesEmptyUnion()
    {
        const string input = """
                             internal union {|#0:Example|}(int, string)
                             {
                             }

                             internal class Outer
                             {
                                 private union {|#1:Inner|}(long, string) { }
                             }
                             """;
        const string fixedData = """
                                 internal union Example(int, string);

                                 internal class Outer
                                 {
                                     private union Inner(long, string);
                                 }
                                 """;

        var unionSupport = await UnionSupport.GetReferenceAsync();

        await VerifyFormatter(input,
                              fixedData,
                              test => test.TestState.AdditionalReferences.Add(unionSupport),
                              Diagnostics(RH5418EmptyUnionsShouldUseSemicolonDeclarationsAnalyzer.DiagnosticId, AnalyzerResources.RH5418MessageFormat, 2));
    }

    #endregion // Tests
}