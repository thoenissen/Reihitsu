using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer"/> and <see cref="RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzerTests : BatchCodeFixTestsBase<RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer, RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying alias usings are reported and fixed when they are not alphabetically ordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    [Requirement("RH7204-003")]
    [Requirement("RH7204-004")]
    public async Task AliasUsingsAreReportedAndFixedWhenTheyAreNotAlphabeticallyOrdered()
    {
        const string testCode = """
                                using StringList = System.Collections.Generic.List<string>;
                                using {|#0:IntList|} = System.Collections.Generic.List<int>;

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using IntList = System.Collections.Generic.List<int>;
                                 using StringList = System.Collections.Generic.List<string>;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer.DiagnosticId, AnalyzerResources.RH7204MessageFormat));
    }

    /// <summary>
    /// Verifies that aliases targeting different root namespaces are not reported, because the formatter orders them by
    /// target root namespace. This is the order the RH72xx code fixes and <c>reihitsu-format</c> produce, so reporting it
    /// would cause the analyzer and formatter to disagree (the fix would no-op while the diagnostic persists)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-002")]
    [Requirement("SHARED-USING-004")]
    public async Task AliasUsingsInDifferentTargetGroupsAreNotReported()
    {
        const string testCode = """
                                using ZAlias = Beta.Item;
                                using AAlias = Charlie.Item;

                                public class TestClass
                                {
                                }

                                namespace Beta
                                {
                                    public class Item
                                    {
                                    }
                                }

                                namespace Charlie
                                {
                                    public class Item
                                    {
                                    }
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that aliases sharing the same target root namespace are still reported and fixed when their alias names are out of order
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    [Requirement("RH7204-004")]
    public async Task AliasUsingsInSameTargetGroupAreReportedWhenOutOfOrder()
    {
        const string testCode = """
                                using StringAlias = System.String;
                                using {|#0:IntAlias|} = System.Int32;

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using IntAlias = System.Int32;
                                 using StringAlias = System.String;

                                 public class TestClass
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, Diagnostics(RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer.DiagnosticId, AnalyzerResources.RH7204MessageFormat));
    }

    /// <summary>
    /// Verifies disabled conditional using blocks are exempt when they cannot be safely reordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    [Requirement("SHARED-USING-009")]
    public async Task DisabledConditionalUsingBlocksAreNotReportedWhenTheyCannotBeSafelyReordered()
    {
        const string testCode = """
                                using ZetaAlias = System.String;
                                #if FEATURE
                                using MiddleAlias = System.Object;
                                #endif
                                using AlphaAlias = System.Int32;
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that every alias sorted before the alias of its group directly above it is reported on its alias name, also when case differs
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    [Requirement("RH7204-003")]
    [Requirement("SHARED-USING-007")]
    public async Task EveryAliasSortedBeforeItsNeighborIsReportedOnItsAliasName()
    {
        const string testCode = """
                                using b = System.Text;
                                using {|RH7204:A|} = System.IO;

                                using Beta = Microsoft.CodeAnalysis.SyntaxNode;
                                using {|RH7204:Alpha|} = Microsoft.CodeAnalysis.SyntaxToken;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the aliases of the global section are checked on their own
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    [Requirement("SHARED-USING-002")]
    public async Task GlobalSectionIsCheckedOnItsOwn()
    {
        const string testCode = """
                                global using B = System.Text;
                                global using {|RH7204:A|} = System.IO;

                                using C2 = System.Text;

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
    [Requirement("RH7204-001")]
    [Requirement("SHARED-USING-009")]
    public async Task BlockWithPreprocessorDirectiveIsNotReported()
    {
        const string testCode = """
                                using B = System.Text;
                                #if DEBUG
                                using System.Diagnostics;
                                #endif
                                using A = System.IO;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an alias of another group and regular directives are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-002")]
    [Requirement("SHARED-USING-004")]
    public async Task AliasesOfOtherGroupsAndOtherKindsAreNotReported()
    {
        const string testCode = """
                                using X = Microsoft.CodeAnalysis.SyntaxNode;
                                using A = System.IO;

                                using System.Linq;
                                using System;

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
    [Requirement("RH7204-004")]
    public async Task CodeFixOrganizesTheWholeBlock()
    {
        const string testCode = """
                                using b = System.Text;
                                using {|RH7204:A|} = System.IO;

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using A = System.IO;
                                 using b = System.Text;

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
    [Requirement("RH7204-004")]
    public async Task CodeFixLeavesTheCodeAfterTheBlockUnchanged()
    {
        const string testCode = """
                                using b = System.Text;
                                using {|RH7204:A|} = System.IO;

                                class C
                                {
                                    void M( ) {   }
                                }
                                """;

        const string fixedCode = """
                                 using A = System.IO;
                                 using b = System.Text;

                                 class C
                                 {
                                     void M( ) {   }
                                 }
                                 """;

        await Verify(testCode, fixedCode);
    }

    /// <summary>
    /// Verifies that an alias is compared with an earlier alias of its group across an alias of another root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    public async Task AliasIsComparedWithAnEarlierAliasOfItsGroupAcrossAnAliasOfAnotherRoot()
    {
        const string testCode = """
                                using B = Microsoft.CodeAnalysis.SyntaxNode;
                                using C2 = System.Text;
                                using {|RH7204:A|} = Microsoft.CodeAnalysis.SyntaxToken;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the code fix organizes a block whose alias is out of order with an earlier alias of its group
    /// across an alias of another root, in one pass
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    [Requirement("RH7204-004")]
    public async Task CodeFixOrganizesABlockOutOfOrderAcrossAnAliasOfAnotherRoot()
    {
        const string testCode = """
                                using B = Microsoft.CodeAnalysis.SyntaxNode;
                                using C2 = System.Text;
                                using {|RH7204:A|} = Microsoft.CodeAnalysis.SyntaxToken;

                                class C
                                {
                                }
                                """;

        const string fixedCode = """
                                 using C2 = System.Text;

                                 using A = Microsoft.CodeAnalysis.SyntaxToken;
                                 using B = Microsoft.CodeAnalysis.SyntaxNode;

                                 class C
                                 {
                                 }
                                 """;

        await Verify(testCode, fixedCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that an alias is compared with every earlier alias of its group, not only with the one directly above it
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    public async Task AliasIsComparedWithEveryEarlierAliasOfItsGroup()
    {
        const string testCode = """
                                using C2 = System.Text;
                                using {|RH7204:A|} = System.IO;
                                using {|RH7204:B|} = System.Linq;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an alias is compared with an earlier alias of its group across an alias of a type that is not a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    public async Task AliasIsComparedWithAnEarlierAliasOfItsGroupAcrossAnAliasOfATypeThatIsNotAName()
    {
        const string testCode = """
                                using B = System.Text;
                                using P = int;
                                using {|RH7204:A|} = System.IO;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an alias of the global section is compared with an earlier alias of its group across an alias of
    /// another root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    [Requirement("SHARED-USING-002")]
    public async Task GlobalAliasIsComparedWithAnEarlierAliasOfItsGroupAcrossAnAliasOfAnotherRoot()
    {
        const string testCode = """
                                global using B = Microsoft.CodeAnalysis.SyntaxNode;
                                global using C2 = System.Text;
                                global using {|RH7204:A|} = Microsoft.CodeAnalysis.SyntaxToken;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that aliases of one group in order are not reported across an alias of another root
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-002")]
    public async Task OrderedAliasesOfAGroupAreNotReportedAcrossAnAliasOfAnotherRoot()
    {
        const string testCode = """
                                using A = Microsoft.CodeAnalysis.SyntaxToken;
                                using C2 = System.Text;
                                using B = Microsoft.CodeAnalysis.SyntaxNode;

                                class C
                                {
                                }
                                """;

        await Verify(testCode, test => test.CompilerDiagnostics = CompilerDiagnostics.None);
    }

    /// <summary>
    /// Verifies that the group of an alias whose target holds whitespace between its tokens is taken from the target's
    /// value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    public async Task AliasWithWhitespaceInItsTargetIsGroupedByTheTargetsValue()
    {
        const string testCode = """
                                using B = System.Text;
                                using {|RH7204:A|} = System . IO;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that the group of an alias whose target has a verbatim prefix is taken from the target's value
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-001")]
    public async Task AliasWithVerbatimTargetIsGroupedByTheTargetsValue()
    {
        const string testCode = """
                                using B = System.Text;
                                using {|RH7204:A|} = @System.IO;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an alias of a predefined type forms a group of its own and is not compared with an alias of a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-002")]
    public async Task AliasOfAPredefinedTypeIsNotComparedWithAnAliasOfAName()
    {
        const string testCode = """
                                using B = System.Text;
                                using A = int;

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    /// <summary>
    /// Verifies that an alias of a tuple type forms a group of its own and is not compared with an alias of a name
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH7204-002")]
    public async Task AliasOfATupleTypeIsNotComparedWithAnAliasOfAName()
    {
        const string testCode = """
                                using B = System.Text;
                                using A = (int X, int Y);

                                class C
                                {
                                }
                                """;

        await Verify(testCode);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                using ZetaAlias = System.String;
                                using {|#0:MiddleAlias|} = System.Object;
                                using {|#1:AlphaAlias|} = System.Int32;

                                public class TestClass
                                {
                                }
                                """;

        const string fixedCode = """
                                 using AlphaAlias = System.Int32;
                                 using MiddleAlias = System.Object;
                                 using ZetaAlias = System.String;

                                 public class TestClass
                                 {
                                 }
                                 """;

        // All three aliases target the same System root group, so each alias name is compared against every
        // earlier one of the group and two of them are reported. Both fixes reorganize the same using list, so
        // their text changes cover the identical span and the surviving one already clears both
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer.DiagnosticId, AnalyzerResources.RH7204MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}