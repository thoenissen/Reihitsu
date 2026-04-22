using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0008DoNotUseDefaultValueTypeConstructorAnalyzer"/> and <see cref="RH0008DoNotUseDefaultValueTypeConstructorCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0008DoNotUseDefaultValueTypeConstructorAnalyzerTests : AnalyzerTestsBase<RH0008DoNotUseDefaultValueTypeConstructorAnalyzer, RH0008DoNotUseDefaultValueTypeConstructorCodeFixProvider>
{
    /// <summary>
    /// Verifying default value type constructors are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task DefaultValueTypeConstructorIsReportedAndFixed()
    {
        const string testCode = """
                                using System;

                                public class Test
                                {
                                    public Guid Run()
                                    {
                                        return new {|#0:Guid|}();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 public class Test
                                 {
                                     public Guid Run()
                                     {
                                         return default(Guid);
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0008DoNotUseDefaultValueTypeConstructorAnalyzer.DiagnosticId, "Do not use default value type constructor."));
    }
}