using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.Indentation.Contributors;

namespace Reihitsu.Formatter.Test.Unit.Indentation.Contributors;

/// <summary>
/// Tests for <see cref="MethodChainAlignmentContributor"/>
/// </summary>
[TestClass]
public class MethodChainAlignmentContributorTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that continuation dots in a method chain are aligned to the first dot column.
    /// </summary>
    [TestMethod]
    public void AlignsContinuationDotsToFirstDot()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    obj.Method1()
                       .Method2()
                       .Method3();
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new MethodChainAlignmentContributor();

        // Act
        contributor.Contribute(invocation, scope, model, context);

        // Assert
        Assert.IsGreaterThan(0, model.Count, "Should produce layout entries for method chain continuation dots");
    }

    /// <summary>
    /// Verifies that single-line method chains do not produce layout entries.
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleLineChain()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    obj.Method1().Method2();
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().Last();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new MethodChainAlignmentContributor();

        // Act
        contributor.Contribute(invocation, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single-line chain should not produce layout entries");
    }

    /// <summary>
    /// Verifies that a single method call (no chain) does not produce layout entries.
    /// </summary>
    [TestMethod]
    public void DoesNotAlignSingleMethodCall()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    obj.Method1();
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var invocation = root.DescendantNodes().OfType<InvocationExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new MethodChainAlignmentContributor();

        // Act
        contributor.Contribute(invocation, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Single method call should not produce layout entries");
    }

    /// <summary>
    /// Verifies that non-invocation nodes are ignored by the contributor.
    /// </summary>
    [TestMethod]
    public void IgnoresNonInvocationNodes()
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
        var contributor = new MethodChainAlignmentContributor();

        // Act
        contributor.Contribute(classDecl, scope, model, context);

        // Assert
        Assert.AreEqual(0, model.Count, "Non-invocation nodes should not produce layout entries");
    }

    /// <summary>
    /// Verifies that conditional access chains are aligned when the node is a conditional access expression.
    /// </summary>
    [TestMethod]
    public void AlignsConditionalAccessChain()
    {
        // Arrange
        const string input = """
            class C
            {
                void M()
                {
                    obj?.Method1()
                       ?.Method2();
                }
            }

            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        var conditionalAccess = root.DescendantNodes().OfType<ConditionalAccessExpressionSyntax>().First();
        var scope = new FormattingScope(0);
        var model = new LayoutModel();
        var context = new FormattingContext(Environment.NewLine);
        var contributor = new MethodChainAlignmentContributor();

        // Act
        contributor.Contribute(conditionalAccess, scope, model, context);

        // Assert
        Assert.IsGreaterThan(0, model.Count, "Should produce layout entries for conditional access chain");
    }

    #endregion // Methods
}