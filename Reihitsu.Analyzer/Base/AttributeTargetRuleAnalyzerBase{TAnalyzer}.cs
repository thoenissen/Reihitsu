using System;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Common analyzer base for attribute-target rules
/// </summary>
/// <typeparam name="TAnalyzer">Analyzer type</typeparam>
public abstract class AttributeTargetRuleAnalyzerBase<TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer, new()
{
    #region Fields

    /// <summary>
    /// Target analyzed by this rule
    /// </summary>
    private readonly AttributeTargets _target;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Title resource name</param>
    /// <param name="messageFormatResourceName">Message resource name</param>
    /// <param name="target">Target analyzed by this rule</param>
    protected AttributeTargetRuleAnalyzerBase(string diagnosticId,
                                              string titleResourceName,
                                              string messageFormatResourceName,
                                              AttributeTargets target)
        : base(diagnosticId, DiagnosticCategory.Formatting, titleResourceName, messageFormatResourceName)
    {
        _target = target;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the attribute list is in scope for this analyzer
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="target">Resolved target</param>
    /// <returns><see langword="true"/> when the attribute list should be analyzed</returns>
    protected virtual bool IsAttributeListInScope(AttributeListSyntax attributeList, AttributeTargets target)
    {
        return target == _target;
    }

    /// <summary>
    /// Tries to resolve an attribute target from an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="target">Resolved target</param>
    /// <returns><see langword="true"/> when a supported target was resolved</returns>
    protected bool TryResolveTarget(AttributeListSyntax attributeList, out AttributeTargets target)
    {
        target = default;

        var explicitTarget = attributeList.Target?.Identifier.ValueText;

        if (string.IsNullOrWhiteSpace(explicitTarget) == false)
        {
            switch (explicitTarget)
            {
                case "assembly":
                    target = AttributeTargets.Assembly;

                    return true;
                case "module":
                    target = AttributeTargets.Module;

                    return true;
                case "field":
                    target = AttributeTargets.Field;

                    return true;
                case "event":
                    target = AttributeTargets.Event;

                    return true;
                case "method":
                    target = AttributeTargets.Method;

                    return true;
                case "param":
                    target = AttributeTargets.Parameter;

                    return true;
                case "property":
                    target = AttributeTargets.Property;

                    return true;
                case "return":
                    target = AttributeTargets.ReturnValue;

                    return true;
                case "typevar":
                    target = AttributeTargets.GenericParameter;

                    return true;
                case "type":
                    break;

                default:
                    return false;
            }
        }

        return TryResolveImplicitTarget(attributeList.Parent, out target);
    }

    /// <summary>
    /// Tries to resolve an implicit target from an owner node
    /// </summary>
    /// <param name="parent">Owner node</param>
    /// <param name="target">Resolved target</param>
    /// <returns><see langword="true"/> when a target was resolved</returns>
    private bool TryResolveImplicitTarget(SyntaxNode parent, out AttributeTargets target)
    {
        target = default;

        switch (parent)
        {
            case ClassDeclarationSyntax:
                {
                    target = AttributeTargets.Class;

                    return true;
                }
            case StructDeclarationSyntax:
                {
                    target = AttributeTargets.Struct;

                    return true;
                }
            case RecordDeclarationSyntax recordDeclaration:
                {
                    target = recordDeclaration.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword)
                                 ? AttributeTargets.Struct
                                 : AttributeTargets.Class;

                    return true;
                }
            case InterfaceDeclarationSyntax:
                {
                    target = AttributeTargets.Interface;

                    return true;
                }
            case EnumDeclarationSyntax:
                {
                    target = AttributeTargets.Enum;

                    return true;
                }
            case DelegateDeclarationSyntax:
                {
                    target = AttributeTargets.Delegate;

                    return true;
                }
            case MethodDeclarationSyntax:
            case OperatorDeclarationSyntax:
            case ConversionOperatorDeclarationSyntax:
            case LocalFunctionStatementSyntax:
            case AccessorDeclarationSyntax:
                {
                    target = AttributeTargets.Method;

                    return true;
                }
            case ConstructorDeclarationSyntax:
                {
                    target = AttributeTargets.Constructor;

                    return true;
                }
            case PropertyDeclarationSyntax:
            case IndexerDeclarationSyntax:
                {
                    target = AttributeTargets.Property;

                    return true;
                }
            case FieldDeclarationSyntax:
                {
                    target = AttributeTargets.Field;

                    return true;
                }
            case EventDeclarationSyntax:
            case EventFieldDeclarationSyntax:
                {
                    target = AttributeTargets.Event;

                    return true;
                }
            case ParameterSyntax:
                {
                    target = AttributeTargets.Parameter;

                    return true;
                }
            case TypeParameterSyntax:
                {
                    target = AttributeTargets.GenericParameter;

                    return true;
                }
            case CompilationUnitSyntax:
                {
                    return false;
                }
            default:
                {
                    return false;
                }
        }
    }

    /// <summary>
    /// Tries to resolve the token that follows an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="token">Resolved token</param>
    /// <returns><see langword="true"/> when a token was resolved</returns>
    protected bool TryGetTokenAfterAttributeList(AttributeListSyntax attributeList, out SyntaxToken token)
    {
        token = attributeList.CloseBracketToken.GetNextToken(includeZeroWidth: false);

        return token.IsKind(SyntaxKind.None) == false;
    }

    #endregion // Methods
}