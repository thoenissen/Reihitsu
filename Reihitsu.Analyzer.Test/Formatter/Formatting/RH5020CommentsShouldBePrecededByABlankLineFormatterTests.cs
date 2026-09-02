using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5020CommentsShouldBePrecededByABlankLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5020CommentsShouldBePrecededByABlankLineFormatterTests : FormatterTestsBase<RH5020CommentsShouldBePrecededByABlankLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter inserts a blank line before the first comment in a comment block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesCommentBlockViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     var value = 1;
                                     // First comment
                                     // Follow-up comment
                                     Consume(value);
                                 }

                                 private void Consume(int value)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var value = 1;

                                         // First comment
                                         // Follow-up comment
                                         Consume(value);
                                     }

                                     private void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 6, 9, 6, 25, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies formatter parity and second-pass stability for four-slash and own-line block comments
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesFourSlashAndBlockCommentsAndIsIdempotent()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method()
                                 {
                                     var first = 1;
                                     {|#0://// First note|}
                                     Consume(first);
                                     var second = 2;
                                     {|#1:/* Second note */|}
                                     Consume(second);
                                 }

                                 private void Consume(int value)
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method()
                                     {
                                         var first = 1;

                                         //// First note
                                         Consume(first);

                                         var second = 2;

                                         /* Second note */
                                         Consume(second);
                                     }

                                     private void Consume(int value)
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat, 2));
    }

    #endregion // Tests
}