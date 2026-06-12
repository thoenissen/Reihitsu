using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer"/> and <see cref="RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzerTests : AnalyzerTestsBase<RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer, RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that standard interpolated strings without interpolation are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyStandardInterpolatedStringWithoutInterpolationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var message = {|#0:$"System ready"|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var message = "System ready";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId, AnalyzerResources.RH3204MessageFormat));
    }

    /// <summary>
    /// Verifies that verbatim interpolated strings without interpolation are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyVerbatimInterpolatedStringWithoutInterpolationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var path = {|#0:$@"C:\Users\John"|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var path = @"C:\Users\John";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId, AnalyzerResources.RH3204MessageFormat));
    }

    /// <summary>
    /// Verifies that verbatim interpolated strings with reversed prefix are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyReversedVerbatimInterpolatedStringWithoutInterpolationIsDetectedAndFixed()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var path = {|#0:@$"C:\Users\John"|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var path = @"C:\Users\John";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId, AnalyzerResources.RH3204MessageFormat));
    }

    /// <summary>
    /// Verifies that raw interpolated strings with single $ and no interpolation are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRawInterpolatedStringSingleDollarWithoutInterpolationIsDetectedAndFixed()
    {
        const string testData = """"
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var text = {|#0:$"""
                                        hello
                                        world
                                        """|};
                                    }
                                }
                                """";
        const string fixedData = """"
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var text = """
                                         hello
                                         world
                                         """;
                                     }
                                 }
                                 """";

        await Verify(testData, fixedData, Diagnostics(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId, AnalyzerResources.RH3204MessageFormat));
    }

    /// <summary>
    /// Verifies that escaped braces are unescaped when the dollar marker is removed, preserving the runtime value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInterpolatedStringWithEscapedBracesIsFixedWithoutChangingValue()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var message = {|#0:$"a {{b}}"|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var message = "a {b}";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId, AnalyzerResources.RH3204MessageFormat));
    }

    /// <summary>
    /// Verifies that escaped braces are unescaped when the dollar marker is removed from a verbatim interpolated string
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyVerbatimInterpolatedStringWithEscapedBracesIsFixedWithoutChangingValue()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var message = {|#0:$@"a {{b}}"|};
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     private static void Method()
                                     {
                                         var message = @"a {b}";
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId, AnalyzerResources.RH3204MessageFormat));
    }

    /// <summary>
    /// Verifies that interpolated strings with interpolation holes are not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyInterpolatedStringWithInterpolationIsNotFlagged()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var name = "Alice";
                                        var message = $"Hello, {name}";
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that multiple interpolated strings without interpolation are all detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleInterpolatedStringsWithoutInterpolationAreDetected()
    {
        const string testData = """
                                internal class Example
                                {
                                    private static void Method()
                                    {
                                        var a = {|#0:$"first"|};
                                        var b = {|#1:$"second"|};
                                        var c = "third";
                                    }
                                }
                                """;

        await Verify(testData, Diagnostics(RH3204InterpolatedStringsWithoutInterpolationShouldNotUseDollarAnalyzer.DiagnosticId, AnalyzerResources.RH3204MessageFormat, 2));
    }

    #endregion // Tests
}