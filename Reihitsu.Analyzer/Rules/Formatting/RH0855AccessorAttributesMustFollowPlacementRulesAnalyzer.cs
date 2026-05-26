using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0855: Accessor attributes must follow placement rules
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0855AccessorAttributesMustFollowPlacementRulesAnalyzer : TargetAttributePlacementAnalyzerBase<RH0855AccessorAttributesMustFollowPlacementRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0855";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0855AccessorAttributesMustFollowPlacementRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0855Title),
               nameof(AnalyzerResources.RH0855MessageFormat),
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