using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Organization;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Organization;

/// <summary>
/// Code fix provider for <see cref="RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider))]
public class RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider : TypeMemberOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH7103StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider()
        : base(RH7103StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, CodeFixResources.RH7103Title)
    {
    }

    #endregion // Constructor

    #region TypeMemberOrderingCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool TryGetTargetMember(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberDeclaration, out MemberDeclarationSyntax targetMember)
    {
        targetMember = null;

        if (OrderingDeclarationUtilities.IsConst(memberDeclaration)
            || OrderingDeclarationUtilities.IsStatic(memberDeclaration) == false)
        {
            return false;
        }

        var memberKind = OrderingDeclarationUtilities.GetMemberKind(memberDeclaration);
        var accessibilityGroup = OrderingDeclarationUtilities.GetAccessibilityGroup(memberDeclaration);

        foreach (var currentMember in typeDeclaration.Members)
        {
            if (currentMember == memberDeclaration)
            {
                break;
            }

            if (OrderingDeclarationUtilities.IsConst(currentMember) == false
                && OrderingDeclarationUtilities.IsStatic(currentMember) == false
                && OrderingDeclarationUtilities.GetMemberKind(currentMember) == memberKind
                && OrderingDeclarationUtilities.GetAccessibilityGroup(currentMember) == accessibilityGroup)
            {
                targetMember = currentMember;

                return true;
            }
        }

        return false;
    }

    #endregion // TypeMemberOrderingCodeFixProviderBase
}