using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5523ParameterAttributesMustFollowPlacementRulesCodeFixProvider))]
public class RH5523ParameterAttributesMustFollowPlacementRulesCodeFixProvider : TargetAttributePlacementCodeFixProviderBase
{
    #region TargetAttributePlacementCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH5523ParameterAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Parameter;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SingleLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH5523Title;

    #endregion // TargetAttributePlacementCodeFixProviderBase
}