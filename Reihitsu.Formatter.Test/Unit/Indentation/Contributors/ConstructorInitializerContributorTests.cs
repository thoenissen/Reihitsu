using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="ConstructorInitializerContributor"/>
/// </summary>
[TestClass]
public class ConstructorInitializerContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the colon token of a base initializer is indented one level from the constructor
    /// </summary>
    [TestMethod]
    public void AlignsBaseInitializerColonToConstructorPlusIndent()
    {
        // Arrange
        const string input = """
                             class C : B
                             {
                                 C()
                                     : base()
                                 {
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var initializer = root.DescendantNodes().OfType<ConstructorInitializerSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        // Pre-populatelayout for the constructor's first token line
        var constructor = root.DescendantNodes().OfType<ConstructorDeclarationSyntax>().First();
        var constructorFirstToken = constructor.GetFirstToken();
        var constructorLine = LayoutComputer.GetLine(constructorFirstToken);
        var constructorColumn = LayoutComputer.GetColumn(constructorFirstToken);

        model.Set(constructorLine, new TokenLayout(constructorColumn, "Block"));

        var contributor = new ConstructorInitializerContributor();
        var expectedColonColumn = constructorColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(initializer, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(initializer.ColonToken))
        {
            var colonLine = LayoutComputer.GetLine(initializer.ColonToken);

            Assert.IsTrue(model.TryGetLayout(colonLine, out var colonLayout));
            Assert.AreEqual(expectedColonColumn, colonLayout.Column, "Colon should be indented +4 from constructor");
        }
    }

    /// <summary>
    /// Verifies that the colon token of a this initializer is indented one level from the constructor
    /// </summary>
    [TestMethod]
    public void AlignsThisInitializerColonToConstructorPlusIndent()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 C(int x)
                                     : this()
                                 {
                                 }
                                 C() { }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var initializer = root.DescendantNodes().OfType<ConstructorInitializerSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        var constructor = (ConstructorDeclarationSyntax)initializer.Parent;
        var constructorFirstToken = constructor.GetFirstToken();
        var constructorLine = LayoutComputer.GetLine(constructorFirstToken);
        var constructorColumn = LayoutComputer.GetColumn(constructorFirstToken);

        model.Set(constructorLine, new TokenLayout(constructorColumn, "Block"));

        var contributor = new ConstructorInitializerContributor();
        var expectedColonColumn = constructorColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(initializer, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(initializer.ColonToken))
        {
            var colonLine = LayoutComputer.GetLine(initializer.ColonToken);

            Assert.IsTrue(model.TryGetLayout(colonLine, out var colonLayout));
            Assert.AreEqual(expectedColonColumn, colonLayout.Column);
        }
    }

    /// <summary>
    /// Verifies that no layout is set when the constructor line has no pre-existing layout
    /// </summary>
    [TestMethod]
    public void DoesNothingWhenConstructorHasNoLayout()
    {
        // Arrange
        const string input = """
                             class C : B
                             {
                                 C()
                                     : base()
                                 {
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var initializer = root.DescendantNodes().OfType<ConstructorInitializerSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ConstructorInitializerContributor();

        // Act — no pre-populated layout for constructor
        contributor.Contribute(initializer, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Without constructor layout, no initializer layout should be set");
    }

    /// <summary>
    /// Verifies that non-constructor-initializer nodes are ignored by the contributor
    /// </summary>
    [TestMethod]
    public void IgnoresNonConstructorInitializerNodes()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M() { }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var classDecl = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ConstructorInitializerContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-constructor-initializer nodes should not produce layout entries");
    }

    #endregion // Methods
}