using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="BinaryExpressionContributor"/>
/// </summary>
[TestClass]
public class BinaryExpressionContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that binary operators on continuation lines are aligned to the left operand column.
    /// </summary>
    [TestMethod]
    public void AlignsOperatorToLeftOperandColumn()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = a
                            && b
                            && c;
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var outerBinary= root.DescendantNodes().OfType<BinaryExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BinaryExpressionContributor();

        // Act
        contributor.Contribute(outerBinary, scope, model, context);

        // Assert
        Assert.IsGreaterThan(0, model.Count, "Should produce layout entries for multi-line binary expressions");

        foreach (var binary in root.DescendantNodes().OfType<BinaryExpressionSyntax>())
        {
            if (LayoutComputer.IsFirstOnLine(binary.OperatorToken))
            {
                var line = LayoutComputer.GetLine(binary.OperatorToken);
                Assert.IsTrue(model.TryGetLayout(line, out _), $"Expected layout for operator on line {line}");
            }
        }
    }

    /// <summary>
    /// Verifies that single-line binary expressions do not produce layout entries.
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleLineExpression()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = a && b && c;
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var binary = root.DescendantNodes().OfType<BinaryExpressionSyntax>().Last();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BinaryExpressionContributor();

        // Act
        contributor.Contribute(binary, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single-line binary expression should not produce layout entries");
    }

    /// <summary>
    /// Verifies that nested binary expressions of the same kind are skipped (only outermost is processed).
    /// </summary>
    [TestMethod]
    public void SkipsNestedBinaryOfSameKind()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    var x = a
                            && b
                            && c;
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);

        var innerBinary= root.DescendantNodes().OfType<BinaryExpressionSyntax>().Last();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BinaryExpressionContributor();

        // Act — contribute the inner binary, which has same-kind parent
        contributor.Contribute(innerBinary, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Inner binary of same kind should be skipped");
    }

    /// <summary>
    /// Verifies that is-pattern expressions align the is keyword to the expression column.
    /// </summary>
    [TestMethod]
    public void AlignsIsKeywordToExpressionColumn()
    {
        // Arrange
        const string input = """
            class C
            {
                void M(object obj)
                {
                    var x = obj
                            is string s;
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var isPattern = root.DescendantNodes().OfType<IsPatternExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BinaryExpressionContributor();

        var expressionColumn = LayoutComputer.GetColumn(isPattern.Expression.GetFirstToken());

        // Act
        contributor.Contribute(isPattern, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(isPattern.IsKeyword))
        {
            var line = LayoutComputer.GetLine(isPattern.IsKeyword);
            Assert.IsTrue(model.TryGetLayout(line, out var layout));
            Assert.AreEqual(expressionColumn, layout.Column, "is keyword should align to expression column");
        }
    }

    /// <summary>
    /// Verifies that the null-coalescing operator produces a layout entry for one-indent alignment.
    /// </summary>
    [TestMethod]
    public void AlignsNullCoalescingOperatorWithOneIndent()
    {
        // Arrange
        const string input = """
            using System.Linq;

            class C
            {
                int M(int[] values)
                {
                    return values?.Where(x => x > 0)
                                  .FirstOrDefault()
                                 ?? -1;
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var binary = root.DescendantNodes().OfType<BinaryExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BinaryExpressionContributor();

        // Act
        contributor.Contribute(binary, scope, model, context);

        // Assert
        Assert.IsGreaterThan(0, model.Count, "Should produce layout entry for null-coalescing alignment");
    }

    /// <summary>
    /// Verifies that string concatenation produces layout entries for aligning the plus operator.
    /// </summary>
    [TestMethod]
    public void AlignsStringConcatenationPlusOperator()
    {
        // Arrange
        const string input = """
            class C
            {
                string M(string name)
                {
                    return string.Concat(name + " "
                        + "suffix");
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var binary = root.DescendantNodes().OfType<BinaryExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BinaryExpressionContributor();

        // Act
        contributor.Contribute(binary, scope, model, context);

        // Assert
        Assert.IsGreaterThan(0, model.Count, "Should produce layout entries for plus operator alignment");
    }

    /// <summary>
    /// Verifies that non-binary-expression nodes are ignored by the contributor.
    /// </summary>
    [TestMethod]
    public void IgnoresNonBinaryExpressionNodes()
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
        var contributor = new BinaryExpressionContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-binary-expression nodes should not produce layout entries");
    }

    /// <summary>
    /// Verifies that or keywords in an is-pattern expression produce layout entries aligned to the first pattern value column.
    /// </summary>
    [TestMethod]
    public void AlignsOrKeywordsInIsPatternExpression()
    {
        // Arrange
        const string input = """
            class C
            {
                bool M(int kind)
                {
                    return kind is 1
                        or 2
                            or 3;
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var binaryPattern = root.DescendantNodes().OfType<BinaryPatternSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new BinaryExpressionContributor();

        var firstPatternColumn = LayoutComputer.GetColumn(binaryPattern.Left.GetFirstToken());

        // Act
        contributor.Contribute(binaryPattern, scope, model, context);

        // Assert
        Assert.IsGreaterThan(0, model.Count, "Should produce layout entries for or keywords in is-pattern");

        foreach (var pattern in root.DescendantNodes().OfType<BinaryPatternSyntax>())
        {
            if (LayoutComputer.IsFirstOnLine(pattern.OperatorToken))
            {
                var line = LayoutComputer.GetLine(pattern.OperatorToken);

                Assert.IsTrue(model.TryGetLayout(line, out var layout), $"Expected layout for or keyword on line {line}");
                Assert.AreEqual(firstPatternColumn, layout.Column, $"or keyword on line {line} should align to first pattern column");
            }
        }
    }

    #endregion // Methods
}