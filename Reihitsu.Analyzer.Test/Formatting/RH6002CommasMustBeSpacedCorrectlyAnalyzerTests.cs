using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6002CommasMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6002CommasMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6002CommasMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6002CommasMustBeSpacedCorrectlyAnalyzer, RH6002CommasMustBeSpacedCorrectlyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that clean code does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticsWhenCodeIsClean()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x, int y)
                                    {
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
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x{|#0:,|}int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that a space before a comma is detected and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySpaceBeforeCommaIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x {|#0:,|} int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that multiple spaces after a comma are detected and collapsed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleSpacesAfterCommaAreDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x{|#0:,|}  int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that malformed commas in sized array rank specifiers are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifySizedArrayRankCommaIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new int[1 {|#0:,|}  1];
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         _ = new int[1, 1];
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that a form feed before a comma is detected and removed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormFeedBeforeCommaIsDetectedAndFixed()
    {
        const string testData = "internal class TestClass\r\n{\r\n    void Method(int x\f{|#0:,|} int y)\r\n    {\r\n    }\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n    void Method(int x, int y)\r\n    {\r\n    }\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that a form feed after a comma is detected and replaced by one space
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyFormFeedAfterCommaIsDetectedAndFixed()
    {
        const string testData = "internal class TestClass\r\n{\r\n    void Method(int x{|#0:,|}\fint y)\r\n    {\r\n    }\r\n}";
        const string fixedData = "internal class TestClass\r\n{\r\n    void Method(int x, int y)\r\n    {\r\n    }\r\n}";

        await Verify(testData, fixedData, Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that spacing around a block comment after a comma matches the formatter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBlockCommentAfterCommaIsNormalized()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x{|#0:,|}/* keep */int y)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x,/* keep */ int y)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    /// <summary>
    /// Verifies that a preprocessor directive on the line-broken side of a comma survives the code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDirectiveOnLineBrokenSideSurvivesFix()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(
                                        int x ,
                                #if FEATURE
                                        int y)
                                #endif
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(
                                         int x,
                                 #if FEATURE
                                         int y)
                                 #endif
                                     {
                                     }
                                 }
                                 """;

        var fixedSource = await ApplyCodeFixAsync(NormalizeToCarriageReturnLineFeed(testData), "FEATURE");

        Assert.AreEqual(NormalizeToCarriageReturnLineFeed(fixedData), fixedSource);
    }

    /// <summary>
    /// Verifies that Fix All normalizes multiple comma diagnostics in one iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMultipleCommaDiagnosticsAreFixedInOneFixAllIteration()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(int x {|#0:,|}  int y, int z)
                                    {
                                        Method(x, y {|#1:,|}  z);
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int x, int y, int z)
                                     {
                                         Method(x, y, z);
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that array-rank commas do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyArrayRankCommasDoNotProduceDiagnostics()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public int[,] Method()
                                    {
                                        return new int[1, 1];
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the comma in an unbound generic type does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUnboundGenericCommaDoesNotProduceDiagnostics()
    {
        const string testData = """
                                using System;
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    public Type Method()
                                    {
                                        return typeof(Dictionary<,>);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the commas in an unbound generic type with multiple omitted type arguments do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyUnboundGenericMultipleCommasDoNotProduceDiagnostics()
    {
        const string testData = """
                                using System;

                                internal class TestClass
                                {
                                    public Type Method()
                                    {
                                        return typeof(Func<,,>);
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a bound generic type with a missing space after the comma is still detected
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyBoundGenericCommaIsStillDetected()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    public Dictionary<int{|#0:,|}string> Method()
                                    {
                                        return null;
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;

                                 internal class TestClass
                                 {
                                     public Dictionary<int, string> Method()
                                     {
                                         return null;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6002CommasMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6002MessageFormat));
    }

    #endregion // Tests
}