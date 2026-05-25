using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="RegionDirectiveUtilities"/>
/// </summary>
[TestClass]
public class RegionDirectiveUtilitiesTests
{
    #region Constants

    /// <summary>
    /// Region-heavy source used by the region utility tests
    /// </summary>
    private const string RegionSource = """
                                        internal class Sample
                                        {
                                            #region Outer
                                            private void First()
                                            {
                                            }

                                            #region Inner
                                            private void Second()
                                            {
                                            }
                                            #endregion // Inner
                                            #endregion // Outer

                                            private void Body()
                                            {
                                                #region InBody
                                                var value = 0;
                                                #endregion // InBody
                                            }

                                            private class Nested
                                            {
                                                #region NestedType
                                                private void Third()
                                                {
                                                }
                                                #endregion // NestedType
                                            }
                                        }
                                        """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that region descriptions are normalized consistently
    /// </summary>
    [TestMethod]
    public void DescriptionHelpersReturnTrimmedDescriptions()
    {
        var regionDirective = (RegionDirectiveTriviaSyntax)GetDirectiveTrivia(RegionSource, SyntaxKind.RegionDirectiveTrivia, "Outer").GetStructure();
        var endRegionDirective = (EndRegionDirectiveTriviaSyntax)GetDirectiveTrivia(RegionSource, SyntaxKind.EndRegionDirectiveTrivia, "Outer").GetStructure();

        Assert.AreEqual("Outer", RegionDirectiveUtilities.GetRegionDescription(regionDirective));
        Assert.AreEqual("Outer", RegionDirectiveUtilities.GetEndRegionDescription(endRegionDirective));
    }

    /// <summary>
    /// Verifies that only method-body regions are treated as element-body directives
    /// </summary>
    [TestMethod]
    public void IsWithinElementBodyDistinguishesTypeAndMethodBodyRegions()
    {
        var typeRegion = GetDirectiveTrivia(RegionSource, SyntaxKind.RegionDirectiveTrivia, "Outer");
        var bodyRegion = GetDirectiveTrivia(RegionSource, SyntaxKind.RegionDirectiveTrivia, "InBody");

        Assert.IsFalse(RegionDirectiveUtilities.IsWithinElementBody(typeRegion));
        Assert.IsTrue(RegionDirectiveUtilities.IsWithinElementBody(bodyRegion));
    }

    /// <summary>
    /// Verifies that only top-level regions owned by the current type are returned
    /// </summary>
    [TestMethod]
    public void GetTopLevelRegionsReturnsOnlyOuterRegionsOwnedByTheType()
    {
        var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit(RegionSource);
        var typeDeclaration = syntaxRoot.DescendantNodes()
                                        .OfType<ClassDeclarationSyntax>()
                                        .Single(obj => obj.Identifier.ValueText == "Sample" && obj.Parent is CompilationUnitSyntax);

        var regions = RegionDirectiveUtilities.GetTopLevelRegions(typeDeclaration);

        Assert.HasCount(1, regions);
        Assert.AreEqual("Outer", RegionDirectiveUtilities.GetRegionDescription((RegionDirectiveTriviaSyntax)regions[0].Region.GetStructure()));
    }

    /// <summary>
    /// Verifies that members inside a top-level region are recognized correctly
    /// </summary>
    [TestMethod]
    public void RegionContainmentHelpersFindContainingTopLevelRegion()
    {
        var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit(RegionSource);
        var typeDeclaration = syntaxRoot.DescendantNodes()
                                        .OfType<ClassDeclarationSyntax>()
                                        .Single(obj => obj.Identifier.ValueText == "Sample" && obj.Parent is CompilationUnitSyntax);
        var secondMethod = typeDeclaration.Members.OfType<MethodDeclarationSyntax>().Single(obj => obj.Identifier.ValueText == "Second");
        var regions = RegionDirectiveUtilities.GetTopLevelRegions(typeDeclaration);

        var hasContainingRegion = RegionDirectiveUtilities.TryFindContainingRegion(secondMethod, regions, out var region);

        Assert.IsTrue(hasContainingRegion);
        Assert.IsTrue(RegionDirectiveUtilities.IsWithinRegion(secondMethod, regions));
        Assert.AreEqual("Outer", RegionDirectiveUtilities.GetRegionDescription((RegionDirectiveTriviaSyntax)region.Region.GetStructure()));
    }

    /// <summary>
    /// Verifies that matching region directives can be located in nested region structures
    /// </summary>
    [TestMethod]
    public void TryFindMatchingDirectiveFindsMatchingNestedRegion()
    {
        var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit(RegionSource);
        var endRegionDirective = GetDirectiveTrivia(syntaxRoot, SyntaxKind.EndRegionDirectiveTrivia, "Inner");

        var result = RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, endRegionDirective, out var matchingDirective);

        Assert.IsTrue(result);
        Assert.AreEqual("Inner", RegionDirectiveUtilities.GetRegionDescription((RegionDirectiveTriviaSyntax)matchingDirective.GetStructure()));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the directive trivia with the requested kind and description
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="syntaxKind">Directive kind</param>
    /// <param name="description">Directive description</param>
    /// <returns>The matching directive trivia</returns>
    private static SyntaxTrivia GetDirectiveTrivia(string source, SyntaxKind syntaxKind, string description)
    {
        return GetDirectiveTrivia(CoreSyntaxTestHelper.ParseCompilationUnit(source), syntaxKind, description);
    }

    /// <summary>
    /// Gets the directive trivia with the requested kind and description from the provided syntax root
    /// </summary>
    /// <param name="syntaxRoot">Syntax root</param>
    /// <param name="syntaxKind">Directive kind</param>
    /// <param name="description">Directive description</param>
    /// <returns>The matching directive trivia</returns>
    private static SyntaxTrivia GetDirectiveTrivia(SyntaxNode syntaxRoot, SyntaxKind syntaxKind, string description)
    {
        return syntaxRoot.DescendantTrivia(descendIntoTrivia: true)
                         .Where(obj => obj.IsKind(syntaxKind))
                         .Single(obj => MatchesDirectiveDescription(obj, description));
    }

    /// <summary>
    /// Determines whether the directive matches the requested description
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="description">Requested description</param>
    /// <returns><see langword="true"/> if the description matches</returns>
    private static bool MatchesDirectiveDescription(SyntaxTrivia directiveTrivia, string description)
    {
        return directiveTrivia.GetStructure() switch
               {
                   RegionDirectiveTriviaSyntax regionDirective => RegionDirectiveUtilities.GetRegionDescription(regionDirective) == description,
                   EndRegionDirectiveTriviaSyntax endRegionDirective => RegionDirectiveUtilities.GetEndRegionDescription(endRegionDirective) == description,
                   _ => false,
               };
    }

    #endregion // Methods
}