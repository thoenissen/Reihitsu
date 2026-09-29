using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer"/> and <see cref="RH5524ParameterAttributeListsMustFollowShapeRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzerTests : BatchCodeFixTestsBase<RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer, RH5524ParameterAttributeListsMustFollowShapeRulesCodeFixProvider>
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
                                    internal void M([First] {|#0:[Second]|} int value) { }
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
                                     internal void M([First, Second] int value) { }
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
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
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
                                    internal void M([First, Second] int value) { }
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
                                    internal void M([First /* keep */] {|#0:[Second]|} int value) { }
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
                                       internal void M([First /* keep */] [Second] int value) { }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that a second <c>property:</c> attribute list on a positional record parameter is reported and merged into the first
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForPropertyTargetedAttributeListsOnRecordParameter()
    {
        const string testData = """
                                internal sealed record Example([property: First] {|#0:[property: Second]|} int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal sealed record Example([property: First, Second] int Id);
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class ThirdAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
    }

    /// <summary>
    /// Verifies that only the attribute lists sharing a target are merged when a list of another target sits between them
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixMergeOnlyListsOfTheSameTarget()
    {
        const string testData = """
                                internal sealed record Example([property: First] [Second] {|#0:[property: Third]|} int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal sealed record Example([property: First, Third] [Second] int Id);
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class ThirdAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
    }

    /// <summary>
    /// Verifies that a <c>property:</c> list and a list without a specifier on one parameter are not reported, because merging them would move an attribute to another target
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForAttributeListsOfDifferentTargets()
    {
        const string testData = """
                                internal sealed record Example([property: First] [Second] int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a <c>property:</c> list and a <c>field:</c> list on one parameter are not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForPropertyAndFieldTargetedAttributeLists()
    {
        const string testData = """
                                internal sealed record Example([property: First] [field: Second] int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that merging <c>property:</c> attribute lists placed on separate lines leaves no stray whitespace before the parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixForPropertyTargetedAttributeListsOnSeparateLines()
    {
        const string testData = """
                                internal sealed record Example([property: First]
                                                               {|#0:[property: Second]|}
                                                               int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal sealed record Example([property: First, Second] int Id);
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class ThirdAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
    }

    /// <summary>
    /// Verifies that a <c>type:</c> list on a parameter is not reported as a duplicate, because the compiler ignores it there and merging would change which attributes apply
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyNoDiagnosticForTypeTargetedAttributeList()
    {
        const string testData = """
                                internal sealed record Example([Second] [type: First] int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class ThirdAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    internal void M([First] {|#0:[Second]|} {|#1:[Third]|} int value) { }
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
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     internal void M([First, Second, Third] int value) { }
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
                                 """;

        // Three single-attribute lists sit on one parameter: the first sibling is exempt from diagnosis, so both
        // diagnostics land on the second and third lists, and both fixes independently recompute the identical
        // merge of all three lists against the same original document, giving the batch fixer two fully
        // overlapping candidate changes to reconcile
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}