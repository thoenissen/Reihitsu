using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
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
    [Requirement("RH7206-001")]
    [Requirement("RH7206-003")]
    [Requirement("RH7206-004")]
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
    [Requirement("RH7206-002")]
    [Requirement("SHARED-USING-004")]
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
    [Requirement("RH7206-001")]
    [Requirement("SHARED-USING-009")]
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

    /// <summary>
    /// Verifies that every static directive sorted before the static directive of its group directly above it is reported on its type name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7206-001")]
    [Requirement("RH7206-003")]
    public async Task EveryStaticDirectiveSortedBeforeItsNeighborIsReportedOnItsTypeName()
    {
        const string testCode = """
                                using static System.Math;
                                using static {|RH7206:System.Console|};

                                using static Microsoft.CodeAnalysis.CSharp.SyntaxFacts;
                                using static {|RH7206:Microsoft.CodeAnalysis.CSharp.SyntaxFactory|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a block containing a preprocessor directive is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7206-001")]
    [Requirement("SHARED-USING-009")]
    public async Task BlockWithPreprocessorDirectiveIsNotReported()
    {
        const string testCode = """
                                using static System.Math;
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                using static System.Console;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a static directive of another group and alias directives are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7206-002")]
    [Requirement("SHARED-USING-004")]
    public async Task StaticDirectivesOfOtherGroupsAndOtherKindsAreNotReported()
    {
        const string testCode = """
                                using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
                                using static System.Console;

                                using B = System.Text;
                                using A = System.IO;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the code fix organizes the whole using block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7206-004")]
    public async Task CodeFixOrganizesTheWholeBlock()
    {
        const string testCode = """
                                using static System.Math;
                                using static {|RH7206:System.Console|};

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using static System.Console;
                                 using static System.Math;

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
    [Requirement("RH7206-004")]
    public async Task CodeFixLeavesTheCodeAfterTheBlockUnchanged()
    {
        const string testCode = """
                                using static System.Math;
                                using static {|RH7206:System.Console|};

                                class C
                                {
                                    void M( ) {   }
                                }
                                """;

        const string fixedCode = """
                                 using static System.Console;
                                 using static System.Math;

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