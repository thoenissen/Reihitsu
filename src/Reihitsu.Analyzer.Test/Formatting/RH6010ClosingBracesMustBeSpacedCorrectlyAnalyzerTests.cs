using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6010ClosingBracesMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer, RH6010ClosingBracesMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        if (true)
                                        {
                                        }
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the issue is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("RH6010-003")]
    [Requirement("RH6010-004")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Property { get; set;{|#0:}|}
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Property { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat));
    }

    /// <summary>
    /// Verifies that interpolated-string braces do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-002")]
    public async Task VerifyInterpolatedStringBracesDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int value)
                                    {
                                        var text = $"{value}";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the closing brace of an array initializer and of an empty brace pair glued to the character
    /// before them are reported on the brace and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("RH6010-003")]
    [Requirement("RH6010-004")]
    public async Task VerifyClosingBracesOfInitializersAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        var x = new int[] { 1{|#0:}|};
                                        var e = new int[] {{|#1:}|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         var x = new int[] { 1 };
                                         var e = new int[] { };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a closing brace directly after a block comment is reported, and that the fix inserts the space
    /// between the comment and the brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("RH6010-004")]
    public async Task VerifyClosingBraceAfterBlockCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Property { get; set; /* note */{|#0:}|}
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Property { get; set; /* note */ }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat));
    }

    /// <summary>
    /// Verifies that a closing brace preceded by several spaces is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    public async Task VerifyClosingBracePrecededBySeveralSpacesIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Property { get; set;  }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int First { get; set;{|#0:}|}
                                    int Second { get; set;{|#1:}|}
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int First { get; set; }
                                     int Second { get; set; }
                                 }
                                 """;

        // Two properties on adjacent lines each carry their own unspaced closing brace; the fixes only insert a
        // space at their own brace's boundary, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}