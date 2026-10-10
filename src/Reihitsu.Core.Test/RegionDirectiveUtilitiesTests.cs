using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    /// Verifies that a matched inner pair is not promoted to a top-level pair when its outer opening is unmatched
    /// </summary>
    [TestMethod]
    public void GetTopLevelRegionsDoesNotPromoteInnerPairWithinUnmatchedOuterRegion()
    {
        var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                   internal class Sample
                                                                   {
                                                                       #region Outer
                                                                       #region Inner
                                                                       #endregion // Inner
                                                                   }
                                                                   """);
        var typeDeclaration = syntaxRoot.DescendantNodes()
                                        .OfType<ClassDeclarationSyntax>()
                                        .Single();

        var allPairs = RegionDirectiveUtilities.GetRegionPairs(typeDeclaration, includeDirective: null);
        var topLevelPairs = RegionDirectiveUtilities.GetTopLevelRegions(typeDeclaration);

        Assert.HasCount(1, allPairs);
        Assert.AreEqual("Inner", RegionDirectiveUtilities.GetRegionDescription((RegionDirectiveTriviaSyntax)allPairs[0].Region.GetStructure()));
        Assert.IsEmpty(topLevelPairs);
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

    /// <summary>
    /// Verifies that sequential lookups resolve independently, guarding against shared search state
    /// </summary>
    [TestMethod]
    public void TryFindMatchingDirectiveResolvesSequentialLookupsIndependently()
    {
        var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit(RegionSource);
        var innerEndRegion = GetDirectiveTrivia(syntaxRoot, SyntaxKind.EndRegionDirectiveTrivia, "Inner");
        var outerEndRegion = GetDirectiveTrivia(syntaxRoot, SyntaxKind.EndRegionDirectiveTrivia, "Outer");

        var innerResult = RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, innerEndRegion, out var innerMatch);
        var outerResult = RegionDirectiveUtilities.TryFindMatchingDirective(syntaxRoot, outerEndRegion, out var outerMatch);

        Assert.IsTrue(innerResult);
        Assert.IsTrue(outerResult);
        Assert.AreEqual("Inner", RegionDirectiveUtilities.GetRegionDescription((RegionDirectiveTriviaSyntax)innerMatch.GetStructure()));
        Assert.AreEqual("Outer", RegionDirectiveUtilities.GetRegionDescription((RegionDirectiveTriviaSyntax)outerMatch.GetStructure()));
    }

    /// <summary>
    /// Verifies that the shared pair enumerator uses LIFO matching, returns pairs in closing traversal order,
    /// includes inactive directives, and omits unmatched directives
    /// </summary>
    [TestMethod]
    public void GetRegionPairsReturnsMatchedPairsInClosingTraversalOrder()
    {
        var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                   #endregion // OrphanEnd

                                                                   #region First
                                                                   #endregion // First

                                                                   #region Outer
                                                                   #region Inner
                                                                   #endregion // Inner
                                                                   #endregion // Outer

                                                                   #if false
                                                                   #region Inactive
                                                                   #endregion // Inactive
                                                                   #endif

                                                                   #region OrphanStart
                                                                   """);

        var pairs = RegionDirectiveUtilities.GetRegionPairs(syntaxRoot, includeDirective: null);
        var descriptions = pairs.Select(pair => $"{GetDescription(pair.Region)}->{GetDescription(pair.EndRegion)}")
                                .ToArray();

        Assert.AreSequenceEqual([
                                    "First->First",
                                    "Inner->Inner",
                                    "Outer->Outer",
                                    "Inactive->Inactive"
                                ],
                                descriptions);
    }

    /// <summary>
    /// Verifies that directive inclusion is evaluated before LIFO pairing rather than filtering completed pairs
    /// </summary>
    [TestMethod]
    public void GetRegionPairsAppliesInclusionBeforePairing()
    {
        var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                   #region Outer
                                                                   #region Excluded
                                                                   #endregion // FirstIncludedEnd
                                                                   #endregion // Outer
                                                                   """);

        var pairs = RegionDirectiveUtilities.GetRegionPairs(syntaxRoot,
                                                            directive => directive.GetStructure() is not RegionDirectiveTriviaSyntax regionDirective
                                                                         || RegionDirectiveUtilities.GetRegionDescription(regionDirective) != "Excluded");

        Assert.HasCount(1, pairs);
        Assert.AreEqual("Outer", GetDescription(pairs[0].Region));
        Assert.AreEqual("FirstIncludedEnd", GetDescription(pairs[0].EndRegion));
    }

    /// <summary>
    /// Verifies that the canonical endregion rewrite preserves exterior trivia and line endings and is idempotent
    /// under LF and CRLF
    /// </summary>
    [TestMethod]
    public void CreateCanonicalEndRegionTriviaPreservesExteriorTriviaAndLineEndings()
    {
        const string source = """
                              internal class Sample
                              {
                                  #region Name
                                  #endregion Wrong
                              }
                              """;
        const string expected = """
                                internal class Sample
                                {
                                    #region Name
                                    #endregion // Name
                                }
                                """;

        foreach (var endOfLine in new[] { "\n", "\r\n" })
        {
            var normalizedSource = NormalizeLineEndings(source, endOfLine);
            var normalizedExpected = NormalizeLineEndings(expected, endOfLine);
            var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit(normalizedSource);
            var endRegion = syntaxRoot.DescendantTrivia(descendIntoTrivia: true)
                                      .Single(trivia => trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia));
            var replacement = RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia((EndRegionDirectiveTriviaSyntax)endRegion.GetStructure(), "Name");
            var rewritten = syntaxRoot.ReplaceTrivia(endRegion, replacement).ToFullString();

            Assert.AreEqual(normalizedExpected, rewritten);

            var rewrittenRoot = CoreSyntaxTestHelper.ParseCompilationUnit(rewritten);
            var rewrittenEndRegion = rewrittenRoot.DescendantTrivia(descendIntoTrivia: true)
                                                  .Single(trivia => trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia));
            var secondReplacement = RegionDirectiveUtilities.CreateCanonicalEndRegionTrivia((EndRegionDirectiveTriviaSyntax)rewrittenEndRegion.GetStructure(), "Name");

            Assert.AreEqual(rewritten, rewrittenRoot.ReplaceTrivia(rewrittenEndRegion, secondReplacement).ToFullString());
        }
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
    /// Gets the normalized description of a region or endregion trivia
    /// </summary>
    /// <param name="directiveTrivia">Region directive trivia</param>
    /// <returns>Normalized description</returns>
    private static string GetDescription(SyntaxTrivia directiveTrivia)
    {
        return directiveTrivia.GetStructure() switch
               {
                   RegionDirectiveTriviaSyntax regionDirective => RegionDirectiveUtilities.GetRegionDescription(regionDirective),
                   EndRegionDirectiveTriviaSyntax endRegionDirective => RegionDirectiveUtilities.GetEndRegionDescription(endRegionDirective),
                   _ => string.Empty,
               };
    }

    /// <summary>
    /// Normalizes source text to the requested line ending
    /// </summary>
    /// <param name="text">Source text</param>
    /// <param name="endOfLine">Requested line ending</param>
    /// <returns>Normalized source text</returns>
    private static string NormalizeLineEndings(string text, string endOfLine)
    {
        var lineFeedOnly = text.Replace("\r\n", "\n");

        return endOfLine == "\n" ? lineFeedOnly : lineFeedOnly.Replace("\n", endOfLine);
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