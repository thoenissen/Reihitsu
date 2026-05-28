using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5504ModuleAttributeListsMustFollowShapeRulesCodeFixProvider))]
public class RH5504ModuleAttributeListsMustFollowShapeRulesCodeFixProvider : TargetAttributeListShapeCodeFixProviderBase
{
    #region TargetAttributeListShapeCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH5504ModuleAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Module;

    /// <inheritdoc/>
    protected override TargetAttributeListShapeMode ListShapeMode => TargetAttributeListShapeMode.SplitLists;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SeparateLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH5504Title;

    #endregion // TargetAttributeListShapeCodeFixProviderBase
}