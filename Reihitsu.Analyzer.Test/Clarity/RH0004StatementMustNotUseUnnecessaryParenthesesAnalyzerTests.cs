using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH0004StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzerTests : AnalyzerTestsBase<RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer, RH0004StatementMustNotUseUnnecessaryParenthesesCodeFixProvider>
{
    /// <summary>
    /// Verifying unnecessary parentheses in return statement are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInReturnAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return {|#0:(value)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses."));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in throw statement are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInThrowAreReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public void Run(Exception ex)
                                    {
                                        throw {|#0:(ex)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public void Run(Exception ex)
                                     {
                                         throw ex;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses."));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in variable assignment are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInAssignmentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        int x = {|#0:(42)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         int x = 42;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses."));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in method argument are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInArgumentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Method(int value)
                                    {
                                    }

                                    public void Run()
                                    {
                                        Method({|#0:(5)|});
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Method(int value)
                                     {
                                     }

                                     public void Run()
                                     {
                                         Method(5);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses."));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in expression-bodied member are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInExpressionBodiedMemberAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Value => {|#0:(10)|};
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Value => 10;
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses."));
    }

    /// <summary>
    /// Verifying unnecessary nested parentheses are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryNestedParenthesesAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return {|#0:({|#1:(value)|})|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return value;
                                     }
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     onConfigure: config => config.NumberOfFixAllIterations = 2,
                     Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses.", 2));
    }

    /// <summary>
    /// Verifying necessary parentheses around cast are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NecessaryParenthesesAroundCastAreNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(object value)
                                    {
                                        return ((int)value);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying necessary parentheses around lambda are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NecessaryParenthesesAroundLambdaAreNotReported()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Func<int> Run()
                                    {
                                        return (() => 5);
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying unnecessary parentheses around safe member access are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundMemberAccessAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(string text)
                                    {
                                        return {|#0:(text)|}.Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(string text)
                                     {
                                         return text.Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses."));
    }

    /// <summary>
    /// Verifying unnecessary parentheses around safe invocation are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesAroundInvocationAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int GetValue()
                                    {
                                        return 0;
                                    }

                                    public int Run()
                                    {
                                        return {|#0:(GetValue())|}.ToString().Length;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int GetValue()
                                     {
                                         return 0;
                                     }

                                     public int Run()
                                     {
                                         return GetValue().ToString().Length;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses."));
    }

    /// <summary>
    /// Verifying unnecessary parentheses in compound assignment are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UnnecessaryParenthesesInCompoundAssignmentAreReportedAndFixed()
    {
        const string testCode = """
                                public class Test
                                {
                                    public void Run()
                                    {
                                        int x = 0;
                                        x += {|#0:(5)|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public void Run()
                                     {
                                         int x = 0;
                                         x += 5;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0004StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statements must not use unnecessary parentheses."));
    }
}