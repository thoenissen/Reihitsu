using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Ordering;

/// <summary>
/// Code fix provider for <see cref="RH0602StaticElementsMustAppearBeforeInstanceElementsAnalyzer"/>.
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0602StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider))]
public class RH0602StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider : TypeMemberOrderingCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0602StaticElementsMustAppearBeforeInstanceElementsCodeFixProvider()
        : base(RH0602StaticElementsMustAppearBeforeInstanceElementsAnalyzer.DiagnosticId, CodeFixResources.RH0602Title)
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