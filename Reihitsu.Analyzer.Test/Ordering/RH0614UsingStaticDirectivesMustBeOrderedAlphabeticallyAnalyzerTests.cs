using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer"/> and <see cref="RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzerTests : AnalyzerTestsBase<RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer, RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyCodeFixProvider>
{
    /// <summary>
    /// Verifying static usings are reported and fixed when they are not alphabetically ordered.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task StaticUsingsAreReportedAndFixedWhenTheyAreNotAlphabeticallyOrdered()
    {
        const string testCode = """
                                using static System.Math;
                                using static {|#0:System.Console|};

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using static System.Console;
                                 using static System.Math;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0614UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer.DiagnosticId, AnalyzerResources.RH0614MessageFormat));
    }
}