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

    /// <summary>
    /// Verifies that the formatter inserts the blank line above a comment below a multi-line block comment behind the
    /// previous member, whose inner line break the analyzer does not count as a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterSeparatesCommentBelowMultiLineTrailingBlockComment()
    {
        const string input = """
                             internal class Example
                             {
                                 private string _d; /* a
                                 b */
                                 {|#0:// Describes|}
                                 public string Description { get; set; }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private string _d; /* a
                                     b */

                                     // Describes
                                     public string Description { get; set; }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              Diagnostics(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in front of the terminal <c>break</c> of a section the formatter braces gets its
    /// blank line in the same pass, so the output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-SWITCH-007")]
    [Requirement("FMT-BLANK-011")]
    public async Task VerifyFormatterSeparatesCommentBeforeTerminalBreakOfBracedSection()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             B();
                                             // before break
                                             break;

                                         case 2:
                                             A();
                                             break;
                                     }
                                 }

                                 private void A()
                                 {
                                 }

                                 private void B()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(int x)
                                     {
                                         switch (x)
                                         {
                                             case 1:
                                                 {
                                                     A();
                                                     B();
                                                 }

                                                 // before break
                                                 break;

                                             case 2:
                                                 {
                                                     A();
                                                 }
                                                 break;
                                         }
                                     }

                                     private void A()
                                     {
                                     }

                                     private void B()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 10, 17, 10, 32, AnalyzerResources.RH5020MessageFormat));
    }

    /// <summary>
    /// Verifies that a comment in front of the terminal <c>break</c> of a switch that stays unbraced gets its blank
    /// line, so the output is clean and stable under LF and CRLF
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("FMT-BLANK-011")]
    public async Task VerifyFormatterSeparatesCommentBeforeTerminalBreakOfUnbracedSection()
    {
        const string input = """
                             internal class Example
                             {
                                 internal void Method(int x)
                                 {
                                     switch (x)
                                     {
                                         case 1:
                                             A();
                                             // before break
                                             break;

                                         case 2:
                                             A();
                                             break;
                                     }
                                 }

                                 private void A()
                                 {
                                 }
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void Method(int x)
                                     {
                                         switch (x)
                                         {
                                             case 1:
                                                 A();

                                                 // before break
                                                 break;

                                             case 2:
                                                 A();
                                                 break;
                                         }
                                     }

                                     private void A()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5020CommentsShouldBePrecededByABlankLineAnalyzer.DiagnosticId, 9, 17, 9, 32, AnalyzerResources.RH5020MessageFormat));
    }

    #endregion // Tests
}