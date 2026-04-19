using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="SwitchExpressionContributor"/>
/// </summary>
[TestClass]
public class SwitchExpressionContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that braces of a switch expression are aligned to the governing expression column.
    /// </summary>
    [TestMethod]
    public void AlignsBracesToGoverningExpression()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     return x switch
                                     {
                                         1 => 10,
                                         2 => 20,
                                         _ => 0
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var switchExpr = root.DescendantNodes().OfType<SwitchExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new SwitchExpressionContributor();

        var switchColumn = LayoutComputer.GetColumn(switchExpr.GoverningExpression.GetFirstToken());

        // Act
        contributor.Contribute(switchExpr, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(switchExpr.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(switchExpr.OpenBraceToken);
            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(switchColumn, openLayout.Column, "Open brace should align to governing expression");
        }

        if (LayoutComputer.IsFirstOnLine(switchExpr.CloseBraceToken))
        {
            var closeLine = LayoutComputer.GetLine(switchExpr.CloseBraceToken);
            Assert.IsTrue(model.TryGetLayout(closeLine, out var closeLayout));
            Assert.AreEqual(switchColumn, closeLayout.Column, "Close brace should align to governing expression");
        }
    }

    /// <summary>
    /// Verifies that arms of a switch expression are indented one level from the governing expression.
    /// </summary>
    [TestMethod]
    public void IndentsArmsOneLevelFromGoverningExpression()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     return x switch
                                     {
                                         1 => 10,
                                         2 => 20,
                                         _ => 0
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var switchExpr = root.DescendantNodes().OfType<SwitchExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new SwitchExpressionContributor();

        var switchColumn = LayoutComputer.GetColumn(switchExpr.GoverningExpression.GetFirstToken());
        var expectedArmColumn = switchColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(switchExpr, scope, model, context);

        // Assert
        foreach (var arm in switchExpr.Arms)
        {
            var firstToken = arm.GetFirstToken();

            if (LayoutComputer.IsFirstOnLine(firstToken))
            {
                var line = LayoutComputer.GetLine(firstToken);
                Assert.IsTrue(model.TryGetLayout(line, out var layout));
                Assert.AreEqual(expectedArmColumn, layout.Column, $"Arm on line {line} should be indented +4 from governing expression");
            }
        }
    }

    /// <summary>
    /// Verifies that non-switch-expression nodes are ignored by the contributor.
    /// </summary>
    [TestMethod]
    public void IgnoresNonSwitchExpressionNodes()
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
        var contributor = new SwitchExpressionContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-switch-expression nodes should not produce layout entries");
    }

    /// <summary>
    /// Verifies that a switch expression with no arms still aligns braces to the governing expression.
    /// </summary>
    [TestMethod]
    public void AlignsBracesWithNoArms()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M(int x)
                                 {
                                     return x switch
                                     {
                                     };
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var switchExpr = root.DescendantNodes().OfType<SwitchExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new SwitchExpressionContributor();

        var switchColumn = LayoutComputer.GetColumn(switchExpr.GoverningExpression.GetFirstToken());

        // Act
        contributor.Contribute(switchExpr, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(switchExpr.OpenBraceToken))
        {
            var openLine = LayoutComputer.GetLine(switchExpr.OpenBraceToken);
            Assert.IsTrue(model.TryGetLayout(openLine, out var openLayout));
            Assert.AreEqual(switchColumn, openLayout.Column);
        }
    }

    #endregion // Methods
}