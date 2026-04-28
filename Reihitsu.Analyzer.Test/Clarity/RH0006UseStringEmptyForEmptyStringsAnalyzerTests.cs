using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0006UseStringEmptyForEmptyStringsAnalyzer"/> and <see cref="RH0006UseStringEmptyForEmptyStringsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0006UseStringEmptyForEmptyStringsAnalyzerTests : AnalyzerTestsBase<RH0006UseStringEmptyForEmptyStringsAnalyzer, RH0006UseStringEmptyForEmptyStringsCodeFixProvider>
{
    /// <summary>
    /// Verifying empty string literal in return statement is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInReturnIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return {|#0:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return string.Empty;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0006UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string literal in variable assignment is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInVariableAssignmentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        string text = {|#0:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         string text = string.Empty;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0006UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string literal in method argument is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInMethodArgumentIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Method(string value)
                                    {
                                    }

                                    public void Run()
                                    {
                                        Method({|#0:""|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Method(string value)
                                     {
                                     }

                                     public void Run()
                                     {
                                         Method(string.Empty);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0006UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string in field initializer is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInFieldInitializerIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    private string _field = {|#0:""|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private string _field = string.Empty;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0006UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string in property initializer is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInPropertyInitializerIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Value { get; set; } = {|#0:""|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Value { get; set; } = string.Empty;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0006UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }

    /// <summary>
    /// Verifying empty string in const field is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInConstFieldIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    private const string Empty = "";
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in const local variable is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInConstLocalIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        const string empty = "";
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in parameter default value is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInParameterDefaultIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Method(string value = "")
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in attribute argument is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInAttributeArgumentIsNotReported()
    {
        const string testCode = """
                                using System.ComponentModel;

                                public class Test
                                {
                                    [Description("")]
                                    public void Method()
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in switch case is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInSwitchCaseIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(string value)
                                    {
                                        switch (value)
                                        {
                                            case "":
                                                return 1;
                                            default:
                                                return 0;
                                        }
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in pattern matching is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInPatternMatchingIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Run(string value)
                                    {
                                        return value is "";
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying empty string in string concatenation is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task EmptyStringInConcatenationIsReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public string Run()
                                    {
                                        return "Hello" + {|#0:""|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public string Run()
                                     {
                                         return "Hello" + string.Empty;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0006UseStringEmptyForEmptyStringsAnalyzer.DiagnosticId, "Use string.Empty for empty strings."));
    }
}