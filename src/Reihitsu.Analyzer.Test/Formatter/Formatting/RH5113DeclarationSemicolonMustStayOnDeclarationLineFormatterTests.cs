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

        await VerifyFormatter(input,
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

        await VerifyFormatter(input,
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

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, 4, 9, 4, 10, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter collapses a stray property-declaration semicolon onto the declaration line
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

        await VerifyFormatter(input,
                              fixedData,
                              ExpectedDiagnostic(RH5113DeclarationSemicolonMustStayOnDeclarationLineAnalyzer.DiagnosticId, 4, 9, 4, 10, AnalyzerResources.RH5113MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter leaves an expression-bodied property's wrapped terminating semicolon untouched,
    /// so the new join stays limited to declarations carrying an initializer
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

        await VerifyFormatter(input);
    }

    /// <summary>
    /// Verifies that a semicolon the author put on the line behind a comment that ends the declaration's line stays on that
    /// line, and that the result stays analyzer-clean: this analyzer exempts the shape because a comment sits in the gap. A
    /// property with an initializer carries the shape, because a comment in front of a variable declaration's semicolon makes
    /// the input unsupported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySemicolonBehindACommentOnTheLineAboveStaysAnalyzerClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    private int A { get; } = 1 // c
                                    ;
                                    private int _b;
                                }
                                """;

        await VerifyFormatter(testData);
    }

    #endregion // Tests
}