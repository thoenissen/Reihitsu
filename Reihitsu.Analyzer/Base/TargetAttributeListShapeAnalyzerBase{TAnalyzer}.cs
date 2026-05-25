using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

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

        if (TryResolveTarget(attributeList, out var target) == false
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

        var siblings = GetSiblingAttributeLists(attributeList).Where(list => TryResolveTarget(list, out var siblingTarget)
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
                   ? GetAttributeLists(owner)
                   : [];
    }

    /// <summary>
    /// Gets attribute lists for an owner node
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <returns>Attribute lists</returns>
    private IReadOnlyList<AttributeListSyntax> GetAttributeLists(SyntaxNode owner)
    {
        return owner switch
               {
                   CompilationUnitSyntax compilationUnit => compilationUnit.AttributeLists,
                   BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.AttributeLists,
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.AttributeLists,
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.AttributeLists,
                   ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.AttributeLists,
                   OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.AttributeLists,
                   ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => conversionOperatorDeclaration.AttributeLists,
                   LocalFunctionStatementSyntax localFunctionStatement => localFunctionStatement.AttributeLists,
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.AttributeLists,
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.AttributeLists,
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.AttributeLists,
                   EventDeclarationSyntax eventDeclaration => eventDeclaration.AttributeLists,
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.AttributeLists,
                   AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.AttributeLists,
                   ParameterSyntax parameter => parameter.AttributeLists,
                   TypeParameterSyntax typeParameter => typeParameter.AttributeLists,
                   _ => []
               };
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