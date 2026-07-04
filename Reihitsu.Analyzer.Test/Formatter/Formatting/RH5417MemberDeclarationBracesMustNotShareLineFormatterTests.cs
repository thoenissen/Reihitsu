using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5417MemberDeclarationBracesMustNotShareLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5417MemberDeclarationBracesMustNotShareLineFormatterTests : FormatterTestsBase<RH5417MemberDeclarationBracesMustNotShareLineAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies that the formatter expands an empty single-line method body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesEmptyMethodBody()
    {
        const string input = """
                             public class C
                             {
                                 protected virtual void Write() {|#0:{|} }
                             }
                             """;
        const string fixedData = """
                                 public class C
                                 {
                                     protected virtual void Write()
                                     {
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies that the formatter expands a single-line property accessor body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesPropertyAccessorBody()
    {
        const string input = """
                             public class C
                             {
                                 public int Value { get {|#0:{|} return 1; } }
                             }
                             """;
        const string fixedData = """
                                 public class C
                                 {
                                     public int Value
                                     {
                                         get
                                         {
                                             return 1;
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFix(input,
                                 fixedData,
                                 Diagnostics(RH5417MemberDeclarationBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    #endregion // Tests
}