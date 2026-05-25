using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Shared helpers for target-based attribute formatting
/// </summary>
internal static class AttributeTargetFormattingShared
{
    #region Methods

    /// <summary>
    /// Determines whether comments or directives are present in a node
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if comments or directives are present; otherwise <see langword="false"/></returns>
    internal static bool HasCommentsOrDirectives(SyntaxNode node)
    {
        foreach (var trivia in node.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsDirective
                || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
                || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets attribute lists attached to an owner node
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <returns>Attribute lists</returns>
    internal static IReadOnlyList<AttributeListSyntax> GetAttributeLists(SyntaxNode owner)
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
    internal static SyntaxNode WithAttributeLists(SyntaxNode owner, SyntaxList<AttributeListSyntax> attributeLists)
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
    internal static bool TryResolveTarget(AttributeListSyntax attributeList, out AttributeTargets target)
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
    internal static bool TryGetTokenAfterAttributeList(AttributeListSyntax attributeList, out SyntaxToken token)
    {
        token = attributeList.CloseBracketToken.GetNextToken(includeZeroWidth: false);

        return token.IsKind(SyntaxKind.None) == false;
    }

    /// <summary>
    /// Determines whether a node is single line
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the node is single line; otherwise <see langword="false"/></returns>
    internal static bool IsSingleLine(SyntaxNode node)
    {
        if (node?.SyntaxTree == null)
        {
            return false;
        }

        var lineSpan = node.SyntaxTree.GetLineSpan(node.Span);

        return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line;
    }

    /// <summary>
    /// Resolves the expected placement mode for an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Expected placement mode</returns>
    internal static TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList)
    {
        if (attributeList.Parent is AccessorDeclarationSyntax accessorDeclaration
            && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
            && IsSingleLine(basePropertyDeclaration))
        {
            return TargetAttributePlacementMode.SingleLine;
        }

        return TryResolveTarget(attributeList, out var target)
               && (target == AttributeTargets.Parameter || target == AttributeTargets.GenericParameter)
                   ? TargetAttributePlacementMode.SingleLine
                   : TargetAttributePlacementMode.SeparateLine;
    }

    /// <summary>
    /// Resolves the expected list-shape mode for an attribute list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Expected list-shape mode</returns>
    internal static TargetAttributeListShapeMode ResolveListShapeMode(AttributeListSyntax attributeList)
    {
        if (attributeList.Parent is AccessorDeclarationSyntax accessorDeclaration
            && accessorDeclaration.Parent?.Parent is BasePropertyDeclarationSyntax basePropertyDeclaration
            && IsSingleLine(basePropertyDeclaration))
        {
            return TargetAttributeListShapeMode.MergedList;
        }

        return TryResolveTarget(attributeList, out var target)
               && (target == AttributeTargets.Parameter || target == AttributeTargets.GenericParameter)
                   ? TargetAttributeListShapeMode.MergedList
                   : TargetAttributeListShapeMode.SplitLists;
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