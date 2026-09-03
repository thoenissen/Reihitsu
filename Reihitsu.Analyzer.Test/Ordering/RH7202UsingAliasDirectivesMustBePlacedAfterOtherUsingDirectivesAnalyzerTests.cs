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
public class RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzerTests : BatchCodeFixTestsBase<RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer, RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesCodeFixProvider>
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

    /// <summary>
    /// Verifies disabled conditional using blocks are exempt when they cannot be safely reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DisabledConditionalUsingBlocksAreNotReportedWhenTheyCannotBeSafelyReordered()
    {
        const string testCode = """
                                using TextAlias = System.String;
                                #if FEATURE
                                using System.Text;
                                #endif
                                using System;
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using {|#0:IntAlias|} = System.Int32;
                                using {|#1:TextAlias|} = System.String;
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

                                 using IntAlias = System.Int32;
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

        // Both aliases precede the same regular using, and each fix reorganizes the whole compilation-unit using
        // list, so the two text changes cover the identical span and the surviving one already clears both
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7202UsingAliasDirectivesMustBePlacedAfterOtherUsingDirectivesAnalyzer.DiagnosticId, AnalyzerResources.RH7202MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}