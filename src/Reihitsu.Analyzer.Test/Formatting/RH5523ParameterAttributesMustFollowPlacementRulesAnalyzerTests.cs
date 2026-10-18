using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Layout;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Formatting;

/// <summary>
/// Test methods for <see cref="RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer"/> and <see cref="RH5523ParameterAttributesMustFollowPlacementRulesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH5523ParameterAttributesMustFollowPlacementRulesAnalyzerTests : BatchCodeFixTestsBase<RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer, RH5523ParameterAttributesMustFollowPlacementRulesCodeFixProvider>
{
    #region Tests

    /// <summary>
    /// Verifies that policy violations are detected and fixed
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("RH5523-002")]
    [Requirement("RH5523-003")]
    [Requirement("SHARED-ATTR-010")]
    public async Task VerifyDiagnosticAndCodeFixForPolicyViolation()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M({|#0:[First]|}
                                                    int value) { }
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
                                     internal void M([First] int value) { }
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
                     Diagnostics(RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5523MessageFormat));
    }

    /// <summary>
    /// Verifies that compliant code is not flagged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    public async Task VerifyNoDiagnosticForCompliantCode()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M([First] int value) { }
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
    [Requirement("RH5523-004")]
    [Requirement("SHARED-ATTR-011")]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentsArePresent()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M({|#0:[First /* keep */]|}
                                                    int value) { }
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
                                       internal void M([First /* keep */]
                                                       int value) { }
                                   }
                                   sealed class FirstAttribute : System.Attribute
                                   {
                                   }
                                   sealed class SecondAttribute : System.Attribute
                                   {
                                   }
                                   """;

        await Verify(testData,
                     Diagnostics(RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5523MessageFormat));

        var actions = await GetCodeFixActionsAsync(codeFixData,
                                                   RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when a comment prevents the parameter from being joined to its
    /// attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("SHARED-ATTR-005")]
    public async Task VerifyNoDiagnosticWhenCommentBlocksJoin()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M([First]
                                                    // Legacy callers pass null.
                                                    int value) { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when a trailing comment would absorb the joined parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("SHARED-ATTR-005")]
    public async Task VerifyNoDiagnosticWhenTrailingCommentBlocksJoin()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M([First] // Keep this explanation.
                                                    int value) { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that a stale diagnostic does not offer a code fix when a trailing comment blocks the join
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-005")]
    public async Task VerifyNoCodeFixWhenTrailingCommentBlocksJoin()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M([First] // Keep this explanation.
                                                    int value) { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;

        var actions = await GetCodeFixActionsAsync(testData,
                                                   RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId,
                                                   root => root.DescendantNodes()
                                                               .OfType<AttributeListSyntax>()
                                                               .First()
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when a preprocessor directive prevents the parameter from being
    /// joined to its attribute list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("SHARED-ATTR-005")]
    public async Task VerifyNoDiagnosticWhenDirectiveBlocksJoin()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M([First]
                                #if FEATURE
                                #endif
                                                    int value) { }
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
    /// Verifies that a <c>property:</c> attribute on its own line before a positional record parameter is reported and joined onto the parameter's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("RH5523-003")]
    [Requirement("SHARED-ATTR-003")]
    public async Task VerifyDiagnosticAndCodeFixForPropertyTargetedAttributeOnRecordParameter()
    {
        const string testData = """
                                internal sealed record Example({|#0:[property: First]|}
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
                                 internal sealed record Example([property: First] int Id);
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
                     Diagnostics(RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5523MessageFormat));
    }

    /// <summary>
    /// Verifies that a <c>property:</c> attribute on a positional record parameter's line is not reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("SHARED-ATTR-003")]
    public async Task VerifyNoDiagnosticForPropertyTargetedAttributeOnRecordParameterLine()
    {
        const string testData = """
                                internal sealed record Example([property: First] int Id);
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
    /// Verifies that a <c>property:</c> attribute separated from its parameter by a line comment is not reported, because joining would move the parameter into the comment
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-ATTR-005")]
    public async Task VerifyNoDiagnosticForPropertyTargetedAttributeFollowedByLineComment()
    {
        const string testData = """
                                internal sealed record Example([property: First] // keep
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

        await Verify(testData);
    }

    /// <summary>
    /// Verifies that an attribute list of a primary constructor parameter is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForPrimaryConstructorParameter()
    {
        const string testData = """
                                internal class Example({|#0:[First]|}
                                                       int value)
                                {
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example([First] int value)
                                 {
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5523MessageFormat));
    }

    /// <summary>
    /// Verifies that an attribute list of a lambda parameter is checked
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("SHARED-ATTR-001")]
    public async Task VerifyDiagnosticAndCodeFixForLambdaParameter()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M()
                                    {
                                        System.Func<int, int> f = ({|#0:[First]|}
                                                                   int value) => value;
                                    }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal class Example
                                 {
                                     internal void M()
                                     {
                                         System.Func<int, int> f = ([First] int value) => value;
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5523MessageFormat));
    }

    /// <summary>
    /// Verifies that only the property-targeted list of a record parameter that is separated from its parameter is reported
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("SHARED-ATTR-003")]
    [Requirement("SHARED-ATTR-004")]
    public async Task VerifyDiagnosticAndCodeFixForPropertyTargetedAttributeInMultiParameterRecord()
    {
        const string testData = """
                                internal sealed record Example({|#0:[property: First]|}
                                                               int Id, [property: First] int Code);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal sealed record Example([property: First] int Id, [property: First] int Code);
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5523MessageFormat));
    }

    /// <summary>
    /// Verifies that the code fix replaces line breaks and blank lines between the lists and the parameter with single spaces
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-003")]
    [Requirement("SHARED-ATTR-010")]
    public async Task VerifyCodeFixJoinsListsSeparatedByBlankLine()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M({|#0:[First]|}

                                                    {|#1:[Second]|}
                                                    int value) { }
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
                                     internal void M([First] [Second] int value) { }
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
                     Diagnostics(RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5523MessageFormat, 2));
    }

    /// <summary>
    /// Verifies that no diagnostic is reported when disabled text lies between the attribute list and the parameter
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("RH5523-001")]
    [Requirement("SHARED-ATTR-005")]
    public async Task VerifyNoDiagnosticWhenDisabledTextBlocksJoin()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M([First]
                                    #if FEATURE
                                                    [Second]
                                    #endif
                                                    int value) { }
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

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                internal class Example
                                {
                                    internal void M({|#0:[First]|}
                                                    {|#1:[Second]|}
                                                    int value) { }
                                }
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;

        const string fixedCode = """
                                 internal class Example
                                 {
                                     internal void M([First] [Second] int value) { }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        // Two attribute lists on one parameter each sit on their own line: the first fix rewrites the gap up to
        // the second list's opening bracket, and the second fix rewrites the gap up to the parameter type, so
        // their fix spans abut
        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5523MessageFormat, 2));
    }

    #endregion // BatchCodeFixTestsBase
}