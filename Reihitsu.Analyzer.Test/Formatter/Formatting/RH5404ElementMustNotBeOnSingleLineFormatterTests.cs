using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5404ElementMustNotBeOnSingleLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5404ElementMustNotBeOnSingleLineFormatterTests : FormatterTestsBase<RH5404ElementMustNotBeOnSingleLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter expands single-line elements
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             internal class {|#0:Example|} { public void Foo() { } }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public void Foo()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter moves the opening brace onto its own line when a leading using
    /// directive precedes the single-line type (regression test for issue #314)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterMovesOpeningBraceWithLeadingUsing()
    {
        const string input = """
                             using System;

                             [Serializable]
                             internal class {|#0:TestClass|} { private int _value; }
                             """;
        const string fixedData = """
                                 using System;

                                 [Serializable]
                                 internal class TestClass
                                 {
                                     private int _value;
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5404ElementMustNotBeOnSingleLineAnalyzer.DiagnosticId, AnalyzerResources.RH5404MessageFormat));
    }

    #endregion // Tests
}