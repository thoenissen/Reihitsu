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

    /// <summary>
    /// Verifies that removing a merged list that directly follows a list of another target without a space keeps the parameter type separated from that list
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticAndCodeFixKeepTokensSeparatedAfterListWithoutTrailingSpace()
    {
        const string testData = """
                                internal sealed record Example([property: First] [field: Second]{|#0:[property: Third]|}
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
                                 internal sealed record Example([property: First, Third] [field: Second] int Id);
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
    /// Verifies that same-target lists followed by a comment on its own line are still reported without offering a
    /// code fix, because merging them would pull the comment up onto the merged list's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentFollowsTheGroup()
    {
        const string testData = """
                                internal sealed record Example([property: First]
                                                               {|#0:[property: Second]|}
                                                               /* keep */ int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal sealed record Example([property: First]
                                                                  [property: Second]
                                                                  /* keep */ int Id);
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
                                                               .ElementAt(1)
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that split parameter-specifier lists on an accessor of a single-line property are still reported and merged
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticForSplitParameterSpecifierListsOnSingleLinePropertyAccessor()
    {
        const string testData = """
                                internal class Example
                                {
                                    public int Value { get; [param: First] {|#0:[param: Second]|} set; }
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
                                     public int Value { get; [param: First, Second] set; }
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
    /// Verifies that a parameter separated from its merged attribute lists by a blank line is joined onto the merged list's line, leaving no trailing whitespace
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixJoinsParameterWhenBlankLineFollowsTheGroup()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M([First]
                                                    {|#0:[Second]|}

                                                    int id)
                                    {
                                    }
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
                                     internal void M([First, Second] int id)
                                     {
                                     }
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
    /// Verifies that a parameter separated from its merged attribute lists by a blank line is joined onto the merged list's line under CRLF line endings
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixJoinsParameterWhenBlankLineFollowsTheGroupWithCarriageReturnLineFeed()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M([First]
                                                    {|#0:[Second]|}

                                                    int id)
                                    {
                                    }
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
                                     internal void M([First, Second] int id)
                                     {
                                     }
                                 }
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(NormalizeToCarriageReturnLineFeed(testData),
                     NormalizeToCarriageReturnLineFeed(fixedData),
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
    }

    /// <summary>
    /// Verifies that a parameter separated from its merged attribute lists by several blank lines, one of them whitespace-only, is joined onto the merged list's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixJoinsParameterWhenSeveralBlankLinesFollowTheGroup()
    {
        // Built line by line, so that the whitespace-only blank line cannot be trimmed away by an editor
        var testData = string.Join("\n",
                                   "internal class Example",
                                   "{",
                                   "    internal void M([First]",
                                   "                    {|#0:[Second]|}",
                                   string.Empty,
                                   "    ",
                                   string.Empty,
                                   "                    int id)",
                                   "    {",
                                   "    }",
                                   "}",
                                   "sealed class FirstAttribute : System.Attribute",
                                   "{",
                                   "}",
                                   "sealed class SecondAttribute : System.Attribute",
                                   "{",
                                   "}");
        var fixedData = string.Join("\n",
                                    "internal class Example",
                                    "{",
                                    "    internal void M([First, Second] int id)",
                                    "    {",
                                    "    }",
                                    "}",
                                    "sealed class FirstAttribute : System.Attribute",
                                    "{",
                                    "}",
                                    "sealed class SecondAttribute : System.Attribute",
                                    "{",
                                    "}");

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
    }

    /// <summary>
    /// Verifies that a positional record parameter separated from its merged <c>property:</c> lists by a blank line is joined onto the merged list's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixJoinsRecordParameterWhenBlankLineFollowsPropertyTargetedGroup()
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
                                """;
        const string fixedData = """
                                 internal sealed record Example([property: First, Second] int Id);
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
    /// Verifies that a list of another target separated from the first merged list by a blank line is joined onto the merged list's line, exactly as without the blank line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixJoinsOtherTargetListWhenBlankLinePrecedesIt()
    {
        const string testData = """
                                internal sealed record Example([property: First]

                                                               [Other]
                                                               {|#0:[property: Second]|}
                                                               int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class OtherAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal sealed record Example([property: First, Second] [Other]
                                                                int Id);
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class OtherAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
    }

    /// <summary>
    /// Verifies that a parameter following a list of another target is joined onto that list's line with a single space when a blank line follows the removed member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixJoinsParameterAfterOtherTargetListWhenBlankLineFollowsTheGroup()
    {
        const string testData = """
                                internal sealed record Example([property: First] [Other] {|#0:[property: Second]|}

                                                               int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class OtherAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal sealed record Example([property: First, Second] [Other] int Id);
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class OtherAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
    }

    /// <summary>
    /// Verifies that a parameter following a list of another target without trailing trivia is separated from it by a single space when a blank line follows the removed member
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixSeparatesParameterFromOtherTargetListWithoutTrailingSpaceWhenBlankLineFollowsTheGroup()
    {
        const string testData = """
                                internal sealed record Example([property: First] [field: Other]{|#0:[property: Second]|}

                                                               int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class OtherAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal sealed record Example([property: First, Second] [field: Other] int Id);
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class OtherAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
    }

    /// <summary>
    /// Verifies that a parameter which still starts its own line after the merge keeps the blank line in front of it and leaves no trailing whitespace behind
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixKeepsBlankLineBeforeParameterThatStillStartsALine()
    {
        const string testData = """
                                internal sealed record Example([property: First]
                                                               [Other]
                                                               {|#0:[property: Second]|}

                                                               int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                sealed class OtherAttribute : System.Attribute
                                {
                                }
                                """;
        const string fixedData = """
                                 internal sealed record Example([property: First, Second] [Other]

                                                                int Id);
                                 sealed class FirstAttribute : System.Attribute
                                 {
                                 }
                                 sealed class SecondAttribute : System.Attribute
                                 {
                                 }
                                 sealed class OtherAttribute : System.Attribute
                                 {
                                 }
                                 """;

        await Verify(testData,
                     fixedData,
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat));
    }

    /// <summary>
    /// Verifies that same-target lists followed by a blank line and a comment are still reported without offering a code fix, because joining the parameter would pull the comment up onto the merged list's line
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyDiagnosticWithoutCodeFixWhenCommentFollowsBlankLineAfterTheGroup()
    {
        const string testData = """
                                internal sealed record Example([property: First]
                                                               {|#0:[property: Second]|}

                                                               /* keep */ int Id);
                                sealed class FirstAttribute : System.Attribute
                                {
                                }
                                sealed class SecondAttribute : System.Attribute
                                {
                                }
                                """;
        const string codeFixData = """
                                   internal sealed record Example([property: First]
                                                                  [property: Second]

                                                                  /* keep */ int Id);
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
                                                               .ElementAt(1)
                                                               .GetLocation());

        Assert.IsEmpty(actions);
    }

    /// <summary>
    /// Verifies that every parameter separated from its merged attribute lists by a blank line is joined onto its merged list's line when several groups are fixed together
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task VerifyCodeFixJoinsEveryParameterWhenBlankLinesFollowSeveralGroups()
    {
        const string testData = """
                                internal class Example
                                {
                                    internal void M([First]
                                                    {|#0:[Second]|}

                                                    int a,
                                                    [First]
                                                    {|#1:[Second]|}

                                                    int b)
                                    {
                                    }
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
                                     internal void M([First, Second] int a,
                                                     [First, Second] int b)
                                     {
                                     }
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
                     Diagnostics(RH5524ParameterAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId, AnalyzerResources.RH5524MessageFormat, 2));
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