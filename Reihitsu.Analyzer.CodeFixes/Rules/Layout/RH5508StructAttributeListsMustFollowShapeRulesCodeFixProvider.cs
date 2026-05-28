using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5508StructAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5508StructAttributeListsMustFollowShapeRulesCodeFixProvider))]
public class RH5508StructAttributeListsMustFollowShapeRulesCodeFixProvider : TargetAttributeListShapeCodeFixProviderBase
{
    #region TargetAttributeListShapeCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH5508StructAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Struct;

    /// <inheritdoc/>
    protected override TargetAttributeListShapeMode ListShapeMode => TargetAttributeListShapeMode.SplitLists;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SeparateLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH5508Title;

    #endregion // TargetAttributeListShapeCodeFixProviderBase
}