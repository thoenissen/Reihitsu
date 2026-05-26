using System;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0856AccessorAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0856AccessorAttributeListsMustFollowShapeRulesCodeFixProvider))]
public class RH0856AccessorAttributeListsMustFollowShapeRulesCodeFixProvider : TargetAttributeListShapeCodeFixProviderBase
{
    #region TargetAttributeListShapeCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH0856AccessorAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Method;

    /// <inheritdoc/>
    protected override TargetAttributeListShapeMode ListShapeMode => TargetAttributeListShapeMode.SplitLists;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SeparateLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH0856Title;

    /// <inheritdoc/>
    protected override bool IsAttributeListInScope(AttributeListSyntax attributeList, AttributeTargets target)
    {
        return base.IsAttributeListInScope(attributeList, target)
               && attributeList.Parent is AccessorDeclarationSyntax;
    }

    /// <inheritdoc/>
    protected override TargetAttributeListShapeMode ResolveListShapeMode(AttributeListSyntax attributeList)
    {
        if (attributeList.Parent is AccessorDeclarationSyntax accessorDeclaration
            && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
            && SyntaxNodeUtilities.IsSingleLine(basePropertyDeclaration))
        {
            return TargetAttributeListShapeMode.MergedList;
        }

        return base.ResolveListShapeMode(attributeList);
    }

    #endregion // TargetAttributeListShapeCodeFixProviderBase
}