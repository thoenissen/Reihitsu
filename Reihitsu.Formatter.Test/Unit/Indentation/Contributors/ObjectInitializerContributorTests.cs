using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="ObjectInitializerContributor"/>
/// </summary>
[TestClass]
public class ObjectInitializerContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that braces of an object initializer are aligned to the new keyword column.
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
                                     var x = new Foo
                                     {
                                         A = 1,
                                         B = 2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var creation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var newColumn = LayoutComputer.GetColumn(creation.NewKeyword);

        // Act
        contributor.Contribute(creation, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(creation.Initializer.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(creation.Initializer.OpenBraceToken);
            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(newColumn, openLayout.Column, "Open brace should align to new keyword");
        }

        if (LayoutComputer.IsFirstOnLine(creation.Initializer.CloseBraceToken))
        {
            var closeLine = LayoutComputer.GetLine(creation.Initializer.CloseBraceToken);
            Assert.IsTrue(model.TryGetLayout(closeLine, out var closeLayout));
            Assert.AreEqual(newColumn, closeLayout.Column, "Close brace should align to new keyword");
        }
    }

    /// <summary>
    /// Verifies that members of an object initializer are indented one level from the new keyword.
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
                                     var x = new Foo
                                     {
                                         A = 1,
                                         B = 2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var creation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var newColumn = LayoutComputer.GetColumn(creation.NewKeyword);
        var expectedMemberColumn = newColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(creation, scope, model, context);

        // Assert
        foreach (var expression in creation.Initializer.Expressions)
        {
            var firstToken = expression.GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);
                Assert.IsTrue(model.TryGetLayout(line, out var layout));
                Assert.AreEqual(expectedMemberColumn, layout.Column, $"Member on line {line} should be indented +4 from new keyword");
            }
        }
    }

    /// <summary>
    /// Verifies that array creation with initializer is handled.
    /// </summary>
    [TestMethod]
    public void AlignsArrayCreationInitializer()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new int[]
                                     {
                                         1,
                                         2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var creation = root.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var newColumn = LayoutComputer.GetColumn(creation.NewKeyword);

        // Act
        contributor.Contribute(creation, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(creation.Initializer.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(creation.Initializer.OpenBraceToken);
            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(newColumn, openLayout.Column);
        }
    }

    /// <summary>
    /// Verifies that object creation without initializer does not produce layout entries.
    /// </summary>
    [TestMethod]
    public void DoesNothingWithoutInitializer()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = new Foo();
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var creation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        // Act
        contributor.Contribute(creation, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Object creation without initializer should not produce layout entries");
    }

    /// <summary>
    /// Verifies that non-creation nodes are ignored by the contributor.
    /// </summary>
    [TestMethod]
    public void IgnoresNonCreationNodes()
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
        var contributor = new ObjectInitializerContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-creation nodes should not produce layout entries");
    }

    /// <summary>
    /// Verifies that a collection initializer nested inside an object initializer produces
    /// layout entries for its braces and members. Documents bug: the contributor does not
    /// handle standalone collection initializer expressions without a new keyword.
    /// </summary>
    [TestMethod]
    public void AlignsCollectionInitializerInsideObjectInitializer()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var result = new Settings
                                                  {
                                                      Items = {
                                                                  new Item()
                                                              }
                                                  };
                                 }
                             }

                             class Settings
                             {
                                 public System.Collections.Generic.List<Item> Items { get; set; }
                             }

                             class Item
                             {
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        // The inner collection initializer: Items = { new Item() }
        var collectionInitializer = root.DescendantNodes()
                                        .OfType<InitializerExpressionSyntax>()
                                        .First(i => i.Parent is AssignmentExpressionSyntax);

        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        // Act — contribute the standalone collection initializer (no 'new' keyword parent)
        contributor.Contribute(collectionInitializer, scope, model, context);

        // Assert — documents bug: contributor does not handle standalone InitializerExpressionSyntax
        Assert.IsGreaterThan(0, model.Count, "Collection initializer should produce layout entries for its braces and members");
    }

    #endregion // Methods
}