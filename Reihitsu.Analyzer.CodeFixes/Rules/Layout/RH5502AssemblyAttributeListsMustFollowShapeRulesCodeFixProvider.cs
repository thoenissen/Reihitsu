using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5502AssemblyAttributeListsMustFollowShapeRulesAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5502AssemblyAttributeListsMustFollowShapeRulesCodeFixProvider))]
public class RH5502AssemblyAttributeListsMustFollowShapeRulesCodeFixProvider : TargetAttributeListShapeCodeFixProviderBase
{
    #region TargetAttributeListShapeCodeFixProviderBase

    /// <inheritdoc/>
    protected override string DiagnosticId => RH5502AssemblyAttributeListsMustFollowShapeRulesAnalyzer.DiagnosticId;

    /// <inheritdoc/>
    protected override AttributeTargets Target => AttributeTargets.Assembly;

    /// <inheritdoc/>
    protected override TargetAttributeListShapeMode ListShapeMode => TargetAttributeListShapeMode.SplitLists;

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode DefaultPlacementMode => TargetAttributePlacementMode.SeparateLine;

    /// <inheritdoc/>
    protected override string CodeFixTitle => CodeFixResources.RH5502Title;

    #endregion // TargetAttributeListShapeCodeFixProviderBase
}