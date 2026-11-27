using System.IO;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Shared filename logic for RH4001
/// </summary>
internal static class RH4001TypeNameShouldMatchFileNameHelper
{
    #region Methods

    /// <summary>
    /// Gets the comparable filename part used by RH4001
    /// </summary>
    /// <param name="filePath">The document file path</param>
    /// <returns>The filename segment before the first dot, without any EF migration timestamp prefix, and without the final extension</returns>
    internal static string GetComparableFileName(string filePath)
    {
        var stem = GetStem(Path.GetFileNameWithoutExtension(filePath));

        return stem.Substring(GetTimestampPrefixLength(stem));
    }

    /// <summary>
    /// Gets the expected filename stem for the given type declaration, formatting generic type parameters with curly braces.
    /// The type name and the type-parameter names are their values: without a verbatim <c>@</c> prefix and with every Unicode escape sequence replaced
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>Expected filename stem without extension</returns>
    internal static string GetExpectedFileNameStem(MemberDeclarationSyntax typeDeclaration)
    {
        var typeName = GetIdentifier(typeDeclaration).ValueText;

        var typeParameterList = typeDeclaration switch
                                {
                                    TypeDeclarationSyntax typeSyntax => typeSyntax.TypeParameterList,
                                    DelegateDeclarationSyntax delegateSyntax => delegateSyntax.TypeParameterList,
                                    _ => null
                                };

        if (typeParameterList is { Parameters.Count: > 0 })
        {
            typeName = $"{typeName}{{{string.Join(",", typeParameterList.Parameters.Select(parameter => parameter.Identifier.ValueText))}}}";
        }

        return typeName;
    }

    /// <summary>
    /// Builds the renamed file name while preserving an Entity Framework migration timestamp prefix of the original file name,
    /// as written, and everything after its first dot. The prefix is the one <see cref="GetComparableFileName"/> removes for the comparison
    /// </summary>
    /// <param name="originalFilePath">Original document file path</param>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The new file name including its extension</returns>
    internal static string GetRenamedFileName(string originalFilePath, MemberDeclarationSyntax typeDeclaration)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilePath);
        var stem = GetStem(fileNameWithoutExtension);
        var prefix = stem.Substring(0, GetTimestampPrefixLength(stem));
        var suffix = fileNameWithoutExtension.Substring(stem.Length);
        var extension = Path.GetExtension(originalFilePath);

        return $"{prefix}{GetExpectedFileNameStem(typeDeclaration)}{suffix}{extension}";
    }

    /// <summary>
    /// Gets the stem of a file name: the part in front of its first dot, or the whole name when it holds no dot
    /// </summary>
    /// <param name="fileNameWithoutExtension">File name without its final extension</param>
    /// <returns>The stem</returns>
    private static string GetStem(string fileNameWithoutExtension)
    {
        var firstDotIndex = fileNameWithoutExtension.IndexOf('.');

        return firstDotIndex >= 0 ? fileNameWithoutExtension.Substring(0, firstDotIndex) : fileNameWithoutExtension;
    }

    /// <summary>
    /// Gets the length of an Entity Framework migration timestamp prefix (14 digits followed by an underscore) at the start of the stem
    /// </summary>
    /// <param name="stem">Filename stem to inspect</param>
    /// <returns>The length of the prefix including its underscore, or 0 when the stem does not start with one</returns>
    private static int GetTimestampPrefixLength(string stem)
    {
        const int timestampLength = 14;

        if (stem.Length > timestampLength && stem[timestampLength] == '_')
        {
            for (var index = 0; index < timestampLength; index++)
            {
                if (char.IsDigit(stem[index]) == false)
                {
                    return 0;
                }
            }

            return timestampLength + 1;
        }

        return 0;
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

    #endregion // Methods
}