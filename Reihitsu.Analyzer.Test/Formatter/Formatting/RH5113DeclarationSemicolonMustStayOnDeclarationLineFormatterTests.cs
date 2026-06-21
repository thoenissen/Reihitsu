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

    #endregion // Tests
}