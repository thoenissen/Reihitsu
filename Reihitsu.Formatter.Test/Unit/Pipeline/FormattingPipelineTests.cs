using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
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
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the pipeline applies rules from multiple phases, including structural
    /// transforms, blank line management, and indentation normalization
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
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);
        var actual = result.ToFullString();

        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the pipeline throws <see cref="OperationCanceledException"/> when the
    /// cancellation token is already cancelled
    /// </summary>
    [TestMethod]
    public void ExecuteCancellationRequestedThrowsOperationCanceled()
    {
        // Arrange
        var input = """
                    class Foo
                    {
                    }
                    """;

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(Environment.NewLine);

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            Assert.ThrowsExactly<OperationCanceledException>(() => FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, cts.Token));
        }
    }

    /// <summary>
    /// Verifies that formatting an empty compilation unit does not cause any errors and
    /// returns a valid syntax node
    /// </summary>
    [TestMethod]
    public void ExecuteEmptyCompilationUnitReturnsNode()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText(string.Empty, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(Environment.NewLine);

        // Act
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(result);

        var actual = result.ToFullString().Trim();

        Assert.AreEqual(string.Empty, actual);
    }

    /// <summary>
    /// Verifies that already well-formatted code passes through the pipeline without any
    /// modifications
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

        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var firstPass = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);
        var canonical = firstPass.ToFullString();

        // Act - format the already-formatted output a second time
        var secondTree = CSharpSyntaxTree.ParseText(canonical, cancellationToken: TestContext.CancellationToken);
        var secondPass = FormattingPipeline.Execute(secondTree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);
        var actual = secondPass.ToFullString();

        // Assert - second pass must produce identical output, ignoring trailing newline
        // differences introduced by the cleanup phase
        Assert.AreEqual(canonical.TrimEnd('\n'), actual.TrimEnd('\n'), "Formatted code should not change when formatted again.");
    }

    /// <summary>
    /// Verifies that observing the pipeline reports every phase in runtime order without changing the final output
    /// </summary>
    [TestMethod]
    public void ExecuteObserverReportsAllPhasesInOrderWithoutChangingOutput()
    {
        // Arrange
        var input = "internal class Sample{public int Value=>1;}";
        var context = new FormattingContext("\n");
        var unobservedTree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var observedTree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var observedPhases = new List<string>();
        var expectedPhases = new[]
                             {
                                 "StructuralTransformPhase",
                                 "RegionFormattingPhase",
                                 "DocumentationCommentFormattingPhase",
                                 "UsingDirectiveOrderingPhase",
                                 "BlankLinePhase",
                                 "LineBreakPhase",
                                 "SwitchCaseBracePhase",
                                 "HorizontalSpacingPhase",
                                 "IndentationPhase",
                                 "RawStringAlignmentPhase",
                                 "CleanupPhase",
                                 "LineEndingNormalizationPhase"
                             };

        // Act
        var unobserved = FormattingPipeline.Execute(unobservedTree.GetRoot(TestContext.CancellationToken),
                                                    context,
                                                    TestContext.CancellationToken);
        var observed = FormattingPipeline.Execute(observedTree.GetRoot(TestContext.CancellationToken),
                                                  context,
                                                  (phaseName, before, after) =>
                                                  {
                                                      observedPhases.Add(phaseName);
                                                      Assert.IsNotNull(before);
                                                      Assert.IsNotNull(after);
                                                  },
                                                  TestContext.CancellationToken);

        // Assert
        CollectionAssert.AreEqual(expectedPhases, observedPhases);
        Assert.AreEqual(unobserved.ToFullString(), observed.ToFullString());
    }

    /// <summary>
    /// Verifies that an observed phase failure is wrapped with the failing phase name and original exception
    /// </summary>
    [TestMethod]
    public void ExecutePhasesObservedFailureAddsPhaseContext()
    {
        // Arrange
        var expected = new NotSupportedException("phase failure");
        var tree = CSharpSyntaxTree.ParseText("internal class Sample;", cancellationToken: TestContext.CancellationToken);
        var phases = new IFormattingPhase[] { new ThrowingPhase(expected) };

        // Act
        var actual = Assert.ThrowsExactly<InvalidOperationException>(() => FormattingPipeline.ExecutePhases(tree.GetRoot(TestContext.CancellationToken),
                                                                                                            new FormattingContext("\n"),
                                                                                                            phases,
                                                                                                            (_, _, _) =>
                                                                                                            {
                                                                                                            },
                                                                                                            TestContext.CancellationToken));

        // Assert
        Assert.Contains(nameof(ThrowingPhase), actual.Message);
        Assert.AreSame(expected, actual.InnerException);
    }

    /// <summary>
    /// Verifies that cancellation raised inside an observed phase keeps its original exception type and identity
    /// </summary>
    [TestMethod]
    public void ExecutePhasesObservedCancellationIsNotWrapped()
    {
        // Arrange
        var expected = new OperationCanceledException("phase cancellation");
        var tree = CSharpSyntaxTree.ParseText("internal class Sample;", cancellationToken: TestContext.CancellationToken);
        var phases = new IFormattingPhase[] { new ThrowingPhase(expected) };

        // Act
        var actual = Assert.ThrowsExactly<OperationCanceledException>(() => FormattingPipeline.ExecutePhases(tree.GetRoot(TestContext.CancellationToken),
                                                                                                             new FormattingContext("\n"),
                                                                                                             phases,
                                                                                                             (_, _, _) =>
                                                                                                             {
                                                                                                             },
                                                                                                             TestContext.CancellationToken));

        // Assert
        Assert.AreSame(expected, actual);
    }

    /// <summary>
    /// Verifies that the unobserved phase path preserves the original phase exception
    /// </summary>
    [TestMethod]
    public void ExecutePhasesUnobservedFailureIsNotWrapped()
    {
        // Arrange
        var expected = new NotSupportedException("phase failure");
        var tree = CSharpSyntaxTree.ParseText("internal class Sample;", cancellationToken: TestContext.CancellationToken);
        var phases = new IFormattingPhase[] { new ThrowingPhase(expected) };

        // Act
        var actual = Assert.ThrowsExactly<NotSupportedException>(() => FormattingPipeline.ExecutePhases(tree.GetRoot(TestContext.CancellationToken),
                                                                                                        new FormattingContext("\n"),
                                                                                                        phases,
                                                                                                        phaseObserver: null,
                                                                                                        TestContext.CancellationToken));

        // Assert
        Assert.AreSame(expected, actual);
    }

    /// <summary>
    /// Verifies that structural transforms (Phase 0) execute before indentation (Phase 3),
    /// ensuring that newly generated block bodies receive correct indentation
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
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var context = new FormattingContext(Environment.NewLine);
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken);
        var actual = result.ToFullString();

        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}