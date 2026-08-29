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
public class RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzerTests : AnalyzerTestsBase<RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer, RH5531AccessorAttributeListsMustFollowShapeRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that multiline accessor violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// unmerged, which is one of the latent defects the RH5408 non-convergence report traced back to (issue
    /// #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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
    /// lists, so this compliant multi-line shape stays silent exactly as it did before the repair (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
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

    #endregion // Tests
}