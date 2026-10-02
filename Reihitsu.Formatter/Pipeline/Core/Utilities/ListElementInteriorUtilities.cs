using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;

namespace Reihitsu.Formatter.Pipeline.Core.Utilities;

/// <summary>
/// Shared decisions about the interior of an element of an argument-, parameter-, attribute-, tuple-, or
/// type-argument-like list: which nodes are such elements, and which nested construct inside an element lays out its
/// own lines. The line-break phase uses them to decide which interior wraps it joins, and the indentation phase uses
/// them to decide which kept lines it aligns with the element, so both phases agree on the same boundary
/// </summary>
internal static class ListElementInteriorUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether a node is an element of a list whose element interiors are joined and aligned with the
    /// element
    /// </summary>
    /// <param name="node">The node</param>
    /// <returns><see langword="true"/> if the node is such a list element; otherwise, <see langword="false"/></returns>
    internal static bool IsListElement(SyntaxNode node)
    {
        return node switch
               {
                   ArgumentSyntax => true,
                   AttributeArgumentSyntax => true,
                   ParameterSyntax { Parent: BaseParameterListSyntax } => true,
                   TypeParameterSyntax => true,
                   FunctionPointerParameterSyntax => true,
                   TupleElementSyntax => true,
                   AttributeSyntax => true,
                   TypeSyntax { Parent: TypeArgumentListSyntax } => true,
                   _ => false
               };
    }

    /// <summary>
    /// Finds the innermost construct between a token and its list element that lays out the lines inside it - a brace
    /// scope, an initializer, an anonymous object, a switch expression, the interior of a collection expression, a
    /// recursive, list, or parenthesized pattern, an interpolation hole, or the clauses of a query
    /// </summary>
    /// <param name="token">The token</param>
    /// <param name="element">The list element containing the token</param>
    /// <returns>The innermost owning construct, or <see langword="null"/> if the element itself owns the token's line</returns>
    internal static SyntaxNode FindOwningConstruct(SyntaxToken token, SyntaxNode element)
    {
        for (var node = token.Parent; node != null && node != element; node = node.Parent)
        {
            if (IsOwningConstruct(node, token))
            {
                return node;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether a node on the path from a token to its list element lays out the token's line itself
    /// </summary>
    /// <param name="node">The node on the path</param>
    /// <param name="token">The token</param>
    /// <returns><see langword="true"/> if the node owns the token's line; otherwise, <see langword="false"/></returns>
    private static bool IsOwningConstruct(SyntaxNode node, SyntaxToken token)
    {
        return node switch
               {
                   CollectionExpressionSyntax collection => token != collection.OpenBracketToken,
                   InitializerExpressionSyntax
                   or AnonymousObjectCreationExpressionSyntax
                   or SwitchExpressionSyntax
                   or RecursivePatternSyntax
                   or ListPatternSyntax
                   or ParenthesizedPatternSyntax
                   or AccessorListSyntax
                   or InterpolationSyntax
                   or QueryBodySyntax => true,
                   _ => SyntaxIndentationUtilities.IsIndentingScope(node)
               };
    }

    #endregion // Methods
}