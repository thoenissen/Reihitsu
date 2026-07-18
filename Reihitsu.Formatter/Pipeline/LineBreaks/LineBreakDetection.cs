using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Stateless line-break detection helpers shared by the line-break subphase rewriters
/// </summary>
internal static class LineBreakDetection
{
    #region Methods

    /// <summary>
    /// Determines whether an accessor list belongs to an auto-property
    /// (all accessors have neither a body nor an expression body)
    /// </summary>
    /// <param name="accessorList">The accessor list to inspect</param>
    /// <returns><see langword="true"/> if the accessor list is part of an auto-property; otherwise, <see langword="false"/></returns>
    public static bool IsAutoPropertyAccessorList(AccessorListSyntax accessorList)
    {
        foreach (var accessor in accessorList.Accessors)
        {
            if (accessor.Body != null || accessor.ExpressionBody != null)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether a syntax node spans multiple lines
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if the node spans multiple lines; otherwise, <see langword="false"/></returns>
    public static bool IsMultiLine(SyntaxNode node)
    {
        var syntaxTree = node.SyntaxTree;

        if (syntaxTree == null)
        {
            return SourceText.From(node.ToString()).Lines.Count > 1;
        }

        // Span (not FullSpan) excludes leading/trailing trivia, so a blank line or comment
        // directly above or below the node does not make its own single-line text count as
        // multi-line. Reuse the tree's cached line table rather than materializing a SourceText.
        var lineSpan = syntaxTree.GetLineSpan(node.Span);

        return lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line;
    }

    #endregion // Methods
}