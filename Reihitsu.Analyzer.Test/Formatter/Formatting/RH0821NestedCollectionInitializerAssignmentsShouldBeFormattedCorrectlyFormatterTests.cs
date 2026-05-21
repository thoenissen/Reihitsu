using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH0821NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer"/>
/// </summary>
[TestClass]
public class RH0821NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyFormatterTests : FormatterTestsBase<RH0821NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter moves the nested collection opening brace onto the assignment line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesOpeningBraceOnNextLineAfterAssignment()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 public List<int> Values { get; set; }

                                 private static void Method()
                                 {
                                     var value = {|#0:new Example
                                                 {
                                                     Values =
                                                     {
                                                         1,
                                                         2
                                                     }
                                                 }|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 
                                 internal class Example
                                 {
                                     public List<int> Values { get; set; }
 
                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      1,
                                                                      2
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0821NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0821MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter separates multiline nested collection elements onto individual lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesMultipleElementsOnSameLineInMultilineInitializer()
    {
        const string input = """
                             using System.Collections.Generic;

                             internal class Example
                             {
                                 public List<int> Values { get; set; }

                                 private static void Method()
                                 {
                                     var value = {|#0:new Example
                                                 {
                                                     Values = {
                                                         1, 2,
                                                         3
                                                               }
                                                 }|};
                                 }
                             }
                             """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 
                                 internal class Example
                                 {
                                     public List<int> Values { get; set; }
 
                                     private static void Method()
                                     {
                                         var value = new Example
                                                     {
                                                         Values = {
                                                                      1,
                                                                      2,
                                                                      3
                                                                  }
                                                     };
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH0821NestedCollectionInitializerAssignmentsShouldBeFormattedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH0821MessageFormat));
    }

    #endregion // Tests
}