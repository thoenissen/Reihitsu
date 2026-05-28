using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider))]
public class RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider : TypeMemberOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsCodeFixProvider()
        : base(RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzer.DiagnosticId, CodeFixResources.RH7109Title)
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