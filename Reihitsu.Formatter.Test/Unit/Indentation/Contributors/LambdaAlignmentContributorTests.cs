using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="LambdaAlignmentContributor"/>
/// </summary>
[TestClass]
public class LambdaAlignmentContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a parenthesized lambda block body is shifted to align the open brace with the open parenthesis.
    /// </summary>
    [TestMethod]
    public void ShiftsParenthesizedLambdaBlockToAnchor()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Action a = () =>
                                     {
                                         DoSomething();
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var lambda = root.DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        // Pre-populate layout for the block lines
        var block = lambda.Block;
        var openBraceLine = LayoutComputer.GetLine(block.OpenBraceToken);
        var closeBraceLine = LayoutComputer.GetLine(block.CloseBraceToken);
        model.Set(openBraceLine, new TokenLayout(8, "Block"));
        model.Set(closeBraceLine, new TokenLayout(8, "Block"));

        // Also set layout for inner statement lines
        foreach (var statement in block.Statements)
        {
            var statementLine = LayoutComputer.GetLine(statement.GetFirstToken());
            model.Set(statementLine, new TokenLayout(12, "Block"));
        }

        var contributor = new LambdaAlignmentContributor();

        var anchorColumn = LayoutComputer.GetColumn(lambda.ParameterList.OpenParenToken);

        // Act
        contributor.Contribute(lambda, scope, model, context);

        // Assert
        Assert.IsTrue(model.TryGetLayout(openBraceLine, out var openLayout));
        Assert.AreEqual(anchorColumn, openLayout.Column, "Open brace should be shifted to anchor column");
    }

    /// <summary>
    /// Verifies that a simple lambda block body is shifted to align with the parameter identifier.
    /// </summary>
    [TestMethod]
    public void ShiftsSimpleLambdaBlockToParameterAnchor()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Action<int> a = x =>
                                     {
                                         DoSomething();
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var lambda = root.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        var block = lambda.Block;
        var openBraceLine = LayoutComputer.GetLine(block.OpenBraceToken);
        var closeBraceLine = LayoutComputer.GetLine(block.CloseBraceToken);
        model.Set(openBraceLine, new TokenLayout(8, "Block"));
        model.Set(closeBraceLine, new TokenLayout(8, "Block"));

        foreach (var statement in block.Statements)
        {
            var statementLine = LayoutComputer.GetLine(statement.GetFirstToken());
            model.Set(statementLine, new TokenLayout(12, "Block"));
        }

        var contributor = new LambdaAlignmentContributor();
        var anchorColumn = LayoutComputer.GetColumn(lambda.Parameter.Identifier);

        // Act
        contributor.Contribute(lambda, scope, model, context);

        // Assert
        Assert.IsTrue(model.TryGetLayout(openBraceLine, out var openLayout));
        Assert.AreEqual(anchorColumn, openLayout.Column, "Open brace should align to parameter identifier");
    }

    /// <summary>
    /// Verifies that expression-bodied lambdas (no block) are not modified.
    /// </summary>
    [TestMethod]
    public void DoesNothingForExpressionBodiedLambda()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Func<int, int> f = x => x + 1;
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var lambda = root.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new LambdaAlignmentContributor();

        // Act
        contributor.Contribute(lambda, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Expression-bodied lambda should not produce layout entries");
    }

    /// <summary>
    /// Verifies that when the block already aligns to the anchor, no shift is performed.
    /// </summary>
    [TestMethod]
    public void DoesNotShiftWhenAlreadyAligned()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     Action a = () =>
                                                {
                                                    DoSomething();
                                                };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var lambda = root.DescendantNodes().OfType<ParenthesizedLambdaExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);

        var block = lambda.Block;
        var openBraceLine = LayoutComputer.GetLine(block.OpenBraceToken);
        var anchorColumn = LayoutComputer.GetColumn(lambda.ParameterList.OpenParenToken);

        // Pre-populate with anchor-aligned layout
        model.Set(openBraceLine, new TokenLayout(anchorColumn, "Block"));

        var contributor = new LambdaAlignmentContributor();

        // Act
        contributor.Contribute(lambda, scope, model, context);

        // Assert — should still have same layout (no shift needed)
        Assert.IsTrue(model.TryGetLayout(openBraceLine, out var layout));
        Assert.AreEqual(anchorColumn, layout.Column, "Already aligned, no shift needed");
    }

    /// <summary>
    /// Verifies that non-lambda nodes are ignored by the contributor.
    /// </summary>
    [TestMethod]
    public void IgnoresNonLambdaNodes()
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
        var contributor = new LambdaAlignmentContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-lambda nodes should not produce layout entries");
    }

    #endregion // Methods
}