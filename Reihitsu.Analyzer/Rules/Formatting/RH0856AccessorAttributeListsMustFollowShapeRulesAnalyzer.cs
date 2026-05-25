using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0856: Accessor attribute lists must follow shape rules
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0856AccessorAttributeListsMustFollowShapeRulesAnalyzer : TargetAttributeListShapeAnalyzerBase<RH0856AccessorAttributeListsMustFollowShapeRulesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0856";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0856AccessorAttributeListsMustFollowShapeRulesAnalyzer()
        : base(DiagnosticId,
               nameof(AnalyzerResources.RH0856Title),
               nameof(AnalyzerResources.RH0856MessageFormat),
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
        if (attributeList.Parent is AccessorDeclarationSyntax accessorDeclaration
            && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
            && IsSingleLine(basePropertyDeclaration))
        {
            return TargetAttributeListShapeMode.MergedList;
        }

        return base.ResolveListShapeMode(attributeList);
    }

    #endregion // TargetAttributeListShapeAnalyzerBase

    #region Methods

    /// <summary>
    /// Determines whether a node is on a single line
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node is single line; otherwise <see langword="false"/></returns>
    private static bool IsSingleLine(SyntaxNode node)
    {
        if (node?.SyntaxTree == null)
        {
            return false;
        }

        var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);

        return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line;
    }

    #endregion // Methods
}