using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Naming;

/// <summary>
/// Test methods for <see cref="RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer"/>
/// </summary>
[TestClass]
public class RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzerTests : AnalyzerTestsBase<RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer>
{
    #region Tests

    /// <summary>
    /// Verifies a diagnostic is reported for a simple lambda parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-001")]
    public async Task VerifyDiagnosticForSimpleLambdaParameter()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        Func<int, int> increment = {|#0:n|} => n + 1;
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4124MessageFormat));
    }

    /// <summary>
    /// Verifies diagnostics are reported for parenthesized lambda and anonymous method parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-001")]
    public async Task VerifyDiagnosticsForParenthesizedLambdaAndAnonymousMethodParameters()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        Func<int, int, int> add = ({|#0:x|}, {|#1:y|}) => x + y;
                                        Func<int, int> increment = delegate(int {|#2:n|}) { return n + 1; };
                                    }
                                }
                                """;

        await Verify(testCode, Diagnostics(RH4124SingleLetterLambdaAndAnonymousMethodParameterNamesAnalyzer.DiagnosticId, AnalyzerResources.RH4124MessageFormat, 3));
    }

    /// <summary>
    /// Verifies no diagnostics are reported for descriptive names or the discard identifier
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-001")]
    public async Task VerifyNoDiagnosticsForDescriptiveNamesAndDiscardIdentifier()
    {
        const string testCode = """
                                using System;

                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process()
                                    {
                                        Func<int, int> increment = value => value + 1;
                                        Func<int, int> ignore = _ => 0;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies no diagnostics are reported for method and local function parameters
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-002")]
    public async Task VerifyNoDiagnosticsForOtherParameterKinds()
    {
        const string testCode = """
                                namespace Reihitsu.Analyzer.Test.Naming.Resources;

                                public class TestClass
                                {
                                    public void Process(int p)
                                    {
                                        static int Double(int n) => n * 2;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies single-letter parameters of simple lambdas, parenthesized lambdas, and anonymous methods are reported, while discards and descriptive names are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-001")]
    public async Task SingleLetterLambdaAndAnonymousMethodParametersAreReported()
    {
        const string testCode = """
                                using System;

                                class C
                                {
                                    void M()
                                    {
                                        Func<int, int> f = {|RH4124:x|} => x;
                                        Func<int, int, int> g = ({|RH4124:a|}, {|RH4124:B|}) => a + B;
                                        Action<int> h = delegate (int {|RH4124:v|}) { };
                                        Func<int, int, int> i = (_, _) => 0;
                                        Func<int, int> j = value => value;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies single-letter parameters of a method and a local function are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH4124-002")]
    public async Task MethodAndLocalFunctionParametersAreNotChecked()
    {
        const string testCode = """
                                class C
                                {
                                    void M(int a)
                                    {
                                        int Local(int b) => b;
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests
}