using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5530AccessorAttributesMustFollowPlacementRulesCodeFixProvider))]
public class RH5530AccessorAttributesMustFollowPlacementRulesCodeFixProvider : TargetAttributePlacementCodeFixProviderBase
{
    #region TargetAttributePlacementCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Method;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SeparateLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH5530Title;

    /// <inheritdoc/>
    protected override bool IsAttributeListInScope(AttributeListSyntax attributeList, AttributeTargets target)
    {
        return base.IsAttributeListInScope(attributeList, target)
               && attributeList.Parent is AccessorDeclarationSyntax;
    }

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList)
    {
        if (attributeList.Parent is AccessorDeclarationSyntax accessorDeclaration
            && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
            && SyntaxNodeUtilities.IsSingleLine(basePropertyDeclaration))
        {
            return TargetAttributePlacementMode.SingleLine;
        }

        return base.ResolvePlacementMode(attributeList);
    }

    #endregion // TargetAttributePlacementCodeFixProviderBase
}