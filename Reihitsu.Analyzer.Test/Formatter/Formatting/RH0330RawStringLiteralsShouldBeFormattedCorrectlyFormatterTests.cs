using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0330RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0330RawStringLiteralsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH0330RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter realigns multi-line raw string literals
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        var input = """"
                    using System;

                    internal class Example
                    {
                        internal void Method()
                        {
                            var value = """
                        Text
                        """;
                        }
                    }
                    """";
        var fixedData = """"
                        using System;

                        internal class Example
                        {
                            internal void Method()
                            {
                                var value = """
                                            Text
                                            """;
                            }
                        }
                        """";

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0330RawStringLiteralsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, 7, 21, 9, 8, AnalyzerResources.RH0330MessageFormat));
    }

    #endregion // Members
}