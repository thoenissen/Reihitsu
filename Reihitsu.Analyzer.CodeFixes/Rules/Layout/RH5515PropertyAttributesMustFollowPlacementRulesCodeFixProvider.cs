using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5515PropertyAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5515PropertyAttributesMustFollowPlacementRulesCodeFixProvider))]
public class RH5515PropertyAttributesMustFollowPlacementRulesCodeFixProvider : TargetAttributePlacementCodeFixProviderBase
{
    #region TargetAttributePlacementCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH5515PropertyAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Property;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SeparateLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH5515Title;

    #endregion // TargetAttributePlacementCodeFixProviderBase
}