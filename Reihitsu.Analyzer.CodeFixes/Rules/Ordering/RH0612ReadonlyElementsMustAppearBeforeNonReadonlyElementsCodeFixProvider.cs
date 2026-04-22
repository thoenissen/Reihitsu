using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0612ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0612ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider))]
public class RH0612ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider : TypeMemberOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0612ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider()
        : base(RH0612ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, CodeFixResources.RH0612Title)
    {
    }

    #endregion // Constructor

    #region TypeMemberOrderingCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool TryGetTargetMember(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberDeclaration, out MemberDeclarationSyntax targetMember)
    {
        targetMember = null;

        if (memberDeclaration is not FieldDeclarationSyntax { Modifiers: var modifiers } fieldDeclaration
            || modifiers.Any(SyntaxKind.ConstKeyword)
            || modifiers.Any(SyntaxKind.ReadOnlyKeyword) == false)
        {
            return false;
        }

        var accessibilityGroup = OrderingDeclarationUtilities.GetAccessibilityGroup(fieldDeclaration);
        var isStaticField = modifiers.Any(SyntaxKind.StaticKeyword);

        foreach (var currentMember in typeDeclaration.Members)
        {
            if (currentMember == memberDeclaration)
            {
                break;
            }

            if (currentMember is FieldDeclarationSyntax currentFieldDeclaration
                && currentFieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword) == false
                && currentFieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) == false
                && currentFieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) == isStaticField
                && OrderingDeclarationUtilities.GetAccessibilityGroup(currentFieldDeclaration) == accessibilityGroup)
            {
                targetMember = currentFieldDeclaration;

                return true;
            }
        }

        return false;
    }

    #endregion // TypeMemberOrderingCodeFixProviderBase
}