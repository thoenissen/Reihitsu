using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5531: Accessor attribute lists must follow shape rules
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5531";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5531AccessorAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5531Title),
               nameof(AnalyzerResources.RH5531MessageFormat),
               AttributeTargets.Method,
               TargetAttributeListShapeMode.SplitLists)
    {
    }

    #endregion // Constructor

    #region AttributeTargetRuleAnalyzerBase

    /// <inheritdoc/>
    protected override bool IsAttributeListInScope(AttributeListSyntax attributeList, AttributeTargets target)
    {
        return base.IsAttributeListInScope(attributeList, target)
               && attributeList.Parent is AccessorDeclarationSyntax;
    }

    #endregion // AttributeTargetRuleAnalyzerBase

    #region TargetAttributeListShapeAnalyzerBase

    /// <inheritdoc/>
    protected override TargetAttributeListShapeMode ResolveListShapeMode(AttributeListSyntax attributeList)
    {
        if (AttributeTargetUtilities.IsAttributeListOnSingleLinePropertyAccessor(attributeList))
        {
            return TargetAttributeListShapeMode.MergedList;
        }

        return base.ResolveListShapeMode(attributeList);
    }

    #endregion // TargetAttributeListShapeAnalyzerBase
}