using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer"/> and <see cref="RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzerTests : BatchCodeFixTestsBase<RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer, RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying System namespace usings are reported and fixed when they appear after other usings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("RH7201-003")]
    [Requirement("RH7201-004")]
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

    /// <summary>
    /// Verifies that a case-variant root namespace remains an ordinary namespace and does not prevent the code fix from moving System first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("RH7201-004")]
    [Requirement("SHARED-USING-005")]
    public async Task CaseVariantSystemRootDoesNotDisagreeWithCanonicalOrdering()
    {
        const string testCode = """
                                using SYSTEM;
                                using {|#0:System|};

                                namespace SYSTEM
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
                                 using System;

                                 using SYSTEM;

                                 namespace SYSTEM
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

    /// <summary>
    /// Verifies that global-qualified case variants remain ordinary global usings when the exact System group moves first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("RH7201-004")]
    [Requirement("SHARED-USING-005")]
    public async Task GlobalQualifiedCaseVariantConvergesBehindExactSystemGlobalUsing()
    {
        const string testCode = """
                                global using global::SYSTEM.Text;
                                global using {|#0:System.Text|};

                                namespace SYSTEM.Text
                                {
                                    public class Helper
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 global using System.Text;

                                 global using global::SYSTEM.Text;

                                 namespace SYSTEM.Text
                                 {
                                     public class Helper
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer.DiagnosticId, AnalyzerResources.RH7201MessageFormat));
    }

    /// <summary>
    /// Verifies that System ordering remains registered for using directives inside a file-scoped namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("RH7201-004")]
    public async Task FileScopedNamespaceSystemUsingIsReportedAndFixed()
    {
        const string testCode = """
                                namespace Example;

                                using Microsoft.Win32;
                                using {|#0:System|};

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 namespace Example;

                                 using System;

                                 using Microsoft.Win32;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer.DiagnosticId, AnalyzerResources.RH7201MessageFormat));
    }

    /// <summary>
    /// Verifies no code fix is offered when the using directives cannot be safely reordered because a preprocessor directive is present
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public async Task NoCodeFixWhenUsingDirectivesCannotBeSafelyReordered()
    {
        const string testCode = """
                                using Alpha;
                                #if DEBUG
                                using System.Linq;
                                #endif
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<UsingDirectiveSyntax>()
                                                               .Single(usingDirective => usingDirective.Name?.ToString() == "System.Linq")
                                                               .GetLocation(),
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies disabled conditional using blocks are exempt when they cannot be safely reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("SHARED-USING-009")]
    public async Task DisabledConditionalUsingBlocksAreNotReportedWhenTheyCannotBeSafelyReordered()
    {
        const string testCode = """
                                using Alpha;
                                #if FEATURE
                                using System.Text;
                                #endif
                                using System;

                                namespace Alpha
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the blank line between two cross-group reordered usings is preserved when the using block is
    /// the only content in the file (no following declaration)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UsingOnlyFileCrossGroupReorderKeepsBlankLineSeparator()
    {
        const string testCode = """
                                using Microsoft.Win32;
                                using {|#0:System.Linq|};
                                """;

        const string fixedCode = """
                                 using System.Linq;

                                 using Microsoft.Win32;
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer.DiagnosticId, AnalyzerResources.RH7201MessageFormat));
    }

    /// <summary>
    /// Verifies that every System directive below a directive of another root is reported on its name, a global qualifier included, while static directives between them do not matter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("RH7201-003")]
    public async Task EverySystemDirectiveBelowAnotherRootIsReportedOnItsName()
    {
        const string testCode = """
                                using Microsoft.CodeAnalysis;
                                using {|RH7201:System.Linq|};
                                using static System.Math;
                                using {|RH7201:global::System.Text|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the global section is checked on its own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("SHARED-USING-002")]
    public async Task GlobalSectionIsCheckedOnItsOwn()
    {
        const string testCode = """
                                global using Microsoft.CodeAnalysis;
                                global using {|RH7201:System|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a local System directive is not compared with a global directive of another root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("SHARED-USING-002")]
    public async Task DirectivesOfDifferentSectionsAreNotCompared()
    {
        const string testCode = """
                                global using Microsoft.CodeAnalysis;
                                using System;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a System directive in a namespace is not compared with a directive of the compilation unit
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("SHARED-USING-001")]
    public async Task DirectivesOfDifferentBlocksAreNotCompared()
    {
        const string testCode = """
                                using Microsoft.CodeAnalysis;

                                namespace N
                                {
                                    using System;

                                    class C
                                    {
                                    }
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a block containing a preprocessor directive is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-001")]
    [Requirement("SHARED-USING-009")]
    public async Task BlockWithPreprocessorDirectiveIsNotReported()
    {
        const string testCode = """
                                using Microsoft.CodeAnalysis;
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                using System;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that static directives, alias directives, and roots that only resemble System are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-002")]
    [Requirement("SHARED-USING-005")]
    public async Task StaticAliasAndLookAlikeDirectivesAreNotReported()
    {
        const string testCode = """
                                using Microsoft.CodeAnalysis;
                                using SYSTEM.Text;
                                using Systematic;
                                using static System.Math;
                                using A = System.Text;

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
    [Requirement("RH7201-004")]
    public async Task CodeFixOrganizesTheWholeBlock()
    {
        const string testCode = """
                                using Microsoft.CodeAnalysis;
                                using {|RH7201:System.Linq|};
                                using {|RH7201:System|};

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;

                                 using Microsoft.CodeAnalysis;

                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the code fix leaves the code after the using block unchanged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7201-004")]
    public async Task CodeFixLeavesTheCodeAfterTheBlockUnchanged()
    {
        const string testCode = """
                                using Microsoft.CodeAnalysis;
                                using {|RH7201:System|};

                                class C
                                {
                                    void M( ) {   }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using Microsoft.CodeAnalysis;

                                 class C
                                 {
                                     void M( ) {   }
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using system;
                                using {|#0:System|};

                                namespace Example
                                {
                                    using SYSTEM.Text;
                                    using {|#1:System.Text|};

                                    public class TestClass
                                    {
                                    }
                                }

                                namespace system
                                {
                                    public class Helper
                                    {
                                    }
                                }

                                namespace SYSTEM.Text
                                {
                                    public class Helper
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using system;

                                 namespace Example
                                 {
                                     using System.Text;

                                     using SYSTEM.Text;

                                     public class TestClass
                                     {
                                     }
                                 }

                                 namespace system
                                 {
                                     public class Helper
                                     {
                                     }
                                 }

                                 namespace SYSTEM.Text
                                 {
                                     public class Helper
                                     {
                                     }
                                 }
                                 """;

        // Verifies that the code fix converges across compilation-unit and block-namespace using scopes in one Fix All iteration
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer.DiagnosticId, AnalyzerResources.RH7201MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}