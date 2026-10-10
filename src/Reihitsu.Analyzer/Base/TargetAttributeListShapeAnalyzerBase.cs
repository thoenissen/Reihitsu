using System.Collections.Generic;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base analyzer for attribute list-shape rules by <see cref="AttributeTargets"/>
/// </summary>
public abstract class TargetAttributeListShapeAnalyzerBase : AttributeTargetRuleAnalyzerBase
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
    /// Gets all sibling attribute lists attached to the same owner
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Sibling attribute lists</returns>
    private static IReadOnlyList<AttributeListSyntax> GetSiblingAttributeLists(AttributeListSyntax attributeList)
    {
        var owner = attributeList.Parent;

        return owner != null
                   ? AttributeTargetUtilities.GetAttributeLists(owner)
                   : [];
    }

    /// <summary>
    /// Analyzes an attribute list
    /// </summary>
    /// <param name="context">Context</param>
    private void OnAttributeList(SyntaxNodeAnalysisContext context)
    {
        var attributeList = (AttributeListSyntax)context.Node;

        if (AttributeTargetUtilities.TryResolveLayoutTarget(attributeList, out var target) == false
            || IsAttributeListInScope(attributeList, target) == false)
        {
            return;
        }

        var listShapeMode = ResolveListShapeMode(attributeList);

        if (listShapeMode == TargetAttributeListShapeMode.SplitLists)
        {
            // The formatter keeps the lists of an inline auto-property accessor merged on the property's line,
            // whatever their explicit target, so a split is never required there
            if (attributeList.Attributes.Count > 1
                && AttributeTargetUtilities.IsAttributeListOnInlineAutoPropertyAccessor(attributeList) == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(attributeList.GetLocation()));
            }

            return;
        }

        // Only lists of the same target can be merged, so a sibling of another target neither makes this list a
        // duplicate nor becomes the group's first list
        var siblings = GetSiblingAttributeLists(attributeList).Where(list => AttributeTargetUtilities.TryResolveLayoutTarget(list, out var siblingTarget)
                                                                             && IsAttributeListInScope(list, siblingTarget)
                                                                             && AttributeTargetUtilities.HaveSameTarget(attributeList, list)
                                                                             && ResolveListShapeMode(list) == TargetAttributeListShapeMode.MergedList)
                                                              .ToArray();

        if (siblings.Length > 1 && ReferenceEquals(attributeList, siblings[0]) == false)
        {
            context.ReportDiagnostic(CreateDiagnostic(attributeList.GetLocation()));
        }
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