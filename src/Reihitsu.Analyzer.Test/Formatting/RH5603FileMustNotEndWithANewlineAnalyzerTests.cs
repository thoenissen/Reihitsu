using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5603FileMustNotEndWithANewlineAnalyzer"/> and <see cref="RH5603FileMustNotEndWithANewlineCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5603FileMustNotEndWithANewlineAnalyzerTests : BatchCodeFixTestsBase<RH5603FileMustNotEndWithANewlineAnalyzer, RH5603FileMustNotEndWithANewlineCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that files without a trailing newline do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenFileDoesNotEndWithNewline()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a trailing newline is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTrailingNewlineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                }
                                
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostic(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId).WithSpan(3, 2, 4, 1).WithMessage(AnalyzerResources.RH5603MessageFormat));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class TestClass
                                {
                                }

                                """;
        const string secondSource = """
                                    internal class OtherClass
                                    {
                                    }

                                    """;
        const string fixedCode = """
                                 internal class TestClass
                                 {
                                 }
                                 """;
        const string secondFixedSource = """
                                         internal class OtherClass
                                         {
                                         }
                                         """;

        // This rule reports at most one diagnostic per syntax tree, so the Fix All scenario needs two documents to
        // report two diagnostics at all. Each document's own trailing newline is removed independently, proving
        // per-document independence rather than within-document interference
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  [
                                      Diagnostic(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId).WithSpan("/0/Test0.cs", 3, 2, 4, 1).WithMessage(AnalyzerResources.RH5603MessageFormat),
                                      Diagnostic(RH5603FileMustNotEndWithANewlineAnalyzer.DiagnosticId).WithSpan("/0/Test1.cs", 3, 2, 4, 1).WithMessage(AnalyzerResources.RH5603MessageFormat)
                                  ],
                                  config =>
                                  {
                                      config.TestState.Sources.Add(("/0/Test1.cs", secondSource));
                                      config.FixedState.Sources.Add(("/0/Test1.cs", secondFixedSource));
                                  });
    }

    #endregion // BatchCodeFixTestsBase
}