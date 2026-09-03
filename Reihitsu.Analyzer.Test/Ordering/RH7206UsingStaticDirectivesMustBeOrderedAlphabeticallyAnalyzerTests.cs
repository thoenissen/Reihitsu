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
public class RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzerTests : BatchCodeFixTestsBase<RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer, RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyCodeFixProvider>
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

    /// <summary>
    /// Verifies that static imports in ordinal-distinct roots are not compared across their group boundary
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task OrdinalDistinctStaticRootGroupsDoNotProduceCrossGroupDiagnostics()
    {
        const string testCode = """
                                using static SYSTEM.Alpha;
                                using static SYSTEM.Zulu;

                                using static system.Bravo;

                                namespace SYSTEM
                                {
                                    public class Alpha;
                                    public class Zulu;
                                }

                                namespace system
                                {
                                    public class Bravo;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies disabled conditional using blocks are exempt when they cannot be safely reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DisabledConditionalUsingBlocksAreNotReportedWhenTheyCannotBeSafelyReordered()
    {
        const string testCode = """
                                using static System.String;
                                #if FEATURE
                                using static System.Console;
                                #endif
                                using static System.Math;
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using static System.String;
                                using static {|#0:System.Math|};
                                using static {|#1:System.Console|};

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using static System.Console;
                                 using static System.Math;
                                 using static System.String;

                                 public class TestClass
                                 {
                                 }
                                 """;

        // All three static directives share the System root group, so each one is compared against its
        // immediate predecessor and two of them are reported. Both fixes reorganize the same using list, so
        // their text changes cover the identical span and the surviving one already clears both
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7206UsingStaticDirectivesMustBeOrderedAlphabeticallyAnalyzer.DiagnosticId, AnalyzerResources.RH7206MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}