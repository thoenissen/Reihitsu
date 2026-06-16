using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="ListPatternContributor"/>
/// </summary>
[TestClass]
public class ListPatternContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that list-pattern entries are indented one level from the opening bracket
    /// </summary>
    [TestMethod]
    public void IndentsPatternsOneLevelFromOpenBracket()
    {
        // Arrange
        var listPattern = ParseListPattern();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ListPatternContributor();

        var bracketColumn = LayoutComputer.GetColumn(listPattern.OpenBracketToken);
        var expectedPatternColumn = bracketColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(listPattern, model, context);

        // Assert
        foreach (var pattern in listPattern.Patterns)
        {
            var firstToken = pattern.GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);

                Assert.IsTrue(model.TryGetLayout(line, out var layout));
                Assert.AreEqual(expectedPatternColumn, layout.Column, $"Pattern on line {line} should be indented +4 from bracket");
            }
        }
    }

    /// <summary>
    /// Verifies that the contributor aligns entries to the opening bracket's re-indented column
    /// rather than its stale source column when the bracket line already has a layout entry
    /// </summary>
    [TestMethod]
    public void AlignsToReIndentedBracketColumn()
    {
        // Arrange
        var listPattern = ParseListPattern();
        var openBracket = listPattern.OpenBracketToken;
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ListPatternContributor();

        // Simulate the bracket's line being re-indented to a different column than the source
        var adjustedBracketColumn = LayoutComputer.GetColumn(openBracket) + 8;

        model.Set(LayoutComputer.GetLine(openBracket), new TokenLayout(adjustedBracketColumn, "Test"));

        // Act
        contributor.Contribute(listPattern, model, context);

        // Assert — patterns follow the adjusted bracket column, not the source column
        foreach (var pattern in listPattern.Patterns)
        {
            var firstToken = pattern.GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);

                Assert.IsTrue(model.TryGetLayout(line, out var layout));
                Assert.AreEqual(adjustedBracketColumn + FormattingContext.IndentSize, layout.Column, "Pattern should align to the re-indented bracket column");
            }
        }

        Assert.IsTrue(model.TryGetLayout(LayoutComputer.GetLine(listPattern.CloseBracketToken), out var closeLayout));
        Assert.AreEqual(adjustedBracketColumn, closeLayout.Column, "Close bracket should align to the re-indented bracket column");
    }

    /// <summary>
    /// Verifies that single-line list patterns do not produce layout entries
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleLineListPattern()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 bool M(int[] o)
                                 {
                                     return o is [1, 2, 3];
                                 }
                             }
                             """;

        var listPattern = ParseListPattern(input);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ListPatternContributor();

        // Act
        contributor.Contribute(listPattern, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single-line list pattern should not produce layout entries");
    }

    /// <summary>
    /// Parses the default multi-line list pattern used by the alignment tests
    /// </summary>
    /// <returns>The parsed list pattern</returns>
    private ListPatternSyntax ParseListPattern()
    {
        const string input = """
                             class C
                             {
                                 bool M(int[] o)
                                 {
                                     return o is
                                            [
                                                1,
                                                2
                                            ];
                                 }
                             }
                             """;

        return ParseListPattern(input);
    }

    /// <summary>
    /// Parses the first list pattern found in the given source text
    /// </summary>
    /// <param name="input">The source text to parse</param>
    /// <returns>The parsed list pattern</returns>
    private ListPatternSyntax ParseListPattern(string input)
    {
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        return root.DescendantNodes().OfType<ListPatternSyntax>().First();
    }

    #endregion // Methods
}