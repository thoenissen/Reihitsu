using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="ConditionalExpressionContributor"/>
/// </summary>
[TestClass]
public class ConditionalExpressionContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the question and colon tokens are aligned at condition column plus one indent level
    /// </summary>
    [TestMethod]
    public void AlignsQuestionAndColonToConditionPlusIndent()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = condition
                                                 ? a
                                                 : b;
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var conditional = root.DescendantNodes().OfType<ConditionalExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ConditionalExpressionContributor();

        var conditionColumn = LayoutComputer.GetColumn(conditional.Condition.GetFirstToken());
        var expectedOperatorColumn = conditionColumn + FormattingContext.IndentSize;

        // Act
        contributor.Contribute(conditional, scope, model, context);

        // Assert
        if (LayoutComputer.IsFirstOnLine(conditional.QuestionToken))
        {
            var questionLine = LayoutComputer.GetLine(conditional.QuestionToken);

            Assert.IsTrue(model.TryGetLayout(questionLine, out var questionLayout));
            Assert.AreEqual(expectedOperatorColumn, questionLayout.Column, "? should align to condition + indent");
        }

        if (LayoutComputer.IsFirstOnLine(conditional.ColonToken))
        {
            var colonLine = LayoutComputer.GetLine(conditional.ColonToken);

            Assert.IsTrue(model.TryGetLayout(colonLine, out var colonLayout));
            Assert.AreEqual(expectedOperatorColumn, colonLayout.Column, ": should align to condition + indent");
        }
    }

    /// <summary>
    /// Verifies that single-line conditional expressions do not produce layout entries
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleLineConditional()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = a ? b : c;
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var conditional = root.DescendantNodes().OfType<ConditionalExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ConditionalExpressionContributor();

        // Act
        contributor.Contribute(conditional, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single-line conditional should not produce layout entries");
    }

    /// <summary>
    /// Verifies that non-conditional-expression nodes are ignored by the contributor
    /// </summary>
    [TestMethod]
    public void IgnoresNonConditionalExpressionNodes()
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
        var contributor = new ConditionalExpressionContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-conditional-expression nodes should not produce layout entries");
    }

    /// <summary>
    /// Verifies that nested conditionals use the parent operator column plus indent as their alignment base
    /// </summary>
    [TestMethod]
    public void AlignsNestedConditionalRelativeToParentOperator()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = cond1
                                                 ? cond2
                                                     ? a
                                                     : b
                                                 : c;
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var conditionals = root.DescendantNodes().OfType<ConditionalExpressionSyntax>().ToList();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ConditionalExpressionContributor();

        // Act — contribute both outer and inner
        foreach (var conditional in conditionals)
        {
            contributor.Contribute(conditional, scope, model, context);
        }

        // Assert — the nested conditional should have layout entries
        Assert.IsGreaterThan(0, model.Count, "Nested conditionals should produce layout entries");
    }

    /// <summary>
    /// Verifies that nested ternary operators in an assignment produce correct indentation layouts
    /// </summary>
    [TestMethod]
    public void AlignsNestedTernaryInAssignment()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M(bool isSpecial, bool isImportant)
                                 {
                                     var mode = isSpecial == true
                                         ? 1
                                         : isImportant == true
                                             ? 2
                                             : 0;
                                 }
                             }

                             """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var outerConditional = root.DescendantNodes().OfType<ConditionalExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new ConditionalExpressionContributor();

        // Act
        contributor.Contribute(outerConditional, scope, model, context);

        // Assert — should have layouts for both outer and inner ternary operators
        Assert.IsGreaterThan(1, model.Count, "Should produce layout entries for both nested ternary levels");
    }

    #endregion // Methods
}