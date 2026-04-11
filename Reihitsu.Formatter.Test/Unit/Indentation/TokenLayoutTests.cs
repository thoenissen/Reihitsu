using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;

namespace Reihitsu.Formatter.Test.Unit.Indentation;

/// <summary>
/// Tests for <see cref="TokenLayout"/>
/// </summary>
[TestClass]
public class TokenLayoutTests
{
    #region Methods

    /// <summary>
    /// Verifies that the <see cref="TokenLayout.Column"/> property returns the value provided
    /// to the constructor.
    /// </summary>
    [TestMethod]
    public void ConstructorSetsColumn()
    {
        // Arrange & Act
        var layout = new TokenLayout(8);

        // Assert
        Assert.AreEqual(8, layout.Column);
    }

    /// <summary>
    /// Verifies that the <see cref="TokenLayout.Source"/> property returns the value provided
    /// to the constructor.
    /// </summary>
    [TestMethod]
    public void ConstructorSetsSource()
    {
        // Arrange & Act
        var layout = new TokenLayout(4, "block");

        // Assert
        Assert.AreEqual("block", layout.Source);
    }

    /// <summary>
    /// Verifies that the <see cref="TokenLayout.Source"/> property defaults to <see langword="null"/>
    /// when not explicitly provided.
    /// </summary>
    [TestMethod]
    public void DefaultSourceIsNull()
    {
        // Arrange & Act
        var layout = new TokenLayout(0);

        // Assert
        Assert.IsNull(layout.Source);
    }

    /// <summary>
    /// Verifies that two <see cref="TokenLayout"/> instances with the same values are considered equal.
    /// </summary>
    [TestMethod]
    public void EqualityWithSameValuesReturnsTrue()
    {
        // Arrange
        var a = new TokenLayout(4, "align");
        var b = new TokenLayout(4, "align");

        // Act & Assert
        Assert.AreEqual(a, b);
    }

    /// <summary>
    /// Verifies that two <see cref="TokenLayout"/> instances with different values are not equal.
    /// </summary>
    [TestMethod]
    public void InequalityWithDifferentValuesReturnsTrue()
    {
        // Arrange
        var a = new TokenLayout(4, "align");
        var b = new TokenLayout(8, "block");

        // Act & Assert
        Assert.AreNotEqual(a, b);
    }

    #endregion // Methods
}