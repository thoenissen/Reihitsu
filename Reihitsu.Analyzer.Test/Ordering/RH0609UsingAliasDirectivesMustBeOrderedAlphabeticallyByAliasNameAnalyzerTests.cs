using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer"/> and <see cref="RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzerTests : AnalyzerTestsBase<RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer, RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider>
{
    /// <summary>
    /// Verifying alias usings are reported and fixed when they are not alphabetically ordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task AliasUsingsAreReportedAndFixedWhenTheyAreNotAlphabeticallyOrdered()
    {
        const string testCode = """
                                using StringList = System.Collections.Generic.List<string>;
                                using {|#0:IntList|} = System.Collections.Generic.List<int>;

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using IntList = System.Collections.Generic.List<int>;
                                 using StringList = System.Collections.Generic.List<string>;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0609UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer.DiagnosticId, AnalyzerResources.RH0609MessageFormat));
    }
}