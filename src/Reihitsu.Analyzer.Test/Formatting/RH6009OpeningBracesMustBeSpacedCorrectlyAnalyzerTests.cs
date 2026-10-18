using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6009OpeningBracesMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzerTests : BatchCodeFixTestsBase<RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer, RH6009OpeningBracesMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
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
    [Requirement("RH6009-001")]
    [Requirement("RH6009-003")]
    [Requirement("RH6009-004")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Property{|#0:{|} get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Property { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6009MessageFormat));
    }

    /// <summary>
    /// Verifies that interpolated strings do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-002")]
    public async Task VerifyInterpolatedStringsDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    string Method(int value)
                                    {
                                        return $"Value: {value}";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the opening braces of a method body, an array initializer, a property pattern, and a switch
    /// expression glued to the character before them are reported on the brace and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    [Requirement("RH6009-003")]
    [Requirement("RH6009-004")]
    public async Task VerifyOpeningBracesOfOtherConstructsAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(object o, int a){|#0:{|}
                                        var x = new int[]{|#1:{|}1 };
                                        var b = o is TestClass{|#2:{|} };
                                        var c = a switch{|#3:{|} _ => 1 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(object o, int a) {
                                         var x = new int[] {1 };
                                         var b = o is TestClass { };
                                         var c = a switch { _ => 1 };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6009MessageFormat, 4));
    }

    /// <summary>
    /// Verifies that an opening brace directly after a block comment is reported, and that the fix inserts the space
    /// between the comment and the brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    [Requirement("RH6009-004")]
    public async Task VerifyOpeningBraceAfterBlockCommentIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Property/* note */{|#0:{|} get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Property/* note */ { get; set; }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6009MessageFormat));
    }

    /// <summary>
    /// Verifies that an opening brace preceded by several spaces is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    public async Task VerifyOpeningBracePrecededBySeveralSpacesIsNotReported()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Property  { get; set; }
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
                                    int First{|#0:{|} get; set; }
                                    int Second{|#1:{|} get; set; }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int First { get; set; }
                                     int Second { get; set; }
                                 }
                                 """;

        // Two properties on adjacent lines each carry their own unspaced opening brace; the fixes only insert a
        // space at their own brace's boundary, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6009MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}