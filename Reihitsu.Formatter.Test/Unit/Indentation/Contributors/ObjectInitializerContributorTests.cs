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
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that braces of an object initializer are aligned to the new keyword column
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var creation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var newColumn = LayoutComputer.GetColumn(creation.NewKeyword);

        // Act
        contributor.Contribute(creation, model, context);

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
    /// Verifies that members of an object initializer are indented one level from the new keyword
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var creation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var newColumn = LayoutComputer.GetColumn(creation.NewKeyword);
        var expectedMemberColumn = newColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(creation, model, context);

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
    /// Verifies that array creation with initializer is handled
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var creation = root.DescendantNodes().OfType<ArrayCreationExpressionSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var newColumn = LayoutComputer.GetColumn(creation.NewKeyword);

        // Act
        contributor.Contribute(creation, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(creation.Initializer.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(creation.Initializer.OpenBraceToken);

            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(newColumn, openLayout.Column);
        }
    }

    /// <summary>
    /// Verifies that object creation without initializer does not produce layout entries
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var creation = root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        // Act
        contributor.Contribute(creation, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Object creation without initializer should not produce layout entries");
    }

    /// <summary>
    /// Verifies that non-creation nodes are ignored by the contributor
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        // Act
        contributor.Contribute(classDecl, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-creation nodes should not produce layout entries");
    }

    /// <summary>
    /// Verifies that a <c>with</c>-expression initializer's braces are aligned to the <c>with</c>
    /// keyword column (issue #430)
    /// </summary>
    [TestMethod]
    public void AlignsWithExpressionInitializerBraces()
    {
        // Arrange
        const string input = """
                             record Point(int X, int Y);

                             class C
                             {
                                 void M(Point p)
                                 {
                                     var y = p with
                                     {
                                         X = 1,
                                         Y = 2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var withExpression = root.DescendantNodes().OfType<WithExpressionSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var withColumn = LayoutComputer.GetColumn(withExpression.WithKeyword);

        // Act
        contributor.Contribute(withExpression, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(withExpression.Initializer.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(withExpression.Initializer.OpenBraceToken);

            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(withColumn, openLayout.Column, "Open brace should align to with keyword");
        }

        if (LayoutComputer.IsFirstOnLine(withExpression.Initializer.CloseBraceToken))
        {
            var closeLine = LayoutComputer.GetLine(withExpression.Initializer.CloseBraceToken);

            Assert.IsTrue(model.TryGetLayout(closeLine, out var closeLayout));
            Assert.AreEqual(withColumn, closeLayout.Column, "Close brace should align to with keyword");
        }
    }

    /// <summary>
    /// Verifies that members of a <c>with</c>-expression initializer are indented one level from
    /// the <c>with</c> keyword (issue #430)
    /// </summary>
    [TestMethod]
    public void IndentsWithExpressionMembersOneLevelFromWithKeyword()
    {
        // Arrange
        const string input = """
                             record Point(int X, int Y);

                             class C
                             {
                                 void M(Point p)
                                 {
                                     var y = p with
                                     {
                                         X = 1,
                                         Y = 2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var withExpression = root.DescendantNodes().OfType<WithExpressionSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var withColumn = LayoutComputer.GetColumn(withExpression.WithKeyword);
        var expectedMemberColumn = withColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(withExpression, model, context);

        // Assert
        foreach (var expression in withExpression.Initializer.Expressions)
        {
            var firstToken = expression.GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);

                Assert.IsTrue(model.TryGetLayout(line, out var layout));
                Assert.AreEqual(expectedMemberColumn, layout.Column, $"Member on line {line} should be indented +4 from with keyword");
            }
        }
    }

    /// <summary>
    /// Verifies that a typed <c>stackalloc</c> initializer's braces are aligned to the
    /// <c>stackalloc</c> keyword column (issue #430)
    /// </summary>
    [TestMethod]
    public void AlignsStackAllocInitializerBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Span<int> s = stackalloc int[]
                                     {
                                         1,
                                         2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var stackAlloc = root.DescendantNodes().OfType<StackAllocArrayCreationExpressionSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var stackAllocColumn = LayoutComputer.GetColumn(stackAlloc.StackAllocKeyword);

        // Act
        contributor.Contribute(stackAlloc, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(stackAlloc.Initializer.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(stackAlloc.Initializer.OpenBraceToken);

            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(stackAllocColumn, openLayout.Column, "Open brace should align to stackalloc keyword");
        }
    }

    /// <summary>
    /// Verifies that a <c>stackalloc</c> expression without an initializer does not produce
    /// layout entries (issue #430)
    /// </summary>
    [TestMethod]
    public void DoesNothingForStackAllocWithoutInitializer()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Span<int> s = stackalloc int[10];
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var stackAlloc = root.DescendantNodes().OfType<StackAllocArrayCreationExpressionSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        // Act
        contributor.Contribute(stackAlloc, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "stackalloc without initializer should not produce layout entries");
    }

    /// <summary>
    /// Verifies that an implicitly-typed <c>stackalloc[]</c> initializer's braces are aligned to
    /// the <c>stackalloc</c> keyword column (issue #430)
    /// </summary>
    [TestMethod]
    public void AlignsImplicitStackAllocInitializerBraces()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Span<int> s = stackalloc[]
                                     {
                                         1,
                                         2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var implicitStackAlloc = root.DescendantNodes().OfType<ImplicitStackAllocArrayCreationExpressionSyntax>().First();
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var stackAllocColumn = LayoutComputer.GetColumn(implicitStackAlloc.StackAllocKeyword);

        // Act
        contributor.Contribute(implicitStackAlloc, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(implicitStackAlloc.Initializer.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(implicitStackAlloc.Initializer.OpenBraceToken);

            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(stackAllocColumn, openLayout.Column, "Open brace should align to stackalloc keyword");
        }
    }

    /// <summary>
    /// Verifies that a bare array initializer attached through <c>EqualsValueClauseSyntax</c> (no
    /// <c>new</c> keyword) indents its members one level deeper than the open brace instead of
    /// being flattened to the same column (issue #430)
    /// </summary>
    [TestMethod]
    public void IndentsEqualsValueClauseInitializerMembersOneLevel()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     int[] x =
                                     {
                                         1,
                                         2
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);
        var initializer = root.DescendantNodes()
                              .OfType<InitializerExpressionSyntax>()
                              .First(node => node.Parent is EqualsValueClauseSyntax);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        var anchorColumn = LayoutComputer.GetColumn(initializer.OpenBraceToken);
        var expectedMemberColumn = anchorColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(initializer, model, context);

        // Assert
        foreach (var expression in initializer.Expressions)
        {
            var firstToken = expression.GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);

                Assert.IsTrue(model.TryGetLayout(line, out var layout));
                Assert.AreEqual(expectedMemberColumn, layout.Column, $"Member on line {line} should be indented +4 from the open brace");
            }
        }

        if (LayoutComputer.IsFirstOnLine(initializer.CloseBraceToken))
        {
            var closeLine = LayoutComputer.GetLine(initializer.CloseBraceToken);

            Assert.IsTrue(model.TryGetLayout(closeLine, out var closeLayout));
            Assert.AreEqual(anchorColumn, closeLayout.Column, "Close brace should stay aligned with the open brace");
        }
    }

    /// <summary>
    /// Verifies that a collection initializer nested inside an object initializer produces
    /// layout entries for its braces and members. Documents bug: the contributor does not
    /// handle standalone collection initializer expressions without a new keyword
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var root = tree.GetRoot(TestContext.CancellationToken);

        // The inner collection initializer: Items = { new Item() }
        var collectionInitializer = root.DescendantNodes()
                                        .OfType<InitializerExpressionSyntax>()
                                        .First(initializer => initializer.Parent is AssignmentExpressionSyntax);

        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ObjectInitializerContributor();

        // Act — contribute the standalone collection initializer (no 'new' keyword parent)
        contributor.Contribute(collectionInitializer, model, context);

        // Assert — documents bug: contributor does not handle standalone InitializerExpressionSyntax
        Assert.IsGreaterThan(0, model.Count, "Collection initializer should produce layout entries for its braces and members");
    }

    #endregion // Methods
}