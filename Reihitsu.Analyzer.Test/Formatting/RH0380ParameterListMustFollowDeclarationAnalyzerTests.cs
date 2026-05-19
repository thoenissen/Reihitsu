using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Formatting;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH0380ParameterListMustFollowDeclarationAnalyzer"/> and <see cref="RH0380ParameterListMustFollowDeclarationCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0380ParameterListMustFollowDeclarationAnalyzerTests : AnalyzerTestsBase<RH0380ParameterListMustFollowDeclarationAnalyzer, RH0380ParameterListMustFollowDeclarationCodeFixProvider>
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
    /// Verifies that a first parameter on the next line after the opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyIssueIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(
                                        {|#0:int|} value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Method(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0380ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH0380MessageFormat));
    }

    /// <summary>
    /// Verifies that an attributed first parameter on the next line after the opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyAttributedFirstParameterOnNextLineIsDetectedAndFixed()
    {
        const string testData = """
                                [System.AttributeUsage(System.AttributeTargets.Parameter)]
                                internal class TagAttribute : System.Attribute { }

                                internal class TestClass
                                {
                                    void Method(
                                        {|#0:[|}Tag] int value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 [System.AttributeUsage(System.AttributeTargets.Parameter)]
                                 internal class TagAttribute : System.Attribute { }

                                 internal class TestClass
                                 {
                                     void Method([Tag] int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0380ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH0380MessageFormat));
    }

    /// <summary>
    /// Verifies that a constructor first parameter on the next line after the opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConstructorFirstParameterOnNextLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    TestClass(
                                        {|#0:int|} value)
                                    {
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     TestClass(int value)
                                     {
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH0380ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH0380MessageFormat));
    }

    #endregion // Tests
}