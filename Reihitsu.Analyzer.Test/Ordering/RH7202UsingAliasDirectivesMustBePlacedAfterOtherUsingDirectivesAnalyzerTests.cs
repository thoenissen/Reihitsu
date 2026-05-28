using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer"/> and <see cref="RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzerTests : AnalyzerTestsBase<RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer, RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying alias usings are reported and fixed when they appear before regular usings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
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

        await Verify(testCode, fixedCode, Diagnostics(RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer.DiagnosticId, AnalyzerResources.RH7202MessageFormat));
    }

    #endregion // Tests
}