using System;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0837MethodAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0837MethodAttributesMustFollowPlacementRulesCodeFixProvider))]
public class RH0837MethodAttributesMustFollowPlacementRulesCodeFixProvider : TargetAttributePlacementCodeFixProviderBase
{
    #region TargetAttributePlacementCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH0837MethodAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Method;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SeparateLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH0837Title;

    /// <inheritdoc/>
    protected override bool IsAttributeListInScope(AttributeListSyntax attributeList, AttributeTargets target)
    {
        return base.IsAttributeListInScope(attributeList, target)
               && attributeList.Parent is not AccessorDeclarationSyntax;
    }

    #endregion // TargetAttributePlacementCodeFixProviderBase
}