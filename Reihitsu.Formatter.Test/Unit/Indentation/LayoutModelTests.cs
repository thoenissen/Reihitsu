using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;

namespace Reihitsu.Formatter.Test.Unit.Indentation;

/// <summary>
/// Tests for <see cref="LayoutModel"/>
/// </summary>
[TestClass]
public class LayoutModelTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that <see cref="LayoutModel.Set"/> stores a layout entry and
    /// <see cref="LayoutModel.TryGetLayout(int, out TokenLayout)"/> retrieves it correctly
    /// </summary>
    [TestMethod]
    public void SetAndTryGetLayoutReturnsCorrectLayout()
    {
        // Arrange
        var model = new LayoutModel();
        var layout = new TokenLayout(8, "block");

        // Act
        model.Set(5, layout);
        var found = model.TryGetLayout(5, out var result);

        // Assert
        Assert.IsTrue(found);
        Assert.AreEqual(8, result.Column);
        Assert.AreEqual("block", result.Source);
    }

    /// <summary>
    /// Verifies that <see cref="LayoutModel.TryGetLayout(int, out TokenLayout)"/> returns
    /// <see langword="false"/> when queried for a line number that has not been set
    /// </summary>
    [TestMethod]
    public void TryGetLayoutForMissingTokenReturnsFalse()
    {
        // Arrange
        var model = new LayoutModel();

        // Act
        var found = model.TryGetLayout(99, out _);

        // Assert
        Assert.IsFalse(found);
    }

    /// <summary>
    /// Verifies that the <see cref="LayoutModel.TryGetLayout(Microsoft.CodeAnalysis.SyntaxToken, out TokenLayout)"/>
    /// overload correctly resolves the token's line number and retrieves the stored layout
    /// </summary>
    [TestMethod]
    public void TryGetLayoutTokenOverloadReturnsCorrectLayout()
    {
        // Arrange
        var model = new LayoutModel();

        const string code = """
                            class Foo
                            {
                            }
                            """;

        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.CancellationTokenSource.Token);
        var root = tree.GetCompilationUnitRoot(TestContext.CancellationTokenSource.Token);

        // The "class" keyword is on line 0
        var classToken = root.DescendantTokens()
                             .First(t => t.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ClassKeyword));

        var lineNumber = classToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var layout = new TokenLayout(0, "root");
        model.Set(lineNumber, layout);

        // Act
        var found = model.TryGetLayout(classToken, out var result);

        // Assert
        Assert.IsTrue(found);
        Assert.AreEqual(0, result.Column);
        Assert.AreEqual("root", result.Source);
    }

    /// <summary>
    /// Verifies that the <see cref="LayoutModel.Count"/> property returns the correct number
    /// of entries after multiple insertions
    /// </summary>
    [TestMethod]
    public void CountReturnsNumberOfEntries()
    {
        // Arrange
        var model = new LayoutModel();

        // Act
        model.Set(0, new TokenLayout(0));
        model.Set(1, new TokenLayout(4));
        model.Set(2, new TokenLayout(8));

        // Assert
        Assert.AreEqual(3, model.Count);
    }

    /// <summary>
    /// Verifies that <see cref="LayoutModel.ShiftRange"/> correctly shifts all entries within
    /// the specified line range by the given delta
    /// </summary>
    [TestMethod]
    public void ShiftRangeShiftsColumnsCorrectly()
    {
        // Arrange
        var model = new LayoutModel();
        model.Set(0, new TokenLayout(0, "init"));
        model.Set(1, new TokenLayout(4, "init"));
        model.Set(2, new TokenLayout(4, "init"));
        model.Set(3, new TokenLayout(0, "init"));

        // Act — shift lines 1–2 right by 4 columns
        model.ShiftRange(1, 2, 4, "shift");

        // Assert
        model.TryGetLayout(0, out var layout0);
        model.TryGetLayout(1, out var layout1);
        model.TryGetLayout(2, out var layout2);
        model.TryGetLayout(3, out var layout3);

        Assert.AreEqual(0, layout0.Column, "Line 0 should be unchanged");
        Assert.AreEqual(8, layout1.Column, "Line 1 should be shifted from 4 to 8");
        Assert.AreEqual(8, layout2.Column, "Line 2 should be shifted from 4 to 8");
        Assert.AreEqual(0, layout3.Column, "Line 3 should be unchanged");
        Assert.AreEqual("shift", layout1.Source, "Source should be updated to shift source");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutModel.ShiftRange"/> clamps columns to zero when a
    /// negative delta would produce a negative column value
    /// </summary>
    [TestMethod]
    public void ShiftRangeWithNegativeDeltaClampsToZero()
    {
        // Arrange
        var model = new LayoutModel();
        model.Set(0, new TokenLayout(2, "init"));

        // Act — shift left by 10, which would make column negative
        model.ShiftRange(0, 0, -10, "clamp");

        // Assert
        model.TryGetLayout(0, out var layout);

        Assert.AreEqual(0, layout.Column, "Column should be clamped to 0");
    }

    /// <summary>
    /// Verifies that <see cref="LayoutModel.ShiftRange"/> with a delta of zero does not
    /// modify any entries
    /// </summary>
    [TestMethod]
    public void ShiftRangeWithZeroDeltaDoesNotModify()
    {
        // Arrange
        var model = new LayoutModel();
        model.Set(0, new TokenLayout(4, "original"));

        // Act
        model.ShiftRange(0, 0, 0, "noop");

        // Assert
        model.TryGetLayout(0, out var layout);

        Assert.AreEqual(4, layout.Column);
        Assert.AreEqual("original", layout.Source, "Source should remain unchanged when delta is zero");
    }

    /// <summary>
    /// Verifies that calling <see cref="LayoutModel.Set"/> with the same line number overwrites
    /// the previously stored layout
    /// </summary>
    [TestMethod]
    public void SetOverwritesPreviousLayout()
    {
        // Arrange
        var model = new LayoutModel();
        model.Set(5, new TokenLayout(4, "first"));

        // Act
        model.Set(5, new TokenLayout(12, "second"));

        // Assert
        model.TryGetLayout(5, out var layout);

        Assert.AreEqual(12, layout.Column);
        Assert.AreEqual("second", layout.Source);
        Assert.AreEqual(1, model.Count, "Count should still be 1 after overwrite");
    }

    /// <summary>
    /// Verifies that a newly created <see cref="LayoutModel"/> has a count of zero
    /// </summary>
    [TestMethod]
    public void EmptyModelCountIsZero()
    {
        // Arrange & Act
        var model = new LayoutModel();

        // Assert
        Assert.AreEqual(0, model.Count);
    }

    #endregion // Methods
}