using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5531AccessorAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer, RH5531AccessorAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that multiline accessor violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-001")]
    [Requirement("RH5531-004")]
    [Requirement("RH5531-005")]
    [Requirement("SHARED-ATTR-014")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-017")]
    public async Task VerifyDiagnosticAndCodeFixForMultilineAccessorPolicyViolation()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        {|#0:[First, Second]|}
                                        get;
                                        set;
                                    }
                                }
                                """;
        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 internal class Example
                                 {
                                     internal int Value
                                     {
                                         [First]
                                         [Second]
                                         get;
                                         set;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant multiline accessor code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-001")]
    [Requirement("SHARED-ATTR-013")]
    public async Task VerifyNoDiagnosticForCompliantMultilineAccessorCode()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        [First]
                                        [Second]
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that single-line accessors prefer merged attribute lists
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-002")]
    [Requirement("RH5531-004")]
    [Requirement("RH5531-005")]
    [Requirement("SHARED-ATTR-015")]
    [Requirement("SHARED-ATTR-016")]
    [Requirement("SHARED-ATTR-018")]
    public async Task VerifyDiagnosticAndCodeFixForSingleLineAccessorPolicyViolation()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value { [First]{|#0:[Second]|} get; set; }
                                }
                                """;
        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 internal class Example
                                 {
                                     internal int Value { [First, Second] get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant single-line accessor code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-002")]
    [Requirement("SHARED-ATTR-014")]
    public async Task VerifyNoDiagnosticForCompliantSingleLineAccessorCode()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value { [First, Second] get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that two separate single-attribute lists on a single-line accessor are merged into one list even
    /// when the property also carries a property-level attribute on its own line. Before the span repair, the
    /// shared single-line predicate misclassified the declaration as multi-line and left the two lists
    /// unmerged, which is one of the latent defects the RH5408 non-convergence report traced back to
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-002")]
    [Requirement("RH5531-005")]
    public async Task VerifyDiagnosticAndCodeFixForSingleLineAccessorPolicyViolationWithPropertyAttribute()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ExampleAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    [Example]
                                    internal int Value { [First]{|#0:[Second]|} get; set; }
                                }
                                """;
        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class ExampleAttribute : System.Attribute
                                 {
                                 }
                                 internal class Example
                                 {
                                     [Example]
                                     internal int Value { [First, Second] get; set; }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat));
    }

    /// <summary>
    /// Verifies that a property-level attribute does not exempt a genuinely multi-line property carrying already
    /// correctly split accessor attribute lists — the span repair only excludes the property's own attribute
    /// lists, so this compliant multi-line shape stays silent exactly as it did before the repair
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-001")]
    public async Task VerifyNoDiagnosticForCompliantMultilineAccessorCodeWithPropertyAttribute()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ExampleAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    [Example]
                                    internal int Value
                                    {
                                        [First]
                                        [Second]
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that commented violations are still reported without offering an unsafe code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-006")]
    [Requirement("SHARED-ATTR-019")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        {|#0:[First, /* keep */ Second]|}
                                        get;
                                        set;
                                    }
                                }
                                """;
        const string codeFixData = """
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   internal class Example
                                   {
                                       internal int Value
                                       {
                                           [First, /* keep */ Second]
                                           get;
                                           set;
                                       }
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that multi-attribute lists of property, indexer, and event accessors in multi-line accessor lists are reported, while separate single-attribute lists are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-001")]
    [Requirement("RH5531-004")]
    [Requirement("SHARED-ATTR-013")]
    [Requirement("SHARED-ATTR-014")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForMultiAttributeListsOfAccessorsInMultiLineAccessorLists()
    {
        const string testData = """
                                class C
                                {
                                    int P
                                    {
                                        {|#0:[A, B]|}
                                        get;
                                        {|#1:[method: A, B]|}
                                        set;
                                    }

                                    int this[int i]
                                    {
                                        {|#2:[A, B]|}
                                        get => i;
                                    }

                                    event System.Action E
                                    {
                                        {|#3:[A, B]|}
                                        add
                                        {
                                        }
                                        {|#4:[type: A, B]|}
                                        remove
                                        {
                                        }
                                    }

                                    int Q
                                    {
                                        [A] [B]
                                        get;
                                        set;
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat, 5));
    }

    /// <summary>
    /// Verifies that on an inline auto-property a list following another list of the same accessor and target is reported, while a merged list and a list of another target are not
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-002")]
    [Requirement("RH5531-004")]
    [Requirement("SHARED-ATTR-014")]
    [Requirement("SHARED-ATTR-015")]
    [Requirement("SHARED-ATTR-016")]
    public async Task VerifyDiagnosticForFollowingListsOfInlineAutoPropertyAccessors()
    {
        const string testData = """
                                class C
                                {
                                    int P { [A] {|#0:[B]|} get; set; }

                                    int Q { [A] {|#1:[method: B]|} get; set; }

                                    [D]
                                    int R { [A] {|#2:[B]|} get; set; } = 1;

                                    int S { [A, B] get; set; }

                                    int T { [A] [type: B] get; set; }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                sealed class D : System.Attribute
                                {
                                }
                                """;

        await Verify(testData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat, 3));
    }

    /// <summary>
    /// Verifies that a multi-attribute list of a method is left to the method rule, although it resolves to the method target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-003")]
    public async Task VerifyNoDiagnosticForMultiAttributeMethodList()
    {
        const string testData = """
                                class C
                                {
                                    [A, B]
                                    void M()
                                    {
                                    }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the code fix splits a list in a multi-line accessor list and merges the lists of an inline auto-property accessor
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-005")]
    [Requirement("SHARED-ATTR-017")]
    [Requirement("SHARED-ATTR-018")]
    public async Task VerifyCodeFixSplitsMultiLineAccessorListAndMergesInlineAccessorLists()
    {
        const string testData = """
                                class C
                                {
                                    int P
                                    {
                                        {|#0:[A, B]|}
                                        get;
                                        set;
                                    }

                                    int Q { [A] {|#1:[B]|} get; set; }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int P
                                     {
                                         [A]
                                         [B]
                                         get;
                                         set;
                                     }

                                     int Q { [A, B] get; set; }
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that the code fix splits a list in a multi-line accessor list and merges the lists of an inline auto-property accessor, under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5531-005")]
    [Requirement("SHARED-ATTR-017")]
    [Requirement("SHARED-ATTR-018")]
    public async Task VerifyCodeFixSplitsMultiLineAccessorListAndMergesInlineAccessorListsWithCarriageReturnLineFeed()
    {
        const string testData = """
                                class C
                                {
                                    int P
                                    {
                                        {|#0:[A, B]|}
                                        get;
                                        set;
                                    }

                                    int Q { [A] {|#1:[B]|} get; set; }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 class C
                                 {
                                     int P
                                     {
                                         [A]
                                         [B]
                                         get;
                                         set;
                                     }

                                     int Q { [A, B] get; set; }
                                 }
                                 sealed class A : System.Attribute
                                 {
                                 }
                                 sealed class B : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that a list following a comment on a one-line auto-property accessor is still reported without offering a code fix
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentSitsBetweenOneLineAccessorLists()
    {
        const string testData = """
                                class C
                                {
                                    int Q { [A] /* c */ {|#0:[B]|} get; set; }
                                }
                                sealed class A : System.Attribute
                                {
                                }
                                sealed class B : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   class C
                                   {
                                       int Q { [A] /* c */ [B] get; set; }
                                   }
                                   sealed class A : System.Attribute
                                   {
                                   }
                                   sealed class B : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .ElementAt(1)
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                sealed class FourthAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        {|#0:[First, Second]|} {|#1:[Third, Fourth]|}
                                        get;
                                        set;
                                    }
                                }
                                """;

        const string fixedCode = """
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class ThirdAttribute : System.Attribute
                                 {
                                 }
                                 sealed class FourthAttribute : System.Attribute
                                 {
                                 }
                                 internal class Example
                                 {
                                     internal int Value
                                     {
                                         [First]
                                         [Second]
                                         [Third]
                                         [Fourth]
                                         get;
                                         set;
                                     }
                                 }
                                 """;

        // Two multi-attribute lists sit back to back on the same get accessor: each fix computes its split
        // against the same original document and both target the same accessor attribute-list run, so the batch
        // fixer discards one of the two conflicting changes in its first pass and needs a second pass to correct
        // the diagnostic that survives
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5531MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}