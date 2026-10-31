using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Regression.Structural;

/// <summary>
/// Tests that a comment of a removed trailing comma that stands below the line the last element ends on leads the closing
/// brace (FMT-STRUCT-027), in the structural-transform phase's tree and in the final output after one pass
/// </summary>
[TestClass]
public class TrailingCommaCommentOwnerTests : FormatterTestsBase
{
    #region Constants

    /// <summary>
    /// An enum whose trailing comma stands on the line below its last member, with a trailing comment
    /// </summary>
    private const string EnumInput = """
                                     enum E { A,
                                     B // about B
                                         , // comma
                                     }
                                     """;

    /// <summary>
    /// An array initializer whose trailing comma is led by a comment on a line of its own, below the last element's line
    /// </summary>
    private const string ArrayInput = """
                                      class C
                                      {
                                          int[] _a = new[] { 1,
                                      2 /* two */
                                      // before comma
                                      , };
                                      }
                                      """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies the example of FMT-STRUCT-027 in one pass: both comments stay on lines of their own in front of the closing
    /// brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-027")]
    public void CommentsOfRemovedTrailingCommasBelowTheLastElementLeadTheClosingBrace()
    {
        const string input = """
                             enum E { A,
                             B // about B
                                 , // comma
                             }

                             class C
                             {
                                 int[] _a = new[] { 1,
                             2 /* two */
                             // before comma
                             , };
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A,
                                    B // about B

                                    // comma
                                }

                                class C
                                {
                                    int[] _a = new[] { 1,
                                                   2 /* two */

                                                   // before comma
                                               };
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    /// <summary>
    /// Verifies in one pass that the trailing comment of an enum's trailing comma on the line below the last member leads the
    /// closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-027")]
    public void TrailingCommentOfAnEnumCommaOnTheNextLineLeadsTheClosingBrace()
    {
        const string expected = """
                                enum E
                                {
                                    A,
                                    B // about B

                                    // comma
                                }
                                """;

        AssertRuleResult(EnumInput, expected);
    }

    /// <summary>
    /// Verifies in one pass that a comment that leads an array initializer's trailing comma on a line of its own leads the
    /// closing brace
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-027")]
    public void OwnLineCommentInFrontOfAnInitializerCommaLeadsTheClosingBrace()
    {
        const string expected = """
                                class C
                                {
                                    int[] _a = new[] { 1,
                                                   2 /* two */

                                                   // before comma
                                               };
                                }
                                """;

        AssertRuleResult(ArrayInput, expected);
    }

    /// <summary>
    /// Verifies that in the structural-transform phase's tree the comment of the removed enum comma leads the closing brace,
    /// and that the tree is the one its text parses back to
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-027")]
    public void RemovedEnumCommasCommentLeadsTheClosingBraceInThePhaseTree()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ObservedPipeline.GetPhaseOutput(NormalizeLineEndings(EnumInput, endOfLine), endOfLine, "StructuralTransformPhase");
            var closeBrace = output.DescendantNodes().OfType<EnumDeclarationSyntax>().Single().CloseBraceToken;

            AssertLeadsAndRoundTrips(output, closeBrace, "// comma", endOfLine);
        }
    }

    /// <summary>
    /// Verifies that in the structural-transform phase's tree the comment in front of the removed initializer comma leads the
    /// closing brace, and that the tree is the one its text parses back to
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-027")]
    public void RemovedInitializerCommasCommentLeadsTheClosingBraceInThePhaseTree()
    {
        foreach (var endOfLine in _lineEndings)
        {
            var output = ObservedPipeline.GetPhaseOutput(NormalizeLineEndings(ArrayInput, endOfLine), endOfLine, "StructuralTransformPhase");
            var closeBrace = output.DescendantNodes().OfType<InitializerExpressionSyntax>().Single().CloseBraceToken;

            AssertLeadsAndRoundTrips(output, closeBrace, "// before comma", endOfLine);
        }
    }

    /// <summary>
    /// Verifies the other side of FMT-STRUCT-027: the comment of a removed trailing comma on the line the last member ends on
    /// stays behind the last member
    /// </summary>
    [TestMethod]
    [Requirement("FMT-STRUCT-027")]
    public void CommentOfARemovedCommaOnTheLastMembersLineStaysBehindIt()
    {
        const string input = """
                             enum E
                             {
                                 A,
                                 B, // about B
                             }
                             """;
        const string expected = """
                                enum E
                                {
                                    A,
                                    B // about B
                                }
                                """;

        AssertRuleResult(input, expected);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Asserts that the comment is leading trivia of the closing brace in the phase's tree, and that the tree parses back to
    /// itself
    /// </summary>
    /// <param name="output">The phase's tree</param>
    /// <param name="closeBrace">The closing brace in the phase's tree</param>
    /// <param name="comment">The comment</param>
    /// <param name="endOfLine">The line ending</param>
    private static void AssertLeadsAndRoundTrips(SyntaxNode output, SyntaxToken closeBrace, string comment, string endOfLine)
    {
        var description = $"Under {DescribeLineEnding(endOfLine)} line endings:{Environment.NewLine}{output.ToFullString()}";

        Assert.Contains(comment, closeBrace.LeadingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)).Select(trivia => trivia.ToString()).ToList(), description);
        Assert.IsNull(PhaseTreeRoundTrip.FindDifference(output, CSharpParseOptions.Default), description);
    }

    #endregion // Methods
}