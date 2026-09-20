using System.Reflection;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

namespace Reihitsu.Formatter.Test.Unit.LineBreaks;

/// <summary>
/// Tests for <see cref="PropertyLayoutLineBreakRewriter"/>
/// </summary>
[TestClass]
public class PropertyLayoutLineBreakRewriterTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the private collapse helper returns the input node unchanged for a property with no
    /// expression body, rather than <see langword="null"/>. The rewriter's only call site already guards this
    /// with <c>node.ExpressionBody != null</c>, so the case is unreachable through formatting today; this test
    /// exercises the helper directly because a rewriter treats a <see langword="null"/> return as node removal,
    /// and a future call site that loses the guard must not delete the property
    /// </summary>
    [TestMethod]
    public void CollapseExpressionBodiedPropertyReturnsNodeForAccessorListProperty()
    {
        // Arrange
        const string source = """
                              internal class Example
                              {
                                  public int Value { get; set; }
                              }
                              """;
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);
        var property = tree.GetRoot(TestContext.CancellationToken)
                           .DescendantNodes()
                           .OfType<PropertyDeclarationSyntax>()
                           .First();

        Assert.IsNull(property.ExpressionBody, "The fixture property must have no expression body for this boundary to be meaningful.");

        var method = typeof(PropertyLayoutLineBreakRewriter).GetMethod("CollapseExpressionBodiedProperty", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsNotNull(method, "The private collapse helper must still exist under this name for the reflection call to be meaningful.");

        // Act
        var result = (PropertyDeclarationSyntax)method.Invoke(null, [property]);

        // Assert
        Assert.IsNotNull(result, "An accessor-list property must be returned unchanged, not deleted.");
        Assert.AreEqual(property, result, "The property passed in must be returned as-is when it has no expression body.");
    }

    #endregion // Methods
}