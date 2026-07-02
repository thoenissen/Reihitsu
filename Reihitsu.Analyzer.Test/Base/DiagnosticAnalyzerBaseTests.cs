using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Analyzer.Test.Base;

/// <summary>
/// Test methods for <see cref="Reihitsu.Analyzer.Base.DiagnosticAnalyzerBase{TAnalyzer}"/>
/// </summary>
[TestClass]
public class DiagnosticAnalyzerBaseTests
{
    #region Tests

    /// <summary>
    /// Verifies that a multi-location diagnostic uses the first location as the primary location
    /// </summary>
    [TestMethod]
    public void CreateDiagnosticWithMultipleLocationsUsesFirstLocationAsPrimary()
    {
        var (first, second) = CreateLocations();

        var diagnostic = new DiagnosticAnalyzerBaseProbe().Probe([first, second]);

        Assert.AreEqual(first, diagnostic.Location);
    }

    /// <summary>
    /// Verifies that a multi-location diagnostic exposes the remaining locations without duplicating the primary location
    /// </summary>
    [TestMethod]
    public void CreateDiagnosticWithMultipleLocationsKeepsRemainingLocationsWithoutDuplicatingPrimary()
    {
        var (first, second) = CreateLocations();

        var diagnostic = new DiagnosticAnalyzerBaseProbe().Probe([first, second]);

        CollectionAssert.AreEqual(new[] { second }, diagnostic.AdditionalLocations.ToArray());
    }

    /// <summary>
    /// Verifies that a single-location diagnostic uses that location as the primary location
    /// </summary>
    [TestMethod]
    public void CreateDiagnosticWithSingleLocationUsesItAsPrimary()
    {
        var (first, _) = CreateLocations();

        var diagnostic = new DiagnosticAnalyzerBaseProbe().Probe([first]);

        Assert.AreEqual(first, diagnostic.Location);
        Assert.IsEmpty(diagnostic.AdditionalLocations);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Creates two distinct locations within a single syntax tree
    /// </summary>
    /// <returns>A tuple of two distinct locations</returns>
    private static (Location First, Location Second) CreateLocations()
    {
        var tree = CSharpSyntaxTree.ParseText("class C { }");

        return (Location.Create(tree, new TextSpan(0, 5)), Location.Create(tree, new TextSpan(6, 1)));
    }

    #endregion // Methods
}