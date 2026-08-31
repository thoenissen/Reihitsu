using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatter.Formatting;

/// <summary>
/// Formatter validation tests for <see cref="RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer"/>
/// </summary>
[TestClass]
public class RH5417FunctionAndAccessorBodyBracesMustNotShareLineFormatterTests : FormatterTestsBase<RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer>
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

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
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

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies formatter parity and second-pass stability for a local function body
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesLocalFunctionBodyAndIsIdempotent()
    {
        const string input = """
                             public class C
                             {
                                 public void Run()
                                 {
                                     void Local() {|#0:{|} }
                                 }
                             }
                             """;
        const string fixedData = """
                                 public class C
                                 {
                                     public void Run()
                                     {
                                         void Local()
                                         {
                                         }
                                     }
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat));
    }

    /// <summary>
    /// Verifies formatter parity and second-pass stability for simple and parenthesized lambdas and anonymous methods
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormatterFixesLambdaAndAnonymousMethodBodiesAndIsIdempotent()
    {
        const string input = """
                             public class C
                             {
                                 System.Func<int, int> Simple = item => {|#0:{|} return item; };
                                 System.Func<int, int> Parenthesized = (item) => {|#1:{|} return item; };
                                 System.Func<int, int> Anonymous = delegate(int item) {|#2:{|} return item; };
                             }
                             """;
        const string fixedData = """
                                 public class C
                                 {
                                     System.Func<int, int> Simple = item =>
                                                                    {
                                                                        return item;
                                                                    };
                                     System.Func<int, int> Parenthesized = (item) =>
                                                                           {
                                                                               return item;
                                                                           };
                                     System.Func<int, int> Anonymous = delegate(int item)
                                     {
                                         return item;
                                     };
                                 }
                                 """;

        await VerifyFormatterFixAndIdempotency(input,
                                               fixedData,
                                               Diagnostics(RH5417FunctionAndAccessorBodyBracesMustNotShareLineAnalyzer.DiagnosticId, AnalyzerResources.RH5417MessageFormat, 3));
    }

    #endregion // Tests
}