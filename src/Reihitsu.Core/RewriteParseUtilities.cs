using System;
using System.Threading;

using Microsoft.CodeAnalysis;
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
    /// version and the preprocessor symbols stay the same
    /// </summary>
    /// <param name="originalRoot">Root of the syntax tree the rewrite started from</param>
    /// <param name="rewrittenRoot">Root with the rewrite applied, built from <paramref name="originalRoot"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the rewritten text parses as written; otherwise, <see langword="false"/></returns>
    public static bool ParsesAsWritten(SyntaxNode originalRoot, SyntaxNode rewrittenRoot, CancellationToken cancellationToken)
    {
        var originalTree = originalRoot.SyntaxTree;
        var originalText = originalTree.GetText(cancellationToken);
        var change = GetChange(originalText.ToString(), rewrittenRoot.ToFullString());

        // The change covers only the text between the common prefix and the common suffix, so the parser re-parses
        // the region around the rewrite and reuses the rest of the original tree
        var reparsedTree = originalTree.WithChangedText(originalText.WithChanges(change));

        return HaveSameTokensAndStructure(reparsedTree.GetRoot(cancellationToken), rewrittenRoot);
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

    /// <summary>
    /// Gets the one text change that turns the original text into the rewritten text: the text between their common
    /// prefix and their common suffix
    /// </summary>
    /// <param name="originalText">Original text</param>
    /// <param name="rewrittenText">Rewritten text</param>
    /// <returns>The text change</returns>
    private static TextChange GetChange(string originalText, string rewrittenText)
    {
        var maximumLength = Math.Min(originalText.Length, rewrittenText.Length);
        var prefixLength = 0;

        while (prefixLength < maximumLength
               && originalText[prefixLength] == rewrittenText[prefixLength])
        {
            prefixLength++;
        }

        var suffixLength = 0;

        while (suffixLength < maximumLength - prefixLength
               && originalText[originalText.Length - suffixLength - 1] == rewrittenText[rewrittenText.Length - suffixLength - 1])
        {
            suffixLength++;
        }

        return new TextChange(TextSpan.FromBounds(prefixLength, originalText.Length - suffixLength),
                              rewrittenText.Substring(prefixLength, rewrittenText.Length - suffixLength - prefixLength));
    }

    #endregion // Methods
}