using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer"/> and <see cref="RH7207UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzerTests : BatchCodeFixTestsBase<RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer, RH7207UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that no diagnostic is reported when there is only a single using directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-002")]
    public async Task NoDiagnosticForSingleUsing()
    {
        const string testCode = """
                                using System;

                                public class TestClass
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when all usings belong to the same root namespace and no blank line separates them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-002")]
    public async Task NoDiagnosticForSingleGroupAlreadySorted()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                public class TestClass
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a comment line between same-group usings is not treated as a blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task NoDiagnosticWhenCommentSeparatesSameGroupUsings()
    {
        const string testCode = """
                                using System;
                                // I/O helpers
                                using System.IO;

                                public class TestClass
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a comment line without a preceding blank line does not separate different using groups
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task DiagnosticWhenCommentSeparatesDifferentGroupsWithoutBlankLine()
    {
        const string testCode = """
                                using {|#0:System|};
                                // Alpha helpers
                                using Alpha;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 // Alpha helpers
                                 using Alpha;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that a region directive exempts a using block that cannot be safely reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("SHARED-USING-009")]
    public async Task NoDiagnosticWhenRegionDirectivePreventsSafeReordering()
    {
        const string testCode = """
                                using System;
                                #region Alpha helpers
                                using Alpha;
                                #endregion

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when usings are already organized into groups with correct blank lines
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-002")]
    public async Task NoDiagnosticWhenAlreadyOrganized()
    {
        const string testCode = """
                                using System;
                                using System.Collections.Generic;
                                using System.Linq;

                                using Alpha;
                                using Alpha.Sub;

                                using Bravo;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Alpha.Sub
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Bravo
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when regular, static, and alias usings are already grouped correctly
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-002")]
    public async Task NoDiagnosticWhenUsingTypesAreAlreadyGrouped()
    {
        const string testCode = """
                                using System;

                                using Alpha;
                                using Alpha.Sub;

                                using static System.Math;

                                using static Bravo.Helper;

                                using TextAlias = System.String;

                                using BravoAlias = Bravo.Helper;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Alpha.Sub
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Bravo
                                {
                                    public class Helper
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a diagnostic is reported on the first using when groups are missing blank line separators
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("RH7207-003")]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-008")]
    public async Task DiagnosticWhenMissingBlankLineBetweenGroups()
    {
        const string testCode = """
                                using {|#0:System|};
                                using System.Collections.Generic;
                                using System.Linq;
                                using Alpha;
                                using Alpha.Sub;
                                using Bravo;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Alpha.Sub
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Bravo
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 using Alpha;
                                 using Alpha.Sub;

                                 using Bravo;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Alpha.Sub
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Bravo
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix also sorts usings alphabetically within each group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-007")]
    public async Task CodeFixSortsUsingWithinGroups()
    {
        const string testCode = """
                                using {|#0:System.Linq|};
                                using System;
                                using System.Collections.Generic;
                                using Alpha.Sub;
                                using Alpha;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Alpha.Sub
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 using Alpha;
                                 using Alpha.Sub;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Alpha.Sub
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix moves a case-variant global using ahead of a local exact-System using and separates the sections
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-002")]
    public async Task CodeFixPlacesGlobalSectionBeforeLocalExactSystemSection()
    {
        const string testCode = """
                                using {|#0:System.Text|};
                                global using SYSTEM.Text;

                                namespace SYSTEM.Text
                                {
                                    public class Placeholder;
                                }
                                """;
        const string fixedCode = """
                                 global using SYSTEM.Text;

                                 using System.Text;

                                 namespace SYSTEM.Text
                                 {
                                     public class Placeholder;
                                 }
                                 """;

        await Verify(testCode,
                     fixedCode,
                     test => test.CompilerDiagnostics = CompilerDiagnostics.None,
                     Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that global status alone creates a group boundary for otherwise matching regular System roots
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-002")]
    public async Task CodeFixSeparatesGlobalAndLocalDirectivesWithSameRoot()
    {
        const string testCode = """
                                global using {|#0:System.Collections|};
                                using System.Text;

                                public class TestClass;
                                """;
        const string fixedCode = """
                                 global using System.Collections;

                                 using System.Text;

                                 public class TestClass;
                                 """;

        await Verify(testCode,
                     fixedCode,
                     Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix only changes using directives in the reported scope
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    public async Task CodeFixDoesNotFormatFollowingType()
    {
        const string testCode = """
                                using {|#0:Alpha.Sub|};
                                using System;

                                internal class Example
                                {
                                    internal int Method()
                                    {
                                        var value=1;return value;
                                    }
                                }

                                namespace Alpha.Sub
                                {
                                    internal static class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using Alpha.Sub;

                                 internal class Example
                                 {
                                     internal int Method()
                                     {
                                         var value=1;return value;
                                     }
                                 }

                                 namespace Alpha.Sub
                                 {
                                     internal static class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that a diagnostic is reported when an extra blank line exists within a group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-008")]
    public async Task DiagnosticWhenExtraBlankLineWithinGroup()
    {
        const string testCode = """
                                using {|#0:System|};

                                using System.Collections.Generic;

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Collections.Generic;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix preserves an attached comment when reordering
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixPreservesAttachedCommentWhenReordering()
    {
        const string testCode = """
                                using {|#0:System.Linq|};
                                // Keep with collections
                                using System.Collections;

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 // Keep with collections
                                 using System.Collections;
                                 using System.Linq;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that System group is placed before other groups in the fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-005")]
    public async Task CodeFixPlacesSystemGroupFirst()
    {
        const string testCode = """
                                using {|#0:Alpha|};
                                using System;
                                using System.Linq;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;

                                 using Alpha;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that static usings are separated from regular usings and grouped by root namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-003")]
    public async Task StaticUsingsAreSeparatedFromRegularUsings()
    {
        const string testCode = """
                                using {|#0:System|};
                                using static Alpha.Helper;
                                using Alpha;
                                using static System.Math;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Helper
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using Alpha;

                                 using static System.Math;

                                 using static Alpha.Helper;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Helper
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that alias usings are grouped by root namespace of their target and kept after other using types
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-004")]
    public async Task AliasUsingsAreGroupedByRootNamespace()
    {
        const string testCode = """
                                using {|#0:System|};
                                using SysLinq = System.Linq;
                                using AlphaAlias = Alpha.Something;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Something
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using SysLinq = System.Linq;

                                 using AlphaAlias = Alpha.Something;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Something
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that alias usings are placed after static usings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-003")]
    public async Task AliasUsingsAreSeparatedFromStaticUsings()
    {
        const string testCode = """
                                using {|#0:System|};
                                using Alias = Alpha.Helper;
                                using static System.Math;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Helper
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using static System.Math;

                                 using Alias = Alpha.Helper;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Helper
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that global using directives are organized by root namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-002")]
    public async Task GlobalUsingDirectivesAreOrganized()
    {
        const string testCode = """
                                global using {|#0:System|};
                                global using System.Linq;
                                global using Alpha;

                                public class TestClass
                                {
                                }

                                namespace Alpha
                                {
                                    public class Placeholder
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 global using System;
                                 global using System.Linq;

                                 global using Alpha;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that using groups inside a file-scoped namespace are reported and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-001")]
    public async Task FileScopedNamespaceUsingGroupsAreReportedAndFixed()
    {
        const string testCode = """
                                namespace Example;

                                using {|#0:Microsoft.Win32|};
                                using System;

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

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when using directives are zero in a compilation unit
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-002")]
    public async Task NoDiagnosticForNoUsings()
    {
        const string testCode = """
                                public class TestClass
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that non-System groups are ordered alphabetically by root namespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-006")]
    public async Task NonSystemGroupsAreOrderedAlphabetically()
    {
        const string testCode = """
                                using {|#0:Zebra.Something|};
                                using Alpha.Something;
                                using System;

                                public class TestClass
                                {
                                }

                                namespace Alpha.Something
                                {
                                }

                                namespace Zebra.Something
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using Alpha.Something;

                                 using Zebra.Something;

                                 public class TestClass
                                 {
                                 }

                                 namespace Alpha.Something
                                 {
                                 }

                                 namespace Zebra.Something
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat));
    }

    /// <summary>
    /// Verifies that no code fix is offered when a conditional directive would be moved
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public async Task NoCodeFixWhenConditionalDirectiveWouldBeMoved()
    {
        const string testCode = """
                                using System;
                                #if DEBUG
                                using Alpha;
                                #endif

                                public class TestClass
                                {
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testCode,
                                                   RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId,
                                                   root =>
                                                   {
                                                       var usingDirective = root.DescendantNodes()
                                                                                .OfType<UsingDirectiveSyntax>()
                                                                                .First();

                                                       return usingDirective.Name?.GetLocation() ?? usingDirective.GetLocation();
                                                   },
                                                   "DEBUG");

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies disabled conditional using blocks are exempt when they cannot be safely reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("SHARED-USING-009")]
    public async Task DisabledConditionalUsingBlocksAreNotReportedWhenTheyCannotBeSafelyReordered()
    {
        const string testCode = """
                                using System.Text;
                                #if FEATURE
                                using System.Web;
                                #endif
                                using System;
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that reordering the only using directives in a file into different groups converges on a
    /// single application of the code fix instead of needing a second pass to add the missing blank-line
    /// separator
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UsingOnlyFileCrossGroupReorderConvergesInOneApplication()
    {
        const string testCode = """
                                using Microsoft.Win32;
                                using System.Linq;
                                """;
        const string expected = """
                                using System.Linq;

                                using Microsoft.Win32;
                                """;

        var actual = await ApplyCodeFixAsync(testCode);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a cross-group pair sharing one line behind a space is separated by a blank line in a
    /// single application of the code fix, without leaving the space behind as trailing whitespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-008")]
    public async Task CrossGroupPairSharingOneLineConvergesInOneApplication()
    {
        const string testCode = """
                                using System; using Microsoft.Win32;
                                """;
        const string expected = """
                                using System;

                                using Microsoft.Win32;
                                """;

        var actual = await ApplyCodeFixAsync(testCode);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that reordering a block keeps a delimited documentation comment on the line of the directive
    /// it trails when its source neighbor stays next to it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReorderKeepsDelimitedDocumentationCommentOnItsLine()
    {
        const string testCode = "using System.Text;\nusing System; /** first */\nusing System.Linq;\n";
        const string expected = "using System; /** first */\nusing System.Linq;\nusing System.Text;\n";

        var actual = await ApplyCodeFixAsync(testCode);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that reordering a block keeps a delimited documentation comment on the line of the directive
    /// it trails when a paragraph separator already ends that line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task ReorderKeepsDelimitedDocumentationCommentBeforeParagraphSeparatorOnItsLine()
    {
        const string testCode = "using System.Text;\nusing System; /** first */\u2029using System.Linq;\n";
        const string expected = "using System; /** first */\u2029using System.Linq;\nusing System.Text;\n";

        var actual = await ApplyCodeFixAsync(testCode);

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that a block whose directives are not in the canonical order is reported on its first directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("RH7207-003")]
    public async Task BlockOutOfCanonicalOrderIsReported()
    {
        const string testCode = """
                                using {|RH7207:System.Linq|};
                                using System;
                                using Microsoft.CodeAnalysis;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a block without a blank line between two groups is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("SHARED-USING-008")]
    public async Task MissingBlankLineBetweenGroupsIsReported()
    {
        const string testCode = """
                                using {|RH7207:System|};
                                using Microsoft.CodeAnalysis;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that two directives of different groups on one line are reported as a missing blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("SHARED-USING-008")]
    public async Task GroupsSharingOneLineAreReported()
    {
        const string testCode = """
                                using {|RH7207:System|}; using Microsoft.CodeAnalysis;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a blank line inside a group is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("SHARED-USING-008")]
    public async Task BlankLineInsideAGroupIsReported()
    {
        const string testCode = """
                                using {|RH7207:System|};

                                using System.Linq;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that only the block that is not organized is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-001")]
    [Requirement("SHARED-USING-001")]
    public async Task OnlyTheBlockThatIsNotOrganizedIsReported()
    {
        const string testCode = """
                                using System;

                                using Microsoft.CodeAnalysis;

                                namespace N
                                {
                                    using {|RH7207:System.Linq|};
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
    [Requirement("RH7207-001")]
    [Requirement("SHARED-USING-009")]
    public async Task BlockWithPreprocessorDirectiveIsNotReported()
    {
        const string testCode = """
                                using System.Linq;
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
    /// Verifies that an organized block is not reported, also when a group boundary has more than one blank line before it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-002")]
    public async Task OrganizedBlockWithSeveralBlankLinesAtABoundaryIsNotReported()
    {
        const string testCode = """
                                using System;


                                using Microsoft.CodeAnalysis;

                                using static System.Math;

                                using A = System.Text;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a block with a single directive is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-002")]
    public async Task BlockWithASingleDirectiveIsNotReported()
    {
        const string testCode = """
                                using static System.Math;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the diagnostic of a block starting with an alias is on the alias target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-003")]
    public async Task DiagnosticOfABlockStartingWithAnAliasIsOnTheAliasTarget()
    {
        const string testCode = """
                                using A = {|RH7207:System.Text|};
                                using System;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the diagnostic of a block starting with an alias of a keyword type covers the whole directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-003")]
    public async Task DiagnosticOfABlockStartingWithAnAliasOfAKeywordTypeCoversTheDirective()
    {
        const string testCode = """
                                {|RH7207:using B = int;|}
                                using System;

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
    [Requirement("RH7207-004")]
    public async Task CodeFixOrganizesTheWholeBlock()
    {
        const string testCode = """
                                using {|RH7207:System.Linq|};
                                using System;
                                using Microsoft.CodeAnalysis;

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
    /// Verifies that the using directives of the compilation unit and of a namespace are organized as separate blocks
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-001")]
    public async Task EveryUsingBlockIsOrganizedOnItsOwn()
    {
        const string testCode = """
                                using {|RH7207:System.Linq|};
                                using System;

                                namespace N
                                {
                                    using {|RH7207:Reihitsu.Core|};
                                    using System.Text;

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;

                                 namespace N
                                 {
                                     using System.Text;

                                     using Reihitsu.Core;

                                     class C
                                     {
                                         int x;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that global directives are placed before every non-global directive in a section of their own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-002")]
    public async Task GlobalDirectivesComeFirst()
    {
        const string testCode = """
                                using {|RH7207:System|};
                                global using Microsoft.CodeAnalysis;
                                global using System.Linq;

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 global using System.Linq;

                                 global using Microsoft.CodeAnalysis;

                                 using System;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that regular directives come before static directives, and static directives before alias directives
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-003")]
    public async Task RegularThenStaticThenAliasDirectives()
    {
        const string testCode = """
                                using static {|RH7207:System.Math|};
                                using A = System.Text;
                                using System;

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using static System.Math;

                                 using A = System.Text;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a group is made of section, kind, and root namespace, and that aliases are grouped by their target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-004")]
    public async Task GroupIsSectionKindAndRootNamespace()
    {
        const string testCode = """
                                using Zeta = {|RH7207:Microsoft.CodeAnalysis.SyntaxNode|};
                                using static System.Math;
                                using Reihitsu.Core;
                                using Alpha = System.Text.StringBuilder;
                                using Microsoft.CodeAnalysis;
                                using System.Linq;
                                using static Reihitsu.Core.CasingUtilities;
                                using System;

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;

                                 using Microsoft.CodeAnalysis;

                                 using Reihitsu.Core;

                                 using static System.Math;

                                 using static Reihitsu.Core.CasingUtilities;

                                 using Alpha = System.Text.StringBuilder;

                                 using Zeta = Microsoft.CodeAnalysis.SyntaxNode;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that only the exact System root is placed first, while case variants are ordinary roots
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-005")]
    public async Task ExactSystemRootComesFirst()
    {
        const string testCode = """
                                using {|RH7207:System.Linq|};
                                using SYSTEM.Text;
                                using System;

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;

                                 using SYSTEM.Text;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the other roots are ordered alphabetically ignoring case, with roots that differ only in case kept as separate groups
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-006")]
    public async Task OtherRootsAreOrderedAlphabetically()
    {
        const string testCode = """
                                using {|RH7207:Systematic|};
                                using system.Text;
                                using foo.Bar;
                                using Foo.Baz;
                                using System;

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using Foo.Baz;

                                 using foo.Bar;

                                 using system.Text;

                                 using Systematic;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that aliases are sorted by alias name within the group of their target root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-007")]
    public async Task AliasesAreSortedByAliasNameWithinTheirGroup()
    {
        const string testCode = """
                                using Zeta = {|RH7207:System.Text.StringBuilder|};
                                using Beta = Microsoft.CodeAnalysis.SyntaxNode;
                                using Alpha = Microsoft.CodeAnalysis.SyntaxToken;
                                using Gamma = System.Console;

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using Gamma = System.Console;
                                 using Zeta = System.Text.StringBuilder;

                                 using Alpha = Microsoft.CodeAnalysis.SyntaxToken;
                                 using Beta = Microsoft.CodeAnalysis.SyntaxNode;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that directives whose keys differ only in case keep their source order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-007")]
    public async Task EqualKeysKeepTheirSourceOrder()
    {
        const string testCode = """
                                using {|RH7207:System.Xml|};
                                using System.collections;
                                using System.Collections;

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using System.collections;
                                 using System.Collections;
                                 using System.Xml;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a blank line is placed between two groups and removed inside a group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-008")]
    public async Task BlankLineBetweenGroupsAndNoneWithinAGroup()
    {
        const string testCode = """
                                using {|RH7207:System|};

                                using System.Linq;
                                using Microsoft.CodeAnalysis;

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;

                                 using Microsoft.CodeAnalysis;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a block whose first directive is led by a nullable directive is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public async Task BlockLedByANullableDirectiveIsNotReported()
    {
        const string testCode = """
                                #nullable enable
                                using System.Linq;
                                using System;

                                class C
                                {
                                    int x;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a preprocessor directive after the last using directive does not prevent the block from being organized
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7207-004")]
    [Requirement("SHARED-USING-009")]
    public async Task DirectiveAfterTheBlockDoesNotPreventOrganizing()
    {
        const string testCode = """
                                using {|RH7207:System.Linq|};
                                using System;
                                #if DEBUG
                                class D
                                {
                                    int y;
                                }
                                #endif

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;
                                 #if DEBUG
                                 class D
                                 {
                                     int y;
                                 }
                                 #endif

                                 class C
                                 {
                                     int x;
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
                                using {|#0:SYSTEM|};
                                using System;

                                namespace Example
                                {
                                    using {|#1:system.Text|};
                                    using System.Text;

                                    public class TestClass
                                    {
                                    }
                                }

                                namespace SYSTEM
                                {
                                    public class Helper
                                    {
                                    }
                                }

                                namespace system.Text
                                {
                                    public class Helper
                                    {
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using SYSTEM;

                                 namespace Example
                                 {
                                     using System.Text;

                                     using system.Text;

                                     public class TestClass
                                     {
                                     }
                                 }

                                 namespace SYSTEM
                                 {
                                     public class Helper
                                     {
                                     }
                                 }

                                 namespace system.Text
                                 {
                                     public class Helper
                                     {
                                     }
                                 }
                                 """;

        // Verifies that exact and case-variant System roots form separate groups across compilation-unit and block-namespace scopes
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7207UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId, AnalyzerResources.RH7207MessageFormat, 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase
}