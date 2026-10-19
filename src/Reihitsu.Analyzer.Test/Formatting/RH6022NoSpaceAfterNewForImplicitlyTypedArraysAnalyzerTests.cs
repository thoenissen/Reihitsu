using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer"/> and <see cref="RH6022NoSpaceAfterNewForImplicitlyTypedArraysCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzerTests : BatchCodeFixTestsBase<RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer, RH6022NoSpaceAfterNewForImplicitlyTypedArraysCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6022-001")]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = new[] { 1 };
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
    [Requirement("RH6022-001")]
    [Requirement("RH6022-002")]
    [Requirement("RH6022-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] values = new{|#0: |}[] { 1 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] values = new[] { 1 };
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer.DiagnosticId, AnalyzerResources.RH6022MessageFormat));
    }

    /// <summary>
    /// Verifies that spaces, a tab, or a run at the end of the line after the <c>new</c> of a one- or multi-dimensional
    /// implicitly typed array creation are reported once per creation, also inside an interpolation hole, and that a
    /// <c>new</c> followed by a line break or by an element type is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6022-001")]
    [Requirement("RH6022-002")]
    public async Task VerifyWhitespaceAfterNewOfImplicitlyTypedArrayIsReported()
    {
        const string testData = """
                                class C
                                {
                                    string M()
                                    {
                                        var a = new{|RH6022: |}[] { 1 };
                                        var b = new{|RH6022:→|}[] { 1 };
                                        var c = new{|RH6022:  |}[,] { { 1 } };
                                        var d = new{|RH6022:·|}
                                            [] { 1 };
                                        var e = new
                                            [] { 1 };
                                        var f = new int [] { 1 };

                                        return $"{new{|RH6022: |}[] { 1 }.Length}";
                                    }
                                }
                                """;

        await Verify(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyWhitespaceAfterNewOfImplicitlyTypedArrayIsReported"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6022-001")]
    [Requirement("RH6022-002")]
    public async Task VerifyWhitespaceAfterNewOfImplicitlyTypedArrayIsReportedCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    string M()
                                    {
                                        var a = new{|RH6022: |}[] { 1 };
                                        var b = new{|RH6022:→|}[] { 1 };
                                        var c = new{|RH6022:  |}[,] { { 1 } };
                                        var d = new{|RH6022:·|}
                                            [] { 1 };
                                        var e = new
                                            [] { 1 };
                                        var f = new int [] { 1 };

                                        return $"{new{|RH6022: |}[] { 1 }.Length}";
                                    }
                                }
                                """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that the fix deletes spaces, a tab, and a run at the end of the line after the <c>new</c> of an implicitly
    /// typed array creation
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6022-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceAfterNewOfImplicitlyTypedArray()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new{|RH6022: |}[] { 1 };
                                        var c = new{|RH6022:→|}[,] { { 1 } };
                                        var d = new{|RH6022:·|}
                                            [] { 1 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var a = new[] { 1 };
                                         var c = new[,] { { 1 } };
                                         var d = new
                                             [] { 1 };
                                     }
                                 }
                                 """;

        await Verify(ExpandWhitespaceMarkers(testData), fixedData);
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixDeletesWhitespaceAfterNewOfImplicitlyTypedArray"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6022-003")]
    [Requirement("SHARED-WS-001")]
    public async Task VerifyFixDeletesWhitespaceAfterNewOfImplicitlyTypedArrayCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new{|RH6022: |}[] { 1 };
                                        var c = new{|RH6022:→|}[,] { { 1 } };
                                        var d = new{|RH6022:·|}
                                            [] { 1 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     void M()
                                     {
                                         var a = new[] { 1 };
                                         var c = new[,] { { 1 } };
                                         var d = new
                                             [] { 1 };
                                     }
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)), NormalizeToCarriageReturnLineFeed(fixedData));
    }

    /// <summary>
    /// Verifies that a directive on the next line of the gap after <c>new</c> keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6022-001")]
    [Requirement("RH6022-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new{|RH6022:·|}
                                #if DEBUG
                                            [] { 1 };
                                #else
                                            [] { 2 };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    /// <summary>
    /// Repeats <see cref="VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLine"/> with the source normalized to CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6022-001")]
    [Requirement("RH6022-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenDirectiveFollowsOnNextLineCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new{|RH6022:·|}
                                #if DEBUG
                                            [] { 1 };
                                #else
                                            [] { 2 };
                                #endif
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(NormalizeToCarriageReturnLineFeed(ExpandWhitespaceMarkers(testData)));
    }

    /// <summary>
    /// Verifies that a comment on the next line of the gap after <c>new</c> keeps the diagnostic and withholds the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH6022-001")]
    [Requirement("RH6022-003")]
    [Requirement("SHARED-WS-002")]
    public async Task VerifyFixIsNotOfferedWhenCommentFollowsOnNextLine()
    {
        const string testData = """
                                class C
                                {
                                    void M()
                                    {
                                        var a = new{|RH6022:·|}
                                            // the rank
                                            [] { 1 };
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ExpandWhitespaceMarkers(testData));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        int[] first = new{|#0: |}[] { 1 };
                                        int[] second = new{|#1: |}[] { 2 };
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         int[] first = new[] { 1 };
                                         int[] second = new[] { 2 };
                                     }
                                 }
                                 """;

        // Two implicitly typed array creations on adjacent lines each carry their own unwanted whitespace run
        // after the new keyword; the fixes only remove their own run, so the batch fixer converges in one pass
        return new FixAllScenario(testData,
                                  fixedData,
                                  Diagnostics(RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer.DiagnosticId, AnalyzerResources.RH6022MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}