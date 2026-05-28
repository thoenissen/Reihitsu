using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer"/> and <see cref="RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzerTests : AnalyzerTestsBase<RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer, RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying System namespace usings are reported and fixed when they appear after other usings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SystemNamespaceUsingsAreReportedAndFixedWhenTheyAppearAfterOtherUsings()
    {
        const string testCode = """
                                using Alpha;
                                using {|#0:System.Linq|};

                                namespace Alpha
                                {
                                    public class Helper
                                    {
                                    }
                                }

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System.Linq;
                                 
                                 using Alpha;

                                 namespace Alpha
                                 {
                                     public class Helper
                                     {
                                     }
                                 }

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer.DiagnosticId, AnalyzerResources.RH7201MessageFormat));
    }

    #endregion // Tests
}