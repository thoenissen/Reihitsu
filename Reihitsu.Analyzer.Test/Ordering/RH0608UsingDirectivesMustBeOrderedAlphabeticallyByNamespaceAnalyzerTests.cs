using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer"/> and <see cref="RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests : AnalyzerTestsBase<RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer, RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider>
{
    /// <summary>
    /// Verifying regular usings are reported and fixed when they are not alphabetically ordered.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task RegularUsingsAreReportedAndFixedWhenTheyAreNotAlphabeticallyOrdered()
    {
        const string testCode = """
                                using System.Linq;
                                using {|#0:System.Collections.Generic|};

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH0608UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH0608MessageFormat));
    }
}