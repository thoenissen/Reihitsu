using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5113DeclarationSemicolonMustStayOnDeclarationLineFormatterTests : FormatterTestsBase<RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter collapses a stray field-declaration semicolon onto the declaration line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesFieldDeclarationViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 private int _value
                                     ;
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private int _value;
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, 4, 9, 4, 10, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses a stray event-field-declaration semicolon onto the declaration line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEventFieldDeclarationViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 public event System.EventHandler Changed
                                     ;
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public event System.EventHandler Changed;
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, 4, 9, 4, 10, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses a stray delegate-declaration semicolon onto the declaration line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesDelegateDeclarationViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 public delegate void Handler(int value)
                                     ;
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public delegate void Handler(int value);
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, 4, 9, 4, 10, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses a stray property-declaration semicolon onto the declaration line
    /// (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesPropertyDeclarationViolation()
    {
        const string input = """
                             internal class Example
                             {
                                 public int Value { get; set; } = 1
                                     ;
                             }
                             """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     public int Value { get; set; } = 1;
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 ExpectedDiagnostic(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, 4, 9, 4, 10, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves an expression-bodied property's wrapped terminating semicolon untouched,
    /// so the new join stays limited to declarations carrying an initializer (issue #612)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterKeepsExpressionBodiedPropertySemicolon()
    {
        const string input = """
                             internal class Example
                             {
                                 public int Value => 1
                                 ;
                             }
                             """;

        await VerifyFormatterStability(input);
    }

    /// <summary>
    /// Verifies that splitting a combined field whose separator the author put on the next line leaves the
    /// generated semicolon on that next line, and that the result stays analyzer-clean. The separator terminates
    /// its declarator exactly as the semicolon terminates the last one, so preserving the author's line break is
    /// the split doing nothing rather than the formatter choosing a layout; this analyzer exempts the shape
    /// because a comment sits in the gap (issue #625)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySplitFieldWithLineBrokenSeparatorStaysAnalyzerClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int _a // c
                                    ;
                                    private int _b;
                                }
                                """;

        await VerifyFormatterStability(testData);
    }

    #endregion // Tests
}