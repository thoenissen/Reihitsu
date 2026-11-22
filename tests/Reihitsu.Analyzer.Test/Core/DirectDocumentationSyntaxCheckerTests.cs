using System.Linq;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Test.Core;

/// <summary>
/// Unit tests for <see cref="DirectDocumentationSyntaxChecker"/>
/// </summary>
[TestClass]
public class DirectDocumentationSyntaxCheckerTests
{
    #region Tests

    /// <summary>
    /// Verifies that direct matches are returned in source order without nested matches
    /// </summary>
    [TestMethod]
    public void GetTagsIncludingNestedReturnsOnlyOrderedDirectMatchesWhenPresent()
    {
        const string source = """
                              /// <remarks><param name="nested">Nested.</param></remarks>
                              /// <param name="first">First.</param>
                              /// <param name="second">Second.</param>
                              class Sample
                              {
                              }
                              """;

        var documentationComment = GetDocumentationComment(source);
        var matches = DirectDocumentationSyntaxChecker.GetTagsIncludingNested(documentationComment, "param");

        Assert.AreSequenceEqual(["first", "second"], matches.Select(DocumentationAnalysisUtilities.GetNameAttributeValue).ToArray());
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the documentation comment for the class in the provided source
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The documentation comment</returns>
    private static DocumentationCommentTriviaSyntax GetDocumentationComment(string source)
    {
        var declaration = CSharpSyntaxTree.ParseText(source)
                                          .GetRoot()
                                          .DescendantNodes()
                                          .OfType<ClassDeclarationSyntax>()
                                          .Single();

        return DirectDocumentationSyntaxChecker.GetDocumentationComment(declaration);
    }

    #endregion // Methods
}