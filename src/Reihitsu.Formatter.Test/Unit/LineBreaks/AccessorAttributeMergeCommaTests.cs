using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests the commas the line-break phase writes where it merges the attribute lists of an accessor (FMT-BREAK-032): every comma
/// between two attributes of the merged list, created or already in the first list, leaves the phase with no trivia in front
/// of it or behind it, while the phase's other output around the merged list stays as FMT-BREAK-031 records it
/// </summary>
[TestClass]
public class AccessorAttributeMergeCommaTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// Name under which the pipeline observer reports the line-break phase
    /// </summary>
    private const string LineBreakPhaseName = "LineBreakPhase";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Runs the pipeline on the input under LF and CRLF line endings and returns, per line ending, the output of the line-break
    /// phase together with a description of the line ending
    /// </summary>
    /// <param name="input">The input source text</param>
    /// <returns>The output of the line-break phase and the description per line ending</returns>
    private static IEnumerable<(SyntaxNode Output, string Description)> GetLineBreakPhaseOutputs(string input)
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ObservedPipeline.GetPhaseOutput(NormalizeLineEndings(input, endOfLine), endOfLine, LineBreakPhaseName);

            yield return (output, $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output.ToFullString()}");
        }
    }

    /// <summary>
    /// Asserts that the accessor's single attribute list in the line-break phase's output holds the expected text and that
    /// every comma between two of its attributes carries no trivia on either side
    /// </summary>
    /// <param name="output">The output of the line-break phase</param>
    /// <param name="accessorKind">The kind of the accessor whose attribute list is checked</param>
    /// <param name="expectedList">The expected text of the merged list, or <see langword="null"/> when the test does not decide it</param>
    /// <param name="description">The description of the case</param>
    private static void AssertMergedListCommasCarryNoTrivia(SyntaxNode output, SyntaxKind accessorKind, string expectedList, string description)
    {
        var accessor = output.DescendantNodes()
                             .OfType<AccessorDeclarationSyntax>()
                             .Single(node => node.IsKind(accessorKind));

        Assert.HasCount(1, accessor.AttributeLists, description);

        var list = accessor.AttributeLists[0];

        if (expectedList != null)
        {
            Assert.AreEqual(expectedList, list.ToString(), description);
        }

        foreach (var comma in list.Attributes.GetSeparators())
        {
            Assert.IsEmpty(comma.LeadingTrivia, $"The comma at {comma.SpanStart} carries leading trivia. {description}");
            Assert.IsEmpty(comma.TrailingTrivia, $"The comma at {comma.SpanStart} carries trailing trivia. {description}");
        }
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifies that the comma the merge creates between two <c>param:</c> lists of a setter in an accessor list on the
    /// declaration's line carries no trivia, and that the space behind the merged list's <c>]</c> stays
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-032")]
    public void CreatedCommaOfParamListsOnTheDeclarationsLineCarriesNoTrivia()
    {
        const string input = """
                             class C
                             {
                                 int P { get; [param: A] [param: B] set; }
                             }
                             """;

        foreach (var (output, description) in GetLineBreakPhaseOutputs(input))
        {
            AssertMergedListCommasCarryNoTrivia(output, SyntaxKind.SetAccessorDeclaration, "[param: A,B]", description);
            Assert.Contains("[param: A,B] set;", output.ToFullString(), description);
        }
    }

    /// <summary>
    /// Verifies that the comma the merge creates between two <c>param:</c> lists of a setter in an accessor list over several
    /// lines carries no trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-032")]
    public void CreatedCommaOfParamListsOverSeveralLinesCarriesNoTrivia()
    {
        const string input = """
                             class C
                             {
                                 int Q
                                 {
                                     get; [param: A]
                                     [param: B] set;
                                 }
                             }
                             """;

        foreach (var (output, description) in GetLineBreakPhaseOutputs(input))
        {
            AssertMergedListCommasCarryNoTrivia(output, SyntaxKind.SetAccessorDeclaration, "[param: A,B]", description);
        }
    }

    /// <summary>
    /// Verifies that a comma the first <c>param:</c> list held with a space behind it in the input loses the space, like the
    /// comma the merge creates, in an indexer's accessor list the phase lays out over several lines
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-032")]
    [Requirement("FMT-BREAK-031")]
    public void CommaTheFirstListHeldWithASpaceCarriesNoTrivia()
    {
        const string input = """
                             class C
                             {
                                 int this[int i] { get => 0; [param: A, B] [param: C] set { } }
                             }
                             """;

        foreach (var (output, description) in GetLineBreakPhaseOutputs(input))
        {
            AssertMergedListCommasCarryNoTrivia(output, SyntaxKind.SetAccessorDeclaration, "[param: A,B,C]", description);
        }
    }

    /// <summary>
    /// Verifies that the commas of lists without a specifier that the merge joins on the declaration's line carry no trivia,
    /// a comma the first list held with a space included
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-032")]
    public void CommasOfListsWithoutSpecifierOnTheDeclarationsLineCarryNoTrivia()
    {
        const string input = """
                             class C
                             {
                                 int R { [A, B] [C] get; set; }
                             }
                             """;

        foreach (var (output, description) in GetLineBreakPhaseOutputs(input))
        {
            AssertMergedListCommasCarryNoTrivia(output, SyntaxKind.GetAccessorDeclaration, "[A,B,C]", description);
            Assert.Contains("[A,B,C] get;", output.ToFullString(), description);
        }
    }

    /// <summary>
    /// Verifies that the comma the merge creates between two <c>typevar:</c> lists of a setter in an accessor list over
    /// several lines carries no trivia
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-032")]
    public void CreatedCommaOfTypevarListsCarriesNoTrivia()
    {
        const string input = """
                             class C
                             {
                                 int Q
                                 {
                                     get => 1;
                                     [typevar: A] [typevar: B] set { }
                                 }
                             }
                             """;

        foreach (var (output, description) in GetLineBreakPhaseOutputs(input))
        {
            AssertMergedListCommasCarryNoTrivia(output, SyntaxKind.SetAccessorDeclaration, "[typevar: A,B]", description);
        }
    }

    /// <summary>
    /// Verifies that the commas between the attributes of three merged <c>param:</c> lists carry no trivia when the first
    /// attribute holds an argument list with a comma, which is no comma between two attributes of the merged list
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-032")]
    public void CommasBetweenAttributesWithAnArgumentListCarryNoTrivia()
    {
        const string input = """
                             class C
                             {
                                 int R
                                 {
                                     get; [param: A(1, 2)] [param: B] [param: C]
                                     set;
                                 }
                             }
                             """;

        foreach (var (output, description) in GetLineBreakPhaseOutputs(input))
        {
            AssertMergedListCommasCarryNoTrivia(output, SyntaxKind.SetAccessorDeclaration, null, description);
        }
    }

    /// <summary>
    /// Verifies the other side of the merge: an accessor's single list with several attributes in an accessor list on the
    /// declaration's line is not merged, so its comma keeps the space it was written with
    /// </summary>
    [TestMethod]
    [Requirement("FMT-BREAK-031")]
    public void CommaOfAnAccessorListTheMergeDoesNotRewriteKeepsItsSpace()
    {
        const string input = """
                             class C
                             {
                                 int P { get; [C, D] set; }
                             }
                             """;

        foreach (var (output, description) in GetLineBreakPhaseOutputs(input))
        {
            var list = output.DescendantNodes()
                             .OfType<AccessorDeclarationSyntax>()
                             .Single(node => node.IsKind(SyntaxKind.SetAccessorDeclaration))
                             .AttributeLists
                             .Single();

            Assert.AreEqual("[C, D]", list.ToString(), description);
        }
    }

    #endregion // Tests
}