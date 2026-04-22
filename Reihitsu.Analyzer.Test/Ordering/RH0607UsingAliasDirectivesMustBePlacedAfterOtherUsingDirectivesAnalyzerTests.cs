using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Rules.Ordering;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer"/> and <see cref="RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider"/>.
/// </summary>
[TestClass]
public class RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzerTests : AnalyzerTestsBase<RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer, RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider>
{
    /// <summary>
    /// Verifying alias usings are reported and fixed when they appear before regular usings.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [TestMethod]
    public async Task AliasUsingsAreReportedAndFixedWhenTheyAppearBeforeRegularUsings()
    {
        const string testCode = """
                                using {|#0:TextAlias|} = System.String;
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

        const string fixedCode = """
                                 using Alpha;
                                 using TextAlias = System.String;

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

        await Verify(testCode, fixedCode, Diagnostics(RH0607UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer.DiagnosticId, AnalyzerResources.RH0607MessageFormat));
    }
}