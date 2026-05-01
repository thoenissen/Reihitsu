using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0001NotOperatorShouldNotBeUsedAnalyzer"/> and <see cref="RH0001NotOperatorShouldNotBeUsedCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0001NotOperatorShouldNotBeUsedAnalyzerTests : AnalyzerTestsBase<RH0001NotOperatorShouldNotBeUsedAnalyzer, RH0001NotOperatorShouldNotBeUsedCodeFixProvider>
{
    #region Members

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

        await Verify(testCode, fixedCode, Diagnostics(RH0001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0001MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH0001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0001MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH0001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0001MessageFormat));
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

        await Verify(testCode, fixedCode, Diagnostics(RH0001NotOperatorShouldNotBeUsedAnalyzer.DiagnosticId, AnalyzerResources.RH0001MessageFormat));
    }

    #endregion // Members
}