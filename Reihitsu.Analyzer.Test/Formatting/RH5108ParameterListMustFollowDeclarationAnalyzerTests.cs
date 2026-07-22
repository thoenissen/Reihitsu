using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5108ParameterListMustFollowDeclarationAnalyzer"/> and <see cref="RH5108ParameterListMustFollowDeclarationCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5108ParameterListMustFollowDeclarationAnalyzerTests : AnalyzerTestsBase<RH5108ParameterListMustFollowDeclarationAnalyzer, RH5108ParameterListMustFollowDeclarationCodeFixProvider>
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

        await Verify(testData, fixedData, Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
    }

    /// <summary>
    /// Verifies that the violation is not flagged when the token gap contains a preprocessor directive, because the
    /// formatter refuses to collapse the first parameter across that directive (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenDirectivesArePresent()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method(
                                #if FEATURE
                                #endif
                                        int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the violation is not flagged when a comment sits in the token gap, because the formatter
    /// refuses to collapse the first parameter across that comment (issue #444)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenCommentSitsInParameterGap()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Method( // note
                                        int value)
                                    {
                                    }
                                }
                                """;

        await Verify(testData);
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

        await Verify(testData, fixedData, Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
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

        await Verify(testData, fixedData, Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
    }

    /// <summary>
    /// Verifies that a local function first parameter on the next line after the opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyLocalFunctionFirstParameterOnNextLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    void Outer()
                                    {
                                        void Local(
                                            {|#0:int|} value)
                                        {
                                        }
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     void Outer()
                                     {
                                         void Local(int value)
                                         {
                                         }
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
    }

    /// <summary>
    /// Verifies that an operator first parameter on the next line after the opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyOperatorFirstParameterOnNextLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static TestClass operator +(
                                        {|#0:TestClass|} left, TestClass right)
                                    {
                                        return left;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public static TestClass operator +(TestClass left, TestClass right)
                                     {
                                         return left;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
    }

    /// <summary>
    /// Verifies that a conversion operator first parameter on the next line after the opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyConversionOperatorFirstParameterOnNextLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal class TestClass
                                {
                                    public static implicit operator int(
                                        {|#0:TestClass|} value)
                                    {
                                        return 0;
                                    }
                                }
                                """;
        const string fixedData = """
                                 internal class TestClass
                                 {
                                     public static implicit operator int(TestClass value)
                                     {
                                         return 0;
                                     }
                                 }
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
    }

    /// <summary>
    /// Verifies that a delegate first parameter on the next line after the opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDelegateFirstParameterOnNextLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal delegate void TestDelegate(
                                    {|#0:int|} value);
                                """;
        const string fixedData = """
                                 internal delegate void TestDelegate(int value);
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
    }

    /// <summary>
    /// Verifies that a record first parameter on the next line after the opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRecordFirstParameterOnNextLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal record TestRecord(
                                    {|#0:int|} Value);
                                """;
        const string fixedData = """
                                 internal record TestRecord(int Value);
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
    }

    /// <summary>
    /// Verifies that a record struct first parameter on the next line after the opening parenthesis is detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyRecordStructFirstParameterOnNextLineIsDetectedAndFixed()
    {
        const string testData = """
                                internal record struct TestRecord(
                                    {|#0:int|} Value);
                                """;
        const string fixedData = """
                                 internal record struct TestRecord(int Value);
                                 """;

        await Verify(testData, fixedData, Diagnostics(RH5108ParameterListMustFollowDeclarationAnalyzer.DiagnosticId, AnalyzerResources.RH5108MessageFormat));
    }

    #endregion // Tests
}