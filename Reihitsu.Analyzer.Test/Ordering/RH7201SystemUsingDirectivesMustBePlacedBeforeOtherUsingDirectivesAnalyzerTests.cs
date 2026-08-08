using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
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

    /// <summary>
    /// Verifies that a case-variant root namespace remains an ordinary namespace and does not prevent the code fix from moving System first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// Verifies that the code fix converges across compilation-unit and block-namespace using scopes in one Fix All iteration
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CaseVariantSystemRootsAcrossScopesConvergeInOneFixAllIteration()
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

        await Verify(testCode,
                     fixedCode,
                     static config => config.NumberOfFixAllIterations = 1,
                     Diagnostics(RH7201SystemUsingDirectivesMustBePlacedBeforeOtherUsingDirectivesAnalyzer.DiagnosticId, AnalyzerResources.RH7201MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that System ordering remains registered for using directives inside a file-scoped namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}