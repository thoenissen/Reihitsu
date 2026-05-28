using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5521InterfaceAttributesMustFollowPlacementRulesCodeFixProvider))]
public class RH5521InterfaceAttributesMustFollowPlacementRulesCodeFixProvider : TargetAttributePlacementCodeFixProviderBase
{
    #region TargetAttributePlacementCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH5521InterfaceAttributesMustFollowPlacementRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Interface;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SeparateLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH5521Title;

    #endregion // TargetAttributePlacementCodeFixProviderBase
}