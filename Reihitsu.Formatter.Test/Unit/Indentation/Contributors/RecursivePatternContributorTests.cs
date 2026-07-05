using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="RecursivePatternContributor"/>
/// </summary>
[TestClass]
public class RecursivePatternContributorTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that recursive-pattern subpatterns are indented one level from the <c>is</c> keyword
    /// </summary>
    [TestMethod]
    public void IndentsSubpatternsOneLevelFromIsKeyword()
    {
        // Arrange
        var recursivePattern = ParseRecursivePattern();
        var isExpression = (IsPatternExpressionSyntax)recursivePattern.Parent;
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new RecursivePatternContributor();

        var anchorColumn = LayoutComputer.GetColumn(isExpression.IsKeyword);
        var expectedSubpatternColumn = anchorColumn + FormattingContext.IndentSize;

        var subpatterns = recursivePattern.PropertyPatternClause.Subpatterns;

        // Act
        contributor.Contribute(recursivePattern, model, context);

        // Assert — every subpattern in the parsed input is on its own line, so each must receive an entry
        var assertedSubpatterns = 0;

        foreach (var subpattern in subpatterns)
        {
            var firstToken = subpattern.GetFirstToken();

            Assert.IsTrue(LayoutComputer.IsFirstOnLine(firstToken), "Each subpattern in the parsed input should be first on its line");

            var line = LayoutComputer.GetLine(firstToken);

            Assert.IsTrue(model.TryGetLayout(line, out var layout), $"Subpattern on line {line} should produce a layout entry");
            Assert.AreEqual(expectedSubpatternColumn, layout.Column, $"Subpattern on line {line} should be indented +4 from the is keyword");

            assertedSubpatterns++;
        }

        Assert.AreEqual(subpatterns.Count, assertedSubpatterns, "All subpatterns should have been asserted");
    }

    /// <summary>
    /// Verifies that the opening and closing braces align to the <c>is</c> keyword column
    /// </summary>
    [TestMethod]
    public void AlignsBracesToIsKeyword()
    {
        // Arrange
        var recursivePattern = ParseRecursivePattern();
        var isExpression = (IsPatternExpressionSyntax)recursivePattern.Parent;
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new RecursivePatternContributor();

        var anchorColumn = LayoutComputer.GetColumn(isExpression.IsKeyword);

        // Act
        contributor.Contribute(recursivePattern, model, context);

        // Assert
        Assert.IsTrue(model.TryGetLayout(LayoutComputer.GetLine(recursivePattern.PropertyPatternClause.OpenBraceToken), out var openLayout));
        Assert.AreEqual(anchorColumn, openLayout.Column, "Open brace should align to the is keyword column");

        Assert.IsTrue(model.TryGetLayout(LayoutComputer.GetLine(recursivePattern.PropertyPatternClause.CloseBraceToken), out var closeLayout));
        Assert.AreEqual(anchorColumn, closeLayout.Column, "Close brace should align to the is keyword column");
    }

    /// <summary>
    /// Verifies that single-line recursive patterns do not produce layout entries
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleLineRecursivePattern()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is { Length: > 0 };
                                 }
                             }
                             """;

        var recursivePattern = ParseRecursivePattern(input);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new RecursivePatternContributor();

        // Act
        contributor.Contribute(recursivePattern, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single-line recursive pattern should not produce layout entries");
    }

    /// <summary>
    /// Parses the default multi-line recursive pattern used by the alignment tests
    /// </summary>
    /// <returns>The parsed recursive pattern</returns>
    private RecursivePatternSyntax ParseRecursivePattern()
    {
        const string input = """
                             class C
                             {
                                 bool M(object o)
                                 {
                                     return o is
                                            {
                                                Length: > 0,
                                                Count: 0
                                            };
                                 }
                             }
                             """;

        return ParseRecursivePattern(input);
    }

    /// <summary>
    /// Parses the first recursive pattern found in the given source text
    /// </summary>
    /// <param name="input">The source text to parse</param>
    /// <returns>The parsed recursive pattern</returns>
    private RecursivePatternSyntax ParseRecursivePattern(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        return root.DescendantNodes().OfType<RecursivePatternSyntax>().First();
    }

    #endregion // Methods
}