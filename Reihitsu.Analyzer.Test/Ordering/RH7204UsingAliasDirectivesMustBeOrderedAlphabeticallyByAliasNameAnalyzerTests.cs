using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Organization;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Ordering;

/// <summary>
/// Test methods for <see cref="RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer"/> and <see cref="RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzerTests : AnalyzerTestsBase<RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameAnalyzer, RH7204UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasNameCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifying alias usings are reported and fixed when they are not alphabetically ordered
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}