using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5516PropertyAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer, RH5516PropertyAttributeListsMustFollowShapeRulesCodeFixProvider>
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
                                internal class Example
                                {
                                    {|#0:[First, Second]|}
                                    internal int Value { get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     [First]
                                     [Second]
                                     internal int Value { get; set; }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5516MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    [First]
                                    [Second]
                                    internal int Value { get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
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
                                internal class Example
                                {
                                    {|#0:[First, /* keep */ Second]|}
                                    internal int Value { get; set; }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal class Example
                                   {
                                       [First, /* keep */ Second]
                                       internal int Value { get; set; }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5516MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    {|#0:[First, Second]|} {|#1:[Third, Fourth]|}
                                    internal int Value { get; set; }
                                }
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
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     [First]
                                     [Second]
                                     [Third]
                                     [Fourth]
                                     internal int Value { get; set; }
                                 }
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
                                 """;

        // Two multi-attribute property lists sit back to back on the same declaration: each fix computes its
        // split against the same original document and both target the same property attribute-list run, so the
        // batch fixer discards one of the two conflicting changes in its first pass and needs a second pass to
        // correct the diagnostic that survives
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5516PropertyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5516MessageFormat, 2),
                                  Configure: config => config.NumberOfFixAllIterations = 2);
    }

    #endregion // BatchCodeFixTestsBase
}