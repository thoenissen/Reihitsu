using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5513: Method attributes must be on their own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5513MethodAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5513";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5513MethodAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5513Title),
               nameof(AnalyzerResources.RH5513MessageFormat),
               AttributeTargets.Method,
               TargetAttributePlacementMode.SeparateLine)
    {
    }

    #endregion // Constructor

    #region AttributeTargetRuleAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsAttributeListInScope(AttributeListSyntax attributeList, AttributeTargets target)
    {
        return base.IsAttributeListInScope(attributeList, target)
               && attributeList.Parent is not AccessorDeclarationSyntax;
    }

    #endregion // AttributeTargetRuleAnalyzerBase
}