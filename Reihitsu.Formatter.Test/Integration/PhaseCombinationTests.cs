using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.BlankLines;
using Reihitsu.Formatter.Pipeline.HorizontalSpacing;
using Reihitsu.Formatter.Pipeline.Indentation;
using Reihitsu.Formatter.Pipeline.LineBreaks;
using Reihitsu.Formatter.Pipeline.StructuralTransforms;

namespace Reihitsu.Formatter.Test.Integration;

/// <summary>
/// Integration tests that verify multiple formatting phases applied in sequence
/// </summary>
[TestClass]
public class PhaseCombinationTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that structural transforms followed by blank line insertion produce the correct output
    /// </summary>
    [TestMethod]
    public void StructuralTransformThenBlankLine()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 int M() => 42;
                                 int N() => 99;
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    int M(){return42;}
                                    int N(){return99;}
                                }

                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        root = StructuralTransformPhase.Execute(root, context, TestContext.CancellationTokenSource.Token);
        root = BlankLinePhase.Execute(root, context, TestContext.CancellationTokenSource.Token);

        var actual = root.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that blank line insertion followed by horizontal spacing produce the correct output
    /// </summary>
    [TestMethod]
    public void BlankLineThenHorizontalSpacing()
    {
        // Arrange
        const string input = """
                             class C
                             {
                                 void M()
                                 {
                                     var x = 1+2;
                                     return;
                                 }
                             }

                             """;
        const string expected = """
                                class C
                                {
                                    void M()
                                    {
                                        var x = 1 + 2;

                                        return;
                                    }
                                }

                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        root = BlankLinePhase.Execute(root, context, TestContext.CancellationTokenSource.Token);
        root = HorizontalSpacingPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        var actual = root.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that line break correction followed by indentation produce properly formatted output
    /// </summary>
    [TestMethod]
    public void LineBreakThenIndentation()
    {
        // Arrange
        const string input = """
                             class C {
                             void M() {
                             var x = 1;
                             }
                             }

                             """;
        const string expected = """
                                class C 
                                {
                                    void M() 
                                    {
                                        var x = 1;
                                    }
                                }

                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        root = LineBreakPhase.Execute(root, context, TestContext.CancellationTokenSource.Token);

        var model = LayoutComputer.Compute(root, context);
        root = IndentationRewriter.Apply(root, model);

        var actual = root.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that structural transforms, line breaks, and horizontal spacing combine correctly
    /// </summary>
    [TestMethod]
    public void StructuralTransformThenLineBreakThenHorizontalSpacing()
    {
        // Arrange
        const string input = """
                             class C {
                                 int M() => 1+2;
                             }

                             """;
        const string expected = """
                                class C 
                                {
                                    int M()
                                {
                                return 1 + 2;
                                }
                                }

                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        root = StructuralTransformPhase.Execute(root, context, TestContext.CancellationTokenSource.Token);
        root = LineBreakPhase.Execute(root, context, TestContext.CancellationTokenSource.Token);
        root = HorizontalSpacingPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        var actual = root.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Verifies that the main phases (structural transforms, blank lines, line breaks, horizontal spacing, and indentation) combine correctly
    /// </summary>
    [TestMethod]
    public void AllMainPhasesCombined()
    {
        // Arrange
        const string input = """
                             class C {
                                 int M() => 1+2;
                                 void N()
                                 {
                                     var x = 1;
                                     return;
                                 }
                             }

                             """;
        const string expected = """
                                class C 
                                {
                                    int M()
                                    {
                                        return 1 + 2;
                                    }
                                    void N()
                                    {
                                        var x = 1;

                                        return;
                                    }
                                }

                                """;

        // Act
        var tree = CSharpSyntaxTree.ParseText(input, cancellationToken: TestContext.CancellationTokenSource.Token);
        var context = new FormattingContext(Environment.NewLine);
        var root = tree.GetRoot(TestContext.CancellationTokenSource.Token);
        root = StructuralTransformPhase.Execute(root, context, TestContext.CancellationTokenSource.Token);
        root = BlankLinePhase.Execute(root, context, TestContext.CancellationTokenSource.Token);
        root = LineBreakPhase.Execute(root, context, TestContext.CancellationTokenSource.Token);
        root = HorizontalSpacingPhase.Execute(root, TestContext.CancellationTokenSource.Token);

        var model = LayoutComputer.Compute(root, context);
        root = IndentationRewriter.Apply(root, model);

        var actual = root.ToFullString();

        // Assert
        Assert.AreEqual(expected, actual);
    }

    #endregion // Methods
}