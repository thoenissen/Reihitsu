using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Spacing;
using Reihitsu.Analyzer.Rules.Spacing;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer"/> and <see cref="RH6011OpeningGenericBracketsMustBeSpacedCorrectlyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzerTests : AnalyzerTestsBase<RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer, RH6011OpeningGenericBracketsMustBeSpacedCorrectlyCodeFixProvider>
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
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List<int>();
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
                                using System.Collections.Generic;
                                internal class TestClass
                                {
                                    void Method()
                                    {
                                        _ = new List{|#0: |}<int>();
                                    }
                                }
                                """;
        const string fixedData = """
                                 using System.Collections.Generic;
                                 internal class TestClass
                                 {
                                     void Method()
                                     {
                                         _ = new List<int>();
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));
    }

    /// <summary>
    /// Verifies that a space before the opening generic bracket of a type parameter list is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyTypeParameterListIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method{|#0: |}<T>(T value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method<T>(T value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH6011OpeningGenericBracketsMustBeSpacedCorrectlyAnalyzer.DiagnosticId, AnalyzerResources.RH6011MessageFormat));
    }

    #endregion // Tests
}