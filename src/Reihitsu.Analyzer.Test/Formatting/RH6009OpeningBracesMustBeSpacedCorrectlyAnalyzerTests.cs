using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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

    /// <summary>
    /// Verifies that every brace of the requirement's example is reported, the opening brace of an array initializer in an
    /// interpolation hole included
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    [Requirement("SHARED-DIAG-003")]
    public async Task VerifyEveryBraceOfTheRequirementExampleIsReported()
    {
        const string testData = """
                                class C
                                {
                                    int P{|RH6009:{|} get; set; }
                                    int Q  { get; set; }
                                    int R/* note */{|RH6009:{|} get; set; }

                                    void M(object o, int a){|RH6009:{|}
                                        var x = new int[]{|RH6009:{|}1 };
                                        var b = o is C{|RH6009:{|} };
                                        var c = a switch{|RH6009:{|} _ => 1 };
                                        var s = $"{new int[]{|RH6009:{|}1 }.Length}";
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
    [Requirement("RH6009-004")]
    public async Task VerifyTheRequirementFixExampleIsFixed()
    {
        const string testData = """
                                class C
                                {
                                    int P{|RH6009:{|} get; set; }
                                    int R/* note */{|RH6009:{|} get; set; }

                                    void M(object o){|RH6009:{|}
                                        var x = new int[]{|RH6009:{|}1 };
                                        var s = $"{new int[]{|RH6009:{|}1 }.Length}";
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int P { get; set; }
                                     int R/* note */ { get; set; }

                                     void M(object o) {
                                         var x = new int[] {1 };
                                         var s = $"{new int[] {1 }.Length}";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData);
    }

    /// <summary>
    /// Verifies that the opening brace of an array initializer in a hole of a regular interpolated string is code, and is
    /// reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    [Requirement("RH6009-003")]
    [Requirement("RH6009-004")]
    public async Task VerifyArrayInitializerBraceInAHoleIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string s = $"{new int[]{|#0:{|}1 }.Length}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = $"{new int[] {1 }.Length}";
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6009MessageFormat));
    }

    /// <summary>
    /// Verifies that the opening brace of an array initializer in a hole is reported and fixed under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    [Requirement("RH6009-004")]
    public async Task VerifyArrayInitializerBraceInAHoleIsDetectedAndFixedWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    string s = $"{new int[]{|#0:{|}1 }.Length}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = $"{new int[] {1 }.Length}";
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData), NormalizeToCarriageReturnLineFeed(fixedData), Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6009MessageFormat));
    }

    /// <summary>
    /// Verifies that the opening braces of a lambda block, an anonymous object, and a switch expression in holes are code,
    /// and are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    [Requirement("RH6009-004")]
    public async Task VerifyLambdaAnonymousObjectAndSwitchBracesInHolesAreDetectedAndFixed()
    {
        const string testData = """
                                using System;

                                class C
                                {
                                    string M(int a)
                                    {
                                        return $"{((Func<int>)(() =>{|#0:{|} return 1; }))()}{new{|#1:{|} A = 1 }.A}{a switch{|#2:{|} _ => 1 }}";
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

        await Verify(testData, fixedData, Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6009MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that the opening brace of an array initializer in a hole of an interpolated verbatim string is code, and
    /// is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    [Requirement("RH6009-004")]
    public async Task VerifyArrayInitializerBraceInAHoleOfAnInterpolatedVerbatimStringIsDetectedAndFixed()
    {
        const string testData = """
                                class C
                                {
                                    string s = $@"{new int[]{|#0:{|}1 }.Length}";
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     string s = $@"{new int[] {1 }.Length}";
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6009MessageFormat));
    }

    /// <summary>
    /// Verifies that the opening brace of an array initializer in a hole of a <c>$$</c> raw string, on a hole line of its
    /// own, is code, and is reported and fixed while the interpolation's own braces are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-001")]
    [Requirement("RH6009-002")]
    [Requirement("RH6009-004")]
    public async Task VerifyArrayInitializerBraceInAMultiLineHoleOfADoubleDollarRawStringIsDetectedAndFixed()
    {
        const string testData = """"
                                class C
                                {
                                    string s = $$"""
                                        {{new int[]{1 }
                                            .Length}}
                                        """;
                                }
                                """";
        const string fixedData = """"
                                 class C
                                 {
                                     string s = $$"""
                                         {{new int[] {1 }
                                             .Length}}
                                         """;
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
                     Diagnostic(RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzer.DiagnosticId).WithSpan(4, 20, 4, 21).WithMessage(AnalyzerResources.RH6009MessageFormat));
    }

    /// <summary>
    /// Verifies that the braces of an interpolation stay exempt also when the interpolated string is nested in the hole
    /// of another one
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-002")]
    public async Task VerifyNoDiagnosticsForBracesOfNestedInterpolations()
    {
        const string testData = """
                                class C
                                {
                                    string s = $"{$"{1}"}";
                                    string t = $"a{$"b{1}"}";
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the opening interpolation braces of a <c>$$</c> raw string stay exempt when they directly precede an
    /// array initializer
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6009-002")]
    public async Task VerifyNoDiagnosticsForInterpolationBracesOfADoubleDollarRawString()
    {
        const string testData = """"
                                class C
                                {
                                    string u = $$"""{{new int[] { 1 }.Length}}""";
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