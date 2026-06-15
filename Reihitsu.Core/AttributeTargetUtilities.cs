using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Shared helpers for attribute-target analysis and rewrites
/// </summary>
public static class AttributeTargetUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the attribute list is attached to an accessor of a single-line property.
    /// Accessor attributes on a single-line property stay on the same line, whereas all other
    /// accessor attributes follow the rule's default placement
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> if the attribute list belongs to a single-line property accessor; otherwise, <see langword="false"/></returns>
    public static bool IsAttributeListOnSingleLinePropertyAccessor(AttributeListSyntax attributeList)
    {
        return attributeList.Parent is AccessorDeclarationSyntax accessorDeclaration
               && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
               && SyntaxNodeUtilities.IsSingleLine(basePropertyDeclaration);
    }

    /// <summary>
    /// Gets attribute lists attached to an owner node
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <returns>Attribute lists</returns>
    public static IReadOnlyList<AttributeListSyntax> GetAttributeLists(SyntaxNode owner)
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

    /// <summary>
    /// Replaces attribute lists on an owner node
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="attributeLists">Replacement attribute lists</param>
    /// <returns>Updated owner node</returns>
    public static SyntaxNode WithAttributeLists(SyntaxNode owner, SyntaxList<AttributeListSyntax> attributeLists)
    {
        return owner switch
               {
                   CompilationUnitSyntax compilationUnit => compilationUnit.WithAttributeLists(attributeLists),
                   BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.WithAttributeLists(attributeLists),
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.WithAttributeLists(attributeLists),
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.WithAttributeLists(attributeLists),
                   ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.WithAttributeLists(attributeLists),
                   OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.WithAttributeLists(attributeLists),
                   ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => conversionOperatorDeclaration.WithAttributeLists(attributeLists),
                   LocalFunctionStatementSyntax localFunctionStatement => localFunctionStatement.WithAttributeLists(attributeLists),
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.WithAttributeLists(attributeLists),
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.WithAttributeLists(attributeLists),
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.WithAttributeLists(attributeLists),
                   EventDeclarationSyntax eventDeclaration => eventDeclaration.WithAttributeLists(attributeLists),
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.WithAttributeLists(attributeLists),
                   AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.WithAttributeLists(attributeLists),
                   ParameterSyntax parameter => parameter.WithAttributeLists(attributeLists),
                   TypeParameterSyntax typeParameter => typeParameter.WithAttributeLists(attributeLists),
                   _ => owner
               };
    }

    /// <summary>
    /// Tries to resolve an attribute target from an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="target">Resolved target</param>
    /// <returns><see langword="true"/> when a supported target was resolved</returns>
    public static bool TryResolveTarget(AttributeListSyntax attributeList, out AttributeTargets target)
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
    /// Tries to resolve the token after an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="token">Resolved token</param>
    /// <returns><see langword="true"/> when token was resolved</returns>
    public static bool TryGetTokenAfterAttributeList(AttributeListSyntax attributeList, out SyntaxToken token)
    {
        token = attributeList.CloseBracketToken.GetNextToken(includeZeroWidth: false);

        return token.IsKind(SyntaxKind.None) == false;
    }

    /// <summary>
    /// Tries to infer a target from an owner node
    /// </summary>
    /// <param name="parent">Owner node</param>
    /// <param name="target">Resolved target</param>
    /// <returns><see langword="true"/> when target was resolved</returns>
    private static bool TryResolveImplicitTarget(SyntaxNode parent, out AttributeTargets target)
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
            default:
                {
                    return false;
                }
        }
    }

    #endregion // Methods
}