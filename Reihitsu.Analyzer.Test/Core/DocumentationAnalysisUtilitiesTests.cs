using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Test.Core;

/// <summary>
/// Unit tests for <see cref="DocumentationAnalysisUtilities"/>
/// </summary>
[TestClass]
public class DocumentationAnalysisUtilitiesTests
{
    #region Tests

    /// <summary>
    /// Verifies that direct summary tags are accepted without expanded XML parsing for members
    /// </summary>
    [TestMethod]
    public void HasRequiredDocumentationReturnsTrueForMemberWithDirectSummaryWithoutSemanticModel()
    {
        const string source = """
                              class Sample
                              {
                                  /// <summary>Summary</summary>
                                  public void Execute()
                                  {
                                  }
                              }
                              """;

        var declaration = GetSingleMethodDeclaration(source);
        var hasRequiredDocumentation = DocumentationAnalysisUtilities.HasRequiredDocumentation(declaration, semanticModel: null, CancellationToken.None);

        Assert.IsTrue(hasRequiredDocumentation);
    }

    /// <summary>
    /// Verifies that direct inheritdoc tags are accepted without expanded XML parsing for members
    /// </summary>
    [TestMethod]
    public void HasRequiredDocumentationReturnsTrueForMemberWithDirectInheritdocWithoutSemanticModel()
    {
        const string source = """
                              class Sample
                              {
                                  /// <inheritdoc/>
                                  public void Execute()
                                  {
                                  }
                              }
                              """;

        var declaration = GetSingleMethodDeclaration(source);
        var hasRequiredDocumentation = DocumentationAnalysisUtilities.HasRequiredDocumentation(declaration, semanticModel: null, CancellationToken.None);

        Assert.IsTrue(hasRequiredDocumentation);
    }

    /// <summary>
    /// Verifies that direct summary tags are accepted without expanded XML parsing for enum members
    /// </summary>
    [TestMethod]
    public void HasRequiredDocumentationReturnsTrueForEnumMemberWithDirectSummaryWithoutSemanticModel()
    {
        const string source = """
                              enum Sample
                              {
                                  /// <summary>Summary</summary>
                                  FirstValue
                              }
                              """;

        var declaration = GetSingleEnumMemberDeclaration(source);
        var hasRequiredDocumentation = DocumentationAnalysisUtilities.HasRequiredDocumentation(declaration, semanticModel: null, CancellationToken.None);

        Assert.IsTrue(hasRequiredDocumentation);
    }

    /// <summary>
    /// Verifies that direct inheritdoc tags are accepted without expanded XML parsing for enum members
    /// </summary>
    [TestMethod]
    public void HasRequiredDocumentationReturnsTrueForEnumMemberWithDirectInheritdocWithoutSemanticModel()
    {
        const string source = """
                              enum Sample
                              {
                                  /// <inheritdoc/>
                                  FirstValue
                              }
                              """;

        var declaration = GetSingleEnumMemberDeclaration(source);
        var hasRequiredDocumentation = DocumentationAnalysisUtilities.HasRequiredDocumentation(declaration, semanticModel: null, CancellationToken.None);

        Assert.IsTrue(hasRequiredDocumentation);
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the single method declaration from the provided source
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The method declaration</returns>
    private static MethodDeclarationSyntax GetSingleMethodDeclaration(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        return syntaxTree.GetRoot()
                         .DescendantNodes()
                         .OfType<MethodDeclarationSyntax>()
                         .Single();
    }

    /// <summary>
    /// Gets the single enum member declaration from the provided source
    /// </summary>
    /// <param name="source">Source text</param>
    /// <returns>The enum member declaration</returns>
    private static EnumMemberDeclarationSyntax GetSingleEnumMemberDeclaration(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        return syntaxTree.GetRoot()
                         .DescendantNodes()
                         .OfType<EnumMemberDeclarationSyntax>()
                         .Single();
    }

    #endregion // Methods
}