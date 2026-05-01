using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0392StatementLambdaOpeningBraceShouldBeAlignedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0392StatementLambdaOpeningBraceShouldBeAlignedFormatterTests : FormatterTestsBase<RH0392StatementLambdaOpeningBraceShouldBeAlignedAnalyzer>
{
    #region Members

    /// <summary>
    /// Verifies that the formatter aligns statement lambda opening braces with the lambda anchor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesViolation()
    {
        const string input = """
                             using System;

                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     Func<object, object> projector = obj =>
                                                                                       {
                                                                                           return obj;
                                                                                       };
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System;

                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         Func<object, object> projector = obj =>
                                                                          {
                                                                              return obj;
                                                                          };
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH0392StatementLambdaOpeningBraceShouldBeAlignedAnalyzer.DiagnosticId, 8, 59, 8, 60, AnalyzerResources.RH0392MessageFormat));
    }

    #endregion // Members
}