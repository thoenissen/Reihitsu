using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0009UseLambdaSyntaxAnalyzer"/> and <see cref="RH0009UseLambdaSyntaxCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0009UseLambdaSyntaxAnalyzerTests : AnalyzerTestsBase<RH0009UseLambdaSyntaxAnalyzer, RH0009UseLambdaSyntaxCodeFixProvider>
{
    /// <summary>
    /// Verifying anonymous methods are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task AnonymousMethodIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Func<int, int, int> Run()
                                    {
                                        return {|#0:delegate|}(int left, int right) { return left + right; };
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Func<int, int, int> Run()
                                     {
                                         return (int left, int right) => left + right;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0009UseLambdaSyntaxAnalyzer.DiagnosticId, "Use lambda syntax."));
    }
}