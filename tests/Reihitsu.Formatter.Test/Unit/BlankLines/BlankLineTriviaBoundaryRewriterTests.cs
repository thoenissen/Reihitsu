using System;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Pipeline.BlankLines.Rewriter;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;
using Reihitsu.Formatter.Test.Helpers;

namespace Reihitsu.Formatter.Test.Unit.BlankLines;

/// <summary>
/// Tests for <see cref="BlankLineTriviaBoundaryRewriter"/> in isolation, without the <c>BlankLineCollapser</c> subphase that
/// runs after it in <see cref="BlankLinePhase"/>. The collapser would absorb a second blank line the rewriter inserted in
/// front of an <c>#endregion</c> directive, so only the rewriter on its own shows whether it took an existing line for the
/// blank line the directive requires
/// </summary>
[TestClass]
public class BlankLineTriviaBoundaryRewriterTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a line holding nothing but whitespace directly above an <c>#endregion</c> directive with a member below
    /// it is taken for the blank line the directive requires, so the rewriter inserts none and returns the input unchanged
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    [Requirement("SHARED-BLANK-005")]
    [Requirement("FMT-BLANK-013")]
    public void WhitespaceOnlyLineAboveEndRegionIsTakenForRequiredBlankLine()
    {
        // Arrange
        const string inputTemplate = """
                                     class C
                                     {
                                         #region R

                                         private int _a;
                                     {WHITESPACE}
                                         #endregion // R

                                         private int _b;
                                     }
                                     """;
        var input = inputTemplate.Replace("{WHITESPACE}", "    ");

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that a line holding nothing but whitespace directly above an <c>#endregion</c> directive in front of the
    /// closing brace of its type is taken for the blank line the directive requires, so the rewriter inserts none and
    /// returns the input unchanged
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-001")]
    [Requirement("SHARED-BLANK-005")]
    [Requirement("FMT-BLANK-013")]
    public void WhitespaceOnlyLineAboveEndRegionBeforeClosingBraceIsTakenForRequiredBlankLine()
    {
        // Arrange
        const string inputTemplate = """
                                     class C
                                     {
                                         #region R

                                         private int _a;
                                     {WHITESPACE}
                                         #endregion // R
                                     }
                                     """;
        var input = inputTemplate.Replace("{WHITESPACE}", "    ");

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(input, actual);
    }

    /// <summary>
    /// Verifies that the rewriter inserts the blank line an <c>#endregion</c> directive requires when the line directly
    /// above it holds a member, so the two tests above cannot pass only because the rewriter never inserts one
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-BLANK-005")]
    [Requirement("FMT-BLANK-013")]
    public void MemberLineAboveEndRegionGetsBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 #region R

                                 private int _a;
                                 #endregion // R

                                 private int _b;
                             }
                             """;
        const string expected = """
                                class C
                                {
                                    #region R

                                    private int _a;

                                    #endregion // R

                                    private int _b;
                                }
                                """;

        // Act
        var actual = ApplyRewriter(input);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Applies <see cref="BlankLineTriviaBoundaryRewriter"/> to the given source text
    /// </summary>
    /// <param name="source">The source text to rewrite</param>
    /// <returns>The rewritten source text</returns>
    private static string ApplyRewriter(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var context = new FormattingContext(Environment.NewLine);
        var editor = new BlankLineEditor(context);
        var rewriter = new BlankLineTriviaBoundaryRewriter(context, editor, CancellationToken.None);
        var result = rewriter.Visit(tree.GetRoot());

        return result.ToFullString();
    }

    #endregion // Methods
}