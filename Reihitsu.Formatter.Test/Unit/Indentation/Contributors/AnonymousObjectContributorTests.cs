using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="AnonymousObjectContributor"/>
/// </summary>
[TestClass]
public class AnonymousObjectContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that braces of an anonymous object are aligned to the new keyword column.
    /// </summary>
    [TestMethod]
    public void AlignsBracesToNewKeyword()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new
                                     {
                                         A = 1,
                                         B = 2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var anon = root.DescendantNodes().OfType<AnonymousObjectCreationExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new AnonymousObjectContributor();

        var newColumn = LayoutComputer.GetColumn(anon.NewKeyword);

        // Act
        contributor.Contribute(anon, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(anon.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(anon.OpenBraceToken);
            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(newColumn, openLayout.Column, "Open brace should align to new keyword");
        }

        if (LayoutComputer.IsFirstOnLine(anon.CloseBraceToken))
        {
            var closeLine = LayoutComputer.GetLine(anon.CloseBraceToken);
            Assert.IsTrue(model.TryGetLayout(closeLine, out var closeLayout));
            Assert.AreEqual(newColumn, closeLayout.Column, "Close brace should align to new keyword");
        }
    }

    /// <summary>
    /// Verifies that members of an anonymous object are indented one level from the new keyword.
    /// </summary>
    [TestMethod]
    public void IndentsMembersOneLevelFromNewKeyword()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new
                                     {
                                         A = 1,
                                         B = 2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var anon = root.DescendantNodes().OfType<AnonymousObjectCreationExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new AnonymousObjectContributor();

        var newColumn = LayoutComputer.GetColumn(anon.NewKeyword);
        var expectedMemberColumn = newColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(anon, scope, model, context);

        // Assert
        foreach (var initializer in anon.Initializers)
        {
            var firstToken = initializer.GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);
                Assert.IsTrue(model.TryGetLayout(line, out var layout));
                Assert.AreEqual(expectedMemberColumn, layout.Column, $"Member on line {line} should be indented +4 from new keyword");
            }
        }
    }

    /// <summary>
    /// Verifies that non-anonymous-object nodes are ignored by the contributor.
    /// </summary>
    [TestMethod]
    public void IgnoresNonAnonymousObjectNodes()
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
        var contributor = new AnonymousObjectContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-anonymous-object nodes should not produce layout entries");
    }

    /// <summary>
    /// Verifies that an anonymous object with no initializers still aligns braces to the new keyword.
    /// </summary>
    [TestMethod]
    public void AlignsBracesWithNoInitializers()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new
                                     {
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var anon = root.DescendantNodes().OfType<AnonymousObjectCreationExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new AnonymousObjectContributor();

        var newColumn = LayoutComputer.GetColumn(anon.NewKeyword);

        // Act
        contributor.Contribute(anon, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(anon.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(anon.OpenBraceToken);
            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(newColumn, openLayout.Column);
        }

        if (LayoutComputer.IsFirstOnLine(anon.CloseBraceToken))
        {
            var closeLine = LayoutComputer.GetLine(anon.CloseBraceToken);
            Assert.IsTrue(model.TryGetLayout(closeLine, out var closeLayout));
            Assert.AreEqual(newColumn, closeLayout.Column);
        }
    }

    #endregion // Methods
}