using System;
using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0850DelegateAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0850DelegateAttributeListsMustFollowShapeRulesCodeFixProvider))]
public class RH0850DelegateAttributeListsMustFollowShapeRulesCodeFixProvider : TargetAttributeListShapeCodeFixProviderBase
{
    #region TargetAttributeListShapeCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH0850DelegateAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Delegate;

    /// <inheritdoc/>
    protected override TargetAttributeListShapeMode ListShapeMode => TargetAttributeListShapeMode.SplitLists;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SeparateLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH0850Title;

    #endregion // TargetAttributeListShapeCodeFixProviderBase
}