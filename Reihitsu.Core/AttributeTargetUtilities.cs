using System;
using System.Collections.Generic;
using System.Linq;

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
               && SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists(basePropertyDeclaration);
    }

    /// <summary>
    /// Determines whether the attribute list is attached to an accessor that the formatter keeps on a single-line
    /// property's line. That holds only when the property is single-line, no accessor has a body or an expression
    /// body, and the accessor list itself contains no comment or directive — otherwise the formatter expands the
    /// accessor list and lays out the attribute lists by their target's default policy. The last two conditions
    /// mirror the formatter's <c>LineBreakDetection.ShouldNormalizeAccessorListBraces</c> and must stay in step
    /// with it. Trivia outside the accessor list, such as a comment after its closing brace, does not count
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> if the formatter keeps the attribute list inline on its property's line; otherwise, <see langword="false"/></returns>
    public static bool IsAttributeListOnInlineAutoPropertyAccessor(AttributeListSyntax attributeList)
    {
        return IsAttributeListOnSingleLinePropertyAccessor(attributeList)
               && attributeList.Parent?.Parent is AccessorListSyntax accessorList
               && accessorList.Accessors.All(accessor => accessor.Body == null && accessor.ExpressionBody == null)
               && SyntaxNodeUtilities.InteriorContainsCommentOrDirective(accessorList) == false;
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
                   MemberDeclarationSyntax memberDeclaration => memberDeclaration.AttributeLists,
                   AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.AttributeLists,
                   LocalFunctionStatementSyntax localFunctionStatement => localFunctionStatement.AttributeLists,
                   LambdaExpressionSyntax lambdaExpression => lambdaExpression.AttributeLists,
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
                   MemberDeclarationSyntax memberDeclaration => memberDeclaration.WithAttributeLists(attributeLists),
                   AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.WithAttributeLists(attributeLists),
                   LocalFunctionStatementSyntax localFunctionStatement => localFunctionStatement.WithAttributeLists(attributeLists),
                   LambdaExpressionSyntax lambdaExpression => lambdaExpression.WithAttributeLists(attributeLists),
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
                    {
                        target = AttributeTargets.Assembly;

                        return true;
                    }
                case "module":
                    {
                        target = AttributeTargets.Module;

                        return true;
                    }
                case "field":
                    {
                        target = AttributeTargets.Field;

                        return true;
                    }
                case "event":
                    {
                        target = AttributeTargets.Event;

                        return true;
                    }
                case "method":
                    {
                        target = AttributeTargets.Method;

                        return true;
                    }
                case "param":
                    {
                        target = AttributeTargets.Parameter;

                        return true;
                    }
                case "property":
                    {
                        target = AttributeTargets.Property;

                        return true;
                    }
                case "return":
                    {
                        target = AttributeTargets.ReturnValue;

                        return true;
                    }
                case "typevar":
                    {
                        target = AttributeTargets.GenericParameter;

                        return true;
                    }
                case "type":
                    {
                        return TryResolveImplicitTarget(attributeList.Parent, out target);
                    }

                default:
                    {
                        return false;
                    }
            }
        }

        return TryResolveImplicitTarget(attributeList.Parent, out target);
    }

    /// <summary>
    /// Tries to resolve the target whose layout rules govern an attribute list. A list attached to a parameter or a
    /// type parameter sits inside a parameter or type-parameter list, where the declaration-level layout its explicit
    /// specifier names (for example <c>property:</c> on a positional record parameter) has no valid form, so such a
    /// list follows its owner's layout rules instead: <see cref="AttributeTargets.Parameter"/> for a parameter and
    /// <see cref="AttributeTargets.GenericParameter"/> for a type parameter. A specifier that already resolves to one
    /// of those two targets keeps it, and every other owner keeps the target <see cref="TryResolveTarget"/> resolves
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="target">Resolved layout target</param>
    /// <returns><see langword="true"/> when a supported target was resolved</returns>
    public static bool TryResolveLayoutTarget(AttributeListSyntax attributeList, out AttributeTargets target)
    {
        if (TryResolveTarget(attributeList, out target) == false)
        {
            return false;
        }

        if (target is AttributeTargets.Parameter or AttributeTargets.GenericParameter)
        {
            return true;
        }

        target = attributeList.Parent switch
                 {
                     ParameterSyntax => AttributeTargets.Parameter,
                     TypeParameterSyntax => AttributeTargets.GenericParameter,
                     _ => target
                 };

        return true;
    }

    /// <summary>
    /// Determines whether two attribute lists resolve to the same attribute target. Only such lists may be merged
    /// into one list, because the merged list keeps the first list's specifier and would otherwise silently apply
    /// the other list's attributes to a different target. A <c>type:</c> list on an owner that is not a type
    /// declaration never qualifies: the compiler ignores it there, so merging it with any other list would change
    /// which attributes apply
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="otherAttributeList">Other attribute list</param>
    /// <returns><see langword="true"/> when both lists resolve to the same supported target</returns>
    public static bool HaveSameTarget(AttributeListSyntax attributeList, AttributeListSyntax otherAttributeList)
    {
        return IsIgnoredTypeTarget(attributeList) == false
               && IsIgnoredTypeTarget(otherAttributeList) == false
               && TryResolveTarget(attributeList, out var target)
               && TryResolveTarget(otherAttributeList, out var otherTarget)
               && target == otherTarget;
    }

    /// <summary>
    /// Determines whether a group of attribute lists on one owner can be merged without touching user-authored trivia.
    /// A comment or directive inside the group would be dropped or moved by the merge, and so would a comment that
    /// leads the token following a member: that token only starts its own line because the member ended the line
    /// before it, so removing the member would pull the comment up onto the merged list's line
    /// </summary>
    /// <param name="group">The owner's attribute lists to merge, in document order</param>
    /// <returns><see langword="true"/> when the group has more than one member and can be merged safely</returns>
    public static bool CanMergeAttributeListGroup(IReadOnlyList<AttributeListSyntax> group)
    {
        return group.Count > 1
               && SyntaxNodeUtilities.GroupInteriorContainsCommentOrDirective(group) == false
               && GetGapTokens(group).Any(token => token.LeadingTrivia.Any(SyntaxTriviaUtilities.IsCommentTrivia)) == false;
    }

    /// <summary>
    /// Merges a group of attribute lists on an owner into the group's first list. The first list keeps its
    /// specifier and leading trivia, the attributes of every later member are appended to it, and the later members
    /// are removed together with their trivia. Removing a member that ended its line, or joining the first list with
    /// what follows it, would otherwise leave the old line indentation of the next token behind as a run of spaces
    /// on the merged line, and a blank line in front of that token would leave the merged line ending in the space
    /// before it. So the token's leading whitespace and line breaks are dropped whenever the token no longer starts a
    /// line, and a single space is added after the token before it when that token would otherwise touch it. The
    /// caller is responsible for choosing a group whose members share one target and that
    /// <see cref="CanMergeAttributeListGroup"/> accepts
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="group">The owner's attribute lists to merge, in document order</param>
    /// <returns>Updated owner node</returns>
    public static SyntaxNode MergeAttributeListGroup(SyntaxNode owner, IReadOnlyList<AttributeListSyntax> group)
    {
        var lists = GetAttributeLists(owner);
        var groupIndices = group.Select(list => IndexOf(lists, list)).ToArray();
        var annotation = new SyntaxAnnotation();

        owner = owner.ReplaceTokens(GetGapTokens(group), (_, rewritten) => rewritten.WithAdditionalAnnotations(annotation));
        lists = GetAttributeLists(owner);

        var firstList = lists[groupIndices[0]];
        var mergedAttributes = groupIndices.SelectMany(index => lists[index].Attributes)
                                           .Select(attribute => attribute.WithLeadingTrivia(SyntaxFactory.TriviaList())
                                                                         .WithTrailingTrivia(SyntaxFactory.TriviaList()));
        var mergedList = firstList.WithAttributes(SyntaxFactory.SeparatedList(mergedAttributes))
                                  .WithTrailingTrivia(SyntaxFactory.Space);
        var updatedLists = lists.Where((_, index) => index == groupIndices[0] || groupIndices.Contains(index) == false)
                                .Select(list => ReferenceEquals(list, firstList) ? mergedList : list);

        owner = WithAttributeLists(owner, SyntaxFactory.List(updatedLists));

        // Deliberately an allowlist: any other leading trivia kind, such as skipped tokens, keeps the token in place
        var joinedTokens = owner.GetAnnotatedTokens(annotation)
                                .Where(token => StartsLine(token) == false
                                                && token.LeadingTrivia.All(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia)
                                                                                     || trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
                                .ToArray();
        var unseparatedTokens = joinedTokens.Select(token => token.GetPreviousToken())
                                            .Where(token => token.TrailingTrivia.Count == 0)
                                            .ToArray();

        owner = owner.ReplaceTokens(joinedTokens.Concat(unseparatedTokens),
                                    (original, rewritten) =>
                                    {
                                        if (joinedTokens.Contains(original))
                                        {
                                            rewritten = rewritten.WithLeadingTrivia(SyntaxFactory.TriviaList());
                                        }

                                        return unseparatedTokens.Contains(original)
                                                   ? rewritten.WithTrailingTrivia(SyntaxFactory.Space)
                                                   : rewritten;
                                    });

        return owner.ReplaceTokens(owner.GetAnnotatedTokens(annotation), (_, rewritten) => rewritten.WithoutAnnotations(annotation));
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
    /// Determines whether an attribute list names the <c>type:</c> target on an owner that is not a type declaration,
    /// where the compiler ignores the list
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <returns><see langword="true"/> when the list's <c>type:</c> specifier does not apply to its owner</returns>
    private static bool IsIgnoredTypeTarget(AttributeListSyntax attributeList)
    {
        return attributeList.Target?.Identifier.ValueText == "type"
               && attributeList.Parent is not (BaseTypeDeclarationSyntax or DelegateDeclarationSyntax);
    }

    /// <summary>
    /// Gets the tokens whose leading gap a merge closes: the token after each member of the group, except where that
    /// token opens the next member, which the merge removes
    /// </summary>
    /// <param name="group">The owner's attribute lists to merge, in document order</param>
    /// <returns>The gap tokens</returns>
    private static SyntaxToken[] GetGapTokens(IReadOnlyList<AttributeListSyntax> group)
    {
        var memberOpenBrackets = group.Skip(1)
                                      .Select(list => list.OpenBracketToken)
                                      .ToArray();

        return group.Select(list => list.CloseBracketToken.GetNextToken())
                    .Where(token => memberOpenBrackets.Contains(token) == false)
                    .ToArray();
    }

    /// <summary>
    /// Determines whether a token starts a line, that is, whether the token before it ends with a line break
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> when the token starts a line</returns>
    private static bool StartsLine(SyntaxToken token)
    {
        var previousToken = token.GetPreviousToken();

        return previousToken.IsKind(SyntaxKind.None)
               || previousToken.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia));
    }

    /// <summary>
    /// Gets the index of an attribute list among an owner's attribute lists
    /// </summary>
    /// <param name="lists">The owner's attribute lists</param>
    /// <param name="attributeList">Attribute list</param>
    /// <returns>Zero-based index of the list</returns>
    private static int IndexOf(IReadOnlyList<AttributeListSyntax> lists, AttributeListSyntax attributeList)
    {
        for (var index = 0; index < lists.Count; index++)
        {
            if (ReferenceEquals(lists[index], attributeList))
            {
                return index;
            }
        }

        throw new ArgumentException("The attribute list does not belong to the owner.", nameof(attributeList));
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
            case DestructorDeclarationSyntax:
            case LocalFunctionStatementSyntax:
            case LambdaExpressionSyntax:
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
            case EnumMemberDeclarationSyntax:
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