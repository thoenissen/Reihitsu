using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Helper methods for declaration ordering analyzers and code fixes.
/// </summary>
internal static class OrderingDeclarationUtilities
{
    #region Methods

    /// <summary>
    /// Checks whether the declaration explicitly declares accessibility.
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns><see langword="true"/> if an accessibility modifier is present</returns>
    internal static bool HasExplicitAccessibilityModifier(SyntaxTokenList modifiers)
    {
        return modifiers.Any(obj => obj.IsKind(SyntaxKind.FileKeyword)
                                    || obj.IsKind(SyntaxKind.PublicKeyword)
                                    || obj.IsKind(SyntaxKind.PrivateKeyword)
                                    || obj.IsKind(SyntaxKind.ProtectedKeyword)
                                    || obj.IsKind(SyntaxKind.InternalKeyword));
    }

    /// <summary>
    /// Gets the accessibility group of the declaration.
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>The accessibility group</returns>
    internal static OrderingAccessibilityGroup GetAccessibilityGroup(MemberDeclarationSyntax memberDeclaration)
    {
        var modifiers = DeclarationModifierUtilities.GetModifiers(memberDeclaration);
        var hasFile = modifiers.Any(SyntaxKind.FileKeyword);
        var hasPublic = modifiers.Any(SyntaxKind.PublicKeyword);
        var hasPrivate = modifiers.Any(SyntaxKind.PrivateKeyword);
        var hasProtected = modifiers.Any(SyntaxKind.ProtectedKeyword);
        var hasInternal = modifiers.Any(SyntaxKind.InternalKeyword);

        if (hasFile)
        {
            return OrderingAccessibilityGroup.File;
        }

        if (hasPublic)
        {
            return OrderingAccessibilityGroup.Public;
        }

        if (hasProtected && hasInternal)
        {
            return OrderingAccessibilityGroup.ProtectedInternal;
        }

        if (hasPrivate && hasProtected)
        {
            return OrderingAccessibilityGroup.PrivateProtected;
        }

        if (hasInternal)
        {
            return OrderingAccessibilityGroup.Internal;
        }

        if (hasProtected)
        {
            return OrderingAccessibilityGroup.Protected;
        }

        if (hasPrivate)
        {
            return OrderingAccessibilityGroup.Private;
        }

        return OrderingAccessibilityGroup.None;
    }

