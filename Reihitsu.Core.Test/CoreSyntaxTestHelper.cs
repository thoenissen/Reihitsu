using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core.Test;

/// <summary>
/// Provides Roslyn parsing helpers for unit tests
/// </summary>
internal static class CoreSyntaxTestHelper
{
    #region Methods

    /// <summary>
    /// Parses the source into a compilation unit
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="languageVersion">Language version</param>
    /// <returns>The parsed compilation unit</returns>
    public static CompilationUnitSyntax ParseCompilationUnit(string source, LanguageVersion languageVersion = LanguageVersion.Latest)
    {
        return (CompilationUnitSyntax)CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(languageVersion)).GetRoot();
    }

    /// <summary>
    /// Gets the single node of the requested type from the source
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <param name="source">Source text</param>
    /// <param name="languageVersion">Language version</param>
    /// <returns>The matching node</returns>
    public static TNode GetSingleNode<TNode>(string source, LanguageVersion languageVersion = LanguageVersion.Latest)
        where TNode : SyntaxNode
    {
        return ParseCompilationUnit(source, languageVersion).DescendantNodes().OfType<TNode>().Single();
    }

    /// <summary>
    /// Gets the single type declaration from the source
    /// </summary>
    /// <param name="source">Source text</param>
    /// <param name="languageVersion">Language version</param>
    /// <returns>The type declaration</returns>
    public static TypeDeclarationSyntax GetSingleTypeDeclaration(string source, LanguageVersion languageVersion = LanguageVersion.Latest)
    {
        return ParseCompilationUnit(source, languageVersion).DescendantNodes().OfType<TypeDeclarationSyntax>().Single();
    }

    /// <summary>
    /// Gets the single member of the requested type from the source
    /// </summary>
    /// <typeparam name="TMember">Member type</typeparam>
    /// <param name="source">Source text</param>
    /// <param name="languageVersion">Language version</param>
    /// <returns>The matching member</returns>
    public static TMember GetSingleMember<TMember>(string source, LanguageVersion languageVersion = LanguageVersion.Latest)
        where TMember : MemberDeclarationSyntax
    {
        return ParseCompilationUnit(source, languageVersion).DescendantNodes().OfType<TMember>().Single();
    }

    /// <summary>
    /// Creates a simple diagnostic at the specified location
    /// </summary>
    /// <param name="location">Diagnostic location</param>
    /// <returns>The diagnostic</returns>
    public static Diagnostic CreateDiagnostic(Location location)
    {
        var descriptor = new DiagnosticDescriptor("TEST0001",
                                                  "Test",
                                                  "Test",
                                                  "Testing",
                                                  DiagnosticSeverity.Warning,
                                                  isEnabledByDefault: true);

        return Diagnostic.Create(descriptor, location);
    }

    #endregion // Methods
}