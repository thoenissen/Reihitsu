using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Design;

/// <summary>
/// Test methods for <see cref="RH0121RazorCodeBlocksShouldNotBeUsedAnalyzer"/>
/// </summary>
[TestClass]
public class RH0121RazorCodeBlocksShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0121RazorCodeBlocksShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Test code
    /// </summary>
    private const string TestCode = """
                                    namespace Example;

                                    internal static class Placeholder
                                    {
                                    }
                                    """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifying Razor code blocks trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRazorCodeBlocksTriggerDiagnostics()
    {
        const string razorSource = """
                                   @page "/counter"

                                   <h1>@CurrentCount</h1>

                                   @code {
                                       private int CurrentCount;
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlock(5, 1));
    }

    /// <summary>
    /// Verifying files without Razor code blocks do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRazorFileWithoutCodeBlocksDoesNotTriggerDiagnostics()
    {
        const string razorSource = """
                                   @page "/counter"

                                   <h1>Counter</h1>
                                   """;

        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)));
    }

    /// <summary>
    /// Verifying .razor.cs files do not trigger diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRazorCodeBehindFilesDoNotTriggerDiagnostics()
    {
        const string codeBehindSource = """
                                        namespace Example;

                                        public partial class Counter
                                        {
                                            private int CurrentCount;
                                        }
                                        """;

        await Verify(TestCode, test => test.TestState.AdditionalFiles.Add(("Counter.razor.cs", codeBehindSource)));
    }

    /// <summary>
    /// Verifying multiple Razor code blocks trigger multiple diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleRazorCodeBlocksTriggerMultipleDiagnostics()
    {
        const string razorSource = """
                                   @code {
                                       private int First;
                                   }

                                   @code {
                                       private int Second;
                                   }
                                   """;

        await Verify(TestCode,
                     test => test.TestState.AdditionalFiles.Add(("Counter.razor", razorSource)),
                     RazorCodeBlock(1, 1),
                     RazorCodeBlock(5, 1));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Razor code block diagnostic
    /// </summary>
    /// <param name="line">Line</param>
    /// <param name="column">Column</param>
    /// <returns>Diagnostic</returns>
    private static DiagnosticResult RazorCodeBlock(int line, int column)
    {
        return Diagnostic(RH0121RazorCodeBlocksShouldNotBeUsedAnalyzer.DiagnosticId).WithLocation("Counter.razor", line, column)
                                                                                    .WithMessage(AnalyzerResources.RH0121MessageFormat);
    }

    #endregion // Methods
}