    /// <summary>
    /// Gets the member kind group of the declaration.
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>The member kind group</returns>
    internal static OrderingMemberKindGroup GetMemberKind(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax => OrderingMemberKindGroup.Type,
                   DelegateDeclarationSyntax => OrderingMemberKindGroup.Delegate,
                   FieldDeclarationSyntax => OrderingMemberKindGroup.Field,
                   ConstructorDeclarationSyntax => OrderingMemberKindGroup.Constructor,
                   DestructorDeclarationSyntax => OrderingMemberKindGroup.Destructor,
                   PropertyDeclarationSyntax => OrderingMemberKindGroup.Property,
                   IndexerDeclarationSyntax => OrderingMemberKindGroup.Indexer,
                   EventDeclarationSyntax => OrderingMemberKindGroup.Event,
                   EventFieldDeclarationSyntax => OrderingMemberKindGroup.EventField,
                   MethodDeclarationSyntax => OrderingMemberKindGroup.Method,
                   OperatorDeclarationSyntax => OrderingMemberKindGroup.Operator,
                   ConversionOperatorDeclarationSyntax => OrderingMemberKindGroup.ConversionOperator,
                   _ => OrderingMemberKindGroup.Unknown,
               };
    }

    /// <summary>
    /// Determines whether the declaration is static.
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration is static</returns>
    internal static bool IsStatic(MemberDeclarationSyntax memberDeclaration)
    {
        if (memberDeclaration is OperatorDeclarationSyntax
                              or ConversionOperatorDeclarationSyntax)
        {
            return true;
        }

        return DeclarationModifierUtilities.GetModifiers(memberDeclaration).Any(SyntaxKind.StaticKeyword);
    }

    /// <summary>
    /// Determines whether the declaration is a const field.
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration is a const field</returns>
    internal static bool IsConst(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration is FieldDeclarationSyntax fieldDeclaration && fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword);
    }

    /// <summary>
    /// Determines whether the declaration is a readonly field.
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration is a readonly field</returns>
    internal static bool IsReadonlyField(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration is FieldDeclarationSyntax fieldDeclaration && fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);
    }

    /// <summary>
    /// Gets the preferred diagnostic location for a declaration.
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>Diagnostic location</returns>
    internal static Location GetDiagnosticLocation(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.Identifier.GetLocation(),
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.GetLocation(),
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables[0].Identifier.GetLocation(),
                   ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.Identifier.GetLocation(),
                   DestructorDeclarationSyntax destructorDeclaration => destructorDeclaration.Identifier.GetLocation(),
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Identifier.GetLocation(),
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.ThisKeyword.GetLocation(),
                   EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.GetLocation(),
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.Declaration.Variables[0].Identifier.GetLocation(),
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.Identifier.GetLocation(),
                   OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.OperatorToken.GetLocation(),
                   ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => conversionOperatorDeclaration.OperatorKeyword.GetLocation(),
                   _ => memberDeclaration.GetLocation(),
               };
    }

    /// <summary>
    /// Finds the containing type declaration and member declaration for a diagnostic.
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="typeDeclaration">Containing type declaration</param>
    /// <param name="memberDeclaration">Containing member declaration</param>
    /// <returns><see langword="true"/> if both declarations were found</returns>
    internal static bool TryGetContainingTypeAndMember(SyntaxNode root, Diagnostic diagnostic, out TypeDeclarationSyntax typeDeclaration, out MemberDeclarationSyntax memberDeclaration)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;

        memberDeclaration = diagnosticNode?.AncestorsAndSelf()
                                          .OfType<MemberDeclarationSyntax>()
                                          .FirstOrDefault(obj => obj.Parent is TypeDeclarationSyntax);
        typeDeclaration = memberDeclaration?.Parent as TypeDeclarationSyntax;

        return typeDeclaration != null && memberDeclaration != null;
    }

    /// <summary>
    /// Finds the declaration associated with a modifier-based diagnostic.
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if a declaration was found</returns>
    internal static bool TryGetMemberDeclaration(SyntaxNode root, Diagnostic diagnostic, out MemberDeclarationSyntax memberDeclaration)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;

        memberDeclaration = diagnosticNode?.AncestorsAndSelf()
                                          .OfType<MemberDeclarationSyntax>()
                                          .FirstOrDefault(obj => obj is BaseTypeDeclarationSyntax
                                                                     or DelegateDeclarationSyntax
                                                                     or FieldDeclarationSyntax
                                                                     or ConstructorDeclarationSyntax
                                                                     or PropertyDeclarationSyntax
                                                                     or EventDeclarationSyntax
                                                                     or EventFieldDeclarationSyntax
                                                                     or MethodDeclarationSyntax
                                                                     or IndexerDeclarationSyntax
                                                                     or OperatorDeclarationSyntax
                                                                     or ConversionOperatorDeclarationSyntax);

        return memberDeclaration != null;
    }

    /// <summary>
    /// Moves a member before another member within the same type declaration.
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="memberToMove">Member to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns>The updated type declaration</returns>
    internal static TypeDeclarationSyntax MoveMemberBefore(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberToMove, MemberDeclarationSyntax targetMember)
    {
        var members = typeDeclaration.Members;
        var memberToMoveIndex = members.IndexOf(memberToMove);
        var targetMemberIndex = members.IndexOf(targetMember);

        if (memberToMoveIndex < 0
            || targetMemberIndex < 0
            || memberToMoveIndex <= targetMemberIndex)
        {
            return typeDeclaration;
        }

        var updatedMembers = members.RemoveAt(memberToMoveIndex)
                                    .Insert(targetMemberIndex, memberToMove);

        return typeDeclaration.WithMembers(updatedMembers);
    }

    #endregion // Methods
}