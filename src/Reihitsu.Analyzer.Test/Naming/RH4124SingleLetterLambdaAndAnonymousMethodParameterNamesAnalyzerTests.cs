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

    #endregion // Tests
}