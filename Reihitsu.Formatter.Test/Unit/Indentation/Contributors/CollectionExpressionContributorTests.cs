using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="CollectionExpressionContributor"/>
/// </summary>
[TestClass]
public class CollectionExpressionContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that elements in a multi-line collection expression are indented one level from the opening bracket.
    /// </summary>
    [TestMethod]
    public void IndentsElementsOneLevelFromOpenBracket()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    int[] x =
                    [
                        1,
                        2,
                        3
                    ];
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var collection = root.DescendantNodes().OfType<CollectionExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new CollectionExpressionContributor();

        var bracketColumn = LayoutComputer.GetColumn(collection.OpenBracketToken);
        var expectedElementColumn = bracketColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(collection, scope, model, context);

        // Assert
        foreach (var element in collection.Elements)
        {
            var firstToken = element.GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);
                Assert.IsTrue(model.TryGetLayout(line, out var layout));
                Assert.AreEqual(expectedElementColumn, layout.Column, $"Element on line {line} should be indented +4 from bracket");
            }
        }
    }

    /// <summary>
    /// Verifies that the closing bracket is aligned to the opening bracket column.
    /// </summary>
    [TestMethod]
    public void AlignsCloseBracketToOpenBracket()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    int[] x =
                    [
                        1,
                        2
                    ];
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var collection = root.DescendantNodes().OfType<CollectionExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new CollectionExpressionContributor();

        var bracketColumn = LayoutComputer.GetColumn(collection.OpenBracketToken);

        // Act
        contributor.Contribute(collection, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(collection.CloseBracketToken))
        {
            var closeLine = LayoutComputer.GetLine(collection.CloseBracketToken);
            Assert.IsTrue(model.TryGetLayout(closeLine, out var layout));
            Assert.AreEqual(bracketColumn, layout.Column, "Close bracket should align to open bracket");
        }
    }

    /// <summary>
    /// Verifies that single-line collection expressions do not produce layout entries.
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleLineCollection()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    int[] x = [1, 2, 3];
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var collection = root.DescendantNodes().OfType<CollectionExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new CollectionExpressionContributor();

        // Act
        contributor.Contribute(collection, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single-line collection should not produce layout entries");
    }

    /// <summary>
    /// Verifies that non-collection-expression nodes are ignored by the contributor.
    /// </summary>
    [TestMethod]
    public void IgnoresNonCollectionExpressionNodes()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = 1;
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new CollectionExpressionContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-collection-expression nodes should not produce layout entries");
    }

    #endregion // Methods
}