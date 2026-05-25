using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="SyntaxTreeRegionSearcher"/>
/// </summary>
[TestClass]
public class SyntaxTreeRegionSearcherTests
{
    #region Constants

    /// <summary>
    /// Nested region source used by the region searcher tests
    /// </summary>
    private const string RegionSource = """
                                        internal class Sample
                                        {
                                            #region Outer
                                            #region Inner
                                            private void Run()
                                            {
                                            }
                                            #endregion // Inner
                                            #endregion // Outer
                                        }
                                        """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that the searcher resolves the matching region for a nested endregion directive
    /// </summary>
    [TestMethod]
    public void SearchRegionPairReturnsMatchingRegionForNestedEndRegion()
    {
        var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit(RegionSource);
        var endRegionDirective = GetDirectiveTrivia(syntaxRoot, SyntaxKind.EndRegionDirectiveTrivia, "Inner");
        var searcher = new SyntaxTreeRegionSearcher();

        var result = searcher.SearchRegionPair(endRegionDirective.Token, endRegionDirective, out var matchingRegionDirective);

        Assert.IsTrue(result);
        Assert.IsTrue(matchingRegionDirective.HasValue);
        Assert.AreEqual("Inner", RegionDirectiveUtilities.GetRegionDescription((RegionDirectiveTriviaSyntax)matchingRegionDirective.Value.GetStructure()));
    }

    /// <summary>
    /// Verifies that only endregion directives are accepted as the search start point
    /// </summary>
    [TestMethod]
    public void SearchRegionPairThrowsForNonEndRegionDirective()
    {
        var syntaxRoot = CoreSyntaxTestHelper.ParseCompilationUnit(RegionSource);
        var regionDirective = GetDirectiveTrivia(syntaxRoot, SyntaxKind.RegionDirectiveTrivia, "Outer");
        var searcher = new SyntaxTreeRegionSearcher();

        Assert.ThrowsExactly<NotSupportedException>(() => searcher.SearchRegionPair(regionDirective.Token, regionDirective, out _));
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