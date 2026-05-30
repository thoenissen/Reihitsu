using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Helper methods for nested type analyzers
/// </summary>
internal static class NestedTypeAnalyzerHelper
{
    #region Methods

    /// <summary>
    /// Gets the identifier location of the declaration
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>Identifier location</returns>
    internal static Location GetIdentifierLocation(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.Identifier.GetLocation(),
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.GetLocation(),
                   _ => memberDeclaration.GetLocation(),
               };
    }

    /// <summary>
    /// Determines whether a declaration is nested in another type declaration
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>True when declaration is nested in another type</returns>
    internal static bool IsNestedType(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration.Parent is TypeDeclarationSyntax;
    }

    /// <summary>
    /// Determines whether a type declaration has a static modifier
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>True when static modifier is present</returns>
    internal static bool IsStaticType(BaseTypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.StaticKeyword));
    }

    #endregion // Methods
}