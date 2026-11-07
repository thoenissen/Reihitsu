using System.Threading;

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
    /// Determines whether a type declaration declares a static type. A partial type is static when any of its parts
    /// carries the <see langword="static"/> modifier, as the compiler builds it (SHARED-DIAG-004), so the type's symbol
    /// decides; without a symbol, the declaration's own modifiers decide
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="semanticModel">Semantic model of the declaration's tree</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True when the declared type is static</returns>
    internal static bool IsStaticType(BaseTypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        return semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) is { } typeSymbol
                   ? typeSymbol.IsStatic
                   : typeDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.StaticKeyword));
    }

    #endregion // Methods
}