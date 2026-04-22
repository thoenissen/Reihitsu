using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Test methods for <see cref="RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer"/> and <see cref="RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzerTests : AnalyzerTestsBase<RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer, RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsCodeFixProvider>
{
    /// <summary>
    /// Verifying unnecessary base qualifiers are reported and fixed.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task UnnecessaryBaseQualifierIsReportedAndFixed()
    {
        const string testCode = """
                                public class BaseType
                                {
                                    protected int GetValue()
                                    {
                                        return 1;
                                    }
                                }

                                public class DerivedType : BaseType
                                {
                                    public int Run()
                                    {
                                        return {|#0:base|}.GetValue();
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class BaseType
                                 {
                                     protected int GetValue()
                                     {
                                         return 1;
                                     }
                                 }

                                 public class DerivedType : BaseType
                                 {
                                     public int Run()
                                     {
                                         return GetValue();
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0002DoNotPrefixCallsWithBaseUnlessLocalImplementationExistsAnalyzer.DiagnosticId, "Do not prefix calls with base. Unless a local implementation exists."));
    }
}