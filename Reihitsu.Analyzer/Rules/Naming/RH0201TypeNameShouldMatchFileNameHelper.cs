using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Shared filename logic for RH0201
/// </summary>
internal static class RH0201TypeNameShouldMatchFileNameHelper
{
    /// <summary>
    /// Gets the comparable filename part used by RH0201
    /// </summary>
    /// <param name="filePath">The document file path</param>
    /// <returns>The filename segment before the first dot and without the final extension</returns>
    internal static string GetComparableFileName(string filePath)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var firstDotIndex = fileNameWithoutExtension.IndexOf('.');

        return firstDotIndex >= 0 ? fileNameWithoutExtension.Substring(0, firstDotIndex) : fileNameWithoutExtension;
    }

    /// <summary>
    /// Gets the expected filename stem for the given type declaration, formatting generic type parameters with curly braces
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>Expected filename stem without extension</returns>
    internal static string GetExpectedFileNameStem(MemberDeclarationSyntax typeDeclaration)
    {
        var typeName = GetIdentifier(typeDeclaration).Text;

        var typeParameterList = typeDeclaration switch
                                {
                                    TypeDeclarationSyntax typeSyntax => typeSyntax.TypeParameterList,
                                    DelegateDeclarationSyntax delegateSyntax => delegateSyntax.TypeParameterList,
                                    _ => null
                                };

        if (typeParameterList is { Parameters.Count: > 0 })
        {
            typeName += "{" + string.Join(",", typeParameterList.Parameters.Select(parameter => parameter.Identifier.Text)) + "}";
        }

        return typeName;
    }

    /// <summary>
    /// Builds the renamed file name while preserving everything after the first dot of the original file name
    /// </summary>
    /// <param name="originalFilePath">Original document file path</param>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The new file name including its extension</returns>
    internal static string GetRenamedFileName(string originalFilePath, MemberDeclarationSyntax typeDeclaration)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilePath);
        var firstDotIndex = fileNameWithoutExtension.IndexOf('.');
        var suffix = firstDotIndex >= 0 ? fileNameWithoutExtension.Substring(firstDotIndex) : string.Empty;
        var extension = Path.GetExtension(originalFilePath);

        return GetExpectedFileNameStem(typeDeclaration) + suffix + extension;
    }

    /// <summary>
    /// Gets the identifier token from a type declaration
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The identifier token</returns>
    private static SyntaxToken GetIdentifier(MemberDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseType => baseType.Identifier,
                   DelegateDeclarationSyntax delegateType => delegateType.Identifier,
                   _ => default
               };
    }
}