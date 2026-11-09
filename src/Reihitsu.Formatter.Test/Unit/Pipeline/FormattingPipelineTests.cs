using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Pipeline.LineEndings;
using Reihitsu.Formatter.Pipeline.Preparation;
using Reihitsu.Formatter.Pipeline.TabReplacement;
using Reihitsu.Formatter.Test.Helpers;

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
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken).Output;
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

            Assert.ThrowsExactly<OperationCanceledException>(() => FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, cts.Token).Output);
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
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken).Output;

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
        var firstPass = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken).Output;
        var canonical = firstPass.ToFullString();

        // Act - format the already-formatted output a second time
        var secondTree = CSharpSyntaxTree.ParseText(canonical, cancellationToken: TestContext.CancellationToken);
        var secondPass = FormattingPipeline.Execute(secondTree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken).Output;
        var actual = secondPass.ToFullString();

        // Assert - second pass must produce identical output, ignoring trailing newline
        // differences introduced by the cleanup phase
        Assert.AreEqual(canonical.TrimEnd('\n'), actual.TrimEnd('\n'), "Formatted code should not change when formatted again.");
    }

    /// <summary>
    /// Verifies that observing the pipeline reports every phase in runtime order without changing the final output
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-001")]
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
                                 "PrecheckPhase",
                                 "PreparationPhase/TabReplacement",
                                 "PreparationPhase/LineEndings",
                                 "StructuralTransformPhase",
                                 "RegionFormattingPhase",
                                 "DocumentationCommentFormattingPhase",
                                 "UsingDirectiveOrderingPhase",
                                 "LineBreakPhase",
                                 "SwitchCaseBracePhase",
                                 "BlankLinePhase",
                                 "HorizontalSpacingPhase",
                                 "IndentationPhase",
                                 "CleanupPhase"
                             };

        // Act
        var unobserved = FormattingPipeline.Execute(unobservedTree.GetRoot(TestContext.CancellationToken),
                                                    context,
                                                    TestContext.CancellationToken).Output;
        var observed = FormattingPipeline.Execute(observedTree.GetRoot(TestContext.CancellationToken),
                                                  context,
                                                  (phaseName, before, after) =>
                                                  {
                                                      observedPhases.Add(phaseName);
                                                      Assert.IsNotNull(before);
                                                      Assert.IsNotNull(after);
                                                  },
                                                  TestContext.CancellationToken).Output;

        // Assert
        Assert.AreSequenceEqual(expectedPhases, observedPhases);
        Assert.AreEqual(unobserved.ToFullString(), observed.ToFullString());
    }

    /// <summary>
    /// Verifies that the TabReplacement sub-phase, run alone, is reported under the name of its row in the phase table
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-001")]
    public void TabReplacementSubPhaseRunAloneIsReportedUnderItsSubPhaseName()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText("class C\n{\n\tint x;\n}", cancellationToken: TestContext.CancellationToken);
        var observedPhases = new List<string>();

        // Act
        var result = FormattingPipeline.ExecutePhases(tree.GetRoot(TestContext.CancellationToken),
                                                      new FormattingContext("\n"),
                                                      [new TabReplacementPhase()],
                                                      (phaseName, _, _) => observedPhases.Add(phaseName),
                                                      TestContext.CancellationToken).Output;

        // Assert
        Assert.AreSequenceEqual(new[] { "PreparationPhase/TabReplacement" }, observedPhases);
        Assert.AreEqual("class C\n{\n    int x;\n}", result.ToFullString());
    }

    /// <summary>
    /// Verifies that the LineEndings sub-phase, run alone, is reported under the name of its row in the phase table
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-001")]
    public void LineEndingsSubPhaseRunAloneIsReportedUnderItsSubPhaseName()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText("class C\r\n{\n    int x;\r}", cancellationToken: TestContext.CancellationToken);
        var observedPhases = new List<string>();

        // Act
        var result = FormattingPipeline.ExecutePhases(tree.GetRoot(TestContext.CancellationToken),
                                                      new FormattingContext("\n"),
                                                      [new LineEndingNormalizationPhase()],
                                                      (phaseName, _, _) => observedPhases.Add(phaseName),
                                                      TestContext.CancellationToken).Output;

        // Assert
        Assert.AreSequenceEqual(new[] { "PreparationPhase/LineEndings" }, observedPhases);
        Assert.AreEqual("class C\n{\n    int x;\n}", result.ToFullString());
    }

    /// <summary>
    /// Verifies that the preparation phase, run alone, is not reported as a step of its own but reports its two sub-phases in
    /// the order of the phase table, each receiving the output of the step before it
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-001")]
    public void PreparationPhaseRunAloneReportsItsTwoSubPhasesInOrder()
    {
        // Arrange
        var preparationPhase = new PreparationPhase();
        var tree = CSharpSyntaxTree.ParseText("class C\r\n{\n\tint x;\r}", cancellationToken: TestContext.CancellationToken);
        var observedPhases = new List<(string Name, string Input, string Output)>();

        // Act
        var result = FormattingPipeline.ExecutePhases(tree.GetRoot(TestContext.CancellationToken),
                                                      new FormattingContext("\n"),
                                                      [preparationPhase],
                                                      (phaseName, before, after) => observedPhases.Add((phaseName, before.ToFullString(), after.ToFullString())),
                                                      TestContext.CancellationToken);

        // Assert
        Assert.AreSequenceEqual(new[] { "PreparationPhase/TabReplacement", "PreparationPhase/LineEndings" }, observedPhases.Select(phase => phase.Name).ToList());
        Assert.AreEqual("class C\r\n{\n\tint x;\r}", observedPhases[0].Input, "The TabReplacement sub-phase should receive the input of the preparation phase.");
        Assert.AreEqual("class C\r\n{\n    int x;\r}", observedPhases[0].Output, "The TabReplacement sub-phase should replace the tab and keep every line break.");
        Assert.AreEqual(observedPhases[0].Output, observedPhases[1].Input, "The LineEndings sub-phase should receive the output of the TabReplacement sub-phase.");
        Assert.AreEqual("class C\n{\n    int x;\n}", observedPhases[1].Output, "The LineEndings sub-phase should rewrite every line break to the detected line ending.");
        Assert.AreEqual(observedPhases[1].Output, result.Output.ToFullString());
    }

    /// <summary>
    /// Verifies that the preparation phase, executed directly as a phase, replaces the tabs in the whitespace of the code and
    /// between directive tokens and rewrites every end-of-line trivia, directive line break, and documentation newline to the
    /// context's line ending, and returns what the pipeline returns when it runs the preparation phase
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-019")]
    public void PreparationPhaseExecutedDirectlyReplacesTabsAndRewritesLineBreaks()
    {
        // Arrange
        const string input = "class C\r\n{\n#region\tA\r\tint x;\u2028    /// <summary>\r    /// A\r\n    /// </summary>\n\tint y;\n#endregion\u0085}";
        const string expected = "class C\n{\n#region    A\n    int x;\n    /// <summary>\n    /// A\n    /// </summary>\n    int y;\n#endregion\n}";
        var root = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);
        var context = new FormattingContext("\n");

        // Act
        var executed = new PreparationPhase().Execute(root, context, TestContext.CancellationToken).ToFullString();
        var pipelined = FormattingPipeline.ExecutePhases(root, context, [new PreparationPhase()], phaseObserver: null, TestContext.CancellationToken).Output.ToFullString();

        // Assert
        Assert.AreEqual(expected, executed);
        Assert.AreEqual(pipelined, executed, "Executing the preparation phase directly should give what the pipeline gives for it.");
    }

    /// <summary>
    /// Verifies that the preparation phase, executed directly with a cancelled token, throws
    /// <see cref="OperationCanceledException"/> instead of returning a result
    /// </summary>
    [TestMethod]
    public void PreparationPhaseExecutedDirectlyWithCancelledTokenThrowsOperationCanceled()
    {
        // Arrange
        var root = CSharpSyntaxTree.ParseText("class C\r\n{\n\tint x;\r}", cancellationToken: TestContext.CancellationToken).GetRoot(TestContext.CancellationToken);

        using (var cts = new CancellationTokenSource())
        {
            cts.Cancel();

            // Act & Assert
            Assert.ThrowsExactly<OperationCanceledException>(() => new PreparationPhase().Execute(root, new FormattingContext("\n"), cts.Token));
        }
    }

    /// <summary>
    /// Verifies that the precheck is the first phase: an input it refuses is returned unchanged, no other phase runs on it, and
    /// only the precheck is reported to the observer
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-001")]
    [Requirement("FMT-UNSUP-001")]
    public void ExecuteRunsNoPhaseBehindThePrecheckOnARefusedInput()
    {
        // Arrange
        const string input = "internal class Sample{void M(){int /* c */ x=1;}}";
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var observedPhases = new List<string>();

        // Act
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken),
                                                new FormattingContext("\n"),
                                                (phaseName, _, _) => observedPhases.Add(phaseName),
                                                TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, result.Output.ToFullString());
        Assert.IsNotNull(result.Unsupported);
        Assert.DoesNotContain(phase => phase != "PrecheckPhase", observedPhases, "No phase behind the precheck may run on a refused input.");
    }

    /// <summary>
    /// Verifies that a phase's refusal is turned into the unsupported outcome where the pipeline produces the precheck's: the
    /// input is returned unchanged, also when a phase in front of the refusing one changed it, and the outcome carries the
    /// phase's reason and location; the refusal does not reach the caller as an exception
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-006")]
    public void ExecutePhasesTurnsAPhasesRefusalIntoTheUnsupportedOutcome()
    {
        // Arrange
        const string input = "internal class Sample;";
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var refusal = new UnsupportedSyntax("a shape the test phase refuses", Location.Create(tree, new TextSpan(9, 5)));
        var phases = new IFormattingPhase[] { new TextChangingPhase(), new ThrowingPhase(new SyntaxNotSupportedException(refusal)) };

        // Act
        var result = FormattingPipeline.ExecutePhases(tree.GetRoot(TestContext.CancellationToken),
                                                      new FormattingContext("\n"),
                                                      phases,
                                                      phaseObserver: null,
                                                      TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, result.Output.ToFullString());
        Assert.IsNotNull(result.Unsupported);
        Assert.AreEqual(refusal.Reason, result.Unsupported.Reason);
        Assert.AreEqual(refusal.Location.SourceSpan, result.Unsupported.Location.SourceSpan);
    }

    /// <summary>
    /// Verifies that a phase's refusal under an observer gives the same outcome, and is neither wrapped in another exception nor
    /// thrown to the observer's caller
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-006")]
    public void ExecutePhasesTurnsAnObservedPhasesRefusalIntoTheUnsupportedOutcome()
    {
        // Arrange
        const string input = "internal class Sample;";
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationToken);
        var refusal = new UnsupportedSyntax("a shape the test phase refuses", Location.Create(tree, new TextSpan(9, 5)));
        var phases = new IFormattingPhase[] { new TextChangingPhase(), new ThrowingPhase(new SyntaxNotSupportedException(refusal)) };
        var observedPhases = new List<string>();

        // Act
        var result = FormattingPipeline.ExecutePhases(tree.GetRoot(TestContext.CancellationToken),
                                                      new FormattingContext("\n"),
                                                      phases,
                                                      (phaseName, _, _) => observedPhases.Add(phaseName),
                                                      TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(input, result.Output.ToFullString());
        Assert.IsNotNull(result.Unsupported);
        Assert.AreEqual(refusal.Reason, result.Unsupported.Reason);
        Assert.AreEqual(refusal.Location.SourceSpan, result.Unsupported.Location.SourceSpan);
        Assert.AreSequenceEqual(new[] { nameof(TextChangingPhase) }, observedPhases);
    }

    /// <summary>
    /// Verifies that the pipeline returns no unsupported outcome for an input it formats
    /// </summary>
    [TestMethod]
    [Requirement("FMT-UNSUP-001")]
    public void ExecuteReportsNoUnsupportedOutcomeForAFormattedInput()
    {
        // Arrange
        var tree = CSharpSyntaxTree.ParseText("internal class Sample{void M(){int x=1; /* c */}}", cancellationToken: TestContext.CancellationToken);

        // Act
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), new FormattingContext("\n"), TestContext.CancellationToken);

        // Assert
        Assert.IsNull(result.Unsupported);
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
                                                                                                            TestContext.CancellationToken).Output);

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
                                                                                                             TestContext.CancellationToken).Output);

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
                                                                                                        TestContext.CancellationToken).Output);

        // Assert
        Assert.AreSame(expected, actual);
    }

    /// <summary>
    /// Verifies that structural transforms (Phase 0) execute before indentation (Phase 3),
    /// ensuring that newly generated block bodies receive correct indentation
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-001")]
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
        var result = FormattingPipeline.Execute(tree.GetRoot(TestContext.CancellationToken), context, TestContext.CancellationToken).Output;
        var actual = result.ToFullString();

        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}