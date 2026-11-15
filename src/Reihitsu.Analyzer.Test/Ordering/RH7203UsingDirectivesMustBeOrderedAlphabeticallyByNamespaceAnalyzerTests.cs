using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer"/> and <see cref="RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzerTests : BatchCodeFixTestsBase<RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer, RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying regular usings are reported and fixed when they are not alphabetically ordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    [Requirement("RH7203-003")]
    [Requirement("RH7203-004")]
    public async Task RegularUsingsAreReportedAndFixedWhenTheyAreNotAlphabeticallyOrdered()
    {
        const string testCode = """
                                using System.Linq;
                                using {|#0:System.Collections.Generic|};

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    /// <summary>
    /// Verifies that System namespace usings stay isolated from other groups
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task SystemNamespaceUsingsDoNotProduceDiagnosticsWhenSorted()
    {
        const string testCode = """
                                using System.Collections.Generic;
                                using System.Linq;
                                using System.Threading;
                                using System.Threading.Tasks;

                                using TestNamespace;
                                using TestNamespace.Subnamespace;

                                namespace TestNamespace
                                {
                                    public class Helper
                                    {
                                    }
                                }

                                namespace TestNamespace.Subnamespace
                                {
                                    public class NestedHelper
                                    {
                                    }
                                }

                                public class TestClass
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a sorted System group followed by a sorted Microsoft group does not produce diagnostics
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-002")]
    public async Task SystemAndMicrosoftGroupsDoNotProduceDiagnosticsWhenIndividuallySorted()
    {
        const string testCode = """
                                using System.Collections.Immutable;
                                using System.Threading;
                                using System.Threading.Tasks;

                                using Microsoft.CodeAnalysis;
                                using Microsoft.CodeAnalysis.CodeActions;
                                using Microsoft.CodeAnalysis.CodeFixes;
                                using Microsoft.CodeAnalysis.CSharp;

                                namespace Microsoft.CodeAnalysis
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CodeActions
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CodeFixes
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CSharp
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Reihitsu.Analyzer.Base
                                {
                                    public class TestClass
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that Microsoft usings are fixed without disturbing the sorted System group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    [Requirement("RH7203-004")]
    public async Task MicrosoftUsingsAreFixedWithoutMovingSortedSystemGroup()
    {
        const string testCode = """
                                using System.Collections.Immutable;
                                using System.Threading;
                                using System.Threading.Tasks;

                                using Microsoft.CodeAnalysis.CSharp;
                                using {|#0:Microsoft.CodeAnalysis|};
                                using {|#1:Microsoft.CodeAnalysis.CodeActions|};
                                using {|#2:Microsoft.CodeAnalysis.CodeFixes|};

                                namespace Microsoft.CodeAnalysis
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CodeActions
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CodeFixes
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Microsoft.CodeAnalysis.CSharp
                                {
                                    public class Placeholder
                                    {
                                    }
                                }

                                namespace Reihitsu.Analyzer.Base
                                {
                                    public class TestClass
                                    {
                                    }
                                }
                                """;
        const string fixedCode = """
                                 using System.Collections.Immutable;
                                 using System.Threading;
                                 using System.Threading.Tasks;

                                 using Microsoft.CodeAnalysis;
                                 using Microsoft.CodeAnalysis.CodeActions;
                                 using Microsoft.CodeAnalysis.CodeFixes;
                                 using Microsoft.CodeAnalysis.CSharp;

                                 namespace Microsoft.CodeAnalysis
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Microsoft.CodeAnalysis.CodeActions
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Microsoft.CodeAnalysis.CodeFixes
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Microsoft.CodeAnalysis.CSharp
                                 {
                                     public class Placeholder
                                     {
                                     }
                                 }

                                 namespace Reihitsu.Analyzer.Base
                                 {
                                     public class TestClass
                                     {
                                     }
                                 }
                                 """;

        // Microsoft.CodeAnalysis.CodeActions and Microsoft.CodeAnalysis.CodeFixes sort before the earlier
        // Microsoft.CodeAnalysis.CSharp of their group, so they are reported although the directive right before each sorts lower
        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that member names in a following ordinal-distinct root cannot create a cross-group diagnostic
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-002")]
    [Requirement("SHARED-USING-004")]
    public async Task OrdinalDistinctRootGroupsDoNotProduceCrossGroupDiagnostics()
    {
        const string testCode = """
                                using SYSTEM.Alpha;
                                using SYSTEM.Zulu;

                                using system.Bravo;

                                namespace SYSTEM.Alpha
                                {
                                    public class Placeholder;
                                }

                                namespace SYSTEM.Zulu
                                {
                                    public class Placeholder;
                                }

                                namespace system.Bravo
                                {
                                    public class Placeholder;
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the shared using-ordering code fix does not format the following type
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-004")]
    public async Task CodeFixDoesNotFormatFollowingType()
    {
        const string testCode = """
                                using System.Linq;
                                using {|#0:System.Collections.Generic|};

                                public class TestClass
                                {
                                    public int Count()
                                    {
                                        var values = new List<int>();
                                        return values.Count;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;

                                 public class TestClass
                                 {
                                     public int Count()
                                     {
                                         var values = new List<int>();
                                         return values.Count;
                                     }
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    /// <summary>
    /// Verifies that a file-header banner above the original first using directive stays at the top of
    /// the scope when the code fix demotes that directive, instead of moving into the middle of the
    /// using block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task CodeFixKeepsBannerAtTopWhenFirstUsingIsDemoted()
    {
        const string testCode = """
                                // Copyright (c) Example Corp. All rights reserved.

                                using System.Linq;
                                using {|#0:System.Collections.Generic|};

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 // Copyright (c) Example Corp. All rights reserved.

                                 using System.Collections.Generic;
                                 using System.Linq;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    /// <summary>
    /// Verifies disabled conditional using blocks are exempt when they cannot be safely reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
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
    /// Verifies the separator between two same-group reordered usings is preserved when the using block is
    /// the only content in the file (no following declaration)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task UsingOnlyFileSameGroupReorderKeepsNewlineSeparator()
    {
        const string testCode = """
                                using System.Linq;
                                using {|#0:System.Collections.Generic|};
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    /// <summary>
    /// Verifies that every regular directive sorted before the directive of its group directly above it is reported on its name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    [Requirement("RH7203-003")]
    public async Task EveryDirectiveSortedBeforeItsNeighborIsReportedOnItsName()
    {
        const string testCode = """
                                using System.Linq;
                                using {|RH7203:System|};
                                using Microsoft.CodeAnalysis.CSharp;
                                using {|RH7203:Microsoft.CodeAnalysis|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the order within a group ignores case
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    [Requirement("SHARED-USING-007")]
    public async Task CaseDoesNotDecideTheOrder()
    {
        const string testCode = """
                                using System.Xml;
                                using {|RH7203:System.collections|};

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
    [Requirement("RH7203-001")]
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
    /// Verifies that equal keys, directives of other groups, and static and alias directives are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-002")]
    [Requirement("SHARED-USING-004")]
    [Requirement("SHARED-USING-007")]
    public async Task EqualKeysOtherGroupsAndOtherKindsAreNotReported()
    {
        const string testCode = """
                                using System.collections;
                                using System.Collections;

                                using Microsoft.B;
                                using microsoft.A;

                                using static System.Math;
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
    [Requirement("RH7203-004")]
    public async Task CodeFixOrganizesTheWholeBlock()
    {
        const string testCode = """
                                using System.Linq;
                                using {|RH7203:System|};
                                using Microsoft.CodeAnalysis.CSharp;
                                using {|RH7203:Microsoft.CodeAnalysis|};

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;

                                 using Microsoft.CodeAnalysis;
                                 using Microsoft.CodeAnalysis.CSharp;

                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies the specification example of a group that is not contiguous: <c>Microsoft.A</c> is compared with the
    /// earlier <c>Microsoft.B</c> of its group and reported, although a directive of the <c>System</c> group lies between
    /// them, and the code fix organizes the block in one pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-007")]
    [Requirement("RH7203-001")]
    [Requirement("RH7203-004")]
    public async Task DirectiveIsComparedWithAnEarlierDirectiveOfItsGroupAcrossAnotherGroup()
    {
        const string testCode = """
                                using Microsoft.B;
                                using System;
                                using {|RH7203:Microsoft.A|};

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using System;

                                 using Microsoft.A;
                                 using Microsoft.B;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies a directive is compared with an earlier directive of its group across a directive of another kind:
    /// <c>Microsoft.A</c> is reported after <c>Microsoft.B</c>, although a <see langword="using"/> <see langword="static"/>
    /// directive lies between them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-007")]
    [Requirement("RH7203-001")]
    public async Task DirectiveIsComparedWithAnEarlierDirectiveOfItsGroupAcrossAnotherKind()
    {
        const string testCode = """
                                using Microsoft.B;
                                using static System.Math;
                                using {|RH7203:Microsoft.A|};

                                class C
                                {
                                    int x;
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies directives of one group that are in order are not reported when a directive of another group lies
    /// between them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-007")]
    [Requirement("RH7203-001")]
    public async Task OrderedDirectivesOfAGroupAreNotReportedAcrossAnotherGroup()
    {
        const string testCode = """
                                using Microsoft.A;
                                using System;
                                using Microsoft.B;

                                class C
                                {
                                    int x;
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the code fix writes one blank line where several stood above a comment that leads a directive of the
    /// same group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-004")]
    public async Task CodeFixWritesOneBlankLineWhereSeveralStoodAboveCommentLeadingDirectiveOfTheSameGroup()
    {
        const string testCode = """
                                using System.Linq;
                                using {|#0:System|};



                                // About Text
                                using System.Text;

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq;

                                 // About Text
                                 using System.Text;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix of a block in a namespace that does not start its line moves a directive onto a line of its
    /// own with the indentation of the line the block's first directive starts on, and leaves the code behind the block on its
    /// line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-004")]
    public async Task CodeFixOfABlockInANamespaceThatDoesNotStartItsLineKeepsTheLineIndentation()
    {
        const string testCode = """
                                namespace A { namespace B { using System.Linq; using {|RH7203:System|}; class C { } } }
                                """;

        const string fixedCode = """
                                 namespace A { namespace B { using System;
                                 using System.Linq; class C { } } }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that the code fix of a block in an indented namespace puts each directive on a line of its own at the
    /// namespace's indentation and does not format the class behind the block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-004")]
    public async Task CodeFixOfANamespaceBlockPlacesSplitDirectivesAtTheScopeIndentation()
    {
        const string testCode = """
                                namespace N
                                {
                                    using System.Linq; using {|RH7203:System|};

                                    class C{ }
                                }
                                """;

        const string fixedCode = """
                                 namespace N
                                 {
                                     using System;
                                     using System.Linq;

                                     class C{ }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a directive is compared with an earlier directive of its group across a regular directive of
    /// another root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    public async Task DirectiveIsComparedWithAnEarlierDirectiveOfItsGroupAcrossARegularDirectiveOfAnotherRoot()
    {
        const string testCode = """
                                using Microsoft.B;
                                using Alpha.X;
                                using {|RH7203:Microsoft.A|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the code fix organizes a block whose directive is out of order with an earlier directive of its
    /// group across a regular directive of another root, in one pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    [Requirement("RH7203-004")]
    public async Task CodeFixOrganizesABlockOutOfOrderAcrossARegularDirectiveOfAnotherRoot()
    {
        const string testCode = """
                                using Microsoft.B;
                                using Alpha.X;
                                using {|RH7203:Microsoft.A|};

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using Alpha.X;

                                 using Microsoft.A;
                                 using Microsoft.B;

                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a directive is compared with every earlier directive of its group, not only with the one directly
    /// above it: <c>Microsoft.B</c> is reported after <c>Microsoft.C</c>, although <c>Microsoft.A</c> lies between them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    public async Task DirectiveIsComparedWithEveryEarlierDirectiveOfItsGroup()
    {
        const string testCode = """
                                using Microsoft.C;
                                using {|RH7203:Microsoft.A|};
                                using {|RH7203:Microsoft.B|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a directive of the <c>System</c> group is compared with every earlier directive of that group
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    public async Task SystemDirectiveIsComparedWithEveryEarlierDirectiveOfTheSystemGroup()
    {
        const string testCode = """
                                using System.Text;
                                using {|RH7203:System.Collections|};
                                using {|RH7203:System.IO|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a directive of the global section is compared with an earlier directive of its group across a
    /// regular directive of another root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    [Requirement("SHARED-USING-002")]
    public async Task GlobalDirectiveIsComparedWithAnEarlierDirectiveOfItsGroupAcrossAnotherRoot()
    {
        const string testCode = """
                                global using Microsoft.B;
                                global using Alpha.X;
                                global using {|RH7203:Microsoft.A|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that directives of one group in order are not reported across a regular directive of another root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-002")]
    public async Task OrderedDirectivesOfAGroupAreNotReportedAcrossARegularDirectiveOfAnotherRoot()
    {
        const string testCode = """
                                using Microsoft.A;
                                using Alpha.X;
                                using Microsoft.B;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that a directive with whitespace between the tokens of its name is compared by its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    public async Task DirectiveWithWhitespaceInItsNameIsComparedByItsValue()
    {
        const string testCode = """
                                using System.Linq;
                                using {|RH7203:System . IO|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a directive written with a verbatim prefix is compared by its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    public async Task VerbatimDirectiveIsComparedByItsValue()
    {
        const string testCode = """
                                using System.Linq;
                                using {|RH7203:@System.IO|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a directive with a comment between the tokens of its name is compared by its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    public async Task DirectiveWithCommentInItsNameIsComparedByItsValue()
    {
        const string testCode = """
                                using System.Linq;
                                using {|RH7203:System/* x */.IO|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a directive whose name holds a Unicode escape is compared by its value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    public async Task UnicodeEscapedDirectiveIsComparedByItsValue()
    {
        const string testCode = """
                                using System.Linq;
                                using {|RH7203:\u0053ystem.IO|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that a directive whose value sorts after the one above it is not reported, although its spelling with a
    /// verbatim prefix would sort before it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-002")]
    public async Task VerbatimSegmentThatSortsAfterByItsValueIsNotReported()
    {
        const string testCode = """
                                using System.Collections;
                                using System.@Linq;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the specification example: a leading <c>global::</c> is not part of the sort key, so a directive sorted
    /// before <c>global::System.IO</c> by its value is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    public async Task GlobalQualifierIsNotPartOfTheSortKey()
    {
        const string testCode = """
                                using global::System.IO;
                                using {|RH7203:System.Collections|};

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the code fix places a directive before <c>global::System.IO</c> when its value sorts first, and
    /// keeps the spelling of the qualified directive
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-001")]
    [Requirement("RH7203-004")]
    public async Task CodeFixPlacesADirectiveBeforeAGlobalQualifiedDirectiveThatSortsAfterIt()
    {
        const string testCode = """
                                using global::System.IO;
                                using {|#0:System.Collections|};

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections;
                                 using global::System.IO;

                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat));
    }

    /// <summary>
    /// Verifies the specification example: <c>global::System.IO</c> after <c>System.Collections</c> is in order by its
    /// value and is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7203-002")]
    public async Task GlobalQualifiedDirectiveThatSortsAfterByItsValueIsNotReported()
    {
        const string testCode = """
                                using System.Collections;
                                using global::System.IO;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies the specification example: a line that starts with <c>#</c> inside a file header comment is comment text,
    /// so the block is reported and the code fix reorders it with the header kept on top
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task HashLineInFileHeaderCommentDoesNotPreventReportAndFix(string endOfLine)
    {
        const string testCode = """
                                /*
                                # Header
                                */
                                using System.Linq;
                                using {|RH7203:System|};

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 /*
                                 # Header
                                 */
                                 using System;
                                 using System.Linq;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that an indented line that starts with <c>#</c> inside a comment between two directives does not prevent the
    /// report, and that the comment moves with the directive it leads
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task HashLineInCommentBetweenDirectivesDoesNotPreventReportAndFix(string endOfLine)
    {
        const string testCode = """
                                using System.Linq;
                                /*
                                    # Note
                                */
                                using {|RH7203:System|};

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 /*
                                     # Note
                                 */
                                 using System;
                                 using System.Linq;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a comment that starts behind a directive's semicolon does not
    /// prevent the report, and that the comment stays behind its directive
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task HashLineInTrailingCommentDoesNotPreventReportAndFix(string endOfLine)
    {
        const string testCode = """
                                using System.Linq; /*
                                # Note
                                */
                                using {|RH7203:System|};

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using System;
                                 using System.Linq; /*
                                 # Note
                                 */

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a delimited documentation comment before the first directive
    /// does not prevent the report and the fix
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task HashLineInDelimitedDocumentationCommentDoesNotPreventReportAndFix(string endOfLine)
    {
        const string testCode = """
                                /**
                                # Header
                                */
                                using System.Linq;
                                using {|RH7203:System|};

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 /**
                                 # Header
                                 */
                                 using System;
                                 using System.Linq;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a comment between the tokens of a directive does not prevent the
    /// report and the fix
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task HashLineInCommentBetweenTokensOfDirectiveDoesNotPreventReportAndFix(string endOfLine)
    {
        const string testCode = """
                                using System.Linq;
                                using /*
                                # Note
                                */ {|RH7203:System|};

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 using /*
                                 # Note
                                 */ System;
                                 using System.Linq;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a comment before the first directive of a block-scoped namespace
    /// does not prevent the report
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task HashLineInCommentOfBlockNamespaceDoesNotPreventReport(string endOfLine)
    {
        const string testCode = """
                                namespace N
                                {
                                    /*
                                    # Header
                                    */
                                    using System.Linq;
                                    using {|RH7203:System|};

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        await Verify(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies that a line that starts with <c>#</c> inside a comment before the first directive of a file-scoped namespace
    /// does not prevent the report and the fix
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task HashLineInCommentOfFileScopedNamespaceDoesNotPreventReportAndFix(string endOfLine)
    {
        const string testCode = """
                                namespace N;

                                /*
                                # Header
                                */
                                using System.Linq;
                                using {|RH7203:System|};

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 namespace N;

                                 /*
                                 # Header
                                 */
                                 using System;
                                 using System.Linq;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that a commented-out conditional block of directives is comment text and does not prevent the report and the
    /// fix
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task CommentedOutConditionalDirectivesDoNotPreventReportAndFix(string endOfLine)
    {
        const string testCode = """
                                /*
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                */
                                using System.Linq;
                                using {|RH7203:System|};

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 /*
                                 #if DEBUG
                                 using System.Diagnostics;
                                 #endif
                                 */
                                 using System;
                                 using System.Linq;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await VerifyCodeFixWithEndOfLine(testCode, fixedCode, endOfLine);
    }

    /// <summary>
    /// Verifies that every diagnostic of a block behind a file header comment with a line that starts with <c>#</c> is
    /// reported and that one fix, applied one at a time or with Fix All, organizes the whole block
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    public async Task HashLineInFileHeaderCommentDoesNotPreventFixAll()
    {
        const string testCode = """
                                /*
                                # Header
                                */
                                using System.Text;
                                using {|RH7203:System.Linq|};
                                using {|RH7203:System.Collections.Generic|};

                                class C
                                {
                                    int x;
                                }
                                """;

        const string fixedCode = """
                                 /*
                                 # Header
                                 */
                                 using System.Collections.Generic;
                                 using System.Linq;
                                 using System.Text;

                                 class C
                                 {
                                     int x;
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that a comment with a line that reads as <c>#else</c> in another build, started inside a conditional group that
    /// closes behind the block of a namespace, does not prevent the report, while no code fix is offered in a document that
    /// holds such a comment
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [Requirement("SHARED-FIX-012")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task HashLineReadAsDirectiveByAnotherBuildIsReportedWithoutFix(string endOfLine)
    {
        const string testCode = """
                                #if !A
                                namespace N
                                {
                                    /*
                                #else
                                namespace M
                                {
                                    // */
                                    using System.Linq;
                                    using {|RH7203:System|};
                                #endif

                                    class C
                                    {
                                        int x;
                                    }
                                }
                                """;

        await VerifyNoCodeFixIsOffered(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies the specification example: a <c>#pragma</c> directive between the tokens of a using directive makes the block
    /// unsafe, so it is not reported
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task PragmaBetweenTokensOfDirectiveIsNotReported(string endOfLine)
    {
        const string testCode = """
                                using System.Linq;
                                using System
                                #pragma warning disable CS0105
                                .Text;
                                using System;

                                class C
                                {
                                    int x;
                                }
                                """;

        await Verify(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies that an <c>#if</c> and its <c>#endif</c> between the tokens of one using directive make the block unsafe, so
    /// it is not reported
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task ConditionalGroupBetweenTokensOfDirectiveIsNotReported(string endOfLine)
    {
        const string testCode = """
                                using System.Linq;
                                using System
                                #if A
                                .Text
                                #endif
                                ;

                                class C
                                {
                                    int x;
                                }
                                """;

        await Verify(ConvertEndOfLine(testCode, endOfLine));
    }

    /// <summary>
    /// Verifies that an <c>#if</c> between the tokens of one using directive and its <c>#endif</c> between the tokens of the
    /// next make the block unsafe, so it is not reported
    /// </summary>
    /// <param name="endOfLine">Line ending of the source</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-USING-009")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task ConditionalGroupAcrossTokensOfTwoDirectivesIsNotReported(string endOfLine)
    {
        const string testCode = """
                                using System
                                #if !A
                                .Text;
                                using System.Collections
                                #endif
                                ;
                                using System.Buffers;

                                class C
                                {
                                    int x;
                                }
                                """;

        await Verify(ConvertEndOfLine(testCode, endOfLine));
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using System.Text;
                                using {|#0:System.Linq|};
                                using {|#1:System.Collections.Generic|};

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using System.Collections.Generic;
                                 using System.Linq;
                                 using System.Text;

                                 public class TestClass
                                 {
                                 }
                                 """;

        // All three directives share the System root group, so each one is compared against every earlier
        // one of the group and two of them are reported. Both fixes reorganize the same using list, so their text
        // changes cover the identical span and the surviving one already clears both
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7203UsingDirectivesMustBeOrderedAlphabeticallyByNamespaceAnalyzer.DiagnosticId, AnalyzerResources.RH7203MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}