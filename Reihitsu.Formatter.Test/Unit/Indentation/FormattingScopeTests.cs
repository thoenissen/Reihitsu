using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Indentation;

namespace Reihitsu.Formatter.Test.Unit.Indentation;

/// <summary>
/// Tests for <see cref="FormattingScope"/>
/// </summary>
[TestClass]
public class FormattingScopeTests
{
    #region Methods

    /// <summary>
    /// Verifies that the constructor correctly assigns all properties.
    /// </summary>
    [TestMethod]
    public void ConstructorSetsProperties()
    {
        // Arrange
        var parent = new FormattingScope(0);

        // Act
        var scope = new FormattingScope(8, ScopeKind.Continuation, parent);

        // Assert
        Assert.AreEqual(8, scope.BaseColumn);
        Assert.AreEqual(ScopeKind.Continuation, scope.Kind);
        Assert.AreSame(parent, scope.Parent);
    }

    /// <summary>
    /// Verifies that <see cref="FormattingScope.CreateChild"/> returns a scope whose
    /// <see cref="FormattingScope.Parent"/> points to the calling scope.
    /// </summary>
    [TestMethod]
    public void CreateChildSetsParent()
    {
        // Arrange
        var root = new FormattingScope(0);

        // Act
        var child = root.CreateChild(4);

        // Assert
        Assert.AreSame(root, child.Parent);
    }

    /// <summary>
    /// Verifies that <see cref="FormattingScope.CreateChild"/> assigns the correct base column
    /// to the child scope.
    /// </summary>
    [TestMethod]
    public void CreateChildSetsBaseColumn()
    {
        // Arrange
        var root = new FormattingScope(0);

        // Act
        var child = root.CreateChild(12, ScopeKind.LambdaBody);

        // Assert
        Assert.AreEqual(12, child.BaseColumn);
        Assert.AreEqual(ScopeKind.LambdaBody, child.Kind);
    }

    /// <summary>
    /// Verifies that a root scope created without a parent has a <see langword="null"/>
    /// <see cref="FormattingScope.Parent"/> property.
    /// </summary>
    [TestMethod]
    public void RootScopeHasNullParent()
    {
        // Arrange & Act
        var root = new FormattingScope(0);

        // Assert
        Assert.IsNull(root.Parent);
    }

    /// <summary>
    /// Verifies that nested scopes form a correct parent chain by creating a three-level
    /// hierarchy and walking from child to grandparent.
    /// </summary>
    [TestMethod]
    public void NestedScopeChainsCorrectly()
    {
        // Arrange
        var root = new FormattingScope(0);
        var child = root.CreateChild(4);

        // Act
        var grandchild = child.CreateChild(8, ScopeKind.Initializer);

        // Assert
        Assert.AreSame(child, grandchild.Parent);
        Assert.AreSame(root, grandchild.Parent.Parent);
        Assert.IsNull(grandchild.Parent.Parent.Parent);
    }

    /// <summary>
    /// Verifies that the default value of <see cref="FormattingScope.Kind"/> is
    /// <see cref="ScopeKind.Block"/> when no kind is explicitly provided.
    /// </summary>
    [TestMethod]
    public void DefaultKindIsBlock()
    {
        // Arrange & Act
        var scope = new FormattingScope(4);

        // Assert
        Assert.AreEqual(ScopeKind.Block, scope.Kind);
    }

    #endregion // Methods
}