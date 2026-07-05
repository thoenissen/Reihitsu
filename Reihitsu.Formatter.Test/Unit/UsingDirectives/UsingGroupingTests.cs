using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Pipeline.UsingDirectives;

namespace Reihitsu.Formatter.Test.Unit.UsingDirectives;

/// <summary>
/// Tests for <see cref="UsingGrouping"/>, the ordering and grouping policy half of the using-directive
/// ordering phase. These pin the canonical order and the group membership decision independently of
/// the leading-trivia reconstruction
/// </summary>
[TestClass]
public class UsingGroupingTests
{
    #region Fields

    /// <summary>
    /// Expected canonical order for the documented grouping example
    /// </summary>
    private static readonly string[] _documentedExampleOrder = ["System", "System.Linq", "MyApp.Services", "System.Math"];

    /// <summary>
    /// Expected canonical order when System namespaces sort first
    /// </summary>
    private static readonly string[] _systemNamespacesFirstOrder = ["System", "MyApp.Services"];

    /// <summary>
    /// Expected canonical order for regular, then static, then alias directives
    /// </summary>
    private static readonly string[] _regularStaticAliasOrder = ["System", "System.Math", "System.Collections"];

    /// <summary>
    /// Expected canonical order for alphabetical sorting within a group
    /// </summary>
    private static readonly string[] _alphabeticalWithinGroupOrder = ["System.Collections", "System.Text"];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Verifies that the canonical order reproduces the documented grouping example
    /// </summary>
    [TestMethod]
    public void ComputeCanonicalOrderReproducesDocumentedExample()
    {
        // Arrange
        var usingDirectives = ParseUsings("""
                                          using MyApp.Services;
                                          using System.Linq;
                                          using System;
                                          using static System.Math;
                                          """);

        // Act
        var canonical = UsingGrouping.ComputeCanonicalOrder(usingDirectives);

        // Assert
        CollectionAssert.AreEqual(_documentedExampleOrder, canonical.Select(GetName).ToList());
    }

    /// <summary>
    /// Verifies that System namespaces sort before other namespaces regardless of input order
    /// </summary>
    [TestMethod]
    public void ComputeCanonicalOrderPlacesSystemNamespacesFirst()
    {
        // Arrange
        var usingDirectives = ParseUsings("""
                                          using MyApp.Services;
                                          using System;
                                          """);

        // Act
        var canonical = UsingGrouping.ComputeCanonicalOrder(usingDirectives);

        // Assert
        CollectionAssert.AreEqual(_systemNamespacesFirstOrder, canonical.Select(GetName).ToList());
    }

    /// <summary>
    /// Verifies that regular usings sort before static usings, which sort before alias usings
    /// </summary>
    [TestMethod]
    public void ComputeCanonicalOrderSortsRegularBeforeStaticBeforeAlias()
    {
        // Arrange
        var usingDirectives = ParseUsings("""
                                          using A = System.Collections;
                                          using static System.Math;
                                          using System;
                                          """);

        // Act
        var canonical = UsingGrouping.ComputeCanonicalOrder(usingDirectives);

        // Assert
        CollectionAssert.AreEqual(_regularStaticAliasOrder, canonical.Select(GetName).ToList());
    }

    /// <summary>
    /// Verifies that directives within the same group sort alphabetically
    /// </summary>
    [TestMethod]
    public void ComputeCanonicalOrderSortsAlphabeticallyWithinGroup()
    {
        // Arrange
        var usingDirectives = ParseUsings("""
                                          using System.Text;
                                          using System.Collections;
                                          """);

        // Act
        var canonical = UsingGrouping.ComputeCanonicalOrder(usingDirectives);

        // Assert
        CollectionAssert.AreEqual(_alphabeticalWithinGroupOrder, canonical.Select(GetName).ToList());
    }

    /// <summary>
    /// Verifies that two regular usings sharing a root namespace are in the same group
    /// </summary>
    [TestMethod]
    public void AreInSameGroupReturnsTrueForSameRootNamespace()
    {
        // Arrange
        var usingDirectives = ParseUsings("""
                                          using System;
                                          using System.Linq;
                                          """);

        // Act & Assert
        Assert.IsTrue(UsingGrouping.AreInSameGroup(usingDirectives[0], usingDirectives[1]));
    }

    /// <summary>
    /// Verifies that two regular usings with different root namespaces are in different groups
    /// </summary>
    [TestMethod]
    public void AreInSameGroupReturnsFalseForDifferentRootNamespace()
    {
        // Arrange
        var usingDirectives = ParseUsings("""
                                          using System;
                                          using MyApp.Services;
                                          """);

        // Act & Assert
        Assert.IsFalse(UsingGrouping.AreInSameGroup(usingDirectives[0], usingDirectives[1]));
    }

    /// <summary>
    /// Verifies that a regular using and a static using sharing a root namespace are in different groups
    /// </summary>
    [TestMethod]
    public void AreInSameGroupReturnsFalseWhenUsingTypeDiffers()
    {
        // Arrange
        var usingDirectives = ParseUsings("""
                                          using System;
                                          using static System.Math;
                                          """);

        // Act & Assert
        Assert.IsFalse(UsingGrouping.AreInSameGroup(usingDirectives[0], usingDirectives[1]));
    }

    /// <summary>
    /// Gets the rendered name of a using directive
    /// </summary>
    /// <param name="usingDirective">Using directive</param>
    /// <returns>The rendered name</returns>
    private static string GetName(UsingDirectiveSyntax usingDirective)
    {
        return usingDirective.Name?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Parses the given source and returns the compilation-unit using directives
    /// </summary>
    /// <param name="code">The C# code to parse</param>
    /// <returns>The using directives</returns>
    private SyntaxList<UsingDirectiveSyntax> ParseUsings(string code)
    {
        var tree = CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.CancellationToken);
        var root = (CompilationUnitSyntax)tree.GetRoot(TestContext.CancellationToken);

        return root.Usings;
    }

    #endregion // Methods
}