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
/// Code fix provider for <see cref="RH7102ConstantsMustAppearBeforeFieldsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider))]
public class RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider : TypeMemberOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7102ConstantsMustAppearBeforeFieldsCodeFixProvider()
        : base(RH7102ConstantsMustAppearBeforeFieldsAnalyzer.DiagnosticId, CodeFixResources.RH7102Title)
    {
    }

    #endregion // Constructor

    #region TypeMemberOrderingCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool TryGetTargetMember(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberDeclaration, out MemberDeclarationSyntax targetMember)
    {
        targetMember = null;

        if (memberDeclaration is not FieldDeclarationSyntax { Modifiers: var modifiers } fieldDeclaration
            || modifiers.Any(SyntaxKind.ConstKeyword) == false)
        {
            return false;
        }

        var accessibilityGroup = OrderingDeclarationUtilities.GetAccessibilityGroup(fieldDeclaration);

        foreach (var currentMember in typeDeclaration.Members)
        {
            if (currentMember == memberDeclaration)
            {
                break;
            }

            if (currentMember is FieldDeclarationSyntax currentFieldDeclaration
                && OrderingDeclarationUtilities.GetAccessibilityGroup(currentFieldDeclaration) == accessibilityGroup
                && currentFieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword) == false)
            {
                targetMember = currentFieldDeclaration;

                return true;
            }
        }

        return false;
    }

    #endregion // TypeMemberOrderingCodeFixProviderBase
}