using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH3001NotOperatorShouldNotBeUsedAnalyzer"/> and <see cref="RH3001NotOperatorShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3001NotOperatorShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH3001NotOperatorShouldNotBeUsedAnalyzer, RH3001NotOperatorShouldNotBeUsedCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying not operator on literal is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnLiteral()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetBool()
                                    {
                                        return {|#0:!|}false;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool GetBool()
                                     {
                                         return false == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on field is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnField()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return {|#0:!|}_field;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on property is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnProperty()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool Property { get; set; }

                                    public bool GetProperty()
                                    {
                                        return {|#0:!|}Property;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool Property { get; set; }

                                     public bool GetProperty()
                                     {
                                         return Property == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on method call is reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnMethodCall()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool GetBool()
                                    {
                                        return true;
                                    }

                                    public bool GetMethod()
                                    {
                                        return {|#0:!|}GetBool();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public bool GetBool()
                                     {
                                         return true;
                                     }

                                     public bool GetMethod()
                                     {
                                         return GetBool() == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    /// <summary>
    /// Verifying not operator on a nullable bool operand is not reported, because <c>!x</c> and <c>x == false</c> are not equivalent for <see langword="null"/>
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnNullableBoolIsNotReported()
    {
        const string testCode = """
                                public class Test
                                {
                                    public bool? GetValue(bool? value)
                                    {
                                        return !value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying not operator on an operand with a user-defined operator is not reported, because the rewrite is not equivalent
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorOnUserDefinedOperatorIsNotReported()
    {
        const string testCode = """
                                public struct Custom
                                {
                                    public static Custom operator !(Custom value)
                                    {
                                        return value;
                                    }
                                }

                                public class Test
                                {
                                    public Custom GetValue(Custom value)
                                    {
                                        return !value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifying leading trivia of the not operator is preserved when the fix is applied
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NotOperatorTriviaIsPreserved()
    {
        const string testCode = """
                                public class Test
                                {
                                    private bool _field;

                                    public bool GetField()
                                    {
                                        return /* comment */ {|#0:!|}_field;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     private bool _field;

                                     public bool GetField()
                                     {
                                         return /* comment */ _field == false;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH3001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH3001MessageFormat));
    }

    #endregion // Tests
}