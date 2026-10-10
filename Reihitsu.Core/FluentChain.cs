using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core.Enumerations;

namespace Reihitsu.Core;

/// <summary>
/// The structure of a member-access chain as one shared model for the formatter and the analyzers.
/// <para>
/// A chain consists of a root, followed by links and attached parts. A link is a member access or member binding
/// with its operator (<c>.</c>, <c>?.</c>, <c>!.</c> or <c>!?.</c>). An attached part (an argument list, an element
/// access, a conditional element access, or a null-forgiving operator not followed by <c>.</c> or <c>?.</c>) belongs
/// to the element in front of it and is never a link.
/// </para>
/// <para>
/// The links in front of the first invoked link form the prefix; every link from the first invoked link on forms the
/// chain part. A chain without an invoked link consists of its prefix only
/// </para>
/// </summary>
public sealed class FluentChain
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="node">The outermost node of the chain</param>
    /// <param name="links">The links in source order</param>
    /// <param name="attachedParts">The attached parts in source order</param>
    private FluentChain(ExpressionSyntax node, IReadOnlyList<FluentChainLink> links, IReadOnlyList<FluentChainAttachedPart> attachedParts)
    {
        Node = node;
        Links = links;
        AttachedParts = attachedParts;
        FirstInvokedLinkIndex = -1;

        for (var linkIndex = 0; linkIndex < links.Count; linkIndex++)
        {
            if (links[linkIndex].IsInvoked)
            {
                FirstInvokedLinkIndex = linkIndex;

                break;
            }
        }
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The outermost node of the chain
    /// </summary>
    public ExpressionSyntax Node { get; }

    /// <summary>
    /// The links in source order
    /// </summary>
    public IReadOnlyList<FluentChainLink> Links { get; }

    /// <summary>
    /// The attached parts in source order
    /// </summary>
    public IReadOnlyList<FluentChainAttachedPart> AttachedParts { get; }

    /// <summary>
    /// The index of the first invoked link, or <c>-1</c> when the chain has no invoked link
    /// </summary>
    public int FirstInvokedLinkIndex { get; }

    /// <summary>
    /// Whether the chain has no invoked link and therefore consists of its prefix only
    /// </summary>
    public bool IsCallLess => FirstInvokedLinkIndex < 0;

    /// <summary>
    /// Whether links precede the first invoked link. Such a prefix allows the first invoked link to stay wrapped
    /// (<c>x.Items</c> ⏎ <c>.Where()</c>)
    /// </summary>
    public bool HasPrefix => FirstInvokedLinkIndex > 0;

    /// <summary>
    /// The chain's first link, or <see langword="null"/> for a chain without links
    /// </summary>
    public FluentChainLink FirstLink => Links.Count > 0 ? Links[0] : null;

    /// <summary>
    /// The last token of the root, directly in front of the first link's operator
    /// </summary>
    public SyntaxToken RootLastToken => Links[0].OperatorToken.GetPreviousToken();

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Creates the chain model for an outermost chain node
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns>The chain, or <see langword="null"/> when the node is not the outermost node of a chain with at least one link</returns>
    public static FluentChain Create(SyntaxNode node)
    {
        return Create(node, true);
    }

    /// <summary>
    /// Creates the chain model for an outermost chain node
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <param name="requireLink">Whether a chain without any link (only attached parts, such as <c>a?[0]</c>) yields <see langword="null"/></param>
    /// <returns>The chain, or <see langword="null"/> when the node is not an outermost chain node or has no link although one is required</returns>
    public static FluentChain Create(SyntaxNode node, bool requireLink)
    {
        if (node is not ExpressionSyntax expression
            || IsOutermostChainNode(expression) == false)
        {
            return null;
        }

        var links = new List<FluentChainLink>();
        var attachedParts = new List<FluentChainAttachedPart>();

        Collect(expression, null, links, attachedParts);

        if (requireLink
            && links.Count == 0)
        {
            return null;
        }

        return new FluentChain(expression, links, attachedParts);
    }

    /// <summary>
    /// Determines whether a node is a node of a chain spine
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if the node can form part of a chain spine</returns>
    public static bool IsChainNode(SyntaxNode node)
    {
        return node switch
               {
                   MemberAccessExpressionSyntax memberAccess => memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression),
                   PostfixUnaryExpressionSyntax postfixUnary => postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression),
                   InvocationExpressionSyntax or ElementAccessExpressionSyntax or ConditionalAccessExpressionSyntax or MemberBindingExpressionSyntax or ElementBindingExpressionSyntax => true,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether a node is the outermost node of a chain spine, i.e. a chain node whose parent does not
    /// continue the chain
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if the node is the outermost chain node</returns>
    public static bool IsOutermostChainNode(SyntaxNode node)
    {
        return IsChainNode(node)
               && node is not MemberBindingExpressionSyntax
               && node is not ElementBindingExpressionSyntax
               && ContinuesChain(node.Parent, node) == false;
    }

    /// <summary>
    /// Gets the outermost node of the chain a chain node belongs to
    /// </summary>
    /// <param name="node">A chain node</param>
    /// <returns>The outermost chain node</returns>
    public static SyntaxNode GetOutermostChainNode(SyntaxNode node)
    {
        var current = node;

        while (current.Parent != null
               && ContinuesChain(current.Parent, current))
        {
            current = current.Parent;
        }

        return current;
    }

    /// <summary>
    /// Determines whether the link at the given index belongs to the prefix
    /// </summary>
    /// <param name="linkIndex">The link index</param>
    /// <returns><see langword="true"/> if the link precedes the first invoked link or the chain has no invoked link</returns>
    public bool IsPrefixLink(int linkIndex)
    {
        return IsCallLess || linkIndex < FirstInvokedLinkIndex;
    }

    /// <summary>
    /// Determines whether a parent node continues the chain of its child
    /// </summary>
    /// <param name="parent">The parent node</param>
    /// <param name="child">The child node</param>
    /// <returns><see langword="true"/> if the parent is a chain node that continues the child's chain</returns>
    private static bool ContinuesChain(SyntaxNode parent, SyntaxNode child)
    {
        return parent switch
               {
                   InvocationExpressionSyntax invocation => invocation.Expression == child,
                   MemberAccessExpressionSyntax memberAccess => memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression) && memberAccess.Expression == child,
                   ElementAccessExpressionSyntax elementAccess => elementAccess.Expression == child,
                   PostfixUnaryExpressionSyntax postfixUnary => postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression),
                   ConditionalAccessExpressionSyntax => true,
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether a member access or member binding is invoked
    /// </summary>
    /// <param name="node">The member access or member binding</param>
    /// <returns><see langword="true"/> if the node is the expression of an invocation</returns>
    private static bool IsInvoked(ExpressionSyntax node)
    {
        return node.Parent is InvocationExpressionSyntax invocation
               && invocation.Expression == node;
    }

    /// <summary>
    /// Gets the null-forgiving operator that directly precedes the <c>?</c> of a conditional access
    /// </summary>
    /// <param name="conditionalAccess">The conditional access</param>
    /// <returns>The null-forgiving operator token, or <see langword="default"/></returns>
    private static SyntaxToken GetNullForgivingInFront(ConditionalAccessExpressionSyntax conditionalAccess)
    {
        return conditionalAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary
               && postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression)
                   ? postfixUnary.OperatorToken
                   : default;
    }

    /// <summary>
    /// Builds the operator token list of a conditional binding
    /// </summary>
    /// <param name="conditionalAccess">The conditional access that owns the binding</param>
    /// <param name="lastToken">The binding's dot or opening bracket</param>
    /// <returns>The operator tokens in source order</returns>
    private static List<SyntaxToken> GetConditionalTokens(ConditionalAccessExpressionSyntax conditionalAccess, SyntaxToken lastToken)
    {
        var tokens = new List<SyntaxToken>();
        var nullForgiving = GetNullForgivingInFront(conditionalAccess);

        if (nullForgiving.IsKind(SyntaxKind.None) == false)
        {
            tokens.Add(nullForgiving);
        }

        tokens.Add(conditionalAccess.OperatorToken);
        tokens.Add(lastToken);

        return tokens;
    }

    /// <summary>
    /// Collects the links and attached parts of a chain spine in source order
    /// </summary>
    /// <param name="node">The spine node to walk</param>
    /// <param name="pendingConditional">The conditional access whose binding is the leftmost node of the walked spine, if any</param>
    /// <param name="links">The links collected so far</param>
    /// <param name="attachedParts">The attached parts collected so far</param>
    private static void Collect(ExpressionSyntax node,
                                ConditionalAccessExpressionSyntax pendingConditional,
                                List<FluentChainLink> links,
                                List<FluentChainAttachedPart> attachedParts)
    {
        switch (node)
        {
            case InvocationExpressionSyntax invocation:
                {
                    Collect(invocation.Expression, pendingConditional, links, attachedParts);

                    attachedParts.Add(new FluentChainAttachedPart(FluentChainAttachedPartKind.ArgumentList, [invocation.ArgumentList.OpenParenToken]));
                }
                break;

            case MemberAccessExpressionSyntax memberAccess when memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression):
                {
                    if (memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary
                        && postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression))
                    {
                        Collect(postfixUnary.Operand, pendingConditional, links, attachedParts);

                        links.Add(new FluentChainLink(memberAccess, [postfixUnary.OperatorToken, memberAccess.OperatorToken], memberAccess.Name, IsInvoked(memberAccess)));
                    }
                    else
                    {
                        Collect(memberAccess.Expression, pendingConditional, links, attachedParts);

                        links.Add(new FluentChainLink(memberAccess, [memberAccess.OperatorToken], memberAccess.Name, IsInvoked(memberAccess)));
                    }
                }
                break;

            case ElementAccessExpressionSyntax elementAccess:
                {
                    Collect(elementAccess.Expression, pendingConditional, links, attachedParts);

                    attachedParts.Add(new FluentChainAttachedPart(FluentChainAttachedPartKind.ElementAccess, [elementAccess.ArgumentList.OpenBracketToken]));
                }
                break;

            case PostfixUnaryExpressionSyntax postfixUnary when postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression):
                {
                    Collect(postfixUnary.Operand, pendingConditional, links, attachedParts);

                    attachedParts.Add(new FluentChainAttachedPart(FluentChainAttachedPartKind.NullForgiving, [postfixUnary.OperatorToken]));
                }
                break;

            case ConditionalAccessExpressionSyntax conditionalAccess:
                {
                    var receiver = conditionalAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary
                                   && postfixUnary.IsKind(SyntaxKind.SuppressNullableWarningExpression)
                                       ? postfixUnary.Operand
                                       : conditionalAccess.Expression;

                    Collect(receiver, pendingConditional, links, attachedParts);
                    Collect(conditionalAccess.WhenNotNull, conditionalAccess, links, attachedParts);
                }
                break;

            case MemberBindingExpressionSyntax memberBinding when pendingConditional != null:
                {
                    links.Add(new FluentChainLink(memberBinding, GetConditionalTokens(pendingConditional, memberBinding.OperatorToken), memberBinding.Name, IsInvoked(memberBinding)));
                }
                break;

            case ElementBindingExpressionSyntax elementBinding when pendingConditional != null:
                {
                    attachedParts.Add(new FluentChainAttachedPart(FluentChainAttachedPartKind.ConditionalElementAccess, GetConditionalTokens(pendingConditional, elementBinding.ArgumentList.OpenBracketToken)));
                }
                break;
        }
    }

    #endregion // Methods
}