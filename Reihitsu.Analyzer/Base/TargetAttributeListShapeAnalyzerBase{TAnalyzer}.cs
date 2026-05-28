using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base analyzer for attribute list-shape rules by <see cref="AttributeTargets"/>
/// </summary>
/// <typeparam name="TAnalyzer">Analyzer type</typeparam>
public abstract class TargetAttributeListShapeAnalyzerBase<TAnalyzer> : AttributeTargetRuleAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    #region Fields

    /// <summary>
    /// List-shape policy
    /// </summary>
    private readonly TargetAttributeListShapeMode _listShapeMode;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Title resource name</param>
    /// <param name="messageFormatResourceName">Message resource name</param>
    /// <param name="target">Target analyzed by this rule</param>
    /// <param name="listShapeMode">List-shape mode</param>
    protected TargetAttributeListShapeAnalyzerBase(string diagnosticId,
                                                   string titleResourceName,
                                                   string messageFormatResourceName,
                                                   AttributeTargets target,
                                                   TargetAttributeListShapeMode listShapeMode)
        : base(diagnosticId, titleResourceName, messageFormatResourceName, target)
    {
        _listShapeMode = listShapeMode;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Resolves the effective list-shape mode for an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Effective list-shape mode</returns>
    protected virtual TargetAttributeListShapeMode ResolveListShapeMode(AttributeListSyntax attributeList)
    {
        return _listShapeMode;
    }

    /// <summary>
    /// Analyzes an attribute list
    /// </summary>
    /// <param name="context">Context</param>
    private void OnAttributeList(SyntaxNodeAnalysisContext context)
    {
        var attributeList = (AttributeListSyntax)context.Node;

        if (AttributeTargetUtilities.TryResolveTarget(attributeList, out var target) == false
            || IsAttributeListInScope(attributeList, target) == false)
        {
            return;
        }

        var listShapeMode = ResolveListShapeMode(attributeList);

        if (listShapeMode == TargetAttributeListShapeMode.SplitLists)
        {
            if (attributeList.Attributes.Count > 1)
            {
                context.ReportDiagnostic(CreateDiagnostic(attributeList.GetLocation()));
            }

            return;
        }

        var siblings = GetSiblingAttributeLists(attributeList).Where(list => AttributeTargetUtilities.TryResolveTarget(list, out var siblingTarget)
                                                                             && IsAttributeListInScope(list, siblingTarget)
                                                                             && ResolveListShapeMode(list) == TargetAttributeListShapeMode.MergedList)
                                                              .ToArray();

        if (siblings.Length > 1 && ReferenceEquals(attributeList, siblings[0]) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(attributeList.GetLocation()));
        }
    }

    /// <summary>
    /// Gets all sibling attribute lists attached to the same owner
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Sibling attribute lists</returns>
    private IReadOnlyList<AttributeListSyntax> GetSiblingAttributeLists(AttributeListSyntax attributeList)
    {
        var owner = attributeList.Parent;

        return owner != null
                   ? AttributeTargetUtilities.GetAttributeLists(owner)
                   : [];
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnAttributeList, Microsoft.CodeAnalysis.CSharp.SyntaxKind.AttributeList);
    }

    #endregion // DiagnosticAnalyzer
}