using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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

    /// <summary>
    /// Verifies that every brace of the requirement's example is reported, the closing brace of an array initializer in an
    /// interpolation hole included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyEveryBraceOfTheRequirementExampleIsReported()
    {
        const string testData = """
                                class C
                                {
                                    int P { get; set;{|RH6010:}|}
                                    int Q { get; set;  }
                                    int R { get; set; /* note */{|RH6010:}|}

                                    void M(int a)
                                    {
                                        var x = new int[] { 1{|RH6010:}|};
                                        var e = new int[] {{|RH6010:}|};
                                        var s = $"{new int[] { 1{|RH6010:}|}.Length}";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies the requirement's fix example: the fix inserts one space in front of each reported brace, the brace of an
    /// array initializer in an interpolation hole included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-004")]
    public async Task VerifyTheRequirementFixExampleIsFixed()
    {
        const string testData = """
                                class C
                                {
                                    int P { get; set;{|RH6010:}|}
                                    int R { get; set; /* note */{|RH6010:}|}

                                    void M(int a)
                                    {
                                        var e = new int[] {{|RH6010:}|};
                                        var s = $"{new int[] { 1{|RH6010:}|}.Length}";
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int P { get; set; }
                                     int R { get; set; /* note */ }

                                     void M(int a)
                                     {
                                         var e = new int[] { };
                                         var s = $"{new int[] { 1 }.Length}";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the closing brace of an array initializer in a hole of a regular interpolated string is code, and is
    /// reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("RH6010-003")]
    [Requirement("RH6010-004")]
    public async Task VerifyArrayInitializerBraceInAHoleIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string s = $"{new int[] { 1{|#0:}|}.Length}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = $"{new int[] { 1 }.Length}";
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat));
    }

    /// <summary>
    /// Verifies that the closing brace of an array initializer in a hole is reported and fixed under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("RH6010-004")]
    public async Task VerifyArrayInitializerBraceInAHoleIsDetectedAndFixedWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    string s = $"{new int[] { 1{|#0:}|}.Length}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = $"{new int[] { 1 }.Length}";
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat));
    }

    /// <summary>
    /// Verifies that the closing brace of an empty array initializer in a hole is code, and is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("RH6010-004")]
    public async Task VerifyEmptyInitializerBraceInAHoleIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string s = $"{new int[] {{|#0:}|}.Length}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = $"{new int[] { }.Length}";
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat));
    }

    /// <summary>
    /// Verifies that the closing braces of a lambda block, an anonymous object, and a switch expression in holes are code,
    /// and are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("RH6010-004")]
    public async Task VerifyLambdaAnonymousObjectAndSwitchBracesInHolesAreDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    string M(int a)
                                    {
                                        return $"{((Func<int>)(() => { return 1;{|#0:}|}))()}{new { A = 1{|#1:}|}.A}{a switch { _ => 1{|#2:}|}}";
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System;

                                 class C
                                 {
                                     string M(int a)
                                     {
                                         return $"{((Func<int>)(() => { return 1; }))()}{new { A = 1 }.A}{a switch { _ => 1 }}";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the closing brace of an array initializer in a hole of an interpolated verbatim string is code, and
    /// is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("RH6010-004")]
    public async Task VerifyArrayInitializerBraceInAHoleOfAnInterpolatedVerbatimStringIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string s = $@"{new int[] { 1{|#0:}|}.Length}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = $@"{new int[] { 1 }.Length}";
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6010MessageFormat));
    }

    /// <summary>
    /// Verifies that the closing brace of an array initializer directly in front of the closing interpolation braces of a
    /// <c>$$</c> raw string is code, and is reported and fixed, while the interpolation's own braces are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-001")]
    [Requirement("RH6010-002")]
    [Requirement("RH6010-004")]
    public async Task VerifyArrayInitializerBraceInAHoleOfADoubleDollarRawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string s = $$"""{{new int[] { 1}.Length}}""";
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string s = $$"""{{new int[] { 1 }.Length}}""";
                                 }
                                 """";

        // "$$" is the caret marker of the test markup, so the source carries no markup and the location is given as a span
        await Verify(testData,
                     fixedData,
                     static test =>
                            {
                                test.TestState.MarkupHandling = MarkupMode.None;
                                test.FixedState.MarkupHandling = MarkupMode.None;
                            },
                     Diagnostic(RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId).WithSpan(3, 36, 3, 37).WithMessage(AnalyzerResources.RH6010MessageFormat));
    }

    /// <summary>
    /// Verifies that the braces of an interpolation stay exempt also when the interpolated string is nested in the hole
    /// of another one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-002")]
    public async Task VerifyNoDiagnosticsForBracesOfNestedInterpolations()
    {
        const string testData = """
                                class C
                                {
                                    string s = $"{$"{1}"}";
                                    string t = $"{$"b{1}"}a";
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the closing interpolation braces of a <c>$$</c> raw string stay exempt when they directly follow an
    /// initializer's closing brace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6010-002")]
    public async Task VerifyNoDiagnosticsForInterpolationBracesOfADoubleDollarRawString()
    {
        const string testData = """"
                                class C
                                {
                                    string u = $$"""{{new int[] { 1 }}}""";
                                }
                                """";

        // "$$" is the caret marker of the test markup, so the source is verified without markup
        await Verify(testData, static test => test.TestState.MarkupHandling = MarkupMode.None);
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