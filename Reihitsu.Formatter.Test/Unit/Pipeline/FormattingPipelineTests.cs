using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for <see cref="Reihitsu.Formatter.Pipeline.FormattingPipeline"/>
/// </summary>
[TestClass]
public class FormattingPipelineTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test.
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the pipeline applies rules from multiple phases, including structural
    /// transforms, blank line management, and indentation normalization.
    /// </summary>
    [TestMethod]
    public void ExecuteAppliesAllRules()
    {
        // Arrange
        var input = """
                    class Foo
                    {
                      public int GetValue() => 42;
                        public void Run()
                        {
                            var x = 1;
                            return;
                        }
                    }
                    """;
        var expected = """
                       class Foo
                       {
                           public int GetValue()
                           {
                               return 42;
                           }
                           public void Run()
                           {
                               var x = 1;
                       
                               return;
                           }
                       }
                       """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the pipeline throws <see cref="OperationCanceledException"/> when the
    /// cancellation token is already cancelled.
    /// </summary>
    [TestMethod]
    public void ExecuteCancellationRequestedThrowsOperationCanceled()
    {
        // Arrange
        var input =
        """
            class Foo
            {
            }
            """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            Assert.ThrowsExactly<OperationCanceledException>(() => FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, cts.Token));
        }
    }

    /// <summary>
    /// Verifies that formatting an empty compilation unit does not cause any errors and
    /// returns a valid syntax node.
    /// </summary>
    [TestMethod]
    public void ExecuteEmptyCompilationUnitReturnsNode()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText(string.Empty, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);

        // Assert
        Assert.IsNotNull(result);

        var actual = result.ToFullString().Trim();

        Assert.AreEqual(string.Empty, actual);
    }

    /// <summary>
    /// Verifies that already well-formatted code passes through the pipeline without any
    /// modifications.
    /// </summary>
    [TestMethod]
    public void ExecuteAlreadyFormattedCodeNoChanges()
    {
        // Arrange - format once to get the canonical form
        var input = """
                    class Foo
                    {
                        public int GetValue()
                        {
                            return 42;
                        }
                    }
                    """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var firstPass = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var canonical = firstPass.ToFullString();

        // Act - format the already-formatted output a second time
        var secondTree = CSharpSyntaxTree.ParseText(canonical, cancellationToken: TestContext.CancellationTokenSource.Token);
        var secondPass = FormattingPipeline.Execute(secondTree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = secondPass.ToFullString();

        // Assert - second pass must produce identical output, ignoring trailing newline
        // differences introduced by the cleanup phase
        Assert.AreEqual(canonical.TrimEnd('\n'), actual.TrimEnd('\n'), "Formatted code should not change when formatted again.");
    }

    /// <summary>
    /// Verifies that structural transforms (Phase 0) execute before indentation (Phase 3),
    /// ensuring that newly generated block bodies receive correct indentation.
    /// </summary>
    [TestMethod]
    public void ExecutePhaseOrderStructuralBeforeIndentation()
    {
        // Arrange - expression-bodied method that needs structural conversion followed by indentation
        var input = """
                    class Foo
                    {
                        public int GetValue() => 42;
                    }
                    """;
        var expected = """
                       class Foo
                       {
                           public int GetValue()
                           {
                               return 42;
                           }
                       }
                       """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationTokenSource.Token), context, TestContext.CancellationTokenSource.Token);
        var actual = result.ToFullString();

        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}