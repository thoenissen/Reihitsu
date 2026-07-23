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