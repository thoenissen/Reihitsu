using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.Core.Utilities;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Tests for <see cref="TokenContainmentUtilities"/>
/// </summary>
[TestClass]
public class TokenContainmentUtilitiesTests
{
    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that a token owned by a node is reported as contained
    /// </summary>
    [TestMethod]
    public void ContainsTokenReturnsTrueForOwnedToken()
    {
        // Arrange
        var declaration = ParseClassDeclaration("class C { }");

        // Act
        var result = TokenContainmentUtilities.ContainsToken(declaration, declaration.Identifier);

        // Assert
        Assert.IsTrue(result, "A token owned by the node should be reported as contained.");
    }

    /// <summary>
    /// Verifies that a default token is not reported as contained
    /// </summary>
    [TestMethod]
    public void ContainsTokenReturnsFalseForDefaultToken()
    {
        // Arrange
        var declaration = ParseClassDeclaration("class C { }");

        // Act
        var result = TokenContainmentUtilities.ContainsToken(declaration, default);

        // Assert
        Assert.IsFalse(result, "A default token should not be reported as contained.");
    }

    /// <summary>
    /// Parses the source and returns its first class declaration
    /// </summary>
    /// <param name="source">The source text</param>
    /// <returns>The class declaration</returns>
    private ClassDeclarationSyntax ParseClassDeclaration(string source)
    {
        var tree = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.CancellationToken);

        return tree.GetRoot(TestContext.CancellationToken)
                   .DescendantNodes()
                   .OfType<ClassDeclarationSyntax>()
                   .First();
    }

    #endregion // Methods
}