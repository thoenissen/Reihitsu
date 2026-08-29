using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5530AccessorAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5530AccessorAttributesMustFollowPlacementRulesAnalyzerTests : AnalyzerTestsBase<RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer, RH5530AccessorAttributesMustFollowPlacementRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        {|#0:[First]|} get;
                                        set;
                                    }
                                }
                                """;
        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 internal class Example
                                 {
                                     internal int Value
                                     {
                                         [First]
                                         get;
                                         set;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5530MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        [First]
                                        get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that single-line accessor attributes remain valid
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLinePropertyAccessorAttribute()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value { [First] get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a single-line accessor attribute remains valid even when the property also carries a
    /// property-level attribute on its own line. Before the span repair, the shared single-line predicate
    /// counted the property attribute's own line and misclassified the declaration as multi-line, wrongly
    /// forcing this compliant accessor attribute apart (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLinePropertyAccessorAttributeWithPropertyAttribute()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class ExampleAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    [Example]
                                    internal int Value { [First] get; set; }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a property-level attribute does not exempt a genuinely multi-line property from the
    /// separate-line placement rule for its own accessor attributes — the span repair only excludes the
    /// property's own attribute lists, not the accessor list that still spans multiple lines (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolationWithPropertyAttribute()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
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
                                        {|#0:[First]|} get;
                                        set;
                                    }
                                }
                                """;
        const string fixedData = """
                                 sealed class FirstAttribute : System.Attribute
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
                                         get;
                                         set;
                                     }
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5530MessageFormat));
    }

    /// <summary>
    /// Verifies that method attributes are not handled by this accessor rule
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForMethodAttributes()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    [First] internal void M() { }
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
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        {|#0:[First /* keep */]|} get;
                                        set;
                                    }
                                }
                                """;
        const string codeFixData = """
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   internal class Example
                                   {
                                       internal int Value
                                       {
                                           [First /* keep */] get;
                                           set;
                                       }
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5530MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a comment between the closing bracket and the accessor keyword keeps the diagnostic from being
    /// reported. The multi-line property resolves to separate-line placement, and the fix refuses that gap under
    /// either placement, so reporting here would leave a diagnostic nobody can clear (issue #420)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticWhenCommentSitsBetweenAttributeListAndAccessor()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    internal int Value
                                    {
                                        [First] /* keep me */ get;
                                        set;
                                    }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the single-line span repair applies to indexers, not only ordinary properties — the
    /// shared predicate is not property-specific, so an indexer combining a property-level attribute with a
    /// single-line accessor attribute must be classified the same way (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineIndexerAccessorAttributeWithPropertyAttribute()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class ExampleAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    [Example]
                                    internal int this[int index] { [First] get { return index; } set { } }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the single-line span repair applies to events with block-bodied single-line accessors. An
    /// event's <c>Type</c> follows the <c>event</c> keyword, so an anchor built from the first modifier or type
    /// token — rather than the first token after the last attribute list — would misclassify this shape
    /// (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineEventAccessorAttributeWithPropertyAttribute()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class ExampleAttribute : System.Attribute
                                {
                                }
                                internal class Example
                                {
                                    [Example]
                                    internal event System.EventHandler Changed { [First] add { } remove { } }
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that the single-line span repair applies to an interface property member, which carries no
    /// modifiers at all. An anchor that assumes a first modifier token exists would misclassify this shape
    /// (issue #729)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForSingleLineInterfaceMemberAccessorAttributeWithPropertyAttribute()
    {
        const string testData = """
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class ExampleAttribute : System.Attribute
                                {
                                }
                                internal interface IExample
                                {
                                    [Example]
                                    int Value { [First] get; set; }
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests
}