using System.Collections.Generic;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Decides whether a rewritten syntax tree still parses into the tokens and the structure it was built with. A rewrite
/// that is built as syntax nodes can write text the parser reads differently: two tokens that touch can lex as one, as
/// <c>return</c> and <c>a</c> do as <c>returna</c>; a <c>&lt;</c> and a later <c>&gt;</c> can read as a type argument
/// list once the tokens between them change; and a name alone in parentheses can read as the type of a cast. Code fixes
/// that must not write such code share this one answer, which the C# parser gives
/// </summary>
public static class RewriteParseUtilities
{
    #region Methods

    /// <summary>
    /// Determines whether the text of a rewritten root parses back into the same tokens and the same structure as the
    /// rewritten root itself. Trivia is not compared; the parse options of the original tree are used, so the language
    /// version and the preprocessor symbols stay the same. The work is bounded by the rewrite: only the smallest node
    /// that contains every change is re-parsed, incrementally, and only the innermost member declaration around it is
    /// compared, so the cost does not grow with the size of the document
    /// </summary>
    /// <param name="originalRoot">Root of the syntax tree the rewrite started from</param>
    /// <param name="rewrittenRoot">Root with the rewrite applied, built from <paramref name="originalRoot"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the rewritten text parses as written; otherwise, <see langword="false"/></returns>
    public static bool ParsesAsWritten(SyntaxNode originalRoot, SyntaxNode rewrittenRoot, CancellationToken cancellationToken)
    {
        if (originalRoot.IsIncrementallyIdenticalTo(rewrittenRoot))
        {
            return true;
        }

        var path = new List<int>();
        var changedOriginal = originalRoot;
        var changedRewritten = rewrittenRoot;
        var comparedRewritten = rewrittenRoot;
        var comparedOriginalStart = originalRoot.FullSpan.Start;
        var comparedDepth = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (changedOriginal is MemberDeclarationSyntax and not BaseNamespaceDeclarationSyntax
                && changedRewritten.RawKind == changedOriginal.RawKind)
            {
                comparedRewritten = changedRewritten;
                comparedOriginalStart = changedOriginal.FullSpan.Start;
                comparedDepth = path.Count;
            }

            var changedChildIndex = GetOnlyChangedChildNode(changedOriginal, changedRewritten);

            if (changedChildIndex < 0)
            {
                break;
            }

            path.Add(changedChildIndex);
            changedOriginal = changedOriginal.ChildNodesAndTokens()[changedChildIndex].AsNode();
            changedRewritten = changedRewritten.ChildNodesAndTokens()[changedChildIndex].AsNode();
        }

        // The change replaces only the text of the smallest node that contains every difference, so the parser re-parses
        // the region around the rewrite and reuses the rest of the original tree
        var originalTree = originalRoot.SyntaxTree;
        var originalText = originalTree.GetText(cancellationToken);
        var change = new TextChange(changedOriginal.FullSpan, changedRewritten.ToFullString());
        var reparsedNode = originalTree.WithChangedText(originalText.WithChanges(change)).GetRoot(cancellationToken);

        for (var depth = 0; depth < comparedDepth; depth++)
        {
            var children = reparsedNode.ChildNodesAndTokens();

            if (path[depth] >= children.Count
                || children[path[depth]].AsNode() is not { } child)
            {
                return false;
            }

            reparsedNode = child;
        }

        return reparsedNode.FullSpan == new TextSpan(comparedOriginalStart, comparedRewritten.FullSpan.Length)
               && HaveSameTokensAndStructure(reparsedNode, comparedRewritten);
    }

    /// <summary>
    /// Gets the index of the only child in which a rewritten node differs from the node it was built from, when that child
    /// is a node on both sides
    /// </summary>
    /// <param name="original">Original node</param>
    /// <param name="rewritten">Rewritten node</param>
    /// <returns>The index of the child, or -1 when the nodes differ in their own children, in a token, or in more than one child</returns>
    private static int GetOnlyChangedChildNode(SyntaxNode original, SyntaxNode rewritten)
    {
        if (original.RawKind != rewritten.RawKind)
        {
            return -1;
        }

        var originalChildren = original.ChildNodesAndTokens();
        var rewrittenChildren = rewritten.ChildNodesAndTokens();

        if (originalChildren.Count != rewrittenChildren.Count)
        {
            return -1;
        }

        var changedChildIndex = -1;

        for (var childIndex = 0; childIndex < originalChildren.Count; childIndex++)
        {
            var originalChild = originalChildren[childIndex];
            var rewrittenChild = rewrittenChildren[childIndex];

            if (originalChild.IsNode
                && rewrittenChild.IsNode
                && originalChild.AsNode().IsIncrementallyIdenticalTo(rewrittenChild.AsNode()))
            {
                continue;
            }

            if (originalChild.IsToken
                && rewrittenChild.IsToken
                && originalChild.AsToken().IsIncrementallyIdenticalTo(rewrittenChild.AsToken()))
            {
                continue;
            }

            if (changedChildIndex >= 0
                || originalChild.IsNode == false
                || rewrittenChild.IsNode == false)
            {
                return -1;
            }

            changedChildIndex = childIndex;
        }

        return changedChildIndex;
    }

    /// <summary>
    /// Determines whether two nodes have the same kind, the same children, and the same tokens, compared by kind and
    /// text. Trivia, and with it every comment and directive, is not compared
    /// </summary>
    /// <param name="left">Left node</param>
    /// <param name="right">Right node</param>
    /// <returns><see langword="true"/> if the nodes have the same tokens and structure; otherwise, <see langword="false"/></returns>
    private static bool HaveSameTokensAndStructure(SyntaxNode left, SyntaxNode right)
    {
        if (left.RawKind != right.RawKind)
        {
            return false;
        }

        var leftChildren = left.ChildNodesAndTokens();
        var rightChildren = right.ChildNodesAndTokens();

        if (leftChildren.Count != rightChildren.Count)
        {
            return false;
        }

        for (var childIndex = 0; childIndex < leftChildren.Count; childIndex++)
        {
            var leftChild = leftChildren[childIndex];
            var rightChild = rightChildren[childIndex];

            if (leftChild.IsToken != rightChild.IsToken)
            {
                return false;
            }

            if (leftChild.IsToken)
            {
                if (leftChild.RawKind != rightChild.RawKind
                    || leftChild.AsToken().IsMissing != rightChild.AsToken().IsMissing
                    || leftChild.AsToken().Text != rightChild.AsToken().Text)
                {
                    return false;
                }
            }
            else if (HaveSameTokensAndStructure(leftChild.AsNode(), rightChild.AsNode()) == false)
            {
                return false;
            }
        }

        return true;
    }

    #endregion // Methods
}