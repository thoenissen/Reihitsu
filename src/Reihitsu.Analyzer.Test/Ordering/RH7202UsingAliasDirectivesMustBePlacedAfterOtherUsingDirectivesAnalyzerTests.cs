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
    [Requirement("RH7202-001")]
    [Requirement("RH7202-003")]
    [Requirement("RH7202-004")]
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
    [Requirement("RH7202-001")]
    [Requirement("SHARED-USING-009")]
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

    /// <summary>
    /// Verifies that every alias directive above a regular directive is reported on its alias name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7202-001")]
    [Requirement("RH7202-003")]
    public async Task EveryAliasAboveARegularDirectiveIsReportedOnItsAliasName()
    {
        const string testCode = """
                                using {|RH7202:A|} = System.Text;
                                using {|RH7202:B|} = System.IO;
                                using System;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a global alias is not compared with a local regular directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7202-001")]
    [Requirement("SHARED-USING-002")]
    public async Task GlobalAliasIsNotComparedWithALocalDirective()
    {
        const string testCode = """
                                global using A = System.Text;
                                using System;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a block containing a preprocessor directive is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7202-001")]
    [Requirement("SHARED-USING-009")]
    public async Task BlockWithPreprocessorDirectiveIsNotReported()
    {
        const string testCode = """
                                using A = System.Text;
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                using System;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an alias followed only by static and alias directives is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7202-002")]
    public async Task AliasFollowedOnlyByStaticDirectivesIsNotReported()
    {
        const string testCode = """
                                using System;
                                using A = System.Text;
                                using static System.Math;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the code fix organizes the whole using block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7202-004")]
    [Requirement("SHARED-USING-003")]
    public async Task CodeFixOrganizesTheWholeBlock()
    {
        const string testCode = """
                                using {|RH7202:A|} = System.Text;
                                using System;

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using A = System.Text;

                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the code fix leaves the code after the using block unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7202-004")]
    public async Task CodeFixLeavesTheCodeAfterTheBlockUnchanged()
    {
        const string testCode = """
                                using {|RH7202:A|} = System.Text;
                                using System;

                                class C
                                {
                                    void M( ) {   }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using A = System.Text;

                                 class C
                                 {
                                     void M( ) {   }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
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