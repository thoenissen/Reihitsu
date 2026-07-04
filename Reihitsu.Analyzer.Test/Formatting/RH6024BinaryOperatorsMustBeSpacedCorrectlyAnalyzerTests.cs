using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6024BinaryOperatorsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer, RH6024BinaryOperatorsMustBeSpacedCorrectlyCodeFixProvider>
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
                                    int Method(int a, int b)
                                    {
                                        return a + b;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that padding around a binary operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyPaddingAroundBinaryOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return a  {|#0:+|}  b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int a, int b)
                                     {
                                         return a + b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that a missing space around a binary operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyMissingSpaceAroundBinaryOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return a{|#0:+|}b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     int Method(int a, int b)
                                     {
                                         return a + b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that a comparison operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyComparisonOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    bool Method(int a, int b)
                                    {
                                        return a{|#0:==|}b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     bool Method(int a, int b)
                                     {
                                         return a == b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that a logical operator is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLogicalOperatorIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    bool Method(bool a, bool b)
                                    {
                                        return a  {|#0:&&|}  b;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     bool Method(bool a, bool b)
                                     {
                                         return a && b;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6024BinaryOperatorsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6024MessageFormat));
    }

    /// <summary>
    /// Verifies that a binary operator that wraps to a new line does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyWrappedBinaryOperatorIsIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    int Method(int a, int b)
                                    {
                                        return a
                                               + b;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that keyword operators such as "is" do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyKeywordOperatorIsIgnored()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    bool Method(object value)
                                    {
                                        return value  is  string;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that generic type arguments do not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyGenericTypeArgumentsAreIgnored()
    {
        const string testData = """
                                using System.Collections.Generic;

                                internal class TestClass
                                {
                                    List<int> Method()
                                    {
                                        return new List<int>();
                                    }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}