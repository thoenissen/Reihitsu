using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5530: Accessor attributes must follow placement rules
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase<RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5530";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5530AccessorAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH5530Title),
               nameof(AnalyzerResources.RH5530MessageFormat),
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
               && attributeList.Parent is AccessorDeclarationSyntax;
    }

    #endregion // AttributeTargetRuleAnalyzerBase

    #region TargetAttributePlacementAnalyzerBase

    /// <inheritdoc/>
    protected override TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList)
    {
        if (attributeList.Parent is AccessorDeclarationSyntax accessorDeclaration
            && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
            && SyntaxNodeUtilities.IsSingleLine(basePropertyDeclaration))
        {
            return TargetAttributePlacementMode.SingleLine;
        }

        return base.ResolvePlacementMode(attributeList);
    }

    #endregion // TargetAttributePlacementAnalyzerBase
}