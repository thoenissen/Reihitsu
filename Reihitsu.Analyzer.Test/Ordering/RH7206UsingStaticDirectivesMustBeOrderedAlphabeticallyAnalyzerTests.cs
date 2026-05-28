using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer"/> and <see cref="RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzerTests : AnalyzerTestsBase<RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer, RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying static usings are reported and fixed when they are not alphabetically ordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer.DiagnosticId, AnalyzerResources.RH7206MessageFormat));
    }

    #endregion // Tests
